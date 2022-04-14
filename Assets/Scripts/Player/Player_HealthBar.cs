using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using MorePPEffects;
using Luminosity.IO;

public class Player_HealthBar : HealthBar {

	public playerGUI playerGUI;
	public GameObject cameraObject;
	private NavMeshAgent navMeshAgent;
	private Coroutine currentPainCoro;

	private AudioClip swordClash;

	private MonoBehaviour[] playerScripts;
	private GameObject currentSword;

	private Headache headacheEffect;
	private bool coroHeadache;

	private float HealthBarLength;

	private bool swordsCrashed = false;
	private bool coroClashed = false;

	private float counterAttackTimer;
	private float bulletBlockingTimer;
	private bool coroBlockBullet;
	private bool coroRunning;


	// Use this for initialization
	void Start () {
		references = GetComponent<References> ();
		if (references.Camera.GetComponent<Headache> ()) {
			headacheEffect = references.Camera.GetComponent<Headache> ();
			headacheEffect.enabled = false;
		}

		animationScript = GetComponent<Player_Animation> ();
		rigidbody = GetComponent<Rigidbody> ();
		transform = GetComponent<Transform> ();
		playerGUI = GetComponent<playerGUI> ();
		HealthBarLength = Screen.width / 3;
		playerScripts = GetComponents<MonoBehaviour> ();
		cameraObject = GetComponent<References> ().Camera;
		rigidbody = GetComponent<Rigidbody> ();
		navMeshAgent = GetComponent<NavMeshAgent> ();
	}

	void HealOverTime (){
		if (!isDead && CurHealth < Maxhealth) {
			if (references.triggered)
				AddjustCurrentHealth (this.transform, healAmountPerSecond * 2f * Time.deltaTime, 0f, true, false);
			else
				AddjustCurrentHealth (this.transform, healAmountPerSecond * Time.deltaTime, 0f, true, false);
		}
	}

	public override void ResetValues(){
		GetComponent<CapsuleCollider> ().radius = references.capsuleColliderRadius;
		references.animationScript.isStandingUp = false;
		inPain = false;
		coroClashed = false;
		swordsCrashed = false;
		if (currentPainCoro != null)
			StopCoroutine (currentPainCoro);
	}
	
	// Update is called once per frame
	void Update () {	

		HealOverTime ();

		if (cheatGodMode) {
			godMode = true;
			cantBeHitMode = true;
		} else {
			godMode = false;
		}

//		AddjustCurrentHealth(0);

		playerGUI.isDead = isDead;
		playerGUI.MaxHealth = Maxhealth;
		playerGUI.CurHealth = CurHealth;
		playerGUI.HealthBarLength = HealthBarLength;

		if (!isDead && CurHealth <= 0){
			isDead = true;
			StartCoroutine (Death ());
		}

		if (curFocus < maxFocus && !GetComponent<Hero_Movement> ().isBlocking) {
			curFocus += Time.deltaTime;
		}

		/*
		if (curFocus < 0)
			curFocus = 0f;
		*/

		if (CurHealth > Maxhealth)
			CurHealth = Maxhealth;
		HealthBarLength = (Screen.width / 3) * (CurHealth / Maxhealth);

	}

/*
	void OnGUI () {
		if (!isDead)
			GUI.Box (new Rect(10, 10, HealthBarLength, 20), CurHealth.ToString("F0") + "/" + Maxhealth );
 	}
*/

