using UnityEngine;
using System.Collections;

public interface IEnnemyState 
{
	void UpdateState ();

	void OnTriggerEnter (Collider other);

	void ToPatrolState();

	void ToAlertState();

	void ToChaseState();
}