using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class NGS_AirComboA : NGS_AttackClass {

	public int AirComboLevel = 4;
	public bool ActivateTimerToReset = false;
	//The more bools, the less readibility, try to stick with the essentials.
	//If you were to press 10 buttons in a row
	//having 10 booleans to check for those would be confusing

	public float currentComboTimer;
	public int currentComboState = 0;

	private Coroutine coro;

	public override void Start (){
		priority = 2;
		base.Start ();
	}

	void Update(){
		ResetComboState ();
		Input();
		if (cooldown > 0f)
			cooldown -= Time.deltaTime;
	}

	void ResetComboState(){
		if (ActivateTimerToReset) {
			currentComboTimer -= Time.deltaTime;
			if (currentComboTimer <= 0 || currentComboState >= 3 || InputManager.GetButtonDown ("Jump") || movementScript.isGrounded) {
				currentComboState = 0;
				ActivateTimerToReset = false;
				currentComboTimer = 0f;
			}
		}
	}

	public override void ForcedReset(){
		ActivateTimerToReset = false;
		currentComboTimer = 0f;
	}
	IEnumerator CheckEmergencyCancel(){
		while (swordCPU.AttackBeingExecuted && !swordCPU.movementScript.isGrounded) {
			yield return null;
		} 
		movementScript.doNotMove = false;
		movementScript.cantShoot = false;
		movementScript.cantStab = false;
		rigidbody.useGravity = true;
		playerAnimation.isAttackingMeleeAir = false;
		swordCPU.damageCol = false;

		ForcedReset ();
		swordCPU.AttackDone ();
	}

	void Input(){
		if (movementScript.isGrounded) {
			currentComboState = 0;
			ForcedReset ();

		} else if (!movementScript.isGrounded && cooldown <= 0f) {
			if (InputManager.GetButtonDown ("Jump")) {
				ForcedReset ();
			}

			if (InputManager.GetButtonDown ("Melee")) {
				switch (currentComboState) {
				case 0:
					if (AirComboLevel > 0) {
						ActivateTimerToReset = true;
						currentComboTimer = 1.0f;
						swordCPU.AttackReady (this);
					}
					break;
				case 1:
					if (AirComboLevel > 1) {
						ActivateTimerToReset = true;
						currentComboTimer = 1.0f;
						swordCPU.AttackReady (this);
					}
					break;
				case 2:
					if (AirComboLevel > 2) {
						ActivateTimerToReset = true;
						currentComboTimer = 1.0f;
						swordCPU.AttackReady (this);
					}
					break;
				case 3:
					if (AirComboLevel > 3) {
						ActivateTimerToReset = true;
						currentComboTimer = 1.0f;
						swordCPU.AttackReady (this);
					}
					break;
				}
			}
		} else if (ActivateTimerToReset) {
			ForcedReset ();
		}
	}

	public IEnumerator AirCheck(){
		while (!swordCPU.movementScript.isGrounded) {
			yield return null;
		}
		yield return new WaitForSeconds(0.1f);
		ForcedReset ();
		swordCPU.AttackDone ();
		yield return new WaitForSeconds(0.1f);
		ForcedReset ();
		swordCPU.AttackDone ();
		StopCoroutine (coro);
	}

	public override IEnumerator ExecuteAttack (){
		StartCoroutine (CheckEmergencyCancel ());
		float distToGround = Player.GetComponent<Collider> ().bounds.extents.y;
		SpeedMultiplier = playerAnimation.finalSpeedMultiplier;
		StartCoroutine (swordCPU.AirCheck ());
		coro = StartCoroutine (AirCheck ());
		if (!Physics.Raycast (transform.position, Vector3.down, distToGround + 0.1f /*+ rigidbody.velocity.y*/ /*0.25f*/, movementScript.isGroundedIgnoreLayers)) {
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
			while (!movementScript.isGrounded) {
				switch (currentComboState) {
				case 0:
				//CheckCurrentAnimatorState ();
					currentComboState = 1;
					movementScript.jumpInertia = false;
					movementScript.inAirSpeed = movementScript.airSpeed;
					boxCollider.size = new Vector3 (boxCollider.size.x * 1.2f, boxCollider.size.y * 1.25f, boxCollider.size.z);
					movementScript.cantShoot = true;
					movementScript.cantStab = true;
					movementScript.doNotMove = true;
					playerAnimation.meleeAttack = 1;
					playerAnimation.isAttackingMeleeAir = true;
					animator.Play ("Air Melee Attacks");

					yield return new WaitForSeconds (0.1f / SpeedMultiplier);
					rigidbody.useGravity = false;
					rigidbody.velocity = Vector3.zero;
					rigidbody.AddForce (Player.transform.up * 1.25f, ForceMode.VelocityChange);
					yield return new WaitForSeconds (0.05f / SpeedMultiplier);
					playerAnimation.isAttackingMeleeAir = false;
					yield return new WaitForSeconds (0.05f / SpeedMultiplier);
					rigidbody.useGravity = true;

					boxCollider.size = swordCPU.boxColliderOrigSize;
					yield return new WaitForSeconds (0.05f / SpeedMultiplier);
					movementScript.doNotMove = false;
					movementScript.cantShoot = false;
					movementScript.cantStab = false;
				//swordCPU.ToggleSwordDamage (0);
					swordCPU.AttackDone ();
					break;

				case 1:
				//CheckCurrentAnimatorState ();
					currentComboState = 2;
					boxCollider.size = new Vector3 (boxCollider.size.x * 1.2f, boxCollider.size.y * 1.25f, boxCollider.size.z);
					movementScript.jumpInertia = false;
					movementScript.inAirSpeed = movementScript.airSpeed;
					movementScript.cantShoot = true;
					movementScript.cantStab = true;
					movementScript.doNotMove = true;
					playerAnimation.meleeAttack = 2;
					playerAnimation.isAttackingMeleeAir = true;

					yield return new WaitForSeconds (0.1f / SpeedMultiplier);
					rigidbody.useGravity = false;
					rigidbody.velocity = Vector3.zero;
					rigidbody.AddForce (Player.transform.up * 1.25f, ForceMode.VelocityChange);
					yield return new WaitForSeconds (0.1f / SpeedMultiplier);
					playerAnimation.isAttackingMeleeAir = false;
					yield return new WaitForSeconds (0.05f / SpeedMultiplier);
					rigidbody.useGravity = true;
					yield return new WaitForSeconds(0.05f / SpeedMultiplier);

					boxCollider.size = swordCPU.boxColliderOrigSize;
					yield return new WaitForSeconds (0.0125f / SpeedMultiplier);
					movementScript.doNotMove = false;
					movementScript.cantShoot = false;
					movementScript.cantStab = false;
					swordCPU.ToggleSwordDamage (0);
					swordCPU.AttackDone ();
					break;

				case 2:
				//CheckCurrentAnimatorState ();
					currentComboState = 3;
					cooldown = 1f;
					boxCollider.size = new Vector3 (boxCollider.size.x * 1.2f, boxCollider.size.y * 1.25f, boxCollider.size.z);
					movementScript.jumpInertia = false;
					movementScript.inAirSpeed = movementScript.airSpeed;
					movementScript.cantShoot = true;
					movementScript.cantStab = true;
					movementScript.doNotMove = true;
					playerAnimation.meleeAttack = 3;
					playerAnimation.isAttackingMeleeAir = true;
					swordCPU.attackDamage = swordCPU.baseDamage * 1.25f;

					yield return new WaitForSeconds (0.1f / SpeedMultiplier);
					rigidbody.useGravity = false;
					rigidbody.velocity = Vector3.zero;
					rigidbody.AddForce (Player.transform.up * 1.25f, ForceMode.VelocityChange);
					yield return new WaitForSeconds (0.05f / SpeedMultiplier);
					playerAnimation.isAttackingMeleeAir = false;
					yield return new WaitForSeconds (0.05f / SpeedMultiplier);
					rigidbody.useGravity = true;

					yield return new WaitForSeconds (0.2f / SpeedMultiplier);
					boxCollider.size = swordCPU.boxColliderOrigSize;
					yield return new WaitForSeconds (0.125f / SpeedMultiplier);
					movementScript.doNotMove = false;
					movementScript.cantShoot = false;
					movementScript.cantStab = false;
					swordCPU.ToggleSwordDamage (0);
					swordCPU.AttackDone ();
					break;
				}
				break;
			}
		} else {
			ForcedReset ();
			swordCPU.AttackDone ();
		}
	}
} 
