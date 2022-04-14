
using UnityEngine;
using System.Collections;
using Luminosity.IO;

public class NGS_StrongAttackB : NGS_AttackClass {

	public int attackLevel = 1;
	public bool ActivateTimerToReset = false;
	//The more bools, the less readibility, try to stick with the essentials.
	//If you were to press 10 buttons in a row
	//having 10 booleans to check for those would be confusing

	public float currentComboTimer;
	public int ComboAIndex = 0;

	public GameObject SignalParticlesFX;
	public GameObject Level1ParticlesFX;
	public GameObject Level2ParticlesFX;
	public GameObject Level3ParticlesFX;

	private bool stopInputDetection = false;
	//public new float stunTimer = 0.75f;

	public override void Start (){
		priority = 2;
		base.Start ();
	}

	void Update(){
		ResetComboState ();
		Input();
	}

	public override bool SpecialOntriggerenter(Collider collider){
		CameraShake.Shake (1f, 2f / Vector3.Distance (Camera.main.transform.position, transform.position) * swordCPU.attackDamage / 50f);
		collider.GetComponent<Rigidbody> ().velocity = Vector3.zero;
		collider.GetComponent<Rigidbody> ().AddForce ((collider.transform.position - transform.position).normalized * 4f, ForceMode.VelocityChange);
		collider.GetComponent<Rigidbody> ().AddForce (Vector3.up * 4f, ForceMode.VelocityChange);
		return true;
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

		if (ComboAIndex == 2 && currentComboTimer > 0f && currentComboTimer < 0.7f && particlesRunning == null) {
			particlesRunning = Instantiate (SignalParticlesFX, Player.transform.position + Player.transform.right * 1 / 4 + Player.transform.forward * 0.75f + Player.transform.up * 0f, Player.transform.rotation);
			particlesRunning.transform.position = transform.position + new Vector3 (3.7e-05f, -8e-05f, 0.000452f);
			particlesRunning.transform.parent = transform;
			particlesRunning.GetComponent<ParticleSystem> ().Play();
			Destroy (particlesRunning, 0.35f);
		} 

		if (!stopInputDetection && InputManager.GetButtonDown ("Melee") && movementScript.isGrounded && cooldown <= 0f) {
			switch (ComboAIndex) {
			case 0:
				ActivateTimerToReset = true;
				currentComboTimer = 0.8f;
				ComboAIndex = 1;
				break;
			case 1:
				ActivateTimerToReset = true;
				currentComboTimer = 1.5f;
				ComboAIndex = 2;
				break;
			case 2:
				if (InputManager.GetAxis ("Horizontal") != 0f || InputManager.GetAxis ("Vertical") != 0f) {
					ForcedReset ();
					break;
				}

				if (currentComboTimer < 0.75f) {
					ActivateTimerToReset = true;
					currentComboTimer = 2f;
					swordCPU.AttackReady (this);
				} else {
					ForcedReset ();
				}
				break;
			}
		}
	}

	public override IEnumerator ExecuteAttack(){
		ForcedReset();
		meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
		SpeedMultiplier = playerAnimation.finalSpeedMultiplier;

		if (!InputManager.GetButton ("Combo Mode")) {
			GameObject FoundTarget = CustomMethods.SearchForTargetInDirection (Player);
			if (FoundTarget)
				StartCoroutine (CustomMethods.SmoothRotateTowards (Player, FoundTarget.transform, 0f));
		}/* else if (Player.GetComponent<References>().currentAutoaimTarget) {
			StartCoroutine (CustomMethods.SmoothRotateTowards (Player, Player.GetComponent<References>().currentAutoaimTarget.transform, 0.1f));
		}*/

		if (particlesRunning != null)
			particlesRunning.GetComponent<ParticleSystem> ().Stop();
				
		CheckCurrentAnimatorState ();
		stopInputDetection = true;
		movementScript.cantShoot = true;
		movementScript.doNotMove = true;
		playerAnimation.meleeAttack = 13;
		playerAnimation.attackLevel = attackLevel;
		playerAnimation.isAttackingMelee = true;
		Player.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
		rigidbody.velocity = Vector3.zero;
		rigidbody.AddForce (Player.transform.forward * 3.5f, ForceMode.VelocityChange);

		if (attackLevel <= 1) {
			swordCPU.attackDamage = swordCPU.baseDamage * 2f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
		} else if (attackLevel == 2) {
			swordCPU.attackDamage = swordCPU.baseDamage * 2.5f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialBlue;
		} else if (attackLevel == 3) {
			swordCPU.attackDamage = swordCPU.baseDamage * 3f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialRed;
		} 

		CustomMethods.PlayClipAt (swordCPU.swordSwing, transform.position);
		yield return new WaitForSeconds (0.025f / SpeedMultiplier);

		yield return new WaitForSeconds (0.1f / SpeedMultiplier);
		//yield return new WaitForSeconds (0.2f);

		if (Level1ParticlesFX) {
			particlesRunning = Instantiate (Level1ParticlesFX, Player.transform.position + Player.transform.right * 1 / 4 + Player.transform.forward * 0.5f + Player.transform.up * 0.2f, Player.transform.rotation * Quaternion.Euler (20f, -4f, -25f));
			particlesRunning.transform.parent = Player.transform;
			ParticleSystem.MainModule partSys = particlesRunning.GetComponent<ParticleSystem> ().main;
			partSys.startSpeedMultiplier = partSys.startSpeedMultiplier * SpeedMultiplier;
			Destroy (particlesRunning, 1f);
		}

		stopInputDetection = false;
		rigidbody.velocity = Vector3.zero;
		swordCPU.attackDamage = swordCPU.baseDamage;

		if (CustomMethods.CheckDisplacementInput ())
			yield return new WaitForSeconds (0.9f / SpeedMultiplier);
		else
			yield return new WaitForSeconds (0.7f / SpeedMultiplier);

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
	}
}