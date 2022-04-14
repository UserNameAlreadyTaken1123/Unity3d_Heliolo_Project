using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;

public class AI_MoveTowardsEnemy : State<AI> {

	protected AI __owner;
	protected bool active = false;

	public bool canSuperJump;
	public bool shouldAvoidAllies;

	protected Vector3 directionOffset;

	public float walkSpeed;
	public float runSpeed;
	public float sprintSpeed;

	[Header(" ")]
	public float walkDistance;
	public float runDistance;
	[Header(" ")]
	public float closeDistance;
	public float _closeDistance;
	[Header(" ")]
	public float jumpChance = 30f;
	protected float jumpChanceReturned;
	public float superJumpPower;
	public float superJumpDistance;
	public float superJumpBaseTimer;
	protected float superJumpTimer;
	[Header(" ")]
	public float avoidTargetDistance;
	public float avoidTargetPower;
	public float avoidTargetBaseCooldown;
	protected float avoidTargetTimer;

	protected RaycastHit hit;
	protected Vector3 directionToTarget;

	protected bool rageMode;
	protected float returnJumpChanceTimer = 1f;

	protected bool doneDelay = false;
	protected float destinationTimer;

	protected Coroutine checkForBreakConditions;
	protected Coroutine coroSuperJump;
	protected bool paraJumpRunning = false;
	protected bool breakConditionsMet = false;

	protected float closeDisUpdateTimer;
	protected bool startedMoving = false;

	public void Awake(){
		superJumpTimer = superJumpBaseTimer * Random.Range (0.5f, 2f);
	}

	public override bool EnterState(AI _owner){
		__owner = _owner;
		_owner.currentState = this.ToString();
		_closeDistance = closeDistance;
		active = true;

		StartCoroutine (Delay (_owner));
		return true;
	}

	public override void ExitState(AI _owner){
		//_owner.references.animationScript.ResetValues ();
		if (!_owner.navMeshAgent.isStopped)
			_owner.navMeshAgent.isStopped = true;
		active = false;
		doneDelay = false;
		startedMoving = false;
	}

	public override void ForceInterruption(AI _owner){
		if (currentCoroutine != null)
			StopCoroutine (currentCoroutine);
		if (_owner.navMeshAgent.isStopped)
			_owner.navMeshAgent.isStopped = false;
		active = false;
		doneDelay = false;
	}