	public override bool AddjustCurrentHealth (Transform attacker, float adj, float stunTime, bool unstoppable, bool overthrows) {
		if (godMode) {
			if (adj < 0)
				adj = 0;
		}

		if (!cantBeHitMode && adj < 0 && DiplomacyManager.AreEnemies (references.team, attacker.GetComponent<References> ().team)) {

			adj = adj * PlayerPrefs.GetInt ("Difficulty", 2)/2;

			Vector3 attackerDir = Vector3.zero;
			float direction = 0f;

			if (references.RightHandWeapon != null && references.RightHandWeapon.GetComponent<NGS_NewCPU> ()) {
				attackerDir = (transform.position - attacker.position).normalized;
				direction = Vector3.Dot (references.RightHandWeapon.GetComponent<NGS_NewCPU> ().BlockingDirection (), attackerDir);
			}

			if (!unstoppable && attacker.GetComponent<AI> () && references.RightHandWeapon != null && references.RightHandWeapon.GetComponent<NGS_NewCPU> () && references.RightHandWeapon.GetComponent<NGS_NewCPU> ().blocking && direction < -0.25f) {
				references.RightHandWeapon.GetComponent<NGS_NewCPU> ().ReceivedImpact (attacker);
				//StartCoroutine (attacker.GetComponent<HealthBar> ().SwordCrash ()); //Queda a cargo del script de bloqueo, que haga lo que quiera con el atacante.
				return false;
	
			} else if (!unstoppable && Vector3.Distance (transform.position, attacker.position) < 2.5f && references.RightHandWeapon != null && references.RightHandWeapon.GetComponent<NGS_NewCPU> () && references.RightHandWeapon.GetComponent<NGS_NewCPU> ().damageCol &&
				//La pregunta por la distancia es temporal, hasta encontrar una forma de distinguir entre projectiles (no balas) y melee
			           Vector3.Dot ((transform.position - attacker.transform.position).normalized, transform.forward) < -0.6f) {
				if (references.RightHandWeapon.GetComponent<NGS_NewCPU>())
					references.RightHandWeapon.GetComponent<NGS_NewCPU>().ForcedReset();
				StartCoroutine (SwordCrash (attacker));
				if (attacker.GetComponent<AI_HealthBar> ())
					attacker.GetComponent<AI_HealthBar> ().StartSwordCrashState (this.transform);
				return false;
			} else {
				CurHealth += adj;
				BloodSplatObject = Instantiate (BloodSplatReference, transform.position, transform.rotation, gameObject.transform);
				Vector3 bloodsplatPos;
				//FROM SWORD TO ENEMY
				if (attacker.GetComponent<References> ().RightHandWeapon) {
					bloodsplatPos = Physics.ClosestPoint (attacker.GetComponent<References> ().RightHandWeapon.transform.position,
						GetComponent<CapsuleCollider> (),
						transform.position,
						transform.rotation);
				} else {
					bloodsplatPos = Physics.ClosestPoint (attacker.transform.position,
						GetComponent<CapsuleCollider> (),
						transform.position,
						transform.rotation);
				}

				if (references.RightHandWeapon.GetComponent<NGS_NewCPU> ())
					references.RightHandWeapon.GetComponent<NGS_NewCPU> ().ForcedReset ();					

				BloodSplatObject.transform.SetParent (this.gameObject);
				BloodSplatObject.transform.position = bloodsplatPos;
				BloodSplatObject.transform.rotation = Quaternion.LookRotation (bloodsplatPos - transform.position);

				animationScript.damageIntensity = painTimer;
				rigidbody.velocity = Vector3.zero;

				if (GetComponent<Hero_Movement> ().isGrounded)
					rigidbody.AddForce ((transform.position - attacker.transform.position).normalized * 3f, ForceMode.VelocityChange);
				else {
					rigidbody.AddForce (Vector3.up * 2f, ForceMode.VelocityChange);
					rigidbody.AddForce ((transform.position - attacker.transform.position).normalized * 3f, ForceMode.VelocityChange);
				}

				int chance = Random.Range (0, 100);	
				if (!isDead && chance < painChance) {
					headacheEffect.strength = -adj * (Random.Range (0.25f, 0.5f) / 2f);
					//if (!inPain)
					if (currentPainCoro != null)
						StopCoroutine (currentPainCoro);
					
					currentPainCoro = StartCoroutine (Pain (attacker, stunTime, overthrows));
					if (headacheEffect && !coroHeadache) {
						StartCoroutine (HeadacheEffect ());
					}
				}
				return true;
			}
		} else if (adj > 0f && !cantBeHitMode) {
			CurHealth += adj;
			return true;
		} else if (cantBeHitMode) {
			StartCoroutine (SlowMotionEffect(0.5f));
			return false;
		} else {
			return false;
		}
	} 

