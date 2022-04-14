using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class NGS_NewCPU : MonoBehaviour {

	public bool ownerIsAI;
	private bool awoke = false;

	[HideInInspector] public Vector3 _defaultPosition;
	[HideInInspector] public Quaternion _defaultRotation;
	[HideInInspector] public Vector3 _defaultScale;

	[HideInInspector] public Hero_Movement movementScript;
	[HideInInspector] public Player_Animation animationScript;
	[HideInInspector] public HealthBar healthBarScript;
	[HideInInspector] public References references;
	public NGS_AttackClass currentAttack;
	public NGS_AttackClass currentQueuedAttack;
	public NGS_AttackClass tempCurrentQueuedAttack;
	private NGS_AttackClass prevCheckedAttack;
	public List <NGS_AttackClass> attacksInputed;
	public List <NGS_AttackClass> attacksQueue;
	[HideInInspector] public BoxCollider weaponBoxCollider;
	[HideInInspector] public MeleeWeaponTrail meleeWeaponTrail;
	[HideInInspector] public MeleeWeaponTrailDistortion meleeWeaponTrailDistortion;
	[HideInInspector] public Coroutine CoroSpecialCororunning;

	public GameObject Player;
	public string weaponName = "Generic Sword";
	public float weaponType = 2f;
	public float attackDamage = 2f;
	[HideInInspector] public float baseDamage = 2f;
	private float backupBaseDamage = 2f;
	public float speedMultiplier = 1f;

	public AudioClip swordSwing;
	public AudioClip swordSlash;
	public AudioClip swordClash;
	public AudioClip onEnableSound;
	public GameObject onEnableParticles;

	public Color SwordSparkColor;
	public GameObject SwordSpark;
	public GameObject GenericHit;

	public bool AttackBeingExecuted;
	public bool AttackStartedThisFrame;

	public List<Collider> targetsSlashed;
	public bool damageCol = false;
	public bool blocking = false;

	public bool swordCanCrash = false;
	public bool unstoppable = false;

	public bool deactivateAllBehavior = false;

	public Vector3 boxColliderOrigSize;
	public Vector3 boxColliderOrigCenter;

	private Coroutine runningCoroutine;
	private bool waitingForInputs;
	private bool initialized;
	private bool isInventory;
	private bool coroSloMoColFx;
	private bool coroHitDelay;
	public bool canCancel = false;
	private float cancelTimer = 0.1f;
	private bool coroSloMoCameraFx;
	private float coroSloMoCooldown;
	private float clrearQueueTimer;
	private MonoBehaviour[] scriptsList;
	private Transform _base;
	private Transform _tip;
	private Vector3 _oldTipPos;
	private Vector3 _oldBasePos;
	private RaycastHit hitInfo;

	public LayerMask targetLayers;

	private PostProcessingProfile origProfile;

	[HideInInspector] public bool cancelInAirCheck;
	private bool runningAirCheck;

	public void Awake(){
		if (!awoke) {
			_defaultPosition = transform.position;
			_defaultRotation = transform.rotation;
			_defaultScale = transform.localScale;
		}
	}

	// Use this for initialization
	void Start () {

		gameObject.layer = 9;
		weaponBoxCollider = GetComponent<BoxCollider>();

		if (!ownerIsAI) {
			scriptsList = GetComponents<MonoBehaviour>();

			if (transform.parent && transform.parent.gameObject.name != "Inventory") {
				isInventory = false;

				meleeWeaponTrail = transform.parent.GetComponent<MeleeWeaponTrail>();
				meleeWeaponTrailDistortion = transform.parent.GetComponent<MeleeWeaponTrailDistortion>();

				_tip = transform.FindChildIncludingDeactivated("Tip");
				_base = transform.FindChildIncludingDeactivated("Base");

				meleeWeaponTrail._tip = _tip;
				meleeWeaponTrail._base = _base;
				meleeWeaponTrailDistortion._tip = _tip;
				meleeWeaponTrailDistortion._base = _base;

				transform.localPosition = _defaultPosition;
				transform.localRotation = _defaultRotation;
				transform.localScale = _defaultScale;

				foreach (MonoBehaviour attack in scriptsList) {
					attack.enabled = true;
				}
				if (Player == null)
					Player = transform.parent.parent.parent.parent.parent.parent.parent.parent.parent.parent.gameObject as GameObject;
				movementScript = Player.GetComponent<Hero_Movement>();
				animationScript = Player.GetComponent<Player_Animation>();
				healthBarScript = Player.GetComponent<Player_HealthBar>();
				references = Player.GetComponent<References>();

				animationScript.SpeedMultiplierB = speedMultiplier;

				animationScript.meleeWeaponType = weaponType;
				boxColliderOrigSize = GetComponent<BoxCollider>().size;
				boxColliderOrigCenter = GetComponent<BoxCollider>().center;

				baseDamage = attackDamage;
				backupBaseDamage = baseDamage;

				healthBarScript.SwordSpark = SwordSpark;
				healthBarScript.SwordSparkColor = SwordSparkColor;

				if (onEnableSound != null) {
					AudioSource sound = CustomMethods.PlayClipAt(onEnableSound, transform.position);
					sound.transform.parent = this.transform;
					sound.volume = 0.8f;
				}

				if (!onEnableParticles && transform.Find("WeaponSpawnP")) {
					onEnableParticles = transform.Find("WeaponSpawnP").gameObject;
				} else if (onEnableParticles) {
					onEnableParticles.GetComponent<ParticleSystem>().Play();
				}

				if (Player.GetComponent<References>().Camera.GetComponent<PostProcessingBehaviour>() && Player.GetComponent<References>().Camera.GetComponent<PostProcessingBehaviour>().profile)
					origProfile = Player.GetComponent<References>().Camera.GetComponent<PostProcessingBehaviour>().profile;
			} else if (transform.parent && transform.parent.gameObject.name == "Inventory") {
				isInventory = true;
				gameObject.layer = 1;

				transform.position = Vector3.one * 1000f;
				transform.localRotation = _defaultRotation;
				transform.localScale = Vector3.one * (-1000f);

				foreach (MonoBehaviour attack in scriptsList) {
					if (attack != this)
						attack.enabled = false;
				}
			}
			AttackBeingExecuted = false;
			initialized = true;
		}
	}

	public void StartupAI(GameObject weaponUser){
		Player = weaponUser;
		gameObject.layer = 9;
		animationScript = Player.GetComponent<Player_Animation> ();
		healthBarScript = Player.GetComponent<HealthBar> ();
		references = Player.GetComponent<References> ();

		animationScript.meleeWeaponType = weaponType;
		boxColliderOrigSize = GetComponent<BoxCollider> ().size;
		boxColliderOrigCenter = GetComponent<BoxCollider> ().center;

		baseDamage = attackDamage;
		backupBaseDamage = baseDamage;

		animationScript.SpeedMultiplierB = speedMultiplier;

		if (transform.FindChildIncludingDeactivated ("Tip") && transform.parent && transform.parent.GetComponent<MeleeWeaponTrail> ()) {
			isInventory = false;

			meleeWeaponTrail = transform.parent.GetComponent<MeleeWeaponTrail>();
			meleeWeaponTrailDistortion = transform.parent.GetComponent<MeleeWeaponTrailDistortion>();

			_tip = transform.FindChildIncludingDeactivated("Tip");
			_base = transform.FindChildIncludingDeactivated("Base");

			meleeWeaponTrail._tip = _tip;
			meleeWeaponTrail._base = _base;
			meleeWeaponTrailDistortion._tip = _tip;
			meleeWeaponTrailDistortion._base = _base;
		} else
			isInventory = true;

		initialized = true;
	}

	public void Reinitialize(){
		Start ();
	}
		
	void OnEnable() {
		Start ();
		deactivateAllBehavior = false;
	}

	void OnDisable() {
		if (!this.enabled)	// this.enabled is true when the game object is about to be destroyed (unless the component owned by said game object was previously disabled via enabled=false).
			ForcedReset ();	// this.enabled is false when the object is being disabled but not about to get destroyed.
							//In general, checking the state of the enabled flag upon receiving OnDisable is not sufficient to determine whether a game object is about to be destroyed. For example, the same situation (OnDisable is messaged and this.enabled returns true) will occur when the game object is directly or indirectly deactivated.
							//Note: OnDisable is called before OnDestroy. https://answers.unity.com/questions/882428/ondisable-getting-called-from-destroy.html
	}

	void LateUpdate () {
		AttackStartedThisFrame = false;
		if (!deactivateAllBehavior) {
			if (damageCol) {
				int origLayer = Player.layer;
				Player.layer = 2;
				RaycastHit[] collidedTargets;

				if (Physics.Raycast (_oldTipPos, (_base.transform.position - _oldTipPos).normalized, out hitInfo, Vector3.Distance (_oldTipPos, _base.position), targetLayers, QueryTriggerInteraction.Ignore)) {
					collidedTargets = Physics.RaycastAll (_oldTipPos, (_base.transform.position - _oldTipPos).normalized, Vector3.Distance (_oldTipPos, _base.position), targetLayers, QueryTriggerInteraction.Ignore);
					foreach (RaycastHit collidedTarget in collidedTargets) {
						OnTriggerEnter (collidedTarget.collider);
					}
				}

				else if (Physics.Raycast (_tip.position, (_oldBasePos - _tip.transform.position).normalized, out hitInfo, Vector3.Distance (_oldBasePos, _tip.position), targetLayers, QueryTriggerInteraction.Ignore)) {
					collidedTargets = Physics.RaycastAll (_tip.position, (_oldBasePos - _tip.transform.position).normalized, Vector3.Distance (_oldBasePos, _tip.position), targetLayers, QueryTriggerInteraction.Ignore);
					foreach (RaycastHit collidedTarget in collidedTargets) {
						OnTriggerEnter (collidedTarget.collider);
					}
				}

				else if (Physics.Raycast (_oldBasePos, (_base.transform.position - _oldBasePos).normalized, out hitInfo, Vector3.Distance (_oldBasePos, _base.position), targetLayers, QueryTriggerInteraction.Ignore)) {
					collidedTargets = Physics.RaycastAll (_oldBasePos, (_base.transform.position - _oldBasePos).normalized, Vector3.Distance (_oldBasePos, _base.position), targetLayers, QueryTriggerInteraction.Ignore);
					foreach (RaycastHit collidedTarget in collidedTargets) {
						OnTriggerEnter (collidedTarget.collider);
					}
				}

				else if (Physics.Raycast (_oldTipPos, (_tip.transform.position - _oldTipPos).normalized, out hitInfo, Vector3.Distance (_oldTipPos, _tip.position), targetLayers, QueryTriggerInteraction.Ignore)) {
					collidedTargets = Physics.RaycastAll (_oldTipPos, (_tip.transform.position - _oldTipPos).normalized, Vector3.Distance (_oldTipPos, _tip.position), targetLayers, QueryTriggerInteraction.Ignore);
					foreach (RaycastHit collidedTarget in collidedTargets) {
						OnTriggerEnter (collidedTarget.collider);
					}
				}

				else if (Physics.Raycast (_tip.position, (Player.transform.position - _tip.position).normalized, out hitInfo, Vector3.Distance (Player.transform.position, _base.position), targetLayers, QueryTriggerInteraction.Ignore)) {
					collidedTargets = Physics.RaycastAll (_tip.position, (Player.transform.position - _tip.position).normalized, Vector3.Distance (Player.transform.position, _base.position), targetLayers, QueryTriggerInteraction.Ignore);
					foreach (RaycastHit collidedTarget in collidedTargets) {
						OnTriggerEnter (collidedTarget.collider);
					}
				}
				Player.layer = origLayer;
			}

			if (initialized && !ownerIsAI && !isInventory) {
				if (movementScript.wasFalling && movementScript.isGrounded/* && AttackBeingExecuted*/ && currentAttack.resetOnLanding) {			
					ForcedReset ();
				}

				if (!healthBarScript.inPain && !healthBarScript.isDead) {
					if (!AttackBeingExecuted && attacksQueue.Count > 0 && !references.triggering)
						ExecuteAttack ();			

					if (attacksQueue.Count > 0) {
						if (clrearQueueTimer >= 0.8f || references.triggering) {
							attacksQueue.Clear ();
							clrearQueueTimer = 0f;
						} else
							clrearQueueTimer += Time.deltaTime / (Time.timeScale + 0.001f);
					}
				} else
					attacksQueue.Clear ();
			
				_oldTipPos = _tip.position;
				_oldBasePos = _base.position;
			}
		}
	}

	void Update() {
		if (Time.deltaTime == 0f) {
			deactivateAllBehavior = true;
		} else
			deactivateAllBehavior = false;

		
		if (!deactivateAllBehavior) {
			/*
			if (initialized && (healthBarScript.inPain || healthBarScript.isDead)) {
				GetComponent<Collider>().enabled = false;
				swordCanCrash = false;
			} else if (isInventory) {
				GetComponent<Collider>().enabled = false;
				swordCanCrash = false;
			}
			*/

			if (coroSloMoCooldown > 0)
				coroSloMoCooldown -= Time.deltaTime;
		}

	}

	public void AttackReady(NGS_AttackClass attack){
		if (!deactivateAllBehavior && !movementScript.isCrouching) {
			if (!healthBarScript.inPain && !healthBarScript.isDead && !references.frameCancel) {
				attacksInputed.Add (attack);
				if (!waitingForInputs)
					StartCoroutine (AttackReady ());
			} else
				ForcedReset ();
		}
	}

	IEnumerator AttackReady (){
		waitingForInputs = true;
		yield return new WaitForEndOfFrame();
		AttackFilter ();
		waitingForInputs = false;		
	}

	void AttackFilter(){
		if (attacksInputed.Count > 1) {
			tempCurrentQueuedAttack = attacksInputed [0];
			foreach (NGS_AttackClass attack in attacksInputed) {
				if (attack.priority > tempCurrentQueuedAttack.priority)
					tempCurrentQueuedAttack = attack;
				attack.ForcedReset ();
			}
			attacksQueue.Add (tempCurrentQueuedAttack);
			attacksInputed = new List<NGS_AttackClass> ();
		} else{
			attacksQueue.Add (attacksInputed [0]);
			attacksInputed = new List<NGS_AttackClass> ();
		}
		tempCurrentQueuedAttack = null;
	}

	void ExecuteAttack(){
		if (!deactivateAllBehavior) {
			if (references.triggered)
				baseDamage = backupBaseDamage * references.damageMultiplier;
			else
				baseDamage = backupBaseDamage;

			AttackBeingExecuted = true;
			currentAttack = attacksQueue [0];
			AttackStartedThisFrame = true;
			runningCoroutine = StartCoroutine (currentAttack.ExecuteAttack ());
		}
	}

	public void AttackDone(){
		if (runningCoroutine != null)
			StopCoroutine (runningCoroutine);
		runningCoroutine = null;
		attacksQueue.Remove (currentAttack);
		AttackBeingExecuted = false;
		ToggleSwordDamage (0);
        references.animationScript.isAttackingMelee = false;
        references.animationScript.isAttackingMeleeAir = false;
    }

	public virtual void AttackCancel(){
		//if (coroSloMoColFx == true) {
			ForcedReset ();
			AttackDone ();
			currentAttack = null;
			currentQueuedAttack = null;
			tempCurrentQueuedAttack = null;
			attacksInputed.Clear ();
			attacksQueue.Clear ();
			animationScript.ResetValues ();
			animationScript.anim.Play ("Combo Displacement");
			ToggleSwordDamage (0);
			foreach (NGS_AttackClass attack in transform) {
				attack.ForcedReset();
				attack.cooldown = 0f;
			}
		//}
	} 

	public virtual void ForcedReset(){
		if (initialized) {
            print("FORCED RESET");
            NGS_AttackClass[] currentAttacks = GetComponents<NGS_AttackClass> ();
			foreach (NGS_AttackClass attack in currentAttacks) {
				attack.ForcedReset ();
				attack.StopAllCoroutines();
				Destroy (attack.particlesRunning);
			}

			if (runningCoroutine != null)
				StopCoroutine (runningCoroutine);
			runningCoroutine = null;

			if (CoroSpecialCororunning != null)
				StopCoroutine (CoroSpecialCororunning);
			CoroSpecialCororunning = null;

			AttackBeingExecuted = false;
			damageCol = false;
			ToggleSwordDamage (0);

			Player.GetComponent<Rigidbody> ().mass = 1f;
			Player.GetComponent<Rigidbody> ().useGravity = true;
			GetComponent<BoxCollider> ().size = boxColliderOrigSize;
			GetComponent<BoxCollider> ().center = boxColliderOrigCenter;
            references.animationScript.isAttackingMelee = false;
            references.animationScript.isAttackingMeleeAir = false;

            if (!ownerIsAI) {
				movementScript.doNotMove = false;
				movementScript.cantStab = false;
				movementScript.cantShoot = false;
				movementScript.isBlocking = false;
				movementScript.velocityLimitXZ = 0;

				animationScript.isAttackingMelee = false;
				animationScript.isAttackingMeleeAir = false;
				animationScript.blocking = false;
				animationScript.blockingBullet = false;
				animationScript.blockingCounter = false;
				animationScript.blockingImpact = false;

				attackDamage = baseDamage;
				attacksQueue.Clear ();
				AttackDone ();
			}
		}
	}

	private void OnTriggerEnter(Collider slashed){
		
		if (!ownerIsAI) {
			if (damageCol &&
			    slashed.gameObject != Player &&
			    !targetsSlashed.Contains (slashed)) {

				if (!canCancel)
					StartCoroutine (AttackCancelWindow ());
				
				targetsSlashed.Add (slashed);
				Physics.Raycast (Player.transform.position, (slashed.transform.position + transform.up * 0.25f - Player.transform.position + transform.up * 0.25f).normalized, out hitInfo, Vector3.Distance (slashed.transform.position, Player.transform.position), LayerMask.GetMask (/*"Player", "Enemy",*/ "Scenario", "Default"));
				if (hitInfo.collider == null || hitInfo.collider.gameObject.GetComponent<HealthBar> ()) {
					HealthBar damage = slashed.gameObject.GetComponent<HealthBar> ();
					CameraShake.Shake (0.025f, 1f / Vector3.Distance (Camera.main.transform.position, transform.position) * attackDamage / 20f);
					StartCoroutine (HitDelay (slashed));

					GameObject GenericHitInstance = Instantiate (GenericHit, transform.position, transform.rotation);
					Vector3 GenericHitPos;
					//FROM ENEMY TO SWORD
					GenericHitInstance.transform.position = Physics.ClosestPoint (damage.gameObject.transform.position,
						GetComponent<Collider>(),
						damage.gameObject.transform.position,
						transform.rotation);					

					/*
					RaycastHit[] collidedTargets;
					collidedTargets = Physics.RaycastAll (_tip.position, (_base.position - _tip.transform.position).normalized, Vector3.Distance (_tip.position, _base.position), targetLayers, QueryTriggerInteraction.Ignore);
					foreach (RaycastHit rayHit in collidedTargets) {
						if (rayHit.collider.gameObject != Player) {
							GenericHitInstance.transform.position = rayHit.point;
							break;
						}				
					}
					*/

					if (damage && !damage.isDead && damage.AddjustCurrentHealth (Player.transform, -attackDamage, currentAttack.stunTime, currentAttack.unstoppable, currentAttack.overthrows)) {
						print ("1.5) overthrows: " + currentAttack.overthrows + " at " + this.GetType().ToString()); 
						if (!coroSloMoColFx && currentAttack.priority > 2 && coroSloMoCooldown <= 0f) {
							StartCoroutine (SlowMotionCollisionEffect ());
							StartCoroutine (SlomoCameraEffect (true));
						} else if (currentAttack.priority > 2 && !coroSloMoCameraFx) {
							StartCoroutine (SlomoCameraEffect (false));
						}

						if (!ownerIsAI && currentAttack.hitSound != null)
							CustomMethods.PlayClipAt (currentAttack.hitSound, transform.position);
						else
							CustomMethods.PlayClipAt (swordSlash, transform.position);
						
						if (currentAttack.SpecialOntriggerenter (slashed)) {
							return;
						} else {
						}
					}
				}
			}
		} else {
			if (damageCol &&
			    slashed.gameObject != Player &&
			    !targetsSlashed.Contains (slashed) &&
			    Player.GetComponent<AI> ()) {
				Physics.Raycast (Player.transform.position, (slashed.transform.position + transform.up * 0.25f - Player.transform.position + transform.up * 0.25f).normalized, out hitInfo, Vector3.Distance (slashed.transform.position, Player.transform.position), LayerMask.GetMask ("Player", "Enemy", "Scenario", "Default"));
				if (hitInfo.collider == null || hitInfo.collider && hitInfo.collider.gameObject.GetComponent<HealthBar> ()) {
					targetsSlashed.Add (slashed);
					Player.GetComponent<AI> ().stateMachine.currentState.OnWeaponTriggerEnter (Player.GetComponent<AI> (), slashed);
				}
			}
		}
	}

	IEnumerator AttackCancelWindow(){
		canCancel = true;
		cancelTimer = 0.075f;
		while (cancelTimer >= 0f) {
			cancelTimer -= Time.deltaTime * Time.timeScale;
			yield return null;
		}
		canCancel = false;
	}

	IEnumerator HitDelay(Collider slashed){
		if (!coroHitDelay /*&& coroSloMoCooldown <= 0f*/) {
			coroHitDelay = true;
			/*
			float currentTimeScale = Time.timeScale;
			float currentFixedDeltaTime = Time.fixedDeltaTime;
			float currentMaximumDelta = Time.maximumDeltaTime;
			Time.timeScale = 0.2f;
			Time.fixedDeltaTime = 0.0025f;
			*/
			yield return new WaitForFixedUpdate ();
			float speedA = references.animationScript.anim.speed;
			float speedB = slashed.gameObject.GetComponent<Animator> ().speed;
			references.animationScript.anim.speed = 0f;
			slashed.gameObject.GetComponent<Animator> ().speed = 0f;
			yield return new WaitForSeconds (0.075f);
			references.animationScript.anim.speed = speedA;
			slashed.gameObject.GetComponent<Animator> ().speed = speedB;
			/*
			Time.timeScale = currentTimeScale;
			Time.fixedDeltaTime = currentFixedDeltaTime;
			Time.maximumDeltaTime = currentMaximumDelta;
			*/
			coroHitDelay = false;
		}
		yield return null;
	}


	public IEnumerator SlowMotionCollisionEffect(){
		if (!coroSloMoColFx && coroSloMoCooldown <= 0f) {
			coroSloMoCooldown = 3.0f;
			coroSloMoColFx = true;

			float currentTimeScale = Time.timeScale;
			float currentFixedDeltaTime = Time.fixedDeltaTime;
			float currentMaximumDelta = Time.maximumDeltaTime;

			Time.timeScale = 0.1f;
			Time.fixedDeltaTime = 0.0025f;
			yield return new WaitForSeconds (0.375f * Time.timeScale);
			Time.timeScale = currentTimeScale;
			Time.fixedDeltaTime = currentFixedDeltaTime;
			Time.maximumDeltaTime = currentMaximumDelta;
			coroSloMoColFx = false;
		}
	}

	public IEnumerator SlomoCameraEffect(bool blurFX){
		coroSloMoCameraFx = true;
		yield return null;
		PostProcessingBehaviour postProcessingBehavior = Player.GetComponent<References> ().Camera.GetComponent<PostProcessingBehaviour> ();
		PostProcessingProfile oldProfile = postProcessingBehavior.profile;
		PostProcessingProfile newProfile = Instantiate(postProcessingBehavior.profile);
		BloomModel.Settings bloomSettings = newProfile.bloom.settings;
		MotionBlurModel.Settings mBSettings = newProfile.motionBlur.settings;

		float startingIntensity = newProfile.bloom.settings.bloom.intensity;
		float startingThreshold = newProfile.bloom.settings.bloom.threshold;
		float finalIntensity = startingIntensity;
		float finalThreshold;

		float startingIntensityMB = newProfile.motionBlur.settings.shutterAngle;
		float finalIntensityMB = startingIntensityMB;
		float startingIntensityFrameBlend = newProfile.motionBlur.settings.frameBlending;
		float finalIntensityFrameBlend = startingIntensityFrameBlend;


		float timer = 0f;
		while (timer < 1f) {
			timer += Time.unscaledDeltaTime / 0.05f;
			bloomSettings.bloom.intensity = Mathf.Lerp (startingIntensity, startingIntensity + (startingIntensity * 1/3), timer * 2f);
			bloomSettings.bloom.threshold = Mathf.Lerp (startingThreshold, 0f, timer * 2f);
			newProfile.bloom.settings = bloomSettings;

			if (blurFX) {
				mBSettings.shutterAngle = Mathf.Lerp (startingIntensityMB, 720f, timer * 4f);
				mBSettings.frameBlending = Mathf.Lerp (startingIntensityMB, 1f, timer * 4f);
				newProfile.motionBlur.settings = mBSettings;
			}

			postProcessingBehavior.profile = newProfile;
			yield return null;

		}

		yield return new WaitForSecondsRealtime (0.1f);

		startingIntensity = newProfile.bloom.settings.bloom.intensity;
		startingThreshold = newProfile.bloom.settings.bloom.threshold;
		finalThreshold = oldProfile.bloom.settings.bloom.threshold;
		finalIntensity = oldProfile.bloom.settings.bloom.intensity;

		startingIntensityMB = newProfile.motionBlur.settings.shutterAngle;
		startingIntensityFrameBlend = newProfile.motionBlur.settings.frameBlending;		
		finalIntensityMB = oldProfile.motionBlur.settings.shutterAngle;
		finalIntensityFrameBlend = oldProfile.motionBlur.settings.frameBlending;

		timer = 0f;
		while (timer < 1f) {
			timer += Time.unscaledDeltaTime / 0.8f;
			bloomSettings.bloom.intensity = Mathf.Lerp (startingIntensity, finalIntensity, timer);
			bloomSettings.bloom.threshold = Mathf.Lerp (startingThreshold, finalThreshold, timer);
			newProfile.bloom.settings = bloomSettings;

			if (blurFX) {
				mBSettings.shutterAngle = Mathf.Lerp (startingIntensityMB, finalIntensityMB, timer);
				mBSettings.frameBlending = Mathf.Lerp (startingIntensityFrameBlend, finalIntensityFrameBlend, timer);
				newProfile.motionBlur.settings = mBSettings;
			}

			postProcessingBehavior.profile = newProfile;
			yield return null;
		}

		postProcessingBehavior.profile = oldProfile;
		yield return null;
		Destroy (newProfile);
		yield return null;
		coroSloMoCameraFx = false;
	}

	public void ToggleSwordDamage(int activated){
		if (activated == 1) {
			_oldTipPos = _tip.position;
			_oldBasePos = _base.position;
			damageCol = true;
			weaponBoxCollider.enabled = true;
			meleeWeaponTrail.Emit = true;
			meleeWeaponTrailDistortion.Emit = true;
			

			if (!ownerIsAI && currentAttack.startSound != null) { 
				CustomMethods.PlayClipAt (currentAttack.startSound, transform.position).transform.SetParent(transform);
			} else { 
				CustomMethods.PlayClipAt (swordSwing, transform.position).transform.SetParent (transform);
			}

			targetsSlashed = new List<Collider>();
		} else {
			damageCol = false;
			weaponBoxCollider.enabled = false;
			if (meleeWeaponTrail) {
				meleeWeaponTrail.Emit = false;
				meleeWeaponTrailDistortion.Emit = false;	
			}
			//meleeWeaponTrail._trailObject.GetComponent<Renderer> ().material = meleeWeaponTrail._material;
			targetsSlashed = new List<Collider>();
		}
	}

	public IEnumerator AirCheck(){
		if (!runningAirCheck) {
			runningAirCheck = true;
			cancelInAirCheck = false;
			while (!movementScript.isGrounded && !cancelInAirCheck) {		
				yield return null;
			}
			if (!cancelInAirCheck) {
				cancelInAirCheck = true;
				ForcedReset ();
				AttackDone ();
			}
			runningAirCheck = false;
		}
		yield break;
	}


	public Vector3 BlockingDirection(){
		if (currentAttack is NGS_SwordBlockA) {
			NGS_SwordBlockA currentAttackSubclass = currentAttack as NGS_SwordBlockA;
			return currentAttackSubclass.blockingDirection;
		} else
			return Player.transform.forward;
	}

	public void ReceivedImpact(Transform attacker){
		if (currentAttack is NGS_SwordBlockA) {
			NGS_SwordBlockA currentAttackSubclass = currentAttack as NGS_SwordBlockA;
			currentAttackSubclass.BlockingImpact (attacker);
		}
	}

	public void ReceivedBullet(Transform shooter, Transform bullet){
		if (currentAttack is NGS_SwordBlockA) {
			NGS_SwordBlockA currentAttackSubclass = currentAttack as NGS_SwordBlockA;
			currentAttackSubclass.BlockingBullet (shooter, bullet);
		}
	}	

	public void ExecuteMethodAttack (string methodName){
		if (currentAttack != null && currentAttack.GetType ().GetMethod (methodName) != null)
			currentAttack.Invoke (methodName, 0f);
		else
			print ("currentAttack is null or does not contain any mehtod called: " + methodName);
	}
}
