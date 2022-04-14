using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;

public class DuncanBoss_IdleState : State<AI> {

	public float alertDistance;
	private bool initialized = false;

	public override bool EnterState(AI _owner){
		Debug.Log ("Entering Startup State");
		_owner.alertDistance = alertDistance;
		_owner.currentState = this.ToString();
		if (initialized)
			InstaSearchForTargets (_owner);
		return true;
	}

	public override void ExitState(AI _owner){
		Debug.Log("Exiting Startup State");
		initialized = true;
	}

	public override void UpdateState(AI _owner){
		if (!_owner.coroRunning)
			currentCoroutine = _owner.StartCoroutine (SearchForTargets(_owner));

		if(_owner.target && _owner.GetComponent<DuncanBoss_MoveTowardsEnemy>()){
			_owner.stateMachine.ChangeState(_owner.GetComponent<DuncanBoss_MoveTowardsEnemy>());
		}
	}

	public void InstaSearchForTargets(AI _owner){
		_owner.coroRunning = true;
		RaycastHit hit2;
		int origLayer = gameObject.layer;
		gameObject.layer = 2;
		foreach (GameObject enemy in _owner.listOfEnemies) {
			if (enemy != null && Physics.Raycast (_owner.references.Head.transform.position, (enemy.transform.position - _owner.references.Head.transform.position).normalized, out hit2, alertDistance * 2f, _owner.searchingForTargetIgnoreLayers)) {
				if (hit2.collider.gameObject == enemy) {
					_owner.target = enemy;
					_owner.isAlerted = true;
					CustomMethods.SmoothRotateTowards (this.gameObject, _owner.target.transform, 0.25f);
				}
			}
		}
		gameObject.layer = origLayer;
		_owner.coroRunning = false;
	}

	IEnumerator SearchForTargets(AI _owner){
		_owner.coroRunning = true;
		yield return new WaitForSeconds (0.1f);
		RaycastHit hit2;
		int origLayer = gameObject.layer;
		gameObject.layer = 2;
		foreach (GameObject enemy in _owner.listOfEnemies) {
			if (enemy != null && Physics.Raycast (_owner.references.Head.transform.position, (enemy.transform.position - _owner.references.Head.transform.position).normalized, out hit2, alertDistance, _owner.searchingForTargetIgnoreLayers)) {
				if (hit2.collider.gameObject == enemy && Vector3.Dot ((enemy.transform.transform.position - transform.position).normalized, transform.forward) > 0.3f) {
					_owner.target = enemy;
					_owner.isAlerted = true;
					CustomMethods.SmoothRotateTowards (this.gameObject, _owner.target.transform, 0.25f);
					//break;
				}
			}
		}
		gameObject.layer = origLayer;
		_owner.coroRunning = false;
	}

	public override void ForceInterruption(AI _owner){
		if (currentCoroutine != null)
			StopCoroutine (currentCoroutine);
		_owner.coroRunning = false;
	}
}