	public override bool BulletDamage(Vector3 hitPoint, Transform shooter, Transform bullet, float adj) {

		if (godMode) {
			if (adj < 0)
				adj = 0;
		}

		if (!cantBeHitMode && adj < 0f && DiplomacyManager.AreEnemies(references.team, shooter.GetComponent<References>().team)) {
			if (adj < 0 && references.RightHandWeapon.GetComponent<NGS_NewCPU> () && references.RightHandWeapon.GetComponent<NGS_NewCPU> ().blocking) {
				Vector3 bulletDir = (transform.position - shooter.position).normalized;
				bulletDir.y = 0f;
				float direction = Vector3.Dot (references.RightHandWeapon.GetComponent<NGS_NewCPU> ().BlockingDirection(), bulletDir);

				if (direction < -0.25f) {
					references.RightHandWeapon.GetComponent<NGS_NewCPU> ().ReceivedBullet (shooter, bullet);
					return false;
				}
			}

			if (adj < 0) {
				BloodSplatObject = Instantiate (BloodSplatObjectBullet, transform.position, transform.rotation, gameObject.transform);
				//FROM BULLET TO ENEMY
				/*Vector3 bloodsplatPos = Physics.ClosestPoint (shooter.position,
					                        GetComponent<CapsuleCollider> (),
					                        transform.position,
					                        transform.rotation);
					                        */
				BloodSplatObject.transform.SetParent (this.gameObject);
				BloodSplatObject.transform.position = bullet.position;
				BloodSplatObject.transform.localScale = BloodSplatObject.transform.localScale * 3 / 5;
				BloodSplatObject.transform.rotation = Quaternion.LookRotation (bullet.position - transform.position);
				rigidbody.AddForce ((transform.position - shooter.position).normalized * 0.1f, ForceMode.VelocityChange);
				rigidbody.velocity = Vector3.zero;
				rigidbody.AddForce ((transform.position - shooter.position).normalized * 0.1f, ForceMode.VelocityChange);

				int chance = Random.Range (0, 100);	
				if (!isDead && chance < painChance) {
					headacheEffect.strength = -adj * (Random.Range (0.1f, 0.25f)) / 2f;
					//if (!inPain)
					if (currentPainCoro != null)
						StopCoroutine (currentPainCoro);
					currentPainCoro = StartCoroutine (BulletPain ());
					if (headacheEffect && !coroHeadache) {
						StartCoroutine (HeadacheEffect ());
					}
				}
			}
			CurHealth += adj;
			return true;
		} else if (adj > 0f && !cantBeHitMode) {
			CurHealth += adj;
			return true;
		} else if (cantBeHitMode) {
			StartCoroutine (SlowMotionEffect(0.5f));
			return false;
		}
		else
			return false;
	}



