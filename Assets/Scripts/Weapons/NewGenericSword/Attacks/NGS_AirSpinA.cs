using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class NGS_AirSpinA : NGS_AttackClass {

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

	public GameObject ParticlesFX;

	float origTimer = 0.75f;

	public override void Start(){
		priority = 4;
		if (level2ManaRequirement == 0f)
			level2ManaRequirement = level1ManaRequirement * 1.25f;
		if (level3ManaRequirement == 0f)
			level3ManaRequirement = level1ManaRequirement * 1.5f;
		base.Start ();
	}

	void Update(){
		Input();
		ResetComboStateTimer(ActivateTimerToReset);
		if (cooldown > 0f)
			cooldown -= Time.deltaTime;
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

	void Input(){
		if (InputManager.GetButton ("Combo Mode") && !movementScript.isGrounded && cooldown <= 0f) {
			switch (currentComboState) {
			case 0:
				if (InputManager.GetAxis ("Vertical") < -0.25f) {
					ActivateTimerToReset = true;
					currentComboTimer = origTimer;
					currentComboState++;
				}
				break;
			case 1:
				if (InputManager.GetAxis ("Horizontal") < -0.25f && currentComboTimer != 0f)
					currentComboState++;
				break;
			case 2:
				if (InputManager.GetAxis ("Vertical") > 0.25f && currentComboTimer != 0f)
					currentComboState++;
				break;
			case 3:
				if (InputManager.GetButtonDown ("Melee") && currentComboTimer != 0f) {
					swordCPU.AttackReady (this);
					currentComboState = 0;
				}
				break;
			}
		}
	}

	public override IEnumerator ExecuteAttack(){
		if (attackLevel == 1 && manaBarScript.CurMana >= level1ManaRequirement) {

			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - transform.up * 0.25f, Quaternion.identity);
				particlesRunning.transform.parent = Player.transform;
				particlesRunning.transform.rotation = Player.transform.rotation * Quaternion.Euler (90f, 0f, 0f);
				Destroy (particlesRunning, 1.5f);
			}

			ForcedReset ();
			cooldown = 1f;
			movementScript.doNotMove = true;
			movementScript.cantShoot = true;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.5f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
			boxCollider.size = new Vector3 (boxCollider.size.x * 1.5f, boxCollider.size.y * 1.5f, boxCollider.size.z * 1.45f);

		} else if (attackLevel == 2 && manaBarScript.CurMana >= level2ManaRequirement) {

			if (Level2ParticlesFX) {
				particlesRunning = Instantiate (Level2ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - transform.up * 0.25f, Quaternion.identity);
				particlesRunning.transform.parent = Player.transform;
				particlesRunning.transform.rotation = Player.transform.rotation * Quaternion.Euler (90f, 0f, 0f);
				Destroy (particlesRunning, 1.5f);
			}

			ForcedReset ();
			cooldown = 0.75f;
			movementScript.doNotMove = true;
			movementScript.cantShoot = true;
			swordCPU.attackDamage = swordCPU.baseDamage * 2f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
			boxCollider.size = new Vector3 (boxCollider.size.x * 1.5f, boxCollider.size.y * 1.5f, boxCollider.size.z * 1.45f);

		} else if (attackLevel == 3 && manaBarScript.CurMana >= level3ManaRequirement) {

			if (Level3ParticlesFX) {
				particlesRunning = Instantiate (Level3ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - transform.up * 0.25f, Quaternion.identity);
				particlesRunning.transform.parent = Player.transform;
				particlesRunning.transform.rotation = Player.transform.rotation * Quaternion.Euler (90f, 0f, 0f);
				Destroy (particlesRunning, 1.5f);
			}

			ForcedReset ();
			cooldown = 0.5f;
			StartCoroutine (swordCPU.AirCheck ());
			CheckCurrentAnimatorState ();
			movementScript.doNotMove = true;
			movementScript.cantShoot = true;
			swordCPU.attackDamage = swordCPU.baseDamage * 2.5f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
			boxCollider.size = new Vector3 (boxCollider.size.x * 1.5f, boxCollider.size.y * 1.5f, boxCollider.size.z * 1.45f);
		} else {
			//No shufiziente manah.
			swordCPU.AttackDone();
			yield break;
		}

		if (ParticlesFX) {
			particlesRunning = Instantiate (ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - Player.transform.up * 0.25f, Player.transform.rotation * Quaternion.Euler (20f, 0f, -90f));
			particlesRunning.transform.parent = Player.transform;
			particlesRunning.transform.localScale = new Vector3 (particlesRunning.transform.lossyScale.x, particlesRunning.transform.lossyScale.y * 2f, particlesRunning.transform.lossyScale.z);
			Destroy (particlesRunning, 1f);
		}

		SpeedMultiplier = playerAnimation.finalSpeedMultiplier;

		playerAnimation.attackLevel = attackLevel;
		playerAnimation.meleeAttack = 4;
		playerAnimation.isAttackingMeleeAir = true;
		CheckCurrentAnimatorState ();
		StartCoroutine (swordCPU.AirCheck ());

		rigidbody.useGravity = false;
		rigidbody.velocity = new Vector3 (rigidbody.velocity.x, 0f, rigidbody.velocity.z);
		rigidbody.AddForce (Vector3.up * 1.3f, ForceMode.VelocityChange);
		rigidbody.AddForce (Player.transform.forward * 1f, ForceMode.VelocityChange);
		yield return new WaitForSeconds (0.2f / SpeedMultiplier);
		rigidbody.useGravity = true;
		yield return new WaitForSeconds (0.1f / SpeedMultiplier);
		boxCollider.size = swordCPU.boxColliderOrigSize;
		meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
		swordCPU.attackDamage = swordCPU.baseDamage;
		movementScript.doNotMove = false;
		movementScript.cantShoot = false;
		playerAnimation.isAttackingMeleeAir = false;
		swordCPU.AttackDone ();
		yield return new WaitForEndOfFrame ();
	}
}
