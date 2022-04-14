
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Luminosity.IO;

public class NGS_ForwardAttack : NGS_AttackClass {

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

	float origTimer = 0.75f;

	public override void Start(){
		priority = 5;
		if (level2ManaRequirement == 0f)
			level2ManaRequirement = level1ManaRequirement * 1.25f;
		if (level3ManaRequirement == 0f)
			level3ManaRequirement = level1ManaRequirement * 1.5f;
		base.Start ();
	}

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

	public override bool SpecialOntriggerenter(Collider collider){
		//collider.GetComponent<HealthBar> ().painTimerOffset = collider.GetComponent<HealthBar> ().painTimer * -0.5f;
		return true;
	}

	void Input(){
		if (InputManager.GetButton ("Combo Mode") && movementScript.isGrounded && cooldown <= 0f) {
			switch (currentComboState) {
			case 0:
				if (InputManager.GetAxis ("Vertical") > 0.25f) {
					ActivateTimerToReset = true;
					currentComboTimer = origTimer;
					currentComboState++;
				}
				break;
			case 1:
				if (InputManager.GetAxis ("Vertical") < -0.25f & currentComboTimer != 0f)
					currentComboState++;
				break;
			case 2:
				if (InputManager.GetAxis ("Vertical") > 0.25f & currentComboTimer != 0f)
					currentComboState++;
				break;
			case 3:
				if (InputManager.GetButtonDown ("Melee") & currentComboTimer != 0f) {
					swordCPU.AttackReady (this);
					currentComboState = 0;
				}
				break;
			}
		}
	}

	public override IEnumerator ExecuteAttack(){
		int value;
		if (attackLevel == 1 && manaBarScript.CurMana >= level1ManaRequirement) {

			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.transform.position + Player.transform.forward * 0.75f + Vector3.up * 0.25f, Player.transform.rotation);
				particlesRunning.transform.parent = Player.transform;
				particlesRunning.transform.rotation = Player.transform.rotation;
				Destroy (particlesRunning, 1.5f);
			}