	IEnumerator Pain(Transform attacker, float stunTime, bool overthrows){
		//if (!inPain) {
			inPain = true;
			//Stop Attack if attacking
			if (references.RightHandWeapon.tag != "Fists" && references.RightHandWeapon.GetComponent<NGS_NewCPU> ())
				references.RightHandWeapon.GetComponent<NGS_NewCPU> ().ForcedReset ();
		GetComponent<Hero_Movement> ().ResetStates();
			GetComponent<Hero_Movement> ().doNotMove = true;
			GetComponent<Player_Animation> ().inPain = true;
			GetComponent<Hero_Movement> ().cantStab = true;
			GetComponent<Hero_Movement> ().cantShoot = true;
			GetComponent<Player_Animation> ().ResetValues ();
			GetComponent<Player_Animation> ().inPain = true;

			//StartCoroutine (CustomMethods.CameraShake (transform.position, 2.5f, 0.2f));
			CameraShake.Shake (0.25f,  0.25f / Vector3.Distance (transform.position, references.Camera.transform.position) * 4f);

			//yield return new WaitForSeconds (painTimer);
			//yield return new WaitForSeconds (painTimerOffset);

		if (!overthrows && GetComponent<Hero_Movement> ().isGrounded && !references.animationScript.isStandingUp && !references.animationScript.anim.GetCurrentAnimatorStateInfo (-1).IsName ("Pain Air")) {
			animationScript.anim.Play ("Pain");
			rigidbody.velocity = Vector3.zero;
			while (stunTime > 0f) {
				GetComponent<Hero_Movement> ().doNotMove = true;
				GetComponent<Hero_Movement> ().cantStab = true;
				GetComponent<Hero_Movement> ().cantShoot = true;

				stunTime -= Time.deltaTime;

				if (!GetComponent<Hero_Movement> ().isGrounded) {
					GetComponent<Collider> ().material.dynamicFriction = 0.5f; 
					GetComponent<Collider> ().material.staticFriction = 0.5f;
				} else {
					GetComponent<Collider> ().material.dynamicFriction = 1.0f; 
					GetComponent<Collider> ().material.staticFriction = 1.0f;
				}

				yield return null;
			}

			GetComponent<Player_Animation> ().isStandingUp = false;
			yield return null;
			GetComponent<Player_Animation> ().isStandingUp = true;
			yield return null;
			GetComponent<Player_Animation> ().isStandingUp = false;

			while (!GetComponent<Hero_Movement> ().isGrounded) {
				GetComponent<Hero_Movement> ().doNotMove = true;
				GetComponent<Hero_Movement> ().cantStab = true;
				GetComponent<Hero_Movement> ().cantShoot = true;

				painTimer -= Time.deltaTime;

				if (!GetComponent<Hero_Movement> ().isGrounded) {
					GetComponent<Collider> ().material.dynamicFriction = 0.5f; 
					GetComponent<Collider> ().material.staticFriction = 0.5f;
				} else {
					GetComponent<Collider> ().material.dynamicFriction = 1.0f; 
					GetComponent<Collider> ().material.staticFriction = 1.0f;
				}
				yield return null;
			}

			GetComponent<Collider> ().material.dynamicFriction = 1.0f; 
			GetComponent<Collider> ().material.staticFriction = 1.0f;
			rigidbody.angularDrag = 0.05f;

			//yield return new WaitForSeconds (0.3f); //transition

			GetComponent<Player_Animation> ().inPain = false;
			GetComponent<Hero_Movement> ().isJumping = false;
			GetComponent<Hero_Movement> ().isBouncing = false;
			GetComponent<Hero_Movement> ().isDashing = false;
			GetComponent<Hero_Movement> ().doNotMove = false;
			GetComponent<Hero_Movement> ().cantStab = false;
			GetComponent<Hero_Movement> ().cantShoot = false;
			inPain = false;
			currentPainCoro = null;
			yield break;
		} else if (overthrows || !GetComponent<Hero_Movement> ().isGrounded || references.animationScript.isStandingUp || references.animationScript.anim.GetCurrentAnimatorStateInfo (-1).IsName ("Pain Air")) {
			animationScript.anim.Play ("Pain Air", -1, 0f);
			transform.rotation = Quaternion.LookRotation ((attacker.transform.position - transform.position), Vector3.up);
			transform.rotation = Quaternion.Euler (0f, transform.rotation.eulerAngles.y, 0f);
			references.rigidbody.AddForce (transform.forward * (-7.5f), ForceMode.VelocityChange);
			GetComponent<CapsuleCollider> ().radius = references.capsuleColliderRadius * 2.5f;
			yield return null;

			if (GetComponent<Hero_Movement> ().isGrounded) {
				yield return new WaitForSeconds (0.2f);
			}

			yield return null;
			GetComponent<CapsuleCollider> ().radius = references.capsuleColliderRadius * 2.5f;
			while (!GetComponent<Hero_Movement> ().isGrounded) {
				GetComponent<Hero_Movement> ().doNotMove = true;
				GetComponent<Hero_Movement> ().cantStab = true;
				GetComponent<Hero_Movement> ().cantShoot = true;

				if (!GetComponent<Hero_Movement> ().isGrounded) {
					GetComponent<Collider> ().material.dynamicFriction = 1f; 
					GetComponent<Collider> ().material.staticFriction = 1f;
				} else {
					GetComponent<Collider> ().material.dynamicFriction = 1.0f; 
					GetComponent<Collider> ().material.staticFriction = 1.0f;
				}
				yield return null;
			}

			references.Landing ();
			references.animationScript.anim.Play ("Lay On Ground", -1, normalizedTime: 0.0f);
			GetComponent<CapsuleCollider> ().radius = references.capsuleColliderRadius * 2.5f;

			while (stunTime > 0f || references.rigidbody.velocity.magnitude > 2f) {
				GetComponent<Hero_Movement> ().doNotMove = true;
				GetComponent<Hero_Movement> ().cantStab = true;
				GetComponent<Hero_Movement> ().cantShoot = true;

				stunTime -= Time.deltaTime;

				if (!GetComponent<Hero_Movement> ().isGrounded) {
					GetComponent<Collider> ().material.dynamicFriction = 1f; 
					GetComponent<Collider> ().material.staticFriction = 1f;
				} else {
					rigidbody.velocity = Vector3.Slerp (rigidbody.velocity, Vector3.zero, 2f * Time.deltaTime);
					GetComponent<Collider> ().material.dynamicFriction = 1.0f; 
					GetComponent<Collider> ().material.staticFriction = 1.0f;
				}
				yield return null;
			}

			GetComponent<Collider> ().material.dynamicFriction = 1.0f; 
			GetComponent<Collider> ().material.staticFriction = 1.0f;
			rigidbody.angularDrag = 0.05f;

			//El pj vuelve en sí
			GetComponent<Player_Animation> ().inPain = false;
			GetComponent<Player_Animation> ().isStandingUp = true;
			CantBeHitMode (1f);
			GetComponent<CapsuleCollider> ().radius = references.capsuleColliderRadius;

			float t = 0.875f;
			while (t > 0f) {
				GetComponent<Hero_Movement> ().doNotMove = true;
				rigidbody.velocity = Vector3.zero;
				t -= Time.deltaTime;
				yield return null;
			}

			//yield return new WaitForSeconds (0.875f);
			GetComponent<CapsuleCollider> ().radius = references.capsuleColliderRadius;
			GetComponent<Player_Animation> ().isStandingUp = false;
			GetComponent<Player_Animation> ().inPain = false;
			GetComponent<Hero_Movement> ().isJumping = false;
			GetComponent<Hero_Movement> ().isBouncing = false;
			GetComponent<Hero_Movement> ().isDashing = false;
			GetComponent<Hero_Movement> ().doNotMove = false;
			GetComponent<Hero_Movement> ().cantStab = false;
			GetComponent<Hero_Movement> ().cantShoot = false;
			inPain = false;
			currentPainCoro = null;
			yield break;
		}
		//}
	}


