using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class NGS_ComboA : NGS_AttackClass {

	public int ComboLevel = 5;
	public bool ActivateTimerToReset = false;
	//The more bools, the less readibility, try to stick with the essentials.
	//If you were to press 10 buttons in a row
	//having 10 booleans to check for those would be confusing

	public float currentComboTimer;
	public int ComboAIndex = 0;

	public GameObject Level1ParticlesFX;
	public GameObject Level2ParticlesFX;
	public GameObject Level3ParticlesFX;

	private bool stopInputDetection = false;

	public override void Start (){
		priority = 1;
		base.Start ();
	}

	void Update(){
		ResetComboState ();
		Input();
	}

	void ResetComboState(){
		currentComboTimer -= Time.deltaTime;
		if (currentComboTimer <= 0 || ComboAIndex >= 4 || InputManager.GetButtonDown ("Jump")) {
			stopInputDetection = false;
			ActivateTimerToReset = false;
			currentComboTimer = 0f;
			ComboAIndex = 0;
		}
	}

	public override void ForcedReset(){
		stopInputDetection = false;
		ActivateTimerToReset = false;
		currentComboTimer = 0f;
		ComboAIndex = 0;
	}

	void Input(){
		if (!stopInputDetection && InputManager.GetButtonDown ("Melee") && movementScript.isGrounded && cooldown <= 0f){
			switch (ComboAIndex) {
			case 0:
				if (ComboLevel > 0) {
					ActivateTimerToReset = true;
					currentComboTimer = 0.8f;
					swordCPU.AttackReady (this);
				}
				break;
			case 1:
				if (ComboLevel > 1) {
					ActivateTimerToReset = true;
					currentComboTimer = 0.8f;
					swordCPU.AttackReady (this);
				}
				break;
			case 2:
				if (ComboLevel > 2) {
					ActivateTimerToReset = true;
					currentComboTimer = 1f;
					swordCPU.AttackReady (this);
				}
				break;
			case 3:
				if (ComboLevel > 3) {
					ActivateTimerToReset = true;
					currentComboTimer = 0.8f;
					swordCPU.AttackReady (this);
				}
				break;
			case 4:
				if (ComboLevel > 4) {
					ActivateTimerToReset = true;
					currentComboTimer = 0.8f;
					swordCPU.AttackReady (this);
				}
				break;
			}
		}
	}

	public override IEnumerator ExecuteAttack(){
		meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
		SpeedMultiplier = playerAnimation.finalSpeedMultiplier;

		if (!InputManager.GetButton ("Combo Mode")) {
			GameObject FoundTarget = CustomMethods.SearchForTargetInDirection (Player);
			if (FoundTarget &&
				!Physics.Raycast (Player.transform.position, (FoundTarget.transform.position - Player.transform.position).normalized, Vector3.Distance (Player.transform.position, FoundTarget.transform.position), LayerMask.GetMask ("Scenario", "Default")) &&
				DiplomacyManager.AreEnemies (Player.GetComponent<References> ().team, FoundTarget.GetComponent<References> ().team)){
				StartCoroutine (CustomMethods.SmoothRotateTowards (Player, FoundTarget.transform, 0f));
			}
		}

		switch (ComboAIndex) {
		case 0:
			CheckCurrentAnimatorState ();
			ComboAIndex = 1;
			stopInputDetection = true;
			movementScript.cantShoot = true;
			movementScript.doNotMove = true;
			playerAnimation.meleeAttack = 1;
			playerAnimation.isAttackingMelee = true;
			Player.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
			rigidbody.velocity = Vector3.zero;
			rigidbody.AddForce (Player.transform.forward * 3.5f, ForceMode.VelocityChange);
			yield return new WaitForSeconds (0.025f / SpeedMultiplier);

			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.transform.position + Player.transform.right * 1/4 + Player.transform.forward * 0.75f + Player.transform.up * 0f, Player.transform.rotation);
				particlesRunning.transform.parent = Player.transform;
				ParticleSystem.MainModule partSys = particlesRunning.GetComponent<ParticleSystem> ().main;
				partSys.startSpeedMultiplier = partSys.startSpeedMultiplier * SpeedMultiplier;
				Destroy (particlesRunning, 1f);
			}

			yield return new WaitForSeconds (0.175f / SpeedMultiplier);

			//yield return new WaitForSeconds (0.2f);

			stopInputDetection = false;
			rigidbody.velocity = Vector3.zero;
			if (CustomMethods.CheckDisplacementInput ())
				yield return new WaitForSeconds (0.2f / SpeedMultiplier);
			else
				yield return new WaitForSeconds (0.1f / SpeedMultiplier);

			movementScript.doNotMove = false;
			movementScript.cantShoot = false;

			animationTimer = 0.8f;
			while (animationTimer > 0f) {
				if (TerminateAnimation ()) {
					animationTimer -= Time.deltaTime;
					yield return new WaitForEndOfFrame ();
				} else
					break;
			}

			playerAnimation.isAttackingMelee = false;
			swordCPU.ToggleSwordDamage (0);
			swordCPU.AttackDone ();
			break;

		case 1:
			CheckCurrentAnimatorState ();
			ComboAIndex = 2;
			stopInputDetection = true;
			movementScript.cantShoot = true;
			movementScript.doNotMove = true;
			playerAnimation.meleeAttack = 2;
			playerAnimation.isAttackingMelee = true;
			Player.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
			rigidbody.velocity = Vector3.zero;
			rigidbody.AddForce (Player.transform.forward * 2f, ForceMode.VelocityChange);

			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.transform.position + Player.transform.right * 1/4 + Player.transform.up * 0.25f, Player.transform.rotation * Quaternion.Euler (-20f, -40f, -210f));
				//particlesRunning.transform.localScale = new Vector3 (particlesRunning.transform.localScale.x, particlesRunning.transform.localScale.y, -particlesRunning.transform.localScale.z);
				particlesRunning.transform.parent = Player.transform;
				ParticleSystem.MainModule partSys = particlesRunning.GetComponent<ParticleSystem> ().main;
				partSys.startSpeedMultiplier = partSys.startSpeedMultiplier * SpeedMultiplier;
				Destroy (particlesRunning, 1f);
			}

			yield return new WaitForSeconds (0.1f / SpeedMultiplier);

			rigidbody.AddForce (Player.transform.forward * 2f, ForceMode.VelocityChange);
			//yield return new WaitForSeconds (0.2f);

			yield return new WaitForSeconds (0.15f / SpeedMultiplier);

			stopInputDetection = false;
			rigidbody.velocity = Vector3.zero;
			if (CustomMethods.CheckDisplacementInput())
				yield return new WaitForSeconds (0.2f / SpeedMultiplier);
			else
				yield return new WaitForSeconds (0.1f / SpeedMultiplier);


			movementScript.doNotMove = false;
			movementScript.cantShoot = false;

			animationTimer = 0.8f;
			while (animationTimer > 0f) {
				if (TerminateAnimation ()) {
					animationTimer -= Time.deltaTime;
					yield return new WaitForEndOfFrame ();
				} else
					break;
			}

			playerAnimation.isAttackingMelee = false;
			swordCPU.ToggleSwordDamage (0);
			swordCPU.AttackDone ();
			break;

		case 2:
			CheckCurrentAnimatorState ();
			ComboAIndex = 3;
			stopInputDetection = true;
			movementScript.cantShoot = true;
			movementScript.doNotMove = true;
			playerAnimation.meleeAttack = 3;
			playerAnimation.isAttackingMelee = true;
			Player.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
			rigidbody.velocity = Vector3.zero;
			rigidbody.AddForce (Player.transform.forward * 1.5f, ForceMode.VelocityChange);

			yield return new WaitForSeconds (0.2f / SpeedMultiplier);
			rigidbody.AddForce (Player.transform.forward * 2f, ForceMode.VelocityChange);

			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - Player.transform.up * 0.25f, Player.transform.rotation * Quaternion.Euler (20f, 0f, -100f));
				//particlesRunning.transform.localScale = new Vector3 (particlesRunning.transform.localScale.x, particlesRunning.transform.localScale.y, -particlesRunning.transform.localScale.z);
				particlesRunning.transform.parent = Player.transform;
				ParticleSystem.MainModule partSys = particlesRunning.GetComponent<ParticleSystem> ().main;
				partSys.startSpeedMultiplier = partSys.startSpeedMultiplier * SpeedMultiplier;
				Destroy (particlesRunning, 1f);
			}

			yield return new WaitForSeconds (0.1f / SpeedMultiplier);
			yield return new WaitForSeconds (0.2f / SpeedMultiplier);

			stopInputDetection = false;
			rigidbody.velocity = Vector3.zero;
			if (CustomMethods.CheckDisplacementInput())
				yield return new WaitForSeconds (0.2f / SpeedMultiplier);
			else
				yield return new WaitForSeconds (0.025f / SpeedMultiplier);

			rigidbody.velocity = Vector3.zero;
			movementScript.doNotMove = false;
			movementScript.cantShoot = false;

			animationTimer = 0.8f;
			while (animationTimer > 0f) {
				if (TerminateAnimation ()) {
					animationTimer -= Time.deltaTime;
					yield return new WaitForEndOfFrame ();
				} else
					break;
			}

			playerAnimation.isAttackingMelee = false;
			swordCPU.ToggleSwordDamage (0);
			swordCPU.AttackDone ();
			break;

		case 3:
			CheckCurrentAnimatorState ();
			ComboAIndex = 4;
			stopInputDetection = true;
			movementScript.cantShoot = true;
			movementScript.doNotMove = true;
			playerAnimation.meleeAttack = 4;
			playerAnimation.isAttackingMelee = true;
			Player.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
			rigidbody.velocity = Vector3.zero;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.5f;
			yield return new WaitForSeconds (0.1f / SpeedMultiplier);
			rigidbody.AddForce (Player.transform.forward * 4f, ForceMode.VelocityChange);

			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.transform.position + Player.transform.forward * 0.5f - Player.transform.up * 0.25f, Player.transform.rotation * Quaternion.Euler (0f, 0f, -90f));
				particlesRunning.transform.localScale = new Vector3 (particlesRunning.transform.localScale.x / particlesRunning.transform.localScale.x, particlesRunning.transform.localScale.y / particlesRunning.transform.localScale.y, particlesRunning.transform.localScale.z);
				particlesRunning.transform.parent = Player.transform;
				ParticleSystem.MainModule partSys = particlesRunning.GetComponent<ParticleSystem> ().main;
				partSys.startSpeedMultiplier = partSys.startSpeedMultiplier * SpeedMultiplier;
				Destroy (particlesRunning, 1f);
			}

			yield return new WaitForSeconds (0.3f / SpeedMultiplier);
			rigidbody.velocity = Vector3.zero;
			yield return new WaitForSeconds (0.2f / SpeedMultiplier);
			//rigidbody.AddForce (Player.transform.forward * 2f, ForceMode.VelocityChange);
			yield return new WaitForSeconds (0.2f / SpeedMultiplier);
			rigidbody.velocity = Vector3.zero;
			//yield return new WaitForSeconds (0.2f);
				//rigidbody.AddForce (Player.transform.forward * 1.5f, ForceMode.VelocityChange);

			swordCPU.attackDamage = swordCPU.baseDamage;
			animationTimer = 0.8f;
			while (animationTimer > 0f) {
				if (TerminateAnimation ()) {
					animationTimer -= Time.deltaTime;
					yield return new WaitForEndOfFrame ();
				} else
					break;
			}

			if (!CustomMethods.CheckDisplacementInput ()) {
				yield return new WaitForSeconds (0.1f / SpeedMultiplier);
				rigidbody.AddForce (Player.transform.forward * 3f, ForceMode.VelocityChange);
			} else
				yield return new WaitForSeconds (0.2f / SpeedMultiplier);

			playerAnimation.isAttackingMelee = false;
			movementScript.doNotMove = false;
			movementScript.cantShoot = false;
			stopInputDetection = false;
			swordCPU.ToggleSwordDamage (0);
			swordCPU.AttackDone ();
			break;
		}
	}
}