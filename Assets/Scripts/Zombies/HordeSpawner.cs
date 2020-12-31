using UnityEngine;
using System.Collections;

public class HordeSpawner : MonoBehaviour
{
	private int hordeCount;
	private int spawnRatePerSec;
	public GameObject enemyObj;
	private bool isSpawning;
	private bool fire;

    void Update()
	{
		if (!fire) {
			if (gameObject.tag == "Horde")
			{
				fire = true;
				SetBasicHorde();
			}
			// true represent the pile bomb is attached to the player
            else if (gameObject.tag == "Bomber" && true)
			{
				fire = true;
				SetBomberHorde();
            }
		}
		if(isSpawning)
        {
			isSpawning = false;
			StartCoroutine(SpawnEnemy());
		}
	}
	IEnumerator SpawnEnemy()
	{
		for (int i = 0; i <hordeCount ; i++)
		{

			Instantiate(enemyObj, transform.position, transform.rotation);
			yield return new WaitForSeconds(1f / spawnRatePerSec);
		}
		isSpawning = false;

		yield break;
	}
	public void SetBomberHorde()
    {
	isSpawning = true;
	hordeCount = 16;
	spawnRatePerSec = 4;
	}
	public void SetBasicHorde()
    {
	isSpawning = true;
	hordeCount=20;
	spawnRatePerSec=4;
	}


}
