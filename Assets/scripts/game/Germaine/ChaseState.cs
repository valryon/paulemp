using UnityEngine;
using System.Collections;

public class ChaseState : IEnnemyState {

	private readonly StatePatternEnnemy enemy;

	public ChaseState(StatePatternEnnemy statePatternEnemy){
		enemy = statePatternEnemy;
	}

	public void UpdateState()
	{
		Look ();
		Chase ();
	}

	public void OnTriggerEnter (Collider other){
	}

	public void ToPatrolState(){
	}

	public void ToAlertState(){
		enemy.currentState = enemy.alertState;
	}

	public void ToChaseState(){
		Debug.Log ("Impossible Impossible");
	}

	private void Look(){
		RaycastHit hit;
		Vector3 enemytoTarget = (enemy.chaseTarget.position + enemy.offset) - enemy.eyes.transform.position;
		if (Physics.Raycast (enemy.eyes.transform.position, enemytoTarget, out hit, enemy.sightRange) && hit.collider.CompareTag ("Player")) {
			enemy.chaseTarget = hit.transform;
		} else {
			ToAlertState ();
		}
	}

	private void Chase()
	{
		enemy.meshRenderFlag.material.color = Color.red;
		enemy.navMeshAgent.destination = enemy.chaseTarget.position;
		enemy.navMeshAgent.Resume ();
	}

}
