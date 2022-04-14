using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class NGS_SwordBlockA : NGS_AttackClass {

	public int attackLevel = 1;
	public float level1FocusRequirement;
	public float level2FocusRequirement;
	public float level3FocusRequirement;

	//The more bools, the less readibility, try to stick with the essentials.
	//If you were to press 10 buttons in a row
	//having 10 booleans to check for those would be confusing

	public Vector3 blockingDirection;
	public bool blockingImpact = false;

	private float inputH;
	private float inputV;
	private bool counteringAttack;
	private float bulletBlockingTimer;
	private float bulletBlockingSlomoTimer = 8f;
	private bool coroBlockingBullet;

	private GameObject cameraGO;
	private Vector3 camFwrd;
	private Vector3 camRight;
	//private Coroutine currentCoroutine = null;


	public GameObject Level1ParticlesFX;
	public GameObject Level2ParticlesFX;
	public GameObject Level3ParticlesFX;

	private Coroutine coroA = null;
	private Coroutine coroB = null;
	private Coroutine coroC = null;

	private bool coroSlomo;
	private float perfectBlock;

	public override void Start(){
		priority = 0;
		if (level2FocusRequirement == 0f)
			level2FocusRequirement = level1FocusRequirement / 1.25f;
		if (level3FocusRequirement == 0f)
			level3FocusRequirement = level1FocusRequirement / 1.5f;
		base.Start ();
		cameraGO = Player.GetComponent<References> ().Camera;
	}

	void Update(){
		Input();
	}

	public override void ForcedReset(){
		if (coroA != null)
			StopCoroutine (coroA);
		if (coroB != null)
			StopCoroutine (coroB);

		if (swordCPU)
			swordCPU.blocking = false;
		blockingImpact = false;
		counteringAttack = false;
		coroBlockingBullet = false;
		bulletBlockingTimer = 0f;
		perfectBlock = 0f;
		blockingDirection = Vector3.zero;
	}

	void Input(){
		if (!swordCPU.AttackBeingExecuted && !swordCPU.blocking && (InputManager.GetButton ("R2") || InputManager.GetAxis ("R2") > 0.5f) &&
			movementScript.isGrounded && playerHealthBarScript.curFocus >= 2f && cooldown <= 0f) {
			swordCPU.AttackReady (this);
		}
	}

	public void BlockingImpact(Transform attacker){
		blockingImpact = true;
		coroA = StartCoroutine (BlockImpact (attacker));
	}

	public void BlockingBullet(Transform shooter, Transform bullet){
		blockingImpact = true;
		bulletBlockingTimer += 0.2f;
		coroA = StartCoroutine (BlockBullet (shooter, bullet));
	}

	public override IEnumerator ExecuteAttack(){
		ForcedReset ();
		playerAnimation.blocking = true;
		movementScript.doNotMove = true;
		yield return new WaitForSeconds (0.025f);
		swordCPU.blocking = true;
		movementScript.isBlocking = true;

		while (movementScript.isGrounded && swordCPU.blocking || blockingImpact) {
			yield return null;
			perfectBlock += Time.deltaTime;

			if (movementScript.isGroundedAndStable)
				rigidbody.velocity = Vector3.zero;

			movementScript.doNotMove = true;
			if (attackLevel == 1)
				playerHealthBarScript.curFocus -= Time.deltaTime * level1FocusRequirement;
			else if (attackLevel == 2)
				playerHealthBarScript.curFocus -= Time.deltaTime * level2FocusRequirement;
			else if (attackLevel == 3)
				playerHealthBarScript.curFocus -= Time.deltaTime * level3FocusRequirement;

			inputH = InputManager.GetAxis ("Horizontal");
			inputV = InputManager.GetAxis ("Vertical");

			camFwrd = cameraGO.transform.forward;
			camRight = cameraGO.transform.right;
			camFwrd.y = 0f;
			camRight.y = 0f;

			blockingDirection = (camFwrd * inputV + camRight * inputH).normalized;

			if (blockingDirection.magnitude <= 0.01f)
				blockingDirection = Player.transform.forward;

			playerAnimation.blockingDirection = blockingDirection;

			if (counteringAttack) {
				yield break;
			}

			if ((!InputManager.GetButton ("R2") && InputManager.GetAxis ("R2") <= 0f) || playerHealthBarScript.curFocus <= 0f) {
				swordCPU.blocking = false;
				movementScript.isBlocking = false;
			}
		}
			
		ForcedReset ();
		blockingDirection = Vector3.zero;
		playerAnimation.blocking = false;
		swordCPU.blocking = false;
		yield return new WaitForSeconds (0.05f);
		movementScript.doNotMove = false;
		swordCPU.attacksQueue.Clear ();
		swordCPU.AttackDone ();
	}

	IEnumerator BlockImpact(Transform attacker){
		if (perfectBlock < 0.2f) {
			print ("perfect block!!!");
			if (attackLevel == 1)
				playerHealthBarScript.curFocus += Time.deltaTime * level1FocusRequirement * 10f;
			else if (attackLevel == 2)
				playerHealthBarScript.curFocus += Time.deltaTime * level2FocusRequirement * 10f;
			else if (attackLevel == 3)
				playerHealthBarScript.curFocus += Time.deltaTime * level3FocusRequirement * 10f;
		} else {
			if (attackLevel == 1)
				playerHealthBarScript.curFocus += Time.deltaTime * level1FocusRequirement * 10f;
			else if (attackLevel == 2)
				playerHealthBarScript.curFocus += Time.deltaTime * level2FocusRequirement * 10f;
			else if (attackLevel == 3)
				playerHealthBarScript.curFocus += Time.deltaTime * level3FocusRequirement * 10f;
		}

		perfectBlock = 0f;
		playerAnimation.blockingImpact = true;
		//CustomMethods.SmoothRotateTowards (Player, attacker, 0.2f);
		if (attacker.GetComponent<References> ().RightHandWeapon) {
			if (attacker.GetComponent<AI> ())
				attacker.GetComponent<AI> ().StartSwordCrashState (Player.transform);
		}
		if (swordCPU.SwordSpark != null) {
			GameObject ImpactSpark = Instantiate (swordCPU.SwordSpark, Player.transform.position + Player.transform.forward * 0.35f + Vector3.up * 0.5f, Player.transform.rotation);
			ImpactSpark.GetComponent<Renderer> ().material.SetColor ("_Color", swordCPU.SwordSparkColor);
		}

		swordCPU.references.Camera.GetComponent<MorePPEffects.RadialBlur> ().RadialBlurFx (0.35f, 1f);
		CustomMethods.PlayClipAt (swordCPU.swordClash, transform.position);
		rigidbody.AddForce ((Player.transform.position - attacker.position).normalized * 3f, ForceMode.VelocityChange);			
		yield return new WaitForSeconds (0.1f);
		playerAnimation.blockingImpact = false;

		if (playerHealthBarScript.curFocus < -0.99f) {
			movementScript.doNotMove = true;
			blockingImpact = false;
			Player.GetComponent<Animator> ().Play ("Pain", -1, normalizedTime: 0.0f);
			yield return new WaitForSeconds (0.65f);
			movementScript.doNotMove = false;
			yield break;
		}

		if (!InputManager.GetButton ("Melee")) {
			yield return new WaitForEndOfFrame ();
			float counterAttackTimer = 0f;
			while (counterAttackTimer < 0.275f) {
				counterAttackTimer += Time.deltaTime;
				if (InputManager.GetButton ("Melee") && (!InputManager.GetButton ("R2") && InputManager.GetAxis ("R2") <= 0f)) {
					//swordCPU.ForcedReset ();
					coroB = StartCoroutine (CounterAttack (attacker));
					yield break;
				}
				yield return null;
			}
		} else {
			yield return new WaitForSeconds (0.35f);
			blockingImpact = false;
			//swordCPU.AttackDone ();
			yield break;
		}
		yield return new WaitForSeconds (0.2f);
		blockingImpact = false;
		//swordCPU.AttackDone ();
	}

	IEnumerator CounterAttack(Transform attacker){
		counteringAttack = true;		
		blockingImpact = false;
		swordCPU.blocking = false;
		playerAnimation.blockingCounter = true;
		playerAnimation.blocking = true;
		playerAnimation.blockingImpact = false;
		boxCollider.size = new Vector3 (boxCollider.size.x, boxCollider.size.y, boxCollider.size.z * 1.2f);
		Player.GetComponent<References> ().AutoAim.GetComponent<EnemiesDetector> ().target = attacker.gameObject;
		StartCoroutine (CustomMethods.SmoothRotateTowards (Player, attacker, 0.1f));

		movementScript.cantShoot = true;
		movementScript.cantStab = true;
		movementScript.doNotMove = true;
		rigidbody.velocity = Vector3.zero;

		yield return new WaitForEndOfFrame ();

		playerAnimation.blockingCounter = true;
		swordCPU.attackDamage = swordCPU.baseDamage * 2.5f;
		meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialBlue;

		yield return new WaitForSeconds (0.15f);
		boxCollider.size = swordCPU.boxColliderOrigSize;
		rigidbody.AddForce (Player.transform.forward * 3f, ForceMode.VelocityChange);

		yield return new WaitForSeconds (0.2f);
		//yield return new WaitForSeconds (0.2f);
		animationTimer = 2f;
		while (animationTimer > 0f) {
			if (TerminateAnimation ()) {
				animationTimer -= Time.deltaTime;
				yield return new WaitForEndOfFrame ();
			} else
				break;
		}
		yield return new WaitForFixedUpdate();
		swordCPU.attackDamage = swordCPU.baseDamage;
		movementScript.cantShoot = false;
		movementScript.cantStab = false;
		movementScript.doNotMove = false;
		playerAnimation.blockingCounter = false;
		playerAnimation.blocking = false;
		rigidbody.velocity = Vector3.zero;
		yield return new WaitForEndOfFrame ();
		counteringAttack = false;
		swordCPU.attacksQueue.Clear ();
		swordCPU.AttackDone ();
	}


	IEnumerator BlockBullet(Transform shooter, Transform bullet){
		if (attackLevel == 1) {
			playerHealthBarScript.curFocus += Time.deltaTime * level1FocusRequirement * 1f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
		} else if (attackLevel == 2) {
			playerHealthBarScript.curFocus += Time.deltaTime * level2FocusRequirement * 2f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialBlue;
		} else if (attackLevel == 3) {
			playerHealthBarScript.curFocus += Time.deltaTime * level3FocusRequirement * 3f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._materialRed;
		}

		meleeWeaponTrail.Emit = true;
		meleeWeaponTrailDistortion.Emit = true;

		playerAnimation.blockingBullet = true;
		CustomMethods.SmoothRotateTowards (Player, shooter, 0.2f);
		if (swordCPU.SwordSpark != null) {
			GameObject bulletImpactSpark = Instantiate (swordCPU.SwordSpark, transform.position + transform.rotation * new Vector3 (Random.Range (-0.75f, 0.75f), Random.Range (-0.5f, 1.25f), Random.Range (1.0f, 1.5f)), transform.rotation);
			bulletImpactSpark.transform.localScale = bulletImpactSpark.transform.localScale / 2f;
			bulletImpactSpark.GetComponent<Renderer> ().material.SetColor ("_Color", swordCPU.SwordSparkColor);
		}

		if (coroC != null)
			StopCoroutine (coroC);

		StartCoroutine (CustomMethods.SmoothRotateTowards (Player, shooter, 0.1f));
		coroC = StartCoroutine (BulletRicochet (bullet.gameObject));
		StartCoroutine (CustomMethods.CameraShake (Player.transform.position, 1f, 0.125f));

		if (!coroBlockingBullet) {
			bulletBlockingTimer += 0.1f;
			coroBlockingBullet = true;
			CustomMethods.PlayClipAt (swordCPU.swordClash, transform.position);
			while (bulletBlockingTimer >= 0) {
				bulletBlockingTimer -= Time.deltaTime;
				yield return null;
			}
			meleeWeaponTrail.Emit = false;
			meleeWeaponTrailDistortion.Emit = false;
			playerAnimation.blockingBullet = false;
			blockingImpact = false;
			coroBlockingBullet = false;
		}
		yield return null;
	}

	IEnumerator BulletRicochet (GameObject bullet){
		float timer = 0.2f;
		if ((!InputManager.GetButton ("Fire") && InputManager.GetAxis ("Fire") <= 0f)) {
			yield return null;
			while (timer > 0) {
				if (InputManager.GetButtonDown ("Fire") || InputManager.GetAxis ("Fire") > 0.5f) {
					StartCoroutine (SlomoRicochet ());

					if (attackLevel == 1)
						playerHealthBarScript.curFocus += Time.deltaTime * level1FocusRequirement * 10;
					else if (attackLevel == 2)
						playerHealthBarScript.curFocus += Time.deltaTime * level2FocusRequirement * 10;
					else if (attackLevel == 3)
						playerHealthBarScript.curFocus += Time.deltaTime * level3FocusRequirement * 10;

					GameObject bulletInstance = (GameObject)Instantiate (bullet, bullet.transform.position, Quaternion.identity);
					bulletInstance.transform.position = bullet.transform.position;
					bulletInstance.transform.rotation = Quaternion.LookRotation (Vector3.Reflect (bullet.transform.forward, (Player.transform.forward).normalized));
			
					bulletInstance.GetComponent<BulletBehavior> ().shooter = Player.transform;
					bulletInstance.GetComponent<BulletBehavior> ().baseDamage = bulletInstance.GetComponent<BulletBehavior> ().baseDamage * 2f;
					bulletInstance.GetComponent<BulletBehavior> ().spread = Random.Range (5f, 9f);
					bulletInstance.GetComponent<BulletBehavior> ().speed = bulletInstance.GetComponent<BulletBehavior> ().speed * 3 / 4;
					bulletInstance.GetComponent<BulletBehavior> ().isRipper = false;
					bulletInstance.transform.GetChild(0).GetComponent<TrailRenderer> ().time = 0.15f;
			
					Physics.IgnoreCollision (bulletInstance.transform.GetComponent<Collider> (), Player.transform.GetComponent<Collider> ());
					yield return null;
					bulletInstance.SetActive (true);
					yield return null;
					break;
				}
				timer -= Time.deltaTime;
				yield return null;
			}
		}
		yield return null;
	}

	IEnumerator SlomoRicochet(){
		if (!coroSlomo) {
			coroSlomo = true;
			float currentTimeScale = Time.timeScale;
			float currentFixedDeltaTime = Time.fixedDeltaTime;
			float currentMaximumDelta = Time.maximumDeltaTime;

			Time.timeScale = 0.1f;
			Time.fixedDeltaTime = 0.0025f;
			yield return new WaitForSeconds (0.5f * Time.timeScale);
			Time.timeScale = currentTimeScale;
			Time.fixedDeltaTime = currentFixedDeltaTime;
			Time.maximumDeltaTime = currentMaximumDelta;
			coroSlomo = false;
		}
	}
}