			swordCPU.attackDamage = swordCPU.attackDamage * 0.55f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
			manaBarScript.CurMana -= level1ManaRequirement;
			value = 5;
			movementScript.velocityLimitXZ = 8f;

		} else if (attackLevel == 2 && manaBarScript.CurMana >= level2ManaRequirement) {

			if (Level2ParticlesFX) {
				particlesRunning = Instantiate (Level2ParticlesFX, Player.transform.position + Player.transform.forward * 0.75f + Vector3.up * 0.25f, Player.transform.rotation);
				particlesRunning.transform.parent = Player.transform;
				particlesRunning.transform.rotation = Player.transform.rotation;
				Destroy (particlesRunning, 1.5f);
			}

			swordCPU.attackDamage = swordCPU.attackDamage * 0.7f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialBlue;
			manaBarScript.CurMana -= level2ManaRequirement;
			value = 10;
			movementScript.velocityLimitXZ = 9f;

		} else if (attackLevel == 3 && manaBarScript.CurMana >= level3ManaRequirement) {

			if (Level3ParticlesFX) {
				particlesRunning = Instantiate (Level3ParticlesFX, Player.transform.position + Player.transform.forward * 0.75f + Vector3.up * 0.25f, Player.transform.rotation);
				particlesRunning.transform.parent = Player.transform;
				particlesRunning.transform.rotation = Player.transform.rotation;
				Destroy (particlesRunning, 1.5f);
			}

			swordCPU.attackDamage = swordCPU.attackDamage * 0.825f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialRed;
			manaBarScript.CurMana -= level3ManaRequirement;
			value = 15;
			movementScript.velocityLimitXZ = 10f;
		} else {
			//No shufiziente manah.
			swordCPU.AttackDone();
			yield break;
		}				

		ForcedReset ();
		SpeedMultiplier = playerAnimation.finalSpeedMultiplier;
		Player.GetComponent<Rigidbody>().mass = 100f;
		CheckCurrentAnimatorState ();
		movementScript.cantShoot = true;
		movementScript.cantStab = true;
		playerAnimation.meleeAttack = 7f;
		playerAnimation.attackLevel = attackLevel;
		playerAnimation.isAttackingMelee = true;
		meleeWeaponTrail.Emit = true;
		movementScript.doNotMove = true;
		//Player.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
		rigidbody.AddForce (Player.transform.forward * 1.0f, ForceMode.VelocityChange);
		rigidbody.velocity = Vector3.zero;

		Quaternion origRot = Player.transform.rotation;

		Player.transform.rotation = Quaternion.Slerp (Player.transform.rotation, Quaternion.LookRotation ((swordCPU.references.rigidbody.velocity).normalized, Vector3.up), 0.5f * Time.deltaTime);
		particlesRunning.transform.rotation = Quaternion.Slerp (particlesRunning.transform.rotation, Quaternion.LookRotation ((swordCPU.references.rigidbody.velocity).normalized, Vector3.up), 0.5f * Time.deltaTime);
			
		yield return new WaitForSeconds (0.01f / SpeedMultiplier);

		boxCollider.size = new Vector3 (swordCPU.boxColliderOrigSize.x * 3, swordCPU.boxColliderOrigSize.y * 3, swordCPU.boxColliderOrigSize.z);

		swordCPU.ToggleSwordDamage(1);
		bool times = false;
		for (int i = 0; i < value; i++) {
			rigidbody.AddForce ((Player.transform.forward * 10f / SpeedMultiplier), ForceMode.VelocityChange);
			particlesRunning.transform.rotation = Quaternion.Slerp (particlesRunning.transform.rotation, Quaternion.LookRotation ((swordCPU.references.rigidbody.velocity).normalized, Vector3.up), 0.5f * Time.deltaTime);
			if (times) {
				boxCollider.size = swordCPU.boxColliderOrigSize;
				CustomMethods.PlayClipAt (swordCPU.swordSwing, transform.position);
				times = false;
			} else {
				boxCollider.size = Vector3.zero;
				swordCPU.targetsSlashed = new List<Collider> ();
				times = true;
			}
			swordCPU.references.Landing ();
			yield return new WaitForSeconds (0.03332f / SpeedMultiplier);
			Player.transform.rotation = Quaternion.Slerp (Player.transform.rotation, Quaternion.LookRotation ((swordCPU.references.rigidbody.velocity).normalized, Vector3.up), 0.5f * Time.deltaTime);
		}

		//playerAnimation.meleeAttack = 0f;
		Player.transform.rotation = origRot;
		particlesRunning.transform.rotation = Quaternion.Slerp (particlesRunning.transform.rotation, Quaternion.LookRotation ((swordCPU.references.rigidbody.velocity).normalized, Vector3.up), 0.5f * Time.deltaTime);
		//particlesRunning.transform.rotation = Quaternion.Slerp (particlesRunning.transform.rotation, Quaternion.LookRotation (Player.transform.forward, Vector3.up), 0.5f * Time.deltaTime);
		//Player.transform.rotation = Quaternion.Slerp (Player.transform.rotation, Quaternion.LookRotation (new Vector3 (0f, rigidbody.rotation.y, 0f), Vector3.up), 0.5f * Time.deltaTime);
		playerAnimation.isAttackingMelee = false;
		boxCollider.size = swordCPU.boxColliderOrigSize;
		boxCollider.center = swordCPU.boxColliderOrigCenter;

		swordCPU.ToggleSwordDamage(0);
		swordCPU.attackDamage = swordCPU.baseDamage;
		movementScript.velocityLimitXZ = 0f;
		yield return new WaitForSeconds (0.2f / SpeedMultiplier);
		if (movementScript.isGrounded) {
			rigidbody.velocity = Vector3.zero;
			rigidbody.AddForce (Player.transform.forward * 2f, ForceMode.VelocityChange);
			playerAnimation.anim.Play ("Cool Landing");
			swordCPU.references.Landing ();
			yield return new WaitForSeconds (0.8f / SpeedMultiplier / attackLevel);
		}
		movementScript.cantShoot = false;
		movementScript.cantStab = false;
		yield return new WaitForSeconds (0.1f / SpeedMultiplier);
		movementScript.doNotMove = false;
		Player.GetComponent<Rigidbody> ().mass = 1f;
		swordCPU.AttackDone ();
	}
}

