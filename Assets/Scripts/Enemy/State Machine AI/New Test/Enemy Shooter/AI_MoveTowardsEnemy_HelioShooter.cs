using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;

public class AI_MoveTowardsEnemy_HelioShooter : AI_MoveTowardsEnemy {

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
			StopCoroutine(currentCoroutine);
		if (_owner.navMeshAgent.isStopped)
			_owner.navMeshAgent.isStopped = false;
		active = false;
		doneDelay = false;
	}

	public override void UpdateState(AI _owner){
		if (doneDelay /*&& !_owner.doNotMove*/) {
			if (_owner.target && _owner.distance <= closeDistance * 2f && _owner.targetIsInSight || _owner.references.animationScript.isBouncing) {
				directionToTarget = _owner.target.transform.position - transform.position;
				directionToTarget.y = 0f;
				transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (directionToTarget), 5 * Time.deltaTime);
			}

			if (!_owner.coroRunning && _owner.isBlader && _owner.distance <= closeDistance && _owner.GetComponent<AI_ComboAState> ()._coolDown <= 0f && _owner.GetComponent<AI_ComboAState> ().returnedChance < _owner.GetComponent<AI_ComboAState> ().chance) {
				_owner.stateMachine.ChangeState (_owner.GetComponent<AI_ComboAState> ());
			} else if (!_owner.coroRunning && _owner.isGunner && _owner.GetComponent<AI_ShootFar> ().canShoot && _owner.GetComponent<AI_ShootFar> ().returnedChance < _owner.GetComponent<AI_ShootFar> ().chance
			           && _owner.distance > (_owner.GetComponent<AI_ShootFar> ().minAttackDistance) && _owner.distance < (_owner.GetComponent<AI_ShootFar> ().maxAttackDistance)) {
				_owner.stateMachine.ChangeState (_owner.GetComponent<AI_ShootFar> ());
			} else if (!_owner.coroRunning && _owner.isCaster && _owner.GetComponent<AI_Fireball> ().canShoot && _owner.GetComponent<AI_Fireball> ().returnedChance < _owner.GetComponent<AI_Fireball> ().chance
			           && _owner.distance > (_owner.GetComponent<AI_Fireball> ().minAttackDistance) && _owner.distance < (_owner.GetComponent<AI_Fireball> ().maxAttackDistance)) {
				_owner.stateMachine.ChangeState (_owner.GetComponent<AI_Fireball> ());
			} else if (!_owner.coroRunning && _owner.distance > closeDistance) {
				currentCoroutine = _owner.StartCoroutine (Displacement (_owner));
			} else if (!_owner.coroRunning) {
				currentCoroutine = _owner.StartCoroutine (Displacement (_owner));
			}

			if (avoidTargetBaseCooldown > 0f) {
				if (avoidTargetTimer > 0f)
					avoidTargetTimer -= Time.deltaTime;
			}

			if (superJumpBaseTimer > 0f) {
				if (superJumpTimer > 0f)
					superJumpTimer -= Time.deltaTime * _owner.distance / 10f;
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
	}

	IEnumerator Delay(AI _owner){
		yield return new WaitForFixedUpdate ();
		if (!doneDelay) {
			_owner.navMeshAgent.isStopped = false;
			doneDelay = true;	
		} else {
			_owner.navMeshAgent.isStopped = true;
		}
	}


	IEnumerator Displacement(AI _owner){
		_owner.coroRunning = true;
		if (!_owner.doNotMove && _owner.target && _owner.target.GetComponent<HealthBar> () && !_owner.target.GetComponent<HealthBar> ().isDead) {
			_owner.navMeshAgent.enabled = true;	
			_owner.distance = Vector3.Distance (transform.position, _owner.target.transform.position);
			/*			if (!Physics.Raycast (Head.transform.position, (target.transform.position - Head.transform.position).normalized, out hit2, alertDistance, searchingForTargetIgnoreLayers)) {
				iKnowWhereYouAre2 -= Time.fixedDeltaTime;
			} else
				iKnowWhereYouAre2 = iKnowWhereYouAre1;
*/				
			/*
			if (distance <= closeDistance * 2f) {
				_owner.navMeshAgent.updatePosition = false;
				_owner.navMeshAgent.updateRotation = false;
				transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (_owner.target.transform.position), 15f * Time.deltaTime);
			} else {
				_owner.navMeshAgent.updatePosition = true;
				_owner.navMeshAgent.updateRotation = true;
			}
			*/

			int direction = Random.Range (-1, 2);
			Vector3 dir = (transform.position - _owner.target.transform.position).normalized;
			float vectorDot = Vector3.Dot (_owner.target.transform.forward, dir);
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

			if (shouldAvoidAllies && _owner.isGrounded && _owner.avoidAlliesScript.alliesInRange.Count > 0) {
				int counter = 0;
				foreach (GameObject ally in _owner.avoidAlliesScript.alliesInRange) {
					if (counter == 2)
						break;
					dir = (transform.position - ally.transform.position).normalized;
					vectorDot = Vector3.Dot (ally.transform.forward, dir);
					if (vectorDot < 0f) {
						directionOffset = directionOffset + dir;
						directionOffset.y = 0.0f;
						directionOffset.x = directionOffset.x + transform.right.x * direction * 2f;
						counter += 1;
					}
				}
				directionOffset = Vector3.Normalize (directionOffset);
				_owner.references.rigidbody.AddForce (directionOffset, ForceMode.VelocityChange);
			}		

			direction = Random.Range (-1, 2);
			dir = (transform.position - _owner.target.transform.position).normalized;
			vectorDot = Vector3.Dot (_owner.target.transform.forward, dir);

			//SUPERJUMP
			if (canSuperJump && superJumpTimer <= 0f && _owner.distance < superJumpDistance + 1f && _owner.distance > closeDistance + 1f) {
				direction = Random.Range (0, 2);
				StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 1.0f, 0.5f));
				superJumpTimer = superJumpBaseTimer * Random.Range (0.5f, 1.5f);
				_owner.navMeshAgent.enabled = false;
				_owner.navMeshAgent.updateRotation = false;
				_owner.references.animationScript.isJumping = true;
				_owner.references.rigidbody.velocity = Vector3.zero;
				yield return new WaitForSeconds (0.2f);
				if (vectorDot > 0.5f) {
					switch (direction) {
					case 0:
						if (Physics.Raycast (_owner.target.transform.position + _owner.target.transform.forward * 1.5f,
							    Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
							StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position + _owner.target.transform.forward * 1.5f, Vector3.Distance (_owner.transform.position, transform.position) / 2f + superJumpPower, 0.75f)); 
						} else if (Physics.Raycast (_owner.target.transform.position + _owner.target.transform.forward * (-1.5f),
							          Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
							StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position + _owner.target.transform.forward * (-1.5f), Vector3.Distance (_owner.transform.position, transform.position) / 2f + superJumpPower, 0.75f)); 					
						} else {
							StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position, Vector3.Distance (_owner.transform.position, transform.position) / 2f + superJumpPower, 0.75f)); 					
						}
						break;
					case 1:
						if (Physics.Raycast (_owner.target.transform.position + _owner.target.transform.forward * (-1.5f),
							    Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
							StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position + _owner.target.transform.forward * (-1.5f), Vector3.Distance (_owner.transform.position, transform.position) / 2f + superJumpPower, 0.75f)); 					
						} else if (Physics.Raycast (_owner.target.transform.position + _owner.target.transform.forward * 1.5f,
							           Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
							StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position + _owner.target.transform.forward * 1.5f, Vector3.Distance (_owner.transform.position, transform.position) / 2f + superJumpPower, 0.75f)); 
						} else {
							StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position, Vector3.Distance (_owner.transform.position, transform.position) / 2f + superJumpPower, 0.75f)); 					
						}break;
					}
				} else
					StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position, Vector3.Distance (_owner.transform.position, transform.position) + superJumpPower, 0.75f)); 
				yield return new WaitForSeconds (0.05f);
				_owner.references.animationScript.isJumping = false;	
				yield return new WaitForSeconds (0.1f);
				while (!_owner.isGrounded) {
					yield return new WaitForFixedUpdate ();
					dir.y = 0f;
					transform.LookAt (dir);
				}
				_owner.references.animationScript.anim.Play ("Cool Landing");
				transform.LookAt (_owner.target.transform.position);
				GetComponent<Rigidbody> ().velocity = Vector3.zero;
				yield return new WaitForSeconds (0.8f);
				_owner.navMeshAgent.enabled = true;	
				_owner.navMeshAgent.updateRotation = true;
			} else if (canSuperJump && superJumpTimer <= superJumpBaseTimer / 2f && _owner.navMeshAgent.velocity.magnitude < 0.02f && _owner.distance > superJumpDistance + 1f && _owner.target.transform.position.y - 2f > transform.position.y) {
				//Superjump si el player está en una plataforma inalcanzable
				direction = Random.Range (0, 2);
				StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 1.0f, 0.5f));
				superJumpTimer = superJumpBaseTimer * Random.Range (0.5f, 1.5f);
				_owner.navMeshAgent.enabled = false;
				_owner.navMeshAgent.updateRotation = false;
				_owner.references.animationScript.isJumping = true;
				_owner.references.rigidbody.velocity = Vector3.zero;
				yield return new WaitForSeconds (0.2f);
				if (vectorDot > 0.5f) {
					switch (direction) {
					case 0:
						if (Physics.Raycast (_owner.target.transform.position + _owner.target.transform.forward * 1.5f,
							Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
							StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position + _owner.target.transform.forward * 1.5f, Vector3.Distance (_owner.transform.position, transform.position) + superJumpPower, 0.75f)); 
						}else if (Physics.Raycast ( _owner.target.transform.position + _owner.target.transform.forward * (-1.5f),
							Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)){
							StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position + _owner.target.transform.forward * (-1.5f), Vector3.Distance (_owner.transform.position, transform.position) + superJumpPower, 0.75f)); 					
						} else
							StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position, Vector3.Distance (_owner.transform.position, transform.position) + superJumpPower, 0.75f)); 					
						break;
					case 1:
						if (Physics.Raycast (_owner.target.transform.position + _owner.target.transform.forward * (-1.5f),
							Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
							StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position + _owner.target.transform.forward * (-1.5f), Vector3.Distance (_owner.transform.position, transform.position) + superJumpPower, 0.75f)); 					
						} else if (Physics.Raycast (_owner.target.transform.position + _owner.target.transform.forward * 1.5f,
							Vector3.down, out hit, 0.1f, _owner.searchingForTargetIgnoreLayers)) {
							StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position + _owner.target.transform.forward * 1.5f, Vector3.Distance (_owner.transform.position, transform.position) + superJumpPower, 0.75f)); 
						} else
							StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position, Vector3.Distance (_owner.transform.position, transform.position) + superJumpPower, 0.75f)); 					
						break;
					}
				} else
					StartCoroutine (CustomMethods.JumpToPositionParabola (gameObject, _owner.target.transform.position + _owner.target.transform.forward * 2.5f, Vector3.Distance (_owner.transform.position, transform.position) + superJumpPower, 0.75f)); 
				yield return new WaitForSeconds (0.05f);
				_owner.references.animationScript.isJumping = false;	
				yield return new WaitForSeconds (0.1f);
				while (!_owner.isGrounded) {
					yield return new WaitForFixedUpdate ();
					dir.y = 0f;
					transform.LookAt (dir);
				}
				_owner.references.animationScript.anim.Play ("Cool Landing");
				transform.LookAt (_owner.target.transform.position);
				GetComponent<Rigidbody> ().velocity = Vector3.zero;
				yield return new WaitForSeconds (0.8f);
				_owner.navMeshAgent.enabled = true;	
				_owner.navMeshAgent.updateRotation = true;
			}

			//ESQUIVAR SI EL PLAYER ME ESTA APUNTANDO!!!
			if (_owner.target && avoidTargetTimer <= 0f && _owner.distance < 10f && _owner.targetIsInSight && vectorDot > -0.7f && _owner.target.GetComponent<Player_Animation> ().anim.GetCurrentAnimatorStateInfo (0).IsName ("Aim")) {
				yield return new WaitForSeconds (randomSeconds);
				StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 0.5f, 0.8f));
				direction = Random.Range (0, 2);
				avoidTargetTimer = avoidTargetBaseCooldown * Random.Range (0.5f, 1.5f);
				_owner.navMeshAgent.enabled = false;
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
			} else if (avoidTargetBaseCooldown > 0f && avoidTargetTimer <= 0f && _owner.distance < avoidTargetDistance && _owner.targetIsInSight && vectorDot > -0.7f) {
				yield return new WaitForSeconds (randomSeconds);
				StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 0.5f, 0.8f));
				direction = Random.Range (-1, 2);
				avoidTargetTimer = avoidTargetBaseCooldown * Random.Range (0.5f, 1.5f);
				_owner.navMeshAgent.enabled = false;
				GetComponent<Rigidbody> ().AddForce (Vector3.up * avoidTargetPower, ForceMode.VelocityChange);
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
			}
			/*
			else if (currentWeapon && currentWeapon.CompareTag ("EquipedWeaponGun")) {
				//Si tiene arma de fuego!
				if (!Physics.Raycast (_owner.references.Head.transform.position, _owner.target.transform.position - _owner.references.Head.transform.position, out hit, GetComponent<EnemyShooter> ().attackRange, _owner.isGroundedIgnoreLayers)) {
					//print ("11");
					_owner.navMeshAgent.isStopped = false;
					_owner.navMeshAgent.speed = walkSpeed;
					_owner.navMeshAgent.SetDestination (_owner.target.transform.position);
				} else if (distance > closeDistance || !_owner.targetIsInSight) {
					_owner.navMeshAgent.enabled = true;
					if (walkSpeed != 0 && distance < 5f) {
						//print ("12");
						_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 1f, 10f * Time.deltaTime);
						_owner.navMeshAgent.isStopped = false;
						_owner.navMeshAgent.speed = walkSpeed;
						_owner.navMeshAgent.SetDestination (_owner.target.transform.position);
					} else if (distance > 5f & distance < 15f || sprintSpeed == 0 & distance > 15f || walkSpeed == 0f & distance < 5f) {
						//print ("13");
						_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 2f, 10f * Time.deltaTime);
						_owner.navMeshAgent.isStopped = false;
						_owner.navMeshAgent.speed = runSpeed;
						_owner.navMeshAgent.SetDestination (_owner.target.transform.position);
					} else if (sprintSpeed != 0 && distance > 15f) {
						//print ("14");
						_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 3f, 10f * Time.deltaTime);
						_owner.navMeshAgent.isStopped = false;
						_owner.navMeshAgent.speed = sprintSpeed;
						_owner.navMeshAgent.SetDestination (_owner.target.transform.position);
					}
				} else {
					//print ("15");
					_owner.navMeshAgent.enabled = false;
					_owner.navMeshAgent.enabled = false;
					//navMeshAgent.isStopped = true;
					//navMeshAgent.speed = 0f;
					//navMeshAgent.SetDestination (target.transform.position);
				}
			} else if (!currentWeapon || currentWeapon && currentWeapon.CompareTag ("EquipedWeaponSword")) {
				//Si tiene arma de melee!
				if (distance > closeDistance) {
					_owner.navMeshAgent.enabled = true;
					if (walkSpeed != 0 && distance < 5f) {
						//print ("1");
						_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 1f, 10f * Time.deltaTime);
						_owner.navMeshAgent.isStopped = false;
						_owner.navMeshAgent.speed = walkSpeed;
						_owner.navMeshAgent.SetDestination (_owner.target.transform.position);
					} else if (distance > 5f & distance < 15f || sprintSpeed == 0 & distance > 15f || walkSpeed == 0f & distance < 5f) {
						//print ("2");
						_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 2f, 10f * Time.deltaTime);
						_owner.navMeshAgent.isStopped = false;
						_owner.navMeshAgent.speed = runSpeed;
						_owner.navMeshAgent.SetDestination (_owner.target.transform.position);
					} else if (sprintSpeed != 0 && distance > 15f) {
						//print ("3");
						_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 3f, 10f * Time.deltaTime);
						_owner.navMeshAgent.isStopped = false;
						_owner.navMeshAgent.speed = sprintSpeed;
						_owner.navMeshAgent.SetDestination (_owner.target.transform.position);
					}
				} else {
					//print ("4");
					_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 0f, 10f * Time.deltaTime);
					_owner.navMeshAgent.enabled = false;
					//navMeshAgent.isStopped = true;
					//navMeshAgent.speed = 0f;
					//navMeshAgent.SetDestination (target.transform.position);
				}
			}
			*/
			else {
				if (_owner.distance > closeDistance) {
					_owner.navMeshAgent.enabled = true;
					if (walkSpeed != 0 && _owner.distance < walkDistance) {
						_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 1f, 4f * Time.deltaTime);
						_owner.navMeshAgent.isStopped = false;
						_owner.navMeshAgent.speed = walkSpeed;
						_owner.navMeshAgent.SetDestination (_owner.target.transform.position);
					} else if (sprintSpeed != 0 && _owner.distance > runDistance) {
						_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 3f, 4f * Time.deltaTime);
						_owner.navMeshAgent.isStopped = false;
						_owner.navMeshAgent.speed = sprintSpeed;
						_owner.navMeshAgent.SetDestination (_owner.target.transform.position);
					} else {
						_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 2f, 4f * Time.deltaTime);
						_owner.navMeshAgent.isStopped = false;
						_owner.navMeshAgent.speed = runSpeed;
						_owner.navMeshAgent.SetDestination (_owner.target.transform.position);
					} 
				} else {
					_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 0f, 4f * Time.deltaTime);
					_owner.navMeshAgent.enabled = false;
					//navMeshAgent.isStopped = true;
					//navMeshAgent.speed = 0f;
					//navMeshAgent.SetDestination (target.transform.position);
				}
			}				
		} else {
			_owner.references.animationScript.displacementSpeed = Mathf.Lerp (_owner.references.animationScript.displacementSpeed, 0f, 4f * Time.deltaTime);
			//_owner.navMeshAgent.enabled = false;
			//navMeshAgent.isStopped = true;
			//navMeshAgent.speed = 0f;
			//navMeshAgent.SetDestination (target.transform.position);
		}

		if (_owner.references.rigidbody.velocity.magnitude > _owner.navMeshAgent.speed)
			_owner.references.rigidbody.velocity = _owner.references.rigidbody.velocity.normalized * _owner.navMeshAgent.speed;

		yield return new RadicalWaitForSeconds (0.1f);
		_owner.coroRunning = false;
	}
}
