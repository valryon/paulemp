using UnityEngine;
using System.Collections;

public class AlertState : IEnnemyState {

	private readonly StatePatternEnnemy enemy;
	private float searchTime;

	public AlertState (StatePatternEnnemy statePatternEnemy)
	{
		enemy = statePatternEnemy;
	}


	public void UpdateState()
	{
		Look ();
		Search ();
	}

	public void OnTriggerEnter (Collider other){
	}

	public void ToPatrolState(){
	}

	public void ToAlertState(){
		Debug.Log ("Peut pas passez de alert à alert");
	}

	public void ToChaseState(){
		enemy.currentState = enemy.chaseState;
		searchTime = 0f;
	}

	private void Look(){
		RaycastHit hit;
		if (Physics.Raycast (enemy.eyes.transform.position, enemy.eyes.transform.forward, out hit, enemy.sightRange) && hit.collider.CompareTag ("Player")) {
			enemy.chaseTarget = hit.transform;
			ToChaseState ();
		}

	}
	private void  Search()
	{
		enemy.meshRenderFlag.material.color = Color.yellow;
		enemy.navMeshAgent.Stop ();
		enemy.transform.Rotate (0, enemy.searchingTurnSpeed * Time.deltaTime, 0);
		searchTime += Time.deltaTime;

		if (searchTime >= enemy.searchingDuration) {
			ToPatrolState ();
		}
	}
}
