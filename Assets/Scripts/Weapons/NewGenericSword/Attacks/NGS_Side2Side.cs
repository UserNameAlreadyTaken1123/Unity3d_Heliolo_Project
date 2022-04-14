using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Luminosity.IO;
using StateStuff;

public class NGS_Side2Side : NGS_AttackClass {

	public int attackLevel = 1;
	public float level1ManaRequirement;
	public float level2ManaRequirement;
	public float level3ManaRequirement;
	private float strenght = 50f;

	public bool ActivateTimerToReset = false;
	//The more bools, the less readibility, try to stick with the essentials.
	//If you were to press 10 buttons in a row
	//having 10 booleans to check for those would be confusing

	public bool prevConditionAttackDone = false;

	public float currentComboTimer;
	public int currentComboState = 0;

	float origTimer = 0.75f;

	private RaycastHit[] hitInfo1;
	private GameObject currentTarget;
	private Vector3 origTargetScale;

	//public new float stunTime = 3.5f;

	public override void Start(){
		priority = 2;
		if (level2ManaRequirement == 0f)
			level2ManaRequirement = level1ManaRequirement * 1.3f;
		if (level3ManaRequirement == 0f)
			level3ManaRequirement = level1ManaRequirement * 1.5f;

		switch (attackLevel) {
			case 0:
				strenght = 50f;
				break;
			case 1:
				strenght = 50f;
				break;
			case 2:
				strenght = 75f;
				break;
			case 3:
				strenght = 100f;
				break;
		}

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

	public override void OnEnable(){
		ForcedReset ();
	}

	public override void ForcedReset(){
		currentComboState = 0;
		ActivateTimerToReset = false;
		currentComboTimer = origTimer;
		prevConditionAttackDone = false;
		if (playerAnimation != null)
			playerAnimation.PenalizeSpeed(false, 1f);
		ParentTargetToSword (0);
		currentTarget = null;
	}

	void Input(){
		if (InputManager.GetButton ("Combo Mode") && movementScript.isGrounded && cooldown <= 0f){
			switch (currentComboState){
			case 0:
				if (InputManager.GetAxis ("Vertical") < -0.75f & InputManager.GetAxis ("Horizontal") > -0.5f & InputManager.GetAxis ("Horizontal") < 0.5f && currentComboTimer != 0f) {
					ActivateTimerToReset = true;
					currentComboTimer = origTimer;
					currentComboState++;
				}
				break;
			case 1:
				if (InputManager.GetAxis ("Vertical") > 0.75f & InputManager.GetAxis ("Horizontal") > -0.5f & InputManager.GetAxis ("Horizontal") < 0.5f && currentComboTimer != 0f)
					currentComboState++;
				break;
			case 2:
				if (InputManager.GetAxis ("Vertical") > -0.5f & InputManager.GetAxis ("Vertical") < 0.5f & InputManager.GetAxis ("Horizontal") < -0.75f && currentComboTimer != 0f)
					currentComboState++;
				break;
			case 3:
				if (InputManager.GetAxis ("Vertical") < -0.75f & InputManager.GetAxis ("Horizontal") > -0.5f & InputManager.GetAxis ("Horizontal") < 0.5f && currentComboTimer != 0f)
					currentComboState++;
				break;
			case 4:
				if (InputManager.GetButtonDown ("Melee") && currentComboTimer != 0f) {
					swordCPU.AttackReady (this);
					currentComboState = 0;
				}
				break;
			}
		}
	}

	public override void ParentTargetToSword(int activated){
		if (activated == 1) {
			hitInfo1 = Physics.RaycastAll(Player.transform.position, Player.transform.forward, 1.5f, swordCPU.targetLayers, QueryTriggerInteraction.Ignore);
			if (hitInfo1.Length > 0) {
				for (int i = 0; i < hitInfo1.Length; i++) {
					if (hitInfo1[i].collider.gameObject != Player.gameObject) {
						currentTarget = hitInfo1[i].collider.gameObject;
						//print (currentTarget.name);
						break;
					}
				}
			} else {
				return;
			}

			if (currentTarget) {
				if (currentTarget.GetComponent<Rigidbody>().mass <= strenght * 1.5f) {
					playerAnimation.PenalizeSpeed(true, strenght / currentTarget.GetComponent<Rigidbody>().mass);

					CustomMethods.PlayClipAt(swordCPU.swordSlash, transform.position);
					if (currentTarget.GetComponent<NavMeshAgent>().enabled)
						currentTarget.GetComponent<NavMeshAgent>().enabled = false;

					if (currentTarget.GetComponent<Player_Animation>())
						currentTarget.GetComponent<Player_Animation>().ResetValues();

					if (currentTarget.GetComponent<State<AI>>())
						currentTarget.GetComponent<State<AI>>().enabled = false;

					currentTarget.GetComponent<Collider>().isTrigger = true;

					currentTarget.GetComponent<Rigidbody>().isKinematic = true;
					origTargetScale = currentTarget.transform.localScale;
					//currentTargetRoot = currentTarget.transform.parent.gameObject;

					currentTarget.SetParent(this.gameObject);
					currentTarget.GetComponent<Collider>().material.dynamicFriction = 1.0f;
					currentTarget.GetComponent<Collider>().material.staticFriction = 1.0f;
					currentTarget.GetComponent<Rigidbody>().angularDrag = 1;

					currentTarget.transform.localScale = new Vector3(currentTarget.transform.localScale.x / this.transform.lossyScale.x,
						currentTarget.transform.localScale.y / this.transform.lossyScale.y,
						currentTarget.transform.localScale.z / this.transform.lossyScale.z);
					currentTarget.transform.localPosition = new Vector3(0f, 0f, 0.00115f); //0.00125f original value

					//currentTarget.GetComponent<HealthBar> ().painTimerOffset = 4f;
					currentTarget.GetComponent<HealthBar>().AddjustCurrentHealth(Player.transform, -swordCPU.attackDamage, stunTime, unstoppable, true);
				} else {
					currentTarget.GetComponent<HealthBar>().AddjustCurrentHealth(Player.transform, -swordCPU.attackDamage / 2f, stunTime / 2f, false, false);
				}

			} else {
				CustomMethods.PlayClipAt(swordCPU.swordSwing, transform.position);
				ForcedReset();
			}
		} else {
			//swordCPU.ToggleSwordDamage (0);
			if (currentTarget) {
				playerAnimation.PenalizeSpeed(false, 1f);
				currentTarget.transform.parent = null;
				currentTarget.transform.localScale = origTargetScale;
				currentTarget.GetComponent<Collider>().isTrigger = false;

				currentTarget.GetComponent<Rigidbody>().isKinematic = false;
				//currentTarget.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.None;
				currentTarget.GetComponent<Player_Animation>().anim.Play("Lay On Ground", -1);
				currentTarget.GetComponent<Rigidbody>().velocity = Vector3.zero;
				currentTarget.GetComponent<Rigidbody>().AddForce(-Player.transform.forward * 2.5f, ForceMode.VelocityChange);
				//currentTarget.GetComponent<Rigidbody> ().AddTorque (currentTarget.transform.right * 2.5f, ForceMode.VelocityChange);
				//currentTarget.GetComponent<Rigidbody> ().AddTorque (Vector3.down * 5f, ForceMode.VelocityChange);
				currentTarget.transform.eulerAngles = Vector3.up * 90f;
				currentTarget = null;
			}
		}
	}

	public override IEnumerator ExecuteAttack(){
		ForcedReset();
		if (attackLevel == 1 && manaBarScript.CurMana >= level1ManaRequirement) {
			manaBarScript.CurMana -= level1ManaRequirement;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.3f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
		} else if (attackLevel == 2 && manaBarScript.CurMana >= level1ManaRequirement) {
			manaBarScript.CurMana -= level1ManaRequirement;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.35f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialBlue;
		} else if (attackLevel == 3 && manaBarScript.CurMana >= level1ManaRequirement) {
			manaBarScript.CurMana -= level1ManaRequirement;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.45f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialRed;
		} else {
			//No shufiziente manah.
			swordCPU.AttackDone();
			yield break;
		}

		SpeedMultiplier = playerAnimation.finalSpeedMultiplier;
		rigidbody.velocity = Vector3.zero;

		boxCollider.size = new Vector3 (boxCollider.size.x, boxCollider.size.y, boxCollider.size.z + 0.0015f);
		movementScript.cantShoot = true;
		movementScript.cantStab = true;
		playerAnimation.meleeAttack = 8f;
		playerAnimation.attackLevel = attackLevel;
		playerAnimation.isAttackingMelee = true;
		rigidbody.AddForce (Player.transform.forward * 5.0f, ForceMode.VelocityChange);
		meleeWeaponTrail.Emit = true;
		movementScript.doNotMove = true;
		Player.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
		yield return new WaitForSeconds (0.1f * 0.2f / SpeedMultiplier);

		//rigidbody.AddForce (Player.transform.forward * 3.0f, ForceMode.VelocityChange);
		//damageCol = true;
		//yield return new WaitForSeconds (1.0f * 0.4f);

		CustomMethods.PlayClipAt (swordCPU.swordSwing, transform.position);

		movementScript.velocityLimitXZ = 5f;


		float ii = 0f;
		while (ii < 0.3f / SpeedMultiplier) {
			rigidbody.AddForce (Player.transform.forward * 1.0f, ForceMode.VelocityChange);
			yield return null;
			ii += Time.deltaTime; 
		}

		ii = 0f;

		float x;
		float y;
		float z;

		while (ii < 0.3f / SpeedMultiplier) {
			x = rigidbody.velocity.x;
			y = rigidbody.velocity.y;
			z = rigidbody.velocity.z;
			yield return new WaitForEndOfFrame ();
			ii += Time.deltaTime;
			x = Mathf.Lerp (x, 0f, 0.3f);
			y = Mathf.Lerp (y, 0f, 0.3f);
			z = Mathf.Lerp (z, 0f, 0.3f);

			rigidbody.velocity = new Vector3 (x, y, z);
		}

		//rigidbody.velocity = Vector3.zero;
		//yield return new WaitForSeconds (0.4f);
		//damageCol = false;

		movementScript.velocityLimitXZ = 5f;
		ii = 0f;
		while (ii < 0.25f / SpeedMultiplier) {
			rigidbody.AddForce (Player.transform.forward * 1.0f, ForceMode.VelocityChange);
			yield return null;
			ii += Time.deltaTime;
		}

		ii = 0f;
		while (ii < 0.25f / SpeedMultiplier) {
			x = rigidbody.velocity.x;
			y = rigidbody.velocity.y;
			z = rigidbody.velocity.z;
			yield return new WaitForEndOfFrame ();
			ii += Time.deltaTime;
			x = Mathf.Lerp (x, 0f, 0.3f);
			y = Mathf.Lerp (y, 0f, 0.3f);
			z = Mathf.Lerp (z, 0f, 0.3f);

			rigidbody.velocity = new Vector3 (x, y, z);
		}
		movementScript.velocityLimitXZ = 0f;

		//rigidbody.velocity = Vector3.zero;
		//yield return new WaitForSeconds (0.8f);

		//playerAnimation.meleeAttack = 0f;

		playerAnimation.isAttackingMelee = false;
		boxCollider.size = swordCPU.boxColliderOrigSize;
		movementScript.doNotMove = false;
		swordCPU.attackDamage = swordCPU.baseDamage;

		meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
		meleeWeaponTrail.Emit = false;
		//yield return new WaitForSeconds (0.2f * 0.3f);
		movementScript.cantShoot = false;
		movementScript.cantStab = false;
		//yield return new WaitForSeconds (0.2f * 0.3f);
		swordCPU.AttackDone ();
	}
}
