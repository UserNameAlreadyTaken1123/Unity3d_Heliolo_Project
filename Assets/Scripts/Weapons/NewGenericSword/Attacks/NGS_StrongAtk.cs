
using UnityEngine;
using System.Collections;
using Luminosity.IO;

public class NGS_StrongAtk : NGS_AttackClass {
	
	public int attackLevel = 1;
	public float level1ManaRequirement;
	public float level2ManaRequirement;
	public float level3ManaRequirement;

	public GameObject ParticlesFX;

	public bool ActivateTimerToReset = false;
	//The more bools, the less readibility, try to stick with the essentials.
	//If you were to press 10 buttons in a row
	//having 10 booleans to check for those would be confusing

	public float timeToRelease = 1.25f;
	private float currentTimer;
	private Coroutine doNotFuckingMove;

	float origTimer;
	private bool released;

	public override void Start(){
		priority = 3;
		resetOnLanding = false;
		origTimer = currentTimer;
		if (level2ManaRequirement == 0f)
			level2ManaRequirement = level1ManaRequirement * 1.25f;
		if (level3ManaRequirement == 0f)
			level3ManaRequirement = level1ManaRequirement * 1.5f;
		base.Start ();
	}

	void Update(){
		Input();
	}

	public override void ForcedReset(){
		currentTimer = 0f;
		if (doNotFuckingMove != null)
			StopCoroutine (doNotFuckingMove);
	}

	public override void OnEnable(){
		ForcedReset ();
	}

	void Input(){
		if (InputManager.GetButton ("Combo Mode") && InputManager.GetButton ("Melee") && cooldown <= 0f) {
			currentTimer += Time.deltaTime;
			if (released && currentTimer >= timeToRelease && movementScript.isGrounded) {
				swordCPU.AttackReady (this);
				released = false;
			}
		} else {
			released = true;
			currentTimer = 0f;
		}
	}

	public override IEnumerator ExecuteAttack(){

		if (attackLevel <= 1) {
			playerAnimation.attackLevel = 1f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.35f + Mathf.Sqrt (swordCPU.baseDamage) * 2.5f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
		} else if (attackLevel == 2) {
			playerAnimation.attackLevel = 2f;
			swordCPU.attackDamage = swordCPU.baseDamage * 1.7f + Mathf.Sqrt (swordCPU.baseDamage) * 3.0f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialBlue;
		} else if (attackLevel == 3) {
			playerAnimation.attackLevel = 3f;
			swordCPU.attackDamage = swordCPU.baseDamage * 2.05f + Mathf.Sqrt (swordCPU.baseDamage) * 4.0f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialRed;
		} else {
			//No shufiziente manah.
			swordCPU.AttackDone();
			yield break;
		}

		if (!InputManager.GetButton ("Combo Mode")) {
			GameObject FoundTarget = CustomMethods.SearchForTargetInDirection (Player);
			if (FoundTarget)
				StartCoroutine (CustomMethods.SmoothRotateTowards (Player, FoundTarget.transform, 0f));
		}/* else if (Player.GetComponent<References>().currentAutoaimTarget) {
			StartCoroutine (CustomMethods.SmoothRotateTowards (Player, Player.GetComponent<References>().currentAutoaimTarget.transform, 0.1f));
		}*/

		ForcedReset ();
		doNotFuckingMove = StartCoroutine (DoNotFuckingMove ());
		movementScript.cantShoot = true;
		movementScript.cantStab = true;
		movementScript.doNotMove = true;
		playerAnimation.meleeAttack = 5f;
		SpeedMultiplier = playerAnimation.finalSpeedMultiplier;
		playerAnimation.isAttackingMelee = true;
		CheckCurrentAnimatorState ();
		playerAnimation.isJumping = true;
		rigidbody.velocity = Vector3.zero;
		Player.GetComponent<Collider> ().material.dynamicFriction = 0.0f;
		Player.GetComponent<Collider> ().material.staticFriction = 0.0f;
		playerAnimation.attackLevel = attackLevel;

		//rigidbody.AddForce (Player.transform.forward * 2f, ForceMode.VelocityChange);
		//boxCollider.size = new Vector3 (boxCollider.size.x * 2f, boxCollider.size.y, boxCollider.size.z);
		yield return new WaitForSeconds (0.3f / SpeedMultiplier);
		rigidbody.AddForce (Player.transform.up * movementScript.jumpBaseForce * 3 / 4, ForceMode.VelocityChange);
		rigidbody.AddForce (Player.transform.forward * movementScript.runSpeed, ForceMode.VelocityChange);

		if (ParticlesFX) {
			particlesRunning = Instantiate (ParticlesFX, Player.transform.position + Player.transform.forward * 0.25f - Player.transform.up * 0.25f, Player.transform.rotation * Quaternion.Euler (20f, 0f, -90f));
			particlesRunning.transform.parent = Player.transform;
			particlesRunning.transform.localScale = new Vector3 (particlesRunning.transform.lossyScale.x, particlesRunning.transform.lossyScale.y * 2f, particlesRunning.transform.lossyScale.z);
			ParticleSystem.MainModule partSys = particlesRunning.GetComponent<ParticleSystem> ().main;
			partSys.startSpeedMultiplier = partSys.startSpeedMultiplier * SpeedMultiplier;
			Destroy (particlesRunning, 1f);
		}

		yield return new WaitForSeconds (0.2f / SpeedMultiplier);
		rigidbody.velocity = Vector3.zero;
		rigidbody.AddForce (Player.transform.up * movementScript.jumpBaseForce * (-2), ForceMode.VelocityChange);

		while (!movementScript.isGrounded) {
			yield return null;
		}

		movementScript.velocityLimitXZ = 0.0f;
		boxCollider.size = new Vector3 (swordCPU.boxColliderOrigSize.x, swordCPU.boxColliderOrigSize.y * 5f, swordCPU.boxColliderOrigSize.z);
		boxCollider.center = new Vector3 (swordCPU.boxColliderOrigCenter.x, swordCPU.boxColliderOrigCenter.y * (-15f), swordCPU.boxColliderOrigCenter.z);
		yield return new WaitForSeconds (0.25f / SpeedMultiplier);
		rigidbody.velocity = Vector3.zero;
		//playerAnimation.meleeAttack = 0f;

		//playerAnimation.attackLevel = 0f;
		//playerAnimation.isAttackingMelee = false;
		boxCollider.size = swordCPU.boxColliderOrigSize;
		boxCollider.center = swordCPU.boxColliderOrigCenter;
		swordCPU.attackDamage = swordCPU.baseDamage;
		yield return new WaitForSeconds (0.15f / SpeedMultiplier);

		animationTimer = 0.8f;
		while (animationTimer > 0f) {
			movementScript.doNotMove = true;
			rigidbody.velocity = Vector3.zero;
			if (TerminateAnimation ()) {
				animationTimer -= Time.deltaTime;
				yield return new WaitForEndOfFrame ();
			} else
				break;
		}

		rigidbody.velocity = Vector3.zero;
		movementScript.cantShoot = false;
		movementScript.cantStab = false;
		playerAnimation.isJumping = false;
		playerAnimation.isAttackingMelee = false;
		yield return new WaitForSeconds (0.2f / SpeedMultiplier);
		StopCoroutine (doNotFuckingMove);
		movementScript.doNotMove = false;
		swordCPU.AttackDone ();
	}

	IEnumerator DoNotFuckingMove(){
		while (true) {
			movementScript.doNotMove = true;
			yield return null;
		}
	}
}
