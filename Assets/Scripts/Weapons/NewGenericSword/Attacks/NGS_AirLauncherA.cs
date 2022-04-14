using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class NGS_AirLauncherA : NGS_AttackClass {

	public int attackLevel = 1;

	public GameObject Level1ParticlesFX;
	public GameObject Level2ParticlesFX;
	public GameObject Level3ParticlesFX;

	public GameObject launcherATarget;
	public bool landedLauncherA;

	//public new bool overthrows = true;

	private bool magicAuxBool;

	// Use this for initialization
	public override void Start () {
		base.Start();
	}

	// Update is called once per frame
	void Update () {
		if (!movementScript.isGrounded && cooldown <= 0f) {
			if (InputManager.GetButtonDown ("Launcher") && InputManager.GetAxis ("Vertical") < -0.25f) {
				swordCPU.AttackReady (this);
			}
		}

		if (cooldown > 0f)
			cooldown -= Time.deltaTime;
	}

	public override void ForcedReset(){
		landedLauncherA = false;
		launcherATarget = null;
		magicAuxBool = false;
	}

	IEnumerator CheckEmergencyCancel(){
		while (swordCPU.AttackBeingExecuted && !swordCPU.movementScript.isGrounded) {
			yield return null;
		} 
		ForcedReset ();
		swordCPU.AttackDone ();
	}

	public override IEnumerator ExecuteAttack(){

		if (Physics.Raycast(transform.position, Vector3.down, Player.GetComponent<Collider>().bounds.extents.y + 0.625f /*0.25f*/, movementScript.isGroundedIgnoreLayers)) {
			swordCPU.AttackDone();
			ForcedReset();
			yield break;
		}

		if (GetComponent<NGS_LauncherA>()) {
			GetComponent<NGS_LauncherA>().StopAllCoroutinesExceptThrowEnemy();
			//GetComponent<NGS_LauncherA>().ForcedReset();
			//GetComponent<NGS_LauncherA>().isObsolete = true;
		}

		//ForcedReset ();
		//StartCoroutine (CheckEmergencyCancel ());
		StartCoroutine (swordCPU.AirCheck ());
		CheckCurrentAnimatorState ();
		magicAuxBool = false;
		movementScript.cantShoot = true;
		movementScript.doNotMove = true;
		movementScript.jumpInertia = false;
		playerAnimation.isAttackingMeleeAir = true;
		SpeedMultiplier = playerAnimation.finalSpeedMultiplier;

		//yield return new WaitForFixedUpdate ();
		playerAnimation.meleeAttack = 5;
		playerAnimation.attackLevel = attackLevel;

		if (attackLevel <= 1) {
			playerAnimation.attackLevel = 1f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.0f;
			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.GetComponent<References>().LeftFoot.transform.position + (Player.GetComponent<References>().LeftFoot.transform.position - Player.GetComponent<References>().RightFoot.transform.position) / 2f, transform.rotation);
				particlesRunning.transform.parent = Player.transform;

			}
			//meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
		} else if (attackLevel == 2) {
			playerAnimation.attackLevel = 2f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.1f;
			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.GetComponent<References>().LeftFoot.transform.position + (Player.GetComponent<References>().LeftFoot.transform.position - Player.GetComponent<References>().RightFoot.transform.position) / 2f, transform.rotation);
				particlesRunning.transform.parent = Player.transform;
			}
			//meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialBlue;
		} else if (attackLevel == 3) {
			playerAnimation.attackLevel = 3f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.2f;
			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.GetComponent<References>().LeftFoot.transform.position + (Player.GetComponent<References>().LeftFoot.transform.position - Player.GetComponent<References>().RightFoot.transform.position) / 2f, transform.rotation);
				particlesRunning.transform.parent = Player.transform;
			}
			//meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialRed;
		}


		particlesRunning.SetActive (false);
		Player.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
		rigidbody.velocity = Vector3.zero;
		rigidbody.AddForce (Player.transform.up * 2f, ForceMode.VelocityChange);

		while (magicAuxBool == false) {
			yield return null;
		}

		if (landedLauncherA) {
			if (swordCPU.CoroSpecialCororunning != null)
				StopCoroutine (swordCPU.CoroSpecialCororunning);
			particlesRunning.SetActive (true);
			particlesRunning.transform.position = (Player.GetComponent<References>().LeftFoot.transform.position + (Player.GetComponent<References>().LeftFoot.transform.position - Player.GetComponent<References>().RightFoot.transform.position) / 2f);
			particlesRunning.transform.rotation = Quaternion.LookRotation (particlesRunning.transform.position - Player.transform.position);
			Destroy (particlesRunning, 1f);
		} else {
			Destroy (particlesRunning);
		}

		yield return new RadicalWaitForSeconds (0.1f / SpeedMultiplier);
		rigidbody.velocity = Vector3.zero;
		if (InputManager.GetButton ("Jump")) {
			cooldown = 0.5f;
			if (landedLauncherA) {
				CheckCurrentAnimatorState ();
				playerAnimation.meleeAttack = 4;
				yield return new WaitForSeconds (0.125f / SpeedMultiplier);
				Rigidbody LauncherATargetRigidbody = launcherATarget.GetComponent<Rigidbody> ();
				LauncherATargetRigidbody.velocity = Vector3.zero;
				LauncherATargetRigidbody.AddForce (Vector3.down * 25f, ForceMode.VelocityChange);
				movementScript.cantShoot = false;
				AudioSource audioSource = CustomMethods.PlayClipAt (hitSound, transform.position);
				audioSource.volume = 0.75f;
				yield return new WaitForFixedUpdate ();
				rigidbody.velocity = Vector3.zero;
				rigidbody.AddForce (Vector3.up * 3.5f, ForceMode.VelocityChange);
				rigidbody.AddForce (-transform.forward * 3f, ForceMode.VelocityChange);
			} else {
				yield return new WaitForFixedUpdate ();
				rigidbody.velocity = Vector3.zero;
				rigidbody.AddForce (Vector3.up * 3f, ForceMode.VelocityChange);
			}

			yield return new WaitForSeconds (0.125f / SpeedMultiplier);

			while (TerminateAnimation() && animationTimer > 0f) {
				animationTimer -= Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}

			landedLauncherA = false;
			movementScript.doNotMove = false;
			movementScript.cantShoot = false;
			playerAnimation.isAttackingMeleeAir = false;
			swordCPU.AttackDone ();
			yield break;
		} else if (!InputManager.GetButton ("Jump")) {
			cooldown = 0.5f;
			if (landedLauncherA) {
				Rigidbody LauncherATargetRigidbody = launcherATarget.GetComponent<Rigidbody> ();
				LauncherATargetRigidbody.AddForce (Vector3.down * 25f, ForceMode.VelocityChange);
				AudioSource audioSource = CustomMethods.PlayClipAt (hitSound, transform.position);
				audioSource.volume = 0.75f;
				yield return new WaitForFixedUpdate ();
				rigidbody.velocity = Vector3.zero;
			} else {
				yield return new WaitForFixedUpdate ();
				rigidbody.velocity = Vector3.zero;
			}

			yield return new WaitForSeconds (0.1f / SpeedMultiplier);
			movementScript.cantShoot = false;
			
			animationTimer = 0.5f;
			while (TerminateAnimation () && animationTimer > 0f) {
				animationTimer -= Time.deltaTime;
				yield return new WaitForEndOfFrame ();
			}

			landedLauncherA = false;
			movementScript.doNotMove = false;
			movementScript.cantShoot = false;
			playerAnimation.isAttackingMeleeAir = false;
			swordCPU.AttackDone ();
		}
	}

	public void ColliderAirLauncherAttackA(){
		//LayerMask layerMask = LayerMask.GetMask ("Enemy", "Player");
		//layerMask = ~layerMask;
		Collider[] hitColliders = Physics.OverlapSphere (Player.GetComponent<References>().LeftFoot.transform.position + (Player.GetComponent<References>().LeftFoot.transform.position - Player.GetComponent<References>().RightFoot.transform.position) / 2f, 0.95f, swordCPU.targetLayers);
		HealthBar damage;

		foreach (Collider collider in hitColliders) {
			if (collider.gameObject != Player) {
				landedLauncherA = true;
				launcherATarget = collider.gameObject;
				break;
			} else {
				landedLauncherA = true;
				launcherATarget = null;
			}
		}

		if (launcherATarget) {
			damage = launcherATarget.GetComponent<HealthBar> ();
			if (!damage.isDead && damage.AddjustCurrentHealth (Player.transform, -5f, stunTime, unstoppable, overthrows)) {
				//damage.GetComponent<HealthBar> ().painTimer = 3f;
				damage.AddjustCurrentHealth (Player.transform, -swordCPU.attackDamage, stunTime, unstoppable, overthrows);
				landedLauncherA = true;
			} else {
				landedLauncherA = false;
				launcherATarget = null;
			}
		} else {
			landedLauncherA = false;
			launcherATarget = null;
		}
		magicAuxBool = true;
	}
}


