
using UnityEngine;
using System.Collections;
using Luminosity.IO;

public class NGS_SpinAttack : NGS_AttackClass {

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
		priority = 6;
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

	void Input(){
		if (InputManager.GetButton ("Combo Mode") && movementScript.isGrounded && cooldown <= 0f) {
			switch (currentComboState) {
			case 0:
				if (InputManager.GetAxis ("Horizontal") < -0.25f) {
					ActivateTimerToReset = true;
					currentComboTimer = origTimer;
					currentComboState++;
				}
				break;
			case 1:
				if (InputManager.GetAxis ("Horizontal") > 0.25f && currentComboTimer != 0f)
					currentComboState++;
				break;
			case 2:
				if (InputManager.GetAxis ("Horizontal") < -0.25f && currentComboTimer != 0f)
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
		ForcedReset ();
		if (attackLevel == 1 && manaBarScript.CurMana >= level1ManaRequirement) {
			if (Level1ParticlesFX) {
				particlesRunning = Instantiate (Level1ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - transform.up * 0.25f, transform.rotation);
				particlesRunning.transform.parent = Player.transform;
				Destroy (particlesRunning, 1.5f);
			}

			manaBarScript.CurMana -= level1ManaRequirement;
			swordCPU.attackDamage = swordCPU.baseDamage * 0.75f;
			boxCollider.size = new Vector3 (boxCollider.size.x, boxCollider.size.y, boxCollider.size.z + 0.0015f);
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;

		} else if (attackLevel == 2 && manaBarScript.CurMana >= level2ManaRequirement) {
			if (Level2ParticlesFX) {
				particlesRunning = Instantiate (Level2ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - transform.up * 0.25f, transform.rotation);
				particlesRunning.transform.parent = Player.transform;
				Destroy (particlesRunning, 1.5f);
			}

			manaBarScript.CurMana -= level2ManaRequirement;
			swordCPU.attackDamage = swordCPU.baseDamage * 0.85f;
			boxCollider.size = new Vector3 (boxCollider.size.x, boxCollider.size.y, boxCollider.size.z + 0.0016f);
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialBlue;

		} else if (attackLevel == 3 && manaBarScript.CurMana >= level3ManaRequirement) {
			if (Level3ParticlesFX) {
				particlesRunning = Instantiate (Level3ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - transform.up * 0.25f, transform.rotation);
				particlesRunning.transform.parent = Player.transform;
				Destroy (particlesRunning, 1.5f);
			}

			manaBarScript.CurMana -= level3ManaRequirement;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.0f;
			boxCollider.size = new Vector3 (boxCollider.size.x, boxCollider.size.y, boxCollider.size.z + 0.0017f);
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialRed;
		} else {
			//No shufiziente manah.
			swordCPU.AttackDone();
			yield break;
		}

		rigidbody.velocity = Vector3.zero;
		SpeedMultiplier = playerAnimation.finalSpeedMultiplier;

		CheckCurrentAnimatorState ();
		movementScript.doNotMove = true;
		movementScript.cantShoot = true;
		movementScript.cantStab = true;
		playerAnimation.meleeAttack = 6f;
		playerAnimation.attackLevel = attackLevel;
		playerAnimation.isAttackingMelee = true;
		rigidbody.AddForce (Player.transform.forward * attackLevel, ForceMode.VelocityChange);
		Player.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
		yield return new WaitForSeconds (0.1f / SpeedMultiplier);

		rigidbody.AddForce (Player.transform.forward * 3.0f, ForceMode.VelocityChange);
		swordCPU.ToggleSwordDamage(1);
		//CustomMethods.PlayClipAt (swordCPU.swordSwing, transform.position);

		yield return new WaitForSeconds (0.45f / SpeedMultiplier);
		//playerAnimation.meleeAttack = 0f;

		if (attackLevel < 3)
			yield return new WaitForSeconds (0.15f / SpeedMultiplier);
		movementScript.cantShoot = false;
		movementScript.cantStab = false;
		if (attackLevel < 2)
			yield return new WaitForSeconds (0.15f / SpeedMultiplier);

		rigidbody.velocity = Vector3.zero;

		animationTimer = 1f;
		while (animationTimer > 0f) {
			if (TerminateAnimation ()) {
				animationTimer -= Time.deltaTime;
				yield return new WaitForEndOfFrame ();
			} else
				break;
		}
		playerAnimation.isAttackingMelee = false;
		boxCollider.size = swordCPU.boxColliderOrigSize;
		movementScript.doNotMove = false;
		swordCPU.ToggleSwordDamage(0);
		swordCPU.attackDamage = swordCPU.baseDamage;
		swordCPU.AttackDone ();
	}
}