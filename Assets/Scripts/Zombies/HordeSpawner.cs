﻿using UnityEngine;
using System.Collections;

public class HordeSpawner : MonoBehaviour
{
	public int hordeCount=20;
	public int spawnRatePerSec=4;
	public GameObject enemyObj;
	private bool isSpawning;
	private bool fire;
	private Transform playerTransform;
	private bool isChasing;
	private bool isBomber;

    void Start()
    {
		playerTransform= GameObject.FindGameObjectWithTag("Player").transform;
		if(gameObject.tag == "Horde")
        {
			isBomber = false;
        }
        else
        {
			isBomber = true;
        }
	}

    void Update()
	{
		if (!fire && !isBomber) {
			fire = true;
			isSpawning = true;
		}
		// true represent the pile bomb is attached to the player
		if (isBomber && true && !fire)
		{
			fire = true;
			SetBomberHorde();
		}
		
		if(isSpawning)
        {
			isSpawning = false;
			StartCoroutine(SpawnEnemy());
		}
		if (!isChasing) {
			for (int i = 0; i < transform.childCount; i++)
			{
				GameObject childObject = transform.GetChild(i).gameObject;
				string childState = childObject.GetComponent<EnemyContoller>().getState();
				if (childState == "chasing" || childState == "attack")
				{
					isChasing = true;
					setAllEnemiesToChase();
					return;
				}
			}
		}
	}
	IEnumerator SpawnEnemy()
	{
		for (int i = 0; i <hordeCount ; i++)
		{


			Vector3 position = new Vector3(transform.position.x+ Random.Range(-5, 5), transform.position.y,transform.position.z+Random.Range(-5, 5));
			GameObject childObject = Instantiate(enemyObj, position, transform.rotation);
			childObject.transform.parent = gameObject.transform;
			yield return new WaitForSeconds(1f / spawnRatePerSec);
		}
		isSpawning = false;
        if (isBomber)
        {
			fire = false;
        }

		yield break;
	}
	public void SetBomberHorde()
    {
	isSpawning = true;
	hordeCount = 16;
	spawnRatePerSec = 4;
	}
	public void setAllEnemiesToChase()
    {
		for (int i = 0; i < transform.childCount; i++)
		{
			GameObject childObject = transform.GetChild(i).gameObject;
			EnemyContoller childEnemyController = childObject.GetComponent<EnemyContoller>();
			string childState = childEnemyController.getState();
			if (childState == "patrol" || childState == "idle")
			{
				childEnemyController.chase(playerTransform);
			}
		}
	}


}
