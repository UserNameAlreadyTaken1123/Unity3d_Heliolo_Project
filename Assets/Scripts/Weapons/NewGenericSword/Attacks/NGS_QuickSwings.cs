using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class NGS_QuickSwings : NGS_AttackClass {

	public int attackLevel = 1;
	public float level1ManaRequirement;
	public float level2ManaRequirement;
	public float level3ManaRequirement;

	public bool ActivateTimerToReset = false;
	//The more bools, the less readibility, try to stick with the essentials.
	//If you were to press 10 buttons in a row
	//having 10 booleans to check for those would be confusing

	public float currentComboTimer;
	public int currentComboState = 0;

	public GameObject Level1ParticlesFX;
	public GameObject Level2ParticlesFX;
	public GameObject Level3ParticlesFX;

	public GameObject ParticleFX;

	float origTimer = 0.35f;

	// Use this for initialization
	public override void Start(){
		priority = 2;			//Revisar en caso de conflictos
		if (level2ManaRequirement == 0f)
			level2ManaRequirement = level1ManaRequirement * 1.25f;
		if (level3ManaRequirement == 0f)
			level3ManaRequirement = level1ManaRequirement * 1.5f;
		base.Start ();
	}
	
	// Update is called once per frame
	void Update(){
		Input();
		ResetComboStateTimer(ActivateTimerToReset);
	}

	void ResetComboStateTimer(bool resetTimer){
		if (resetTimer){
			currentComboTimer -= Time.deltaTime;
			if (currentComboTimer <= 0){
				ForcedReset ();
			}
		}
	}

	public override void ForcedReset(){
		currentComboState = 0;
		ActivateTimerToReset = false;
		currentComboTimer = origTimer;
	}

	void Input (){
		if (cooldown <= 0f) {
			switch (currentComboState) {
			case 0:
				if (InputManager.GetButtonDown ("Melee")) {
					ActivateTimerToReset = true;
					currentComboTimer = origTimer;
					currentComboState++;
				}
				break;
			case 1:
				if (InputManager.GetButtonDown ("Melee") && currentComboTimer != 0f)
					currentComboState++;
				break;
			case 2:
				if (InputManager.GetButtonDown ("Melee") && currentComboTimer != 0f && movementScript.isGrounded) {
					swordCPU.AttackReady (this);
					currentComboState = 0;
				}
				break;
			}
		}
	}

	public override IEnumerator ExecuteAttack(){
		ForcedReset ();

		if (attackLevel == 1 && manaBarScript.CurMana >= level1ManaRequirement) {
			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - transform.up * 0.25f, transform.rotation);
				particlesRunning.transform.parent = Player.transform;
				Destroy (particlesRunning, 1.5f);
			}

			manaBarScript.CurMana -= level1ManaRequirement;
			swordCPU.attackDamage = swordCPU.baseDamage * 0.55f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;

		} else if (attackLevel == 2 && manaBarScript.CurMana >= level2ManaRequirement) {
			if (Level2ParticlesFX) {
				particlesRunning = Instantiate (Level2ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - transform.up * 0.25f, transform.rotation);
				particlesRunning.transform.parent = Player.transform;
				Destroy (particlesRunning, 1.5f);
			}

			manaBarScript.CurMana -= level2ManaRequirement;
			swordCPU.attackDamage = swordCPU.baseDamage * 0.625f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialBlue;

		} else if (attackLevel == 3 && manaBarScript.CurMana >= level3ManaRequirement) {
			if (Level3ParticlesFX) {
				particlesRunning = Instantiate (Level3ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - transform.up * 0.25f, transform.rotation);
				particlesRunning.transform.parent = Player.transform;
				Destroy (particlesRunning, 1.5f);
			}

			manaBarScript.CurMana -= level3ManaRequirement;
			swordCPU.attackDamage = swordCPU.baseDamage * 0.7f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialRed;
		} else {
			//No shufiziente manah.
			swordCPU.AttackDone();
			yield break;
		}

		rigidbody.velocity = Vector3.zero;
		SpeedMultiplier = playerAnimation.finalSpeedMultiplier;

		if (!InputManager.GetButton("Combo Mode")) {
			GameObject FoundTarget = CustomMethods.SearchForTargetInDirection(Player);
			if (FoundTarget)
				StartCoroutine(CustomMethods.SmoothRotateTowards(Player, FoundTarget.transform, 0f));
		}

		CheckCurrentAnimatorState ();
		boxCollider.size = new Vector3 (boxCollider.size.x * 2.5f, boxCollider.size.y * 2.5f, boxCollider.size.z * 1.25f);
		movementScript.cantShoot = true;
		movementScript.doNotMove = true;
		playerAnimation.meleeAttack = 12;
		playerAnimation.isAttackingMelee = true;
		Player.GetComponent<Collider> ().material.dynamicFriction = 1.0f;

		//First Swing Start
		while (!swordCPU.damageCol) {
			yield return null;
		}

		if (ParticleFX) {
			particlesRunning = Instantiate (ParticleFX, Player.transform.position + Player.transform.right * 0 + Player.transform.forward * 0.25f + Player.transform.up * 0.25f, Player.transform.rotation * Quaternion.Euler (30f, -35f, -145f));
			//particlesRunning.transform.localScale = new Vector3 (particlesRunning.transform.localScale.x, particlesRunning.transform.localScale.y, -particlesRunning.transform.localScale.z);
			particlesRunning.transform.parent = Player.transform;
			Destroy (particlesRunning, 1f);
		}

		rigidbody.velocity = Vector3.zero;
		rigidbody.AddForce (Player.transform.forward * 3f, ForceMode.VelocityChange);
		while (swordCPU.damageCol) {
			yield return null;
		}

		//Second Swing Start
		while (!swordCPU.damageCol) {
			yield return null;
		}

		if (ParticleFX) {
			particlesRunning = Instantiate (ParticleFX, Player.transform.position + Player.transform.right * 1/4 + Player.transform.up * 0.5f, Player.transform.rotation * Quaternion.Euler (20f, 40f, -20f));
			//particlesRunning.transform.localScale = new Vector3 (particlesRunning.transform.localScale.x, particlesRunning.transform.localScale.y, -particlesRunning.transform.localScale.z);
			particlesRunning.transform.parent = Player.transform;
			Destroy (particlesRunning, 1f);
		}

		rigidbody.velocity = Vector3.zero;
		rigidbody.AddForce (Player.transform.forward * 3f, ForceMode.VelocityChange);
		while (swordCPU.damageCol) {
			yield return null;
		}

		//Third Swing Start
		while (!swordCPU.damageCol) {
			yield return null;
		}

		if (ParticleFX) {
			particlesRunning = Instantiate (ParticleFX, Player.transform.position + Player.transform.right * 0f + Player.transform.forward * 0.25f + Player.transform.up * 0.5f, Player.transform.rotation * Quaternion.Euler (20f, -40f, -155f));
			//particlesRunning.transform.localScale = new Vector3 (particlesRunning.transform.localScale.x, particlesRunning.transform.localScale.y, -particlesRunning.transform.localScale.z);
			particlesRunning.transform.parent = Player.transform;
			Destroy (particlesRunning, 1f);
		}

		rigidbody.velocity = Vector3.zero;
		rigidbody.AddForce (Player.transform.forward * 3f, ForceMode.VelocityChange);
		while (swordCPU.damageCol) {
			yield return null;
		}

		//Fourth Swing Start
		while (!swordCPU.damageCol) {
			yield return null;
		}

		if (ParticleFX) {
			particlesRunning = Instantiate (ParticleFX, Player.transform.position + Player.transform.right * 1/4 + Player.transform.forward * 0.75f + Player.transform.up * 0.25f, Player.transform.rotation);
			particlesRunning.transform.parent = Player.transform;
			Destroy (particlesRunning, 1f);
		}

		//Fifth Swing Start
		while (!swordCPU.damageCol) {
			yield return null;
		}

		if (ParticleFX) {
			particlesRunning = Instantiate (ParticleFX, Player.transform.position + Player.transform.right * 0f + Player.transform.forward * 0.25f + Player.transform.up * 0.5f, Player.transform.rotation * Quaternion.Euler (20f, -40f, -155f));
			//particlesRunning.transform.localScale = new Vector3 (particlesRunning.transform.localScale.x, particlesRunning.transform.localScale.y, -particlesRunning.transform.localScale.z);
			particlesRunning.transform.parent = Player.transform;
			Destroy (particlesRunning, 1f);
		}

		rigidbody.velocity = Vector3.zero;
		rigidbody.AddForce (Player.transform.forward * 3f, ForceMode.VelocityChange);
		while (swordCPU.damageCol) {
			yield return null;
		}

		//Sixth Swing Start
		while (!swordCPU.damageCol) {
			yield return null;
		}

		if (ParticleFX) {
			particlesRunning = Instantiate (ParticleFX, Player.transform.position + Player.transform.right * 0f + Player.transform.forward * 0.25f + Player.transform.up * 0.5f, Player.transform.rotation * Quaternion.Euler (20f, -40f, -155f));
			//particlesRunning.transform.localScale = new Vector3 (particlesRunning.transform.localScale.x, particlesRunning.transform.localScale.y, -particlesRunning.transform.localScale.z);
			particlesRunning.transform.parent = Player.transform;
			Destroy (particlesRunning, 1f);
		}

		rigidbody.velocity = Vector3.zero;
		rigidbody.AddForce (Player.transform.forward * 3f, ForceMode.VelocityChange);
		while (swordCPU.damageCol) {
			yield return null;
		}

		rigidbody.velocity = Vector3.zero;
		rigidbody.AddForce (Player.transform.forward * 3f, ForceMode.VelocityChange);
		while (swordCPU.damageCol) {
			yield return null;
		}

		boxCollider.size = swordCPU.boxColliderOrigSize;
		yield return new WaitForSeconds (0.15f / SpeedMultiplier);
		while (playerAnimation.anim.GetCurrentAnimatorStateInfo (0).normalizedTime < 0.75f) {
			yield return null;
		}

		movementScript.doNotMove = false;
		movementScript.cantShoot = false;
		playerAnimation.isAttackingMelee = false;
		swordCPU.ToggleSwordDamage (0);

		while (playerAnimation.anim.GetCurrentAnimatorStateInfo (0).normalizedTime > 0.75f) {
			swordCPU.ToggleSwordDamage (0);
			yield return null;
		}

		swordCPU.AttackDone ();
	}
}