	IEnumerator BulletPain(){
		//if (!inPain) {
			inPain = true;
		GetComponent<Hero_Movement> ().ResetStates();
		animationScript.anim.Play ("Pain", -1, 0f);
			if (references.RightHandWeapon.tag != "Fists" && references.RightHandWeapon.GetComponent<NGS_NewCPU> ())
				references.RightHandWeapon.GetComponent<NGS_NewCPU> ().ForcedReset ();
			GetComponent<Hero_Movement> ().doNotMove = true;
			GetComponent<Player_Animation> ().inPain = true;
			GetComponent<Hero_Movement> ().cantStab = true;
			GetComponent<Hero_Movement> ().cantShoot = true;
			GetComponent<Player_Animation> ().ResetValues ();

			//StartCoroutine (CustomMethods.CameraShake (transform.position, 1.25f, 0.1f));
			CameraShake.Shake (0.25f,  0.25f / Vector3.Distance (transform.position, references.Camera.transform.position) * 4f);

			yield return new WaitForSeconds (painTimer * 0.25f);
			//yield return new WaitForSeconds (painTimer);
			yield return new WaitForSeconds (0.25f); //transition
			GetComponent<Player_Animation> ().inPain = false;
			GetComponent<Hero_Movement> ().isJumping = false;
			GetComponent<Hero_Movement> ().isBouncing = false;
			GetComponent<Hero_Movement> ().isDashing = false;
			GetComponent<Hero_Movement> ().doNotMove = false;
			GetComponent<Hero_Movement> ().cantStab = false;
			GetComponent<Hero_Movement> ().cantShoot = false;
			inPain = false;
		//}
	}

	IEnumerator HeadacheEffect(){
		if (!coroHeadache) {
			coroHeadache = true;
			headacheEffect.enabled = true;
			headacheEffect.speed = 20f;
			yield return new WaitForEndOfFrame ();

			while (headacheEffect.strength > 0f) {
				headacheEffect.strength = headacheEffect.strength - (10 * Time.deltaTime);
				yield return null;
			}

			headacheEffect.strength = 0f;
			headacheEffect.enabled = false;
			coroHeadache = false;
			}
	}

