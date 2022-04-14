using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class NGS_MaeGeri : NGS_AttackClass {


	public int attackLevel = 1;

	public GameObject launcherATarget;
	public bool landedLauncherA;

	public GameObject Level1ParticlesFX;
	public GameObject Level2ParticlesFX;
	public GameObject Level3ParticlesFX;

	private bool magicAuxBool;

	//public new bool overthrows = true;

	// Use this for initialization

	public override void Start () {
		base.Start ();
	}

	public override void ForcedReset(){
		landedLauncherA = false;
		launcherATarget = null;
		magicAuxBool = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (/*InputManager.GetButton ("Combo Mode") &&*/ movementScript.isGrounded && !movementScript.isJumping && cooldown <= 0f){
			if (InputManager.GetButtonDown ("Launcher") && InputManager.GetAxis ("Vertical") > 0.75f) {
				swordCPU.AttackReady (this);
			}
		}

		if (cooldown > 0f)
			cooldown -= Time.deltaTime;
	}

	public void ColliderMaeGeri(){
		//LayerMask layerMask = LayerMask.GetMask ("Enemy", "Player");
		//layerMask = ~layerMask;
		Collider[] hitColliders = Physics.OverlapSphere (Player.GetComponent<References>().RightFoot.transform.position + Vector3.down * 0.5f, 0.5f, swordCPU.targetLayers);
		HealthBar damage;

		if (hitColliders.Length > 0) {
			print ("1) overthrows: " + overthrows + " at " + this.GetType().ToString()); 
			foreach (Collider collider in hitColliders) {
				if (collider.gameObject != Player && collider.gameObject.GetComponent<HealthBar> ()) {
					landedLauncherA = true;
					launcherATarget = collider.gameObject;
					break;
				} else {
					landedLauncherA = false;
					launcherATarget = null;
				}
			}
		}

		if (launcherATarget) {
			damage = launcherATarget.GetComponent<HealthBar> ();
			if (damage && !damage.isDead && damage.AddjustCurrentHealth (Player.transform, -5f, stunTime, unstoppable, overthrows)) {
				//damage.GetComponent<HealthBar> ().painTimerOffset = 1.5f;
				damage.AddjustCurrentHealth (Player.transform, -swordCPU.attackDamage * 4 / 5, stunTime, unstoppable, overthrows);
				//StartCoroutine (SlowMotionCollisionEffect ());
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

	public override IEnumerator ExecuteAttack(){

		ForcedReset ();
		cooldown = 0.5f;

		if (!InputManager.GetButton ("Combo Mode")) {
			GameObject FoundTarget = CustomMethods.SearchForTargetInDirection (Player);
			if (FoundTarget)
				StartCoroutine (CustomMethods.SmoothRotateTowards (Player, FoundTarget.transform, 0.125f));
		}/* else if (Player.GetComponent<References>().currentAutoaimTarget) {
			StartCoroutine (CustomMethods.SmoothRotateTowards (Player, Player.GetComponent<References>().currentAutoaimTarget.transform, 0.1f));
		}*/

		//CheckCurrentAnimatorState ();
		magicAuxBool = false;
		movementScript.cantShoot = true;
		movementScript.doNotMove = true;
		playerAnimation.isAttackingMelee = true;
		SpeedMultiplier = playerAnimation.finalSpeedMultiplier;

		//yield return new WaitForFixedUpdate ();
		playerAnimation.meleeAttack = 11;
		playerAnimation.attackLevel = attackLevel;

		if (attackLevel <= 1) {
			playerAnimation.attackLevel = 1f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.0f;
			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.GetComponent<References> ().RightFoot.transform.position, transform.rotation);
				particlesRunning.transform.parent = Player.transform;

			}
			//meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
		} else if (attackLevel == 2) {
			playerAnimation.attackLevel = 2f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.1f;
			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.GetComponent<References> ().RightFoot.transform.position, transform.rotation);
				particlesRunning.transform.parent = Player.transform;
			}
			//meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialBlue;
		} else if (attackLevel == 3) {
			playerAnimation.attackLevel = 3f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.2f;
			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX,  Player.GetComponent<References> ().RightFoot.transform.position, transform.rotation);
				particlesRunning.transform.parent = Player.transform;
			}
			//meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialRed;
		}

		if (particlesRunning)
			particlesRunning.SetActive (false);
		Player.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
		rigidbody.velocity = Vector3.zero;

		while (magicAuxBool == false) {
			rigidbody.velocity = Vector3.zero;
			yield return null;
		}

		yield return new RadicalWaitForSeconds (0.1f / SpeedMultiplier);
		rigidbody.velocity = Vector3.zero;
		if (landedLauncherA) {
			if (particlesRunning)
				particlesRunning.SetActive (true);
			Rigidbody LauncherATargetRigidbody = launcherATarget.GetComponent<Rigidbody> ();
			LauncherATargetRigidbody.velocity = Vector3.zero;
			LauncherATargetRigidbody.AddForce (Player.transform.forward * 5f, ForceMode.VelocityChange);
			LauncherATargetRigidbody.AddForce (Vector3.up * 4f, ForceMode.VelocityChange);
			movementScript.cantShoot = false;
			AudioSource audioSource = CustomMethods.PlayClipAt (hitSound, transform.position);
			audioSource.volume = 0.75f;
		}

		yield return new WaitForSeconds (0.075f / SpeedMultiplier);
		landedLauncherA = false;
		movementScript.doNotMove = false;
		movementScript.cantShoot = false;
		playerAnimation.isAttackingMelee = false;
		swordCPU.AttackDone ();
	}
}
