using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;

public class DuncanBoss_MoveTowardsEnemy : State<AI> {

	private AI __owner;
	private bool active = false;

	public float walkSpeed;

	[Header(" ")]
	public float closeDistance;
	[Header(" ")]
	public float avoidTargetDistance;
	public float avoidTargetPower;
	public float avoidTargetBaseCooldown;
	private float avoidTargetTimer;

	private RaycastHit hit;
	private Vector3 directionToTarget;

	private bool rageMode;

	// Use this for initialization
	void Start () {
		
	}

	public override bool EnterState(AI _owner){
		Debug.Log ("Entering 'Move towards Enemy' State");
		__owner = _owner;
		_owner.navMeshAgent.isStopped = false;
		_owner.currentState = this.ToString();
		active = true;
		return true;
	}

	public override void ExitState(AI _owner){
		Debug.Log("Exiting 'Move towards Enemy' State");
		_owner.references.animationScript.ResetValues ();
		_owner.navMeshAgent.isStopped = true;
		active = false;
	}

	public override void ForceInterruption(AI _owner){
		if (currentCoroutine != null)
			StopCoroutine (currentCoroutine);
		_owner.navMeshAgent.enabled = false;
		active = false;
	}

	public override void UpdateState(AI _owner){
		if (!_owner.doNotMove) {
			if (_owner.target && _owner.distance <= closeDistance * 2f && _owner.targetIsInSight || _owner.references.animationScript.isBouncing) {
				directionToTarget = _owner.target.transform.position - transform.position;
				directionToTarget.y = 0f;
				transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (directionToTarget), 5 * Time.deltaTime);
			}
			/*
			if (_owner.distance <= closeDistance && _owner.GetComponent<AI_ComboAState> ()._coolDown <= 0f) {
				_owner.stateMachine.ChangeState (_owner.GetComponent<AI_ComboAState> ());
			} else*/ if (!_owner.coroRunning && _owner.distance > closeDistance) {
				currentCoroutine = _owner.StartCoroutine (Displacement (_owner));
			} else if (!_owner.coroRunning) {
				currentCoroutine = _owner.StartCoroutine (Displacement (_owner));
			}

			if (avoidTargetBaseCooldown > 0f) {
				if (avoidTargetTimer > 0f)
					avoidTargetTimer -= Time.deltaTime;
			}

			if (_owner.navMeshAgent.velocity.magnitude > 0.1f) {
				_owner.references.animationScript.isMoving = true;
				_owner.references.animationScript.inputV = 1f;
				_owner.references.animationScript.inputH = 1f;
			} else {
				_owner.references.animationScript.isMoving = false;
				_owner.references.animationScript.inputV = 0f;
				_owner.references.animationScript.inputH = 0f;
			}
		}
	}

	void FixedUpdate(){
		if (active && __owner.navMeshAgent.isActiveAndEnabled && __owner.references.rigidbody.velocity.magnitude > __owner.navMeshAgent.speed)
			__owner.references.rigidbody.velocity = __owner.references.rigidbody.velocity.normalized * __owner.navMeshAgent.speed;
	}

	IEnumerator Displacement(AI _owner){
		_owner.coroRunning = true;
		if (!_owner.doNotMove && _owner.target && _owner.target.GetComponent<HealthBar> () && !_owner.target.GetComponent<HealthBar> ().isDead) {
			_owner.navMeshAgent.enabled = true;	
			_owner.distance = Vector3.Distance (transform.position, _owner.target.transform.position);

			//Off mesh link tester
			if (_owner.navMeshAgent.isOnOffMeshLink) {
				_owner.navMeshAgent.isStopped = true;
				yield return new WaitForSeconds (0.25f);
				//StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 1.0f, 0.5f));
				StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.navMeshAgent.currentOffMeshLinkData.endPos, Vector3.Distance (gameObject.transform.position, _owner.navMeshAgent.currentOffMeshLinkData.endPos) * 0.75f, 0.5f));
				yield return new WaitForSeconds (0.75f);
				_owner.navMeshAgent.isStopped = false;
			}

			int direction = Random.Range (-1, 2);
			Vector3 dir = (transform.position - _owner.target.transform.position).normalized;
			float vectorDot = Vector3.Dot (_owner.target.transform.forward, dir);
			float randomSeconds = Random.Range (0.01f, 0.3f); 

			if (_owner.target && avoidTargetTimer <= 0f && _owner.distance < 15f && _owner.targetIsInSight && vectorDot > -0.6f && _owner.target.GetComponent<Player_Animation> ().anim.GetCurrentAnimatorStateInfo (0).IsName ("Aim")) {
				yield return new WaitForSeconds (randomSeconds);
				StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 0.25f, 1f));
				direction = Random.Range (0, 2);
				avoidTargetTimer = avoidTargetBaseCooldown * Random.Range (0.5f, 1.5f);
				_owner.navMeshAgent.enabled = false;
				GetComponent<Rigidbody> ().velocity = Vector3.zero;
				GetComponent<Rigidbody> ().AddForce (Vector3.up * avoidTargetPower, ForceMode.VelocityChange);
				_owner.references.animationScript.anim.Play ("Bouncing");
				switch (direction) {
				case 0:
					GetComponent<Rigidbody> ().AddForce (transform.right * avoidTargetPower * 2.125f, ForceMode.VelocityChange);
					break;
				case 1:
					GetComponent<Rigidbody> ().AddForce (-transform.right * avoidTargetPower * 2.125f, ForceMode.VelocityChange);					
					break;
				}

				_owner.references.animationScript.isBouncing = true;
				yield return new WaitForSeconds (0.5f);
				_owner.references.animationScript.isBouncing = false;
				if (!_owner.isGrounded) {
					while (!_owner.isGrounded) {
						yield return new WaitForFixedUpdate ();
					}
				}
				GetComponent<Rigidbody> ().velocity = Vector3.zero;
				_owner.references.animationScript.isBouncing = false;	
				yield return new WaitForSeconds (0.5f);
				_owner.navMeshAgent.enabled = true;				

			} else if (avoidTargetBaseCooldown > 0f && avoidTargetTimer <= 0f && _owner.distance < avoidTargetDistance && _owner.targetIsInSight && vectorDot > -0.5f /*&& _owner.target.GetComponent<References> ().animationScript.isAttackingMelee*/) {
				yield return new WaitForSeconds (randomSeconds);
				StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 0.25f, 1f));
				direction = Random.Range (-1, 2);
				avoidTargetTimer = avoidTargetBaseCooldown * Random.Range (0.5f, 1.5f);
				_owner.navMeshAgent.enabled = false;
				GetComponent<Rigidbody> ().velocity = Vector3.zero;
				GetComponent<Rigidbody> ().AddForce (Vector3.up * avoidTargetPower * 0.75f, ForceMode.VelocityChange);
				GetComponent<Rigidbody> ().AddForce ((transform.position - _owner.target.transform.position).normalized * avoidTargetPower, ForceMode.VelocityChange);
				_owner.references.animationScript.anim.Play ("Bouncing");
				switch (direction) {
				case -1:
					break;
				case 0:
					GetComponent<Rigidbody> ().AddForce (transform.right * avoidTargetPower * 2f, ForceMode.VelocityChange);
					break;
				case 1:
					GetComponent<Rigidbody> ().AddForce (-transform.right * avoidTargetPower * 2f, ForceMode.VelocityChange);					
					break;
				}

				_owner.references.animationScript.isBouncing = true;
				yield return new WaitForSeconds (0.5f);
				_owner.references.animationScript.isBouncing = false;
				if (!_owner.isGrounded) {
					while (!_owner.isGrounded) {
						yield return new WaitForFixedUpdate ();
					}
				}

				GetComponent<Rigidbody> ().velocity = Vector3.zero;
				_owner.references.animationScript.isBouncing = false;	
				yield return new WaitForSeconds (0.5f);
				_owner.navMeshAgent.enabled = true;						
			} else {
				if (_owner.distance > closeDistance) {
					_owner.navMeshAgent.enabled = true;
					_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 1f, 4f * Time.deltaTime);
					_owner.navMeshAgent.isStopped = false;
					_owner.navMeshAgent.speed = walkSpeed;
					_owner.navMeshAgent.SetDestination (_owner.target.transform.position);

				} else {
					_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 0f, 4f * Time.deltaTime);
					_owner.navMeshAgent.enabled = false;
					yield return new RadicalWaitForSeconds (0.1f);
				}
			}				

			yield return new RadicalWaitForSeconds (0.1f);
			_owner.coroRunning = false;
		}
	}
}
