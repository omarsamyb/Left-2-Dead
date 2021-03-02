using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Horde : MonoBehaviour
{
    public GameObject zombiePrefab;
    InfectedController infectedController;
    private float spawnRadius;
    private int count;
    private NavMeshHit navMeshHit;
    private Vector3 randomPoint;
    private bool playerDetected;
    private List<InfectedController> spawned = new List<InfectedController>();

    void Start()
    {
        spawnRadius = 4f;
        count = 20;
        Spawn();
    }
    private void Spawn()
    {
        if (NavMesh.SamplePosition(transform.position, out navMeshHit, 2.0f, 1 << NavMesh.GetAreaFromName("Walkable")))
        {
            infectedController = Instantiate(zombiePrefab, navMeshHit.position, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f)).GetComponent<InfectedController>();
            infectedController.HordeDetectPlayer += AlarmZombies;
            spawned.Add(infectedController);
            count--;
        }
        while(count > 0)
        {
            randomPoint = (transform.position - transform.forward * spawnRadius) + Random.insideUnitSphere * spawnRadius;
            if (NavMesh.SamplePosition(randomPoint, out navMeshHit, 2.0f, 1 << NavMesh.GetAreaFromName("Walkable")))
            {
                infectedController = Instantiate(zombiePrefab, navMeshHit.position, Quaternion.Euler(0f, transform.rotation.eulerAngles.y + Random.Range(-40f, 40f), 0f)).GetComponent<InfectedController>();
                infectedController.HordeDetectPlayer += AlarmZombies;
                spawned.Add(infectedController);
                count--;
            }
        }
    }
    public void AlarmZombies()
    {
        if (!playerDetected)
        {
            playerDetected = true;
            foreach (InfectedController infected in spawned)
            {
                if (infected.state == InfectedState.idle || infected.state == InfectedState.patrol || infected.state == InfectedState.empty)
                {
                    infected.ChasePlayer();
                }
                infected.HordeDetectPlayer -= AlarmZombies;
            }
            Destroy(this, 1f);
        }
    }
}
