
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class NGS_RocketDownA : NGS_AttackClass {

	public int attackLevel = 1;

	public GameObject Level1ParticlesFX;
	public GameObject Level2ParticlesFX;
	public GameObject Level3ParticlesFX;

	public GameObject ParticlesFX;

	//The more bools, the less readibility, try to stick with the essentials.
	//If you were to press 10 buttons in a row
	//having 10 booleans to check for those would be confusing

	public GameObject launcherATarget;
	public bool landedLauncherA;

	private bool magicAuxBool;
	private Coroutine extraAttack;
	public float powerTimer = 0f;

	//public new bool overthrows = true;

	// Use this for initialization
	public override void Start () {
		resetOnLanding = false;
		base.Start();
	}
	
	// Update is called once per frame
	void Update(){
		Input();
	}


	public override void ForcedReset(){
		landedLauncherA = false;
		launcherATarget = null;
		magicAuxBool = false;
		powerTimer = 0f;
		if (extraAttack != null) {
			StopCoroutine (extraAttack);
		}
	}

	IEnumerator CheckEmergencyCancel(){
		while (swordCPU.AttackBeingExecuted && !swordCPU.movementScript.isGrounded) {
			yield return null;
		} 
		//ForcedReset ();
		//swordCPU.AttackDone ();

		swordCPU.cancelInAirCheck = true;
		rigidbody.velocity = Vector3.zero;
		//Player.GetComponent<References> ().Landing ();
		//Player.GetComponent<References> ().LandingSound ();
		powerTimer = 0f;
		swordCPU.ToggleSwordDamage (0);
		boxCollider.size = swordCPU.boxColliderOrigSize;
		boxCollider.center = swordCPU.boxColliderOrigCenter;
		Player.GetComponent<Rigidbody> ().mass = 1f;
		CheckCurrentAnimatorState ();
		playerAnimation.anim.Play ("Cool Landing");

		CustomMethods.PlayClipAt (swordCPU.swordClash, transform.position);
		Player.GetComponent<References> ().Landing ();
		Player.GetComponent<References> ().LandingSound ();
		if (swordCPU.SwordSpark != null) {
			GameObject ImpactSpark = Instantiate (swordCPU.SwordSpark, transform.position + transform.forward * 0.5f, Player.transform.rotation);
			ImpactSpark.GetComponent<Renderer> ().material.SetColor ("_Color", swordCPU.SwordSparkColor);
		}

		magicAuxBool = false;
		if (!InputManager.GetButton ("Jump")) {
			yield return null;
			float timer = 0.15f;
			while (timer > 0f) {
				if (InputManager.GetButton ("Jump")) {
					swordCPU.cancelInAirCheck = true;
					extraAttack = StartCoroutine (ExtraAttack ());
					yield break;
				}
				timer -= Time.deltaTime;
				yield return null;
			}
		}

		movementScript.cantShoot = false;
		landedLauncherA = false;
		launcherATarget = null;
		playerAnimation.isAttackingMeleeAir = false;
		yield return new WaitForSeconds(0.25f);
		movementScript.doNotMove = false;
		movementScript.cantShoot = false;
		movementScript.cantStab = false;
		swordCPU.AttackDone();
	}

	void Input(){
		if (!movementScript.isGrounded && cooldown <= 0f) {
			if (InputManager.GetButtonDown ("Launcher") & InputManager.GetAxis ("Vertical") > 0.25f) {
				swordCPU.AttackReady (this);
			}
		}
	}

	public override bool SpecialOntriggerenter(Collider collider){
		if (!landedLauncherA) {
			landedLauncherA = true;
			launcherATarget = collider.gameObject;
			//launcherATarget.GetComponent<Rigidbody> ().MovePosition (Player.transform.position + Player.transform.forward);
			//launcherATarget.GetComponent<Rigidbody> ().AddForce (Vector3.up * 10f, ForceMode.VelocityChange);
			//collider.GetComponent<HealthBar> ().painTimerOffset = 1f;
		}
		return true;
	}

	public override IEnumerator ExecuteAttack() {
		if (Physics.Raycast(transform.position, Vector3.down, Player.GetComponent<Collider>().bounds.extents.y + 0.625f /*0.25f*/, movementScript.isGroundedIgnoreLayers)) {
			swordCPU.AttackDone();
			ForcedReset();
			yield break;
		}

		if (!InputManager.GetButton("Combo Mode")) {
			GameObject FoundTarget = CustomMethods.SearchForTargetInDirection(Player);
			if (FoundTarget)
				StartCoroutine(CustomMethods.SmoothRotateTowards(Player, FoundTarget.transform, 0.125f));
		}

		if (GetComponent<NGS_LauncherA>()) {
			GetComponent<NGS_LauncherA>().StopAllCoroutinesExceptThrowEnemy();
			//GetComponent<NGS_LauncherA>().ForcedReset();
			//GetComponent<NGS_LauncherA>().isObsolete = true;
		}

		//StartCoroutine(CheckEmergencyCancel());
		StartCoroutine(swordCPU.AirCheck());
		CheckCurrentAnimatorState();
		movementScript.cantShoot = true;
		movementScript.cantStab = true;
		movementScript.doNotMove = true;
		swordCPU.cancelInAirCheck = true;
		playerAnimation.isAttackingMeleeAir = true;
		SpeedMultiplier = playerAnimation.finalSpeedMultiplier;

		//yield return new WaitForFixedUpdate ();
		playerAnimation.meleeAttack = 6;
		playerAnimation.attackLevel = attackLevel;
		playerAnimation.anim.Play("Air Melee Attacks");

		if (attackLevel <= 1) {
			playerAnimation.attackLevel = 1f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.125f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer>().material = meleeWeaponTrail._material;
		} else if (attackLevel == 2) {
			playerAnimation.attackLevel = 2f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.225f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer>().material = meleeWeaponTrail._materialBlue;
		} else if (attackLevel == 3) {
			playerAnimation.attackLevel = 3f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.35f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer>().material = meleeWeaponTrail._materialRed;
		}

		boxCollider.size = new Vector3(swordCPU.boxColliderOrigSize.x * 1.25f, swordCPU.boxColliderOrigSize.y * 1.25f, swordCPU.boxColliderOrigSize.z);
		Player.GetComponent<Collider>().material.dynamicFriction = 1.0f;
		rigidbody.velocity = Vector3.zero;
		yield return new WaitForSeconds(0.05f / SpeedMultiplier);

		rigidbody.velocity = Vector3.zero;
		rigidbody.AddForce(Player.transform.up * 3f, ForceMode.VelocityChange);
		rigidbody.AddForce(Player.transform.forward * 1f, ForceMode.VelocityChange);

		//float waitTime = 0.25f;

		while (!swordCPU.damageCol) {
			movementScript.doNotMove = true;
			rigidbody.velocity = Vector3.zero;
			yield return null;
		}

		while (swordCPU.damageCol && !landedLauncherA && playerAnimation.anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.25f) {
			movementScript.doNotMove = true;
			rigidbody.velocity = Vector3.zero;
			yield return null;
		}

		Player.GetComponent<Rigidbody>().mass = 100f;
		boxCollider.size = new Vector3(swordCPU.boxColliderOrigSize.x * 5f, swordCPU.boxColliderOrigSize.y * 2.5f, swordCPU.boxColliderOrigSize.z * 1.5f);
		boxCollider.center = new Vector3(swordCPU.boxColliderOrigCenter.x * 15f, swordCPU.boxColliderOrigCenter.y, swordCPU.boxColliderOrigCenter.z / 2f);
		//yield return new WaitForSeconds (0.1f);
		rigidbody.velocity = Vector3.zero;
		rigidbody.AddForce(Vector3.down * 25f, ForceMode.VelocityChange);

		//Player.transform.rotation = Player.transform.rotation * Quaternion.Euler (0f, 40f, 0f);
		//yield return new WaitForSeconds (0.1f);
		//playerAnimation.meleeAttack = 10;
		while (!movementScript.isGrounded) {
			rigidbody.velocity = Vector3.down * 25f;
			powerTimer = Time.fixedDeltaTime;
			if (landedLauncherA && !magicAuxBool) {
				Rigidbody LauncherATargetRigidbody = launcherATarget.GetComponent<Rigidbody>();
				LauncherATargetRigidbody.velocity = rigidbody.velocity;
				magicAuxBool = true;
			}
			yield return new WaitForFixedUpdate();
		}

		rigidbody.velocity = Vector3.zero;
		Player.GetComponent<References>().Landing();
		Player.GetComponent<References>().LandingSound();
		powerTimer = 0f;
		swordCPU.ToggleSwordDamage(0);
		boxCollider.size = swordCPU.boxColliderOrigSize;
		boxCollider.center = swordCPU.boxColliderOrigCenter;
		Player.GetComponent<Rigidbody>().mass = 1f;
		CheckCurrentAnimatorState();
		playerAnimation.anim.Play("Cool Landing");
		yield return new WaitForFixedUpdate();
		CustomMethods.PlayClipAt(swordCPU.swordClash, transform.position);
		CameraShake.Shake(0.045f, 1f / Vector3.Distance(Camera.main.transform.position, transform.position) * 7.5f);
		Player.GetComponent<References>().Landing();
		Player.GetComponent<References>().LandingSound();
		if (swordCPU.SwordSpark != null) {
			GameObject ImpactSpark = Instantiate(swordCPU.SwordSpark, transform.position + transform.forward * 0.5f, Player.transform.rotation);
			ImpactSpark.GetComponent<Renderer>().material.SetColor("_Color", swordCPU.SwordSparkColor);
		}

		magicAuxBool = false;
		if (!InputManager.GetButton("Jump")) {
			yield return null;
			float timer = 0.1f;
			while (timer > 0f) {
				movementScript.doNotMove = true;
				if (InputManager.GetButton("Jump")) {
					swordCPU.cancelInAirCheck = true;
					extraAttack = StartCoroutine(ExtraAttack());
					yield break;
				}
				timer -= Time.deltaTime;
				yield return null;
			}
		}

		movementScript.cantShoot = false;
		landedLauncherA = false;
		launcherATarget = null;
		playerAnimation.isAttackingMeleeAir = false;
		yield return new WaitForSeconds(0.25f);
		movementScript.doNotMove = false;
		movementScript.cantShoot = false;
		movementScript.cantStab = false;
		swordCPU.AttackDone();
	}

	IEnumerator ExtraAttack(){
		//StartCoroutine (swordCPU.AirCheck ());
		movementScript.doNotMove = true;
		magicAuxBool = true;
		playerAnimation.isGrounded = false;

		if (attackLevel <= 1) {
			playerAnimation.attackLevel = 1f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.15f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
		} else if (attackLevel == 2) {
			playerAnimation.attackLevel = 2f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.25f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialBlue;
		} else if (attackLevel == 3) {
			playerAnimation.attackLevel = 3f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.4f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialRed;
		}

		rigidbody.velocity = Vector3.zero;
		yield return new WaitForSeconds (0.1f / SpeedMultiplier);
		Player.GetComponent<Rigidbody> ().mass = 1000f;
		movementScript.doNotMove = true;
		rigidbody.velocity = Vector3.zero;
		StartCoroutine (CustomMethods.JumpToPositionParabola (Player, Player.transform.position + Player.transform.forward * 4f, 0.5f, 0.625f));
		//rigidbody.MovePosition (transform.position + Vector3.up * 0.1f + transform.forward * 0.1f);
		//rigidbody.AddForce (Vector3.up * 2.5f, ForceMode.VelocityChange);
		//rigidbody.AddForce (Player.transform.forward * 5f, ForceMode.VelocityChange);

		yield return new WaitForSeconds (0.1f / SpeedMultiplier);

		if (ParticlesFX) {
			particlesRunning = Instantiate (ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - Player.transform.up * 0.25f, Player.transform.rotation * Quaternion.Euler (20f, 0f, -90f));
			particlesRunning.transform.parent = Player.transform;
			particlesRunning.transform.localScale = new Vector3 (particlesRunning.transform.lossyScale.x, particlesRunning.transform.lossyScale.y * 2f, particlesRunning.transform.lossyScale.z);
			Destroy (particlesRunning, 1f);
		}

		movementScript.doNotMove = true;
		playerAnimation.isAttackingMeleeAir = true;
		playerAnimation.anim.Play ("Air Melee Attacks");
		playerAnimation.meleeAttack = 4;
		CheckCurrentAnimatorState ();
		swordCPU.ToggleSwordDamage (1);

		float timer = 0f;
		while (!movementScript.isGrounded && timer < 0.6f) {
			movementScript.doNotMove = true;
			timer += Time.deltaTime;
			yield return null;
		}

		swordCPU.cancelInAirCheck = true;
		swordCPU.ToggleSwordDamage (0);

		if (timer < 0.6f) {
			CheckCurrentAnimatorState ();
			playerAnimation.anim.Play ("Cool Landing");
			Player.GetComponent<References> ().Landing ();
			Player.GetComponent<References> ().LandingSound ();

			yield return new WaitForSeconds (0.125f);
			rigidbody.velocity = Vector3.zero;
			yield return new WaitForSeconds (0.125f);
			if (CustomMethods.CheckDisplacementInput())
				yield return new WaitForSeconds (0.125f);
		}

		movementScript.doNotMove = false;
		movementScript.cantShoot = false;
		movementScript.cantStab = false;
		landedLauncherA = false;
		playerAnimation.isAttackingMeleeAir = false;
		Player.GetComponent<Rigidbody> ().mass = 1f;
		swordCPU.AttackDone ();
		yield return null;
	}
}