	public override IEnumerator SwordCrash(Transform other){
		if (!coroClashed) {
			coroClashed = true;
			swordsCrashed = false;

			CameraShake.Shake (0.35f,  0.15f / Vector3.Distance (transform.position, references.Camera.transform.position) * 8f);

			if (SwordSpark != null) {
				GameObject ImpactSpark = Instantiate (SwordSpark, transform.position + transform.forward * 0.25f, transform.rotation);
				ImpactSpark.GetComponent<Renderer> ().material.SetColor ("_Color", SwordSparkColor);
			}

			rigidbody.velocity = Vector3.zero;
			if (GetComponent<Hero_Movement> ().isGrounded)
				rigidbody.AddForce ((transform.position - other.position).normalized * 3f, ForceMode.VelocityChange);
			else
				rigidbody.AddForce ((transform.position - other.position).normalized * 8f, ForceMode.VelocityChange);

			if (references.RightHandWeapon && references.RightHandWeapon.GetComponent<NGS_NewCPU> ()) {
				references.RightHandWeapon.GetComponent<NGS_NewCPU> ().ForcedReset ();
				GetComponent<Player_Animation> ().ResetValues ();

				GetComponent<Animator> ().Play ("SwordCrash", -1, normalizedTime: 0.0f);
				swordClash = references.RightHandWeapon.GetComponent<NGS_NewCPU> ().swordClash;
				CustomMethods.PlayClipAt (swordClash, transform.position);
			}

			StartCoroutine (SlowMotionEffect (0.5f));
			if (Camera.main.GetComponent<RadialBlur> ())
				Camera.main.GetComponent<RadialBlur> ().RadialBlurFx (0.25f, 1f);

			//Si atacka antes de tiempo, se penaliza el tiempo de "stun".
/*			int timerA = 60;
			bool attackedA = false;
			print ("Attack while you can!");
			while (timerA > 0) {
				yield return new WaitForFixedUpdate ();
				if (Input.GetButtonDown ("Melee")) {
					print ("Wrong! U didnt Chained it!");
					attackedA = true;
				}
				timerA = timerA - 1;
			}

			//Si atacka justo a tiempo, se compensa el tiempo de "stun".
			int timerB = 20;
			bool attackedB = false;
			if (!attackedA) {
				print ("Attack while you can!");
				while (timerB > 0) {
					yield return new WaitForFixedUpdate ();
					GetComponent<Hero_Movement> ().cantStab = false;
					if (Input.GetButtonDown ("Melee")) {
						print ("Great! U chained the attack succesfully!");
						attackedB = true;
						timerB = 0;
						GetComponent<Hero_Movement> ().doNotMove = false;
						GetComponent<Hero_Movement> ().cantStab = false;
						GetComponent<Hero_Movement> ().cantShoot = false;
					}
					timerB = timerB - 1;
				}
			}

			if (!attackedB) {
				print ("Time's up! U missed it!");
				GetComponent<Hero_Movement> ().doNotMove = true;
				GetComponent<Hero_Movement> ().cantStab = true;
				GetComponent<Hero_Movement> ().cantShoot = true;
				yield return new WaitForSeconds (0.5f);

				GetComponent<Hero_Movement> ().doNotMove = false;
				GetComponent<Hero_Movement> ().cantStab = false;
				GetComponent<Hero_Movement> ().cantShoot = false;
			}

*/
			////////////////////////////////////////
			float timer = 0.4f;
			while (timer > 0f) {
				GetComponent<Hero_Movement> ().doNotMove = true;
				GetComponent<Hero_Movement> ().cantStab = true;
				GetComponent<Hero_Movement> ().cantShoot = true;
				yield return new WaitForEndOfFrame ();
				timer -= Time.deltaTime;

			}

			GetComponent<Hero_Movement> ().doNotMove = false;
			GetComponent<Hero_Movement> ().cantStab = false;
			GetComponent<Hero_Movement> ().cantShoot = false;

			////////////////////////////////////////

			coroClashed = false;
			yield break;
		}
	}


	private IEnumerator SlowMotionEffect(float duration){
		//yield return new WaitForFixedUpdate ();
		if (!coroRunning) {
			coroRunning = true;
			Time.timeScale = 0.2f;
			Time.fixedDeltaTime = 0.005f;
			while (duration > 0) {
				duration -= Time.deltaTime;
			}
			Time.timeScale = PlayerPrefs.GetFloat ("GameSpeed", 1f);
			Time.fixedDeltaTime = PlayerPrefs.GetFloat ("GameFixedUpdateSpeed", 0.01f);
			Time.maximumDeltaTime = PlayerPrefs.GetFloat ("GameMaximumFixedTimestep", 0.05f);
			coroRunning = false;
		}
		yield return null;
	}