	public override void UpdateState(AI _owner){
		if (doneDelay /*&& !_owner.doNotMove*/) {
			if (_owner.target && _owner.distance <= _closeDistance * 2f && (_owner.targetIsInSight || _owner.references.animationScript.isBouncing)) {
				directionToTarget = _owner.target.transform.position - transform.position;
				directionToTarget.y = 0f;
				transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (directionToTarget), 5 * Time.deltaTime);
			}

			if (!_owner.coroRunning && _owner.target && _owner.isBlader && _owner.distance <= _closeDistance && _owner.GetComponent<AI_ComboAState> ().canAttack) {
				_owner.stateMachine.ChangeState (_owner.GetComponent<AI_ComboAState> ());
			} else if (!_owner.coroRunning && _owner.target && _owner.isGunner && _owner.GetComponent<AI_ShootFar> ().canShoot &&
			           _owner.distance > (_owner.GetComponent<AI_ShootFar> ().minAttackDistance) && _owner.distance < (_owner.GetComponent<AI_ShootFar> ().maxAttackDistance) &&
			           _owner.healthBar.CurHealth <= _owner.healthBar.Maxhealth * 4 / 5) {
				_owner.stateMachine.ChangeState (_owner.GetComponent<AI_ShootFar> ());
				return;
			} else if (!_owner.coroRunning && _owner.target && _owner.isCaster/*&& _owner.GetComponent<AI_Fireball> ().canShoot && _owner.GetComponent<AI_Fireball> ().returnedChance < _owner.GetComponent<AI_Fireball> ().chance*/
			           && _owner.distance > (_owner.GetComponent<AI_Fireball> ().minAttackDistance) && _owner.distance < (_owner.GetComponent<AI_Fireball> ().maxAttackDistance) &&
			           _owner.healthBar.CurHealth <= _owner.healthBar.Maxhealth * 1 / 2) {
				_owner.stateMachine.ChangeState (_owner.GetComponent<AI_Fireball> ());
			} else if (!_owner.coroRunning && _owner.distance > _closeDistance) {
				startedMoving = true;
				currentCoroutine = _owner.StartCoroutine (Displacement (_owner));
			} else if (!_owner.coroRunning) {
				startedMoving = true;
				currentCoroutine = _owner.StartCoroutine (Displacement (_owner));
			} else {
				startedMoving = false;
			}

			if (avoidTargetBaseCooldown > 0f) {
				if (avoidTargetTimer > 0f)
					avoidTargetTimer -= Time.deltaTime;
			}

			if (superJumpBaseTimer > 0f) {
				if (superJumpTimer > 0f)
					superJumpTimer -= Time.deltaTime * _owner.distance / 10f;
			}

			if (returnJumpChanceTimer > 0) {
				returnJumpChanceTimer -= Time.deltaTime;
			} else {
				jumpChanceReturned = Random.Range (0f, 100f);
				returnJumpChanceTimer = 1f;
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

		if (!rageMode && _owner.healthBar.CurHealth < _owner.healthBar.Maxhealth / 2f) {
			rageMode = true;
			superJumpBaseTimer = superJumpBaseTimer / 2f;
			avoidTargetTimer = avoidTargetTimer / 2f;
		}


		closeDisUpdateTimer -= Time.deltaTime;
		if (destinationTimer <= 0f && _owner.navMeshAgent.isActiveAndEnabled) {
			destinationTimer = Random.Range (0.125f, 0.25f);
			if (!_owner.hasLeader || _owner.target != null) {
				_owner.navMeshAgent.SetDestination (_owner.target.transform.position);
				_closeDistance = closeDistance;
			}
			else {
				if (!startedMoving && closeDisUpdateTimer <= 0f) {
					closeDisUpdateTimer = 2f;
					_closeDistance = closeDistance * Random.Range (0.35f, 2f);
				}
				_owner.navMeshAgent.SetDestination (_owner.leader.transform.position);
			}
		} else {
			destinationTimer = destinationTimer - (Time.deltaTime / (_owner.distance * Random.Range(0.5f, 1.5f) / _closeDistance));
		}
	}

	void FixedUpdate(){

		if (active && __owner.navMeshAgent.isActiveAndEnabled && __owner.navMeshAgent.velocity.magnitude > __owner.navMeshAgent.speed)
			__owner.navMeshAgent.velocity = Vector3.ClampMagnitude (__owner.navMeshAgent.velocity, __owner.navMeshAgent.speed);
	}

	protected IEnumerator Delay(AI _owner){
		yield return new WaitForFixedUpdate ();
		if (!doneDelay) {
			_owner.navMeshAgent.isStopped = false;
			doneDelay = true;	
		} else {
			_owner.navMeshAgent.isStopped = true;
		}
	}

	protected IEnumerator CheckForBreakConditions (){
		breakConditionsMet = false;
		while (paraJumpRunning) {
			if (__owner.healthBar.justGotHurt || __owner.healthBar.references.triggering) {
				//__owner.references.rigidbody.velocity = Vector3.zero;
				breakConditionsMet = true;
				break;
			}

			yield return null;
		}
	}


	IEnumerator Displacement(AI _owner){
		_owner.coroRunning = true;

		//Evitar aliados siempre!!!
		if (shouldAvoidAllies && _owner.isGrounded && _owner.avoidAlliesScript.alliesInRange.Count > 0) {
			int direction = Random.Range (-1, 2);
			float vectorDot = Vector3.Dot (_owner.target.transform.forward, directionToTarget);
			float randomSeconds = Random.Range (0.01f, 0.3f); 
			directionOffset = Vector3.zero;

			int counter = 0;
			foreach (GameObject ally in _owner.avoidAlliesScript.alliesInRange) {
				if (counter == 2)
					break;
				vectorDot = Vector3.Dot (ally.transform.forward, (transform.position - ally.transform.position).normalized);
				if (vectorDot < 0f) {
					directionOffset = directionOffset + (transform.position - ally.transform.position).normalized;
					directionOffset.y = 0.0f;
					directionOffset.x = directionOffset.x + transform.right.x * direction;
					counter += 1;
				}
			}
			directionOffset = Vector3.Normalize (directionOffset);
			directionOffset.y = 0f;
			//_owner.references.rigidbody.AddForce (directionOffset * 10f, ForceMode.VelocityChange);
		}

		if (!_owner.doNotMove && _owner.target && _owner.target.GetComponent<HealthBar> () && !_owner.target.GetComponent<HealthBar> ().isDead) {
			_owner.navMeshAgent.enabled = true;	
			_owner.distance = Vector3.Distance (transform.position, _owner.target.transform.position);
			/*			if (!Physics.Raycast (Head.transform.position, (target.transform.position - Head.transform.position).normalized, out hit2, alertDistance, searchingForTargetIgnoreLayers)) {
				iKnowWhereYouAre2 -= Time.fixedDeltaTime;
			} else
				iKnowWhereYouAre2 = iKnowWhereYouAre1;
*/				
			/*
			if (distance <= _closeDistance * 2f) {
				_owner.navMeshAgent.updatePosition = false;
				_owner.navMeshAgent.updateRotation = false;
				transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (_owner.target.transform.position), 15f * Time.deltaTime);
			} else {
				_owner.navMeshAgent.updatePosition = true;
				_owner.navMeshAgent.updateRotation = true;
			}
			*/

			int direction = Random.Range (-1, 2);
			directionToTarget = (transform.position - _owner.target.transform.position).normalized;
			float vectorDot = Vector3.Dot (_owner.target.transform.forward, directionToTarget);
			float randomSeconds = Random.Range (0.01f, 0.3f); 
			directionOffset = Vector3.zero;

			//Off mesh link tester
			if (_owner.navMeshAgent.isOnOffMeshLink) {
				_owner.references.animationScript.isJumping = true;
				_owner.navMeshAgent.isStopped = true;
				yield return new WaitForSeconds (0.25f);
				//StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 1.0f, 0.5f));
				StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.navMeshAgent.currentOffMeshLinkData.endPos, Vector3.Distance (gameObject.transform.position, _owner.navMeshAgent.currentOffMeshLinkData.endPos) * 0.75f, 0.5f));
				yield return new WaitForSeconds (0.5f);
				_owner.references.animationScript.isJumping = false;
				_owner.references.animationScript.anim.Play ("Cool Landing");
				yield return new WaitForSeconds (0.25f);
				_owner.navMeshAgent.isStopped = false;
			}

			/*
			if (shouldAvoidAllies && _owner.isGrounded && _owner.avoidAlliesScript.alliesInRange.Count > 0) {
				int counter = 0;
			foreach (GameObject ally in _owner.avoidAlliesScript.alliesInRange) {
				if (counter == 2)
					break;
				vectorDot = Vector3.Dot (ally.transform.forward, (transform.position - ally.transform.position).normalized);
				if (vectorDot < 0f) {
					directionOffset = directionOffset + (transform.position - ally.transform.position).normalized;
					directionOffset.y = 0.0f;
					directionOffset.x = directionOffset.x + transform.right.x * direction;
					counter += 1;
				}
			}
				directionOffset = Vector3.Normalize (directionOffset);
				_owner.references.rigidbody.AddForce (directionOffset, ForceMode.VelocityChange);
			}		
			*/

			direction = Random.Range (-1, 2);
			directionToTarget = (transform.position - _owner.target.transform.position).normalized;
			vectorDot = Vector3.Dot (_owner.target.transform.forward, directionToTarget);

			//SUPERJUMP
			if (jumpChanceReturned < jumpChance) {
				if (canSuperJump && superJumpTimer <= 0f && _owner.distance < superJumpDistance + 1f && _owner.distance > _closeDistance + 1f) { //salto normal, para acortar distancia a alta velocidad.
					superJumpTimer = superJumpBaseTimer + superJumpBaseTimer * Random.Range (0f, 1f);
					_owner.doNotMove = true;
					_owner.navMeshAgent.enabled = false;
					_owner.navMeshAgent.updateRotation = false;
					_owner.checkForGround = false;
					_owner.isGrounded = false;
					_owner.references.animationScript.isJumping = true;
					_owner.references.animationScript.anim.Play ("Jump");
					_owner.references.rigidbody.velocity = Vector3.zero;
					yield return new WaitForSeconds (0.3f);

					_owner.references.rigidbody.velocity = Vector3.zero;
					_owner.references.collider.material.dynamicFriction = 0.0f;
					_owner.references.collider.material.staticFriction = 0.0f;

					Vector3 startPos = transform.position;
					Vector3 tempPos = startPos;
					Vector3 prevPos = startPos;
					Vector3 destination = _owner.target.transform.position + _owner.target.transform.forward * 2f * Random.Range (-1, 1) + _owner.target.transform.right * 2f * Random.Range (-1, 1);

					float height = Vector3.Distance (_owner.transform.position, _owner.target.transform.position) / 5f + superJumpPower;
					float duration = Vector3.Distance (_owner.transform.position, _owner.target.transform.position) / 2.5f;
					float normalizedTimeA = 0.0f;

					paraJumpRunning = true;
					breakConditionsMet = false;

					if (checkForBreakConditions != null)
						StopCoroutine (checkForBreakConditions);

					checkForBreakConditions = StartCoroutine (CheckForBreakConditions ());
					coroSuperJump = StartCoroutine (CustomMethods.SuperJumpExecution(_owner.references.rigidbody, destination, height));


					/*
					while (!breakConditionsMet && (normalizedTimeA < 1 || !_owner.isGrounded && normalizedTimeA > 1)) {
						if (breakConditionsMet)
							break;

						prevPos = tempPos;

						float yOffset = height * 4.0f * (normalizedTimeA - normalizedTimeA * normalizedTimeA); // formula para la altura
						tempPos = CustomMethods.Vector3LerpUnclamped (startPos, destination, normalizedTimeA) + yOffset * Vector3.up;

						RaycastHit hitPoint;
						if (Physics.Raycast (prevPos + (destination - startPos).normalized * _owner.references.capsuleCollider.radius / 2f, tempPos - prevPos, out hitPoint, _owner.references.capsuleCollider.radius, LayerMask.GetMask ("Scenario", "Default"), QueryTriggerInteraction.Ignore)) { //detectar colision y mover al punto de impacto
							_owner.references.rigidbody.MovePosition (_owner.references.rigidbody.position + (prevPos - hitPoint.point).normalized * _owner.references.capsuleCollider.radius);
							break;
						}

						//rigidbody.MovePosition (tempPos); //posicionar rigidbody y actualizar loop
						_owner.references.rigidbody.MovePosition (Vector3.Lerp (transform.position, tempPos, 0.25f));
						normalizedTimeA += Time.deltaTime + (Time.deltaTime - Time.deltaTime * Time.deltaTime) / duration;
						yield return new WaitForFixedUpdate ();
					}
					*/

					yield return new WaitForSeconds (0.1f);

					_owner.checkForGround = true;
					paraJumpRunning = false;

					if (!_owner.isGrounded) {
						while (!_owner.isGrounded) {
							yield return null;
						}
					}

					//if (_owner.isGrounded) {
						_owner.references.animationScript.isJumping = false;
						_owner.references.animationScript.anim.Play ("Cool Landing");
						_owner.references.rigidbody.velocity = Vector3.zero;
					//}

					yield return new WaitForSeconds (1.0f);
					_owner.doNotMove = false;
					_owner.navMeshAgent.enabled = true;	
					_owner.navMeshAgent.updateRotation = true;
					_owner.coroRunning = false;

					yield break;
				} else if (superJumpTimer <= superJumpBaseTimer / 2f && _owner.navMeshAgent.velocity.magnitude < 0.01f && _owner.distance > superJumpDistance + 1f && _owner.distance < superJumpDistance * 1.5f /*&& _owner.target.transform.position.y - 2f > transform.position.y*/) {
					//Superjump si el player está en una plataforma inalcanzable
					direction = Random.Range(-1, 2);
					superJumpTimer = superJumpBaseTimer + superJumpBaseTimer * Random.Range(0f, 1f);
					_owner.doNotMove = true;
					_owner.navMeshAgent.enabled = false;
					_owner.navMeshAgent.updateRotation = false;
					_owner.checkForGround = false;
					_owner.isGrounded = false;
					_owner.references.animationScript.isJumping = true;
					_owner.references.animationScript.anim.Play("Jump");
					_owner.references.rigidbody.velocity = Vector3.zero;
					yield return new WaitForSeconds(0.3f);

					_owner.references.rigidbody.velocity = Vector3.zero;
					_owner.references.collider.material.dynamicFriction = 0.0f;
					_owner.references.collider.material.staticFriction = 0.0f;

					Vector3 startPos = transform.position;
					Vector3 tempPos = startPos;
					Vector3 prevPos = startPos;
					Vector3 destination = _owner.target.transform.position + _owner.target.transform.forward * 2f * Random.Range(-1, 1) + _owner.target.transform.right * 2f * Random.Range(-1, 1);

					float height = Vector3.Distance(_owner.transform.position, _owner.target.transform.position) / 5f + superJumpPower;
					float duration = Vector3.Distance(_owner.transform.position, _owner.target.transform.position) / 2.5f;
					float normalizedTimeA = 0.0f;

					paraJumpRunning = true;
					breakConditionsMet = false;

					if (checkForBreakConditions != null)
						StopCoroutine(checkForBreakConditions);

					switch (direction) {
						case -1:
							if (Physics.Raycast(_owner.target.transform.position + _owner.target.transform.forward * (-1.5f),
									Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
								destination = hit.point;
							} else if (Physics.Raycast(_owner.target.transform.position + _owner.target.transform.forward * 1.5f,
										   Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
								destination = hit.point;
							}
							break;
						case 0:
							if (Physics.Raycast(_owner.target.transform.position + _owner.target.transform.forward * 1.5f,
									Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
								destination = hit.point;
							} else if (Physics.Raycast(_owner.target.transform.position + _owner.target.transform.forward * (-1.5f),
										   Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
								destination = hit.point;
							}
							break;
						case 1:
							if (Physics.Raycast(_owner.target.transform.position + _owner.target.transform.forward * (-1.5f),
									Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
								destination = hit.point;
							} else if (Physics.Raycast(_owner.target.transform.position + _owner.target.transform.forward * 1.5f,
										   Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
								destination = hit.point;
							}
							break;
					}

					checkForBreakConditions = StartCoroutine(CheckForBreakConditions());
					coroSuperJump = StartCoroutine(CustomMethods.SuperJumpExecution(_owner.references.rigidbody, destination, height));


					/*
					while (!breakConditionsMet && (normalizedTimeA < 1 || !_owner.isGrounded && normalizedTimeA > 1)) {
						if (breakConditionsMet)
							break;

						prevPos = tempPos;

						float yOffset = height * 4.0f * (normalizedTimeA - normalizedTimeA * normalizedTimeA); // formula para la altura
						tempPos = CustomMethods.Vector3LerpUnclamped (startPos, destination, normalizedTimeA) + yOffset * Vector3.up;

						RaycastHit hitPoint;
						if (Physics.Raycast (prevPos + (destination - startPos).normalized * _owner.references.capsuleCollider.radius / 2f, tempPos - prevPos, out hitPoint, _owner.references.capsuleCollider.radius, LayerMask.GetMask ("Scenario", "Default"), QueryTriggerInteraction.Ignore)) { //detectar colision y mover al punto de impacto
							_owner.references.rigidbody.MovePosition (_owner.references.rigidbody.position + (prevPos - hitPoint.point).normalized * _owner.references.capsuleCollider.radius);
							break;
						}

						//rigidbody.MovePosition (tempPos); //posicionar rigidbody y actualizar loop
						_owner.references.rigidbody.MovePosition (Vector3.Lerp (transform.position, tempPos, 0.25f));
						normalizedTimeA += Time.deltaTime + (Time.deltaTime - Time.deltaTime * Time.deltaTime) / duration;
						yield return new WaitForFixedUpdate ();
					}
					*/

					yield return new WaitForFixedUpdate();
					_owner.checkForGround = true;
					paraJumpRunning = false;

					if (!_owner.isGrounded) {
						while (!_owner.isGrounded) {
							yield return null;
						}
					}

					//if (_owner.isGrounded) {
					_owner.references.animationScript.isJumping = false;
					_owner.references.animationScript.anim.Play("Cool Landing");
					_owner.references.rigidbody.velocity = Vector3.zero;
					//}

					yield return new WaitForSeconds(1.0f);
					_owner.doNotMove = false;
					_owner.navMeshAgent.enabled = true;
					_owner.navMeshAgent.updateRotation = true;
					_owner.coroRunning = false;

					yield break;
				}
			}

			//BOUNCE
			if (_owner.target && avoidTargetTimer <= 0f && _owner.distance < 10f && _owner.targetIsInSight && vectorDot > -0.6f && _owner.target.GetComponent<Player_Animation> ().anim.GetCurrentAnimatorStateInfo (0).IsName ("Aim")) {
				_owner.doNotMove = true;
				yield return new WaitForSeconds (randomSeconds);
				StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 0.5f, 0.8f));
				direction = Random.Range (0, 2);
				avoidTargetTimer = avoidTargetBaseCooldown * Random.Range (0.5f, 1.5f);
				_owner.navMeshAgent.enabled = false;
				GetComponent<Rigidbody> ().AddForce (Vector3.up * avoidTargetPower, ForceMode.VelocityChange);
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
				_owner.doNotMove = false;
				_owner.coroRunning = false;
				yield break;

			} else if (avoidTargetBaseCooldown > 0f && avoidTargetTimer <= 0f && _owner.distance < avoidTargetDistance && _owner.targetIsInSight && vectorDot > -0.5f && _owner.target.GetComponent<References> ().animationScript.isAttackingMelee) {
				_owner.doNotMove = true;
				yield return new WaitForSeconds (randomSeconds);
				StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 0.5f, 0.8f));
				direction = Random.Range (-1, 2);
				avoidTargetTimer = avoidTargetBaseCooldown * Random.Range (0.5f, 1.5f);
				_owner.navMeshAgent.enabled = false;
				GetComponent<Rigidbody> ().AddForce (Vector3.up * avoidTargetPower, ForceMode.VelocityChange);
				GetComponent<Rigidbody> ().AddForce ((transform.position - _owner.target.transform.position).normalized * avoidTargetPower, ForceMode.VelocityChange);

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
				yield return new WaitForSeconds (0.4f);
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
				_owner.doNotMove = false;
				_owner.coroRunning = false;
				yield break;
			}

			if (_owner.distance > _closeDistance) {
				_owner.navMeshAgent.enabled = true;
				if (walkSpeed != 0 && _owner.distance < walkDistance) {
					_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 1f, 5f * Time.deltaTime);
					_owner.navMeshAgent.isStopped = false;
					_owner.navMeshAgent.speed = walkSpeed;
				} else if (sprintSpeed != 0 && _owner.distance > runDistance) {
					_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 3f, 5f * Time.deltaTime);
					_owner.navMeshAgent.isStopped = false;
					_owner.navMeshAgent.speed = sprintSpeed;
				} else {
					_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 2f, 5f * Time.deltaTime);
					_owner.navMeshAgent.isStopped = false;
					_owner.navMeshAgent.speed = runSpeed;
				} 
			} else {
				_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 0f, 5f * Time.deltaTime);
				_owner.navMeshAgent.enabled = false;
			}	
		} else if (_owner.hasLeader) {
			if (_owner.distance > _closeDistance) {
				_owner.navMeshAgent.enabled = true;
				if (walkSpeed != 0 && _owner.distance < walkDistance) {
					_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 1f, 5f * Time.deltaTime);
					_owner.navMeshAgent.isStopped = false;
					_owner.navMeshAgent.speed = walkSpeed;
				} else if (sprintSpeed != 0 && _owner.distance > runDistance) {
					_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 3f, 5f * Time.deltaTime);
					_owner.navMeshAgent.isStopped = false;
					_owner.navMeshAgent.speed = sprintSpeed;
				} else {
					_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 2f, 5f * Time.deltaTime);
					_owner.navMeshAgent.isStopped = false;
					_owner.navMeshAgent.speed = runSpeed;
				} 
			} else {
				_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 0f, 5f * Time.deltaTime);
				_owner.navMeshAgent.enabled = false;
			}
		}
		yield return new RadicalWaitForSeconds (0.1f);
		_owner.coroRunning = false;
	}
}