	IEnumerator Death(){
		rigidbody.drag = 1f;
		rigidbody.AddForce (-transform.forward * 2f, ForceMode.VelocityChange);
		gameObject.layer = 17;
		//cameraObject.GetComponent<Third_Person_Camera> ().noInputControls = true;
		transform.rotation = Quaternion.Euler (0f, transform.rotation.eulerAngles.y, 0f);
		GetComponent<Player_Animation> ().isDead = true;
		cameraObject.GetComponent<Third_Person_Camera> ().xSpeed = cameraObject.GetComponent<Third_Person_Camera> ().xSpeed / 2f;
		cameraObject.GetComponent<Third_Person_Camera> ().ySpeed = cameraObject.GetComponent<Third_Person_Camera> ().ySpeed / 4f;
		GetComponent<CapsuleCollider> ().height = references.capsuleColliderHeight * 0.5f;
		GetComponent<CapsuleCollider> ().radius = references.capsuleColliderRadius * 2f;
		GetComponent<CapsuleCollider> ().center = references.capsuleColliderCenter - Vector3.up * references.capsuleColliderHeight/4;
		GetComponent<CapsuleCollider> ().material.dynamicFriction = 1.0f;

		GetComponent<Hero_Movement> ().jumpInertia = false;
		GetComponent<Hero_Movement> ().isGrounded = true;
		GetComponent<Hero_Movement> ().doNotMove = true;
		GetComponent<Hero_Movement> ().speed = 0f;
		GetComponent<Hero_Movement> ().runSpeed = 0f;
		GetComponent<Hero_Movement> ().walkSpeed = 0f;
		GetComponent<Hero_Movement> ().sprintSpeed = 0f;
		GetComponent<Hero_Movement> ().airSpeed = 0f;
		GetComponent<Hero_Movement> ().inAirSpeed = 0f;

		if (GetComponent<SwordsActivatorDeactivator>().currentSword.tag != "Fists") {
			yield return new WaitForFixedUpdate();
			GameObject sword = references.RightHandWeapon;
			sword.transform.parent = null;
			sword.layer = 17;
			sword.GetComponent<Collider> ().enabled = true;
			sword.GetComponent<Collider> ().isTrigger = false;
			sword.GetComponent<BoxCollider> ().size = new Vector3 (0.0001f, sword.GetComponent<BoxCollider> ().size.y, sword.GetComponent<BoxCollider> ().size.z);
			sword.AddComponent<Rigidbody> ();
			sword.GetComponent<Rigidbody> ().isKinematic = false;
			sword.GetComponent<Rigidbody> ().useGravity = true;
			sword.GetComponent<Rigidbody> ().mass = 10f;
			sword.GetComponent<Rigidbody>().AddForce (transform.forward * 3f);
			sword.GetComponent<Rigidbody>().AddForce (Vector3.up, ForceMode.VelocityChange);
			sword.GetComponent<Rigidbody>().maxAngularVelocity = 10f;
			sword.GetComponent<Rigidbody>().AddTorque (new Vector3 (Random.Range (-5f, 5f ), Random.Range (-5f, 5f ), Random.Range (-5f, 5f )));
		}

		if (GetComponent<GunsActivatorDeactivator>().currentGun.tag != "Fists") {
			yield return new WaitForFixedUpdate();
			GameObject gun = GetComponent<GunsActivatorDeactivator> ().currentGun;
			gun.transform.parent = null;
			gun.layer = 17;
			gun.AddComponent<BoxCollider> ();
			gun.GetComponent<BoxCollider> ().enabled = true;
			gun.GetComponent<BoxCollider> ().isTrigger = false;
			gun.GetComponent<BoxCollider> ().size = new Vector3 (0.0001f, gun.GetComponent<BoxCollider> ().size.y, gun.GetComponent<BoxCollider> ().size.z);
			gun.AddComponent<Rigidbody> ();
			gun.GetComponent<Rigidbody> ().isKinematic = false;
			gun.GetComponent<Rigidbody> ().useGravity = true;
			gun.GetComponent<Rigidbody> ().mass = 5f;
			gun.GetComponent<Rigidbody>().AddForce (transform.forward * 3);
			gun.GetComponent<Rigidbody>().AddForce (Vector3.up, ForceMode.VelocityChange);
			gun.GetComponent<Rigidbody>().maxAngularVelocity = 100f;
			gun.GetComponent<Rigidbody>().AddTorque (new Vector3 (Random.Range (-5, 5f ), Random.Range (-5f, 5f ), Random.Range (-5f, 5f )));
		}

		foreach (MonoBehaviour script in playerScripts) {
			if (script != gameObject.GetComponent<Player_Animation>())
				script.enabled = false;
		}

		yield return new WaitForFixedUpdate();
		StartCoroutine (SlowMotionEffect (0.6f));
		isDead = true;
	}
}

