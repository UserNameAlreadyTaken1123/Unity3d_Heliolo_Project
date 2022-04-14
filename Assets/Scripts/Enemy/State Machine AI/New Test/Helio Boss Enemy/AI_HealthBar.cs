using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AI_HealthBar : HealthBar {

	private AI genericAI;

	public bool canBeStunned;
	public float stunTimer = 5.0f;
	public bool canRage;
	public float rageTimer = 5.0f;
	public float flinchChance = 1.0f;

	public bool swordsCrashed;
	private bool coroClashed = false;

	public float maxAcumulatedPain;
	public float curAcumulatedPain;
	private bool acumulatedPainLocker = false;
	private float acumulatedPainLockerTimer = 1f;

	public GameObject removedEffect;

	private LayerMask searchingTargetIgnoreMask;

	private bool coroDeath;

	// Use this for initialization
	void Start () {
		animationScript = GetComponent<Player_Animation> ();
		genericAI = GetComponent<AI> ();
		animationScript = GetComponent<Player_Animation> ();
		collider = GetComponent<Collider> ();
		rigidbody = GetComponent<Rigidbody> ();
		transform = GetComponent<Transform> ();
		references = GetComponent<References> ();

		Maxhealth = Maxhealth * PlayerPrefs.GetInt ("Difficulty", 2) / 2f;
		CurHealth = Maxhealth;
	}

	void HealOverTime (){
		if (!isDead && CurHealth < Maxhealth)
			AddjustCurrentHealth (this.transform, healAmountPerSecond * Time.deltaTime, 0f, true, false);
	}

	// Update is called once per frame
	void Update () {	
		HealOverTime ();

		if (cheatGodMode) {
			godMode = true;
			cantBeHitMode = true;
		}

		if (!isDead) {
			if (CurHealth <= 0){
				isDead = true;
				genericAI.StartDeathState ();
			}

			if (acumulatedPainLockerTimer <= 0) {
				acumulatedPainLocker = false;
			} else {
				acumulatedPainLocker = true;
				acumulatedPainLockerTimer -= Time.deltaTime;
			}

			if (!acumulatedPainLocker && curAcumulatedPain > 0f)
				curAcumulatedPain -= Time.deltaTime * 10f;
		}

		if (CurHealth > Maxhealth)
			CurHealth = Maxhealth;
	}

	public override bool AddjustCurrentHealth (Transform attacker, float adj, float stunTime, bool unstoppable, bool overthrows) {
		if (godMode) {
			if (adj < 0)
				adj = 0;
		}

		if (!cantBeHitMode && adj < 0 && DiplomacyManager.AreEnemies(references.team, attacker.GetComponent<References>().team)) {
			//La pregunta por la distancia es temporal, hasta encontrar una forma de distinguir entre projectiles y melee
			if (!unstoppable && Vector3.Distance (transform.position, attacker.position) < 2.5f &&
				(references.RightHandWeapon != null && references.RightHandWeapon.GetComponent<NGS_NewCPU> () && references.RightHandWeapon.GetComponent<NGS_NewCPU> ().damageCol) &&
				Vector3.Dot ((attacker.transform.transform.position - transform.position).normalized, transform.forward) < -0.5f) {
				genericAI.StartSwordCrashState (attacker);
				if (attacker.GetComponent<HealthBar> ())
					StartCoroutine (attacker.GetComponent<HealthBar> ().SwordCrash (this.transform));
				else if (attacker.GetComponent<AI_HealthBar> ())
					attacker.GetComponent<AI_HealthBar> ().StartSwordCrashState (this.transform);
				return false;
			} else {
				CurHealth += adj;
				justGotHurt = true;
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
					
				BloodSplatObject.transform.SetParent (this.gameObject);
				BloodSplatObject.transform.position = bloodsplatPos;
				BloodSplatObject.transform.rotation = Quaternion.LookRotation (bloodsplatPos - transform.position);

				animationScript.damageIntensity = painTimer;
				rigidbody.velocity = Vector3.zero;

				if (genericAI.isGrounded)
					rigidbody.AddForce ((transform.position - attacker.transform.position).normalized * 3f, ForceMode.VelocityChange);
				else {
					rigidbody.AddForce (Vector3.up * 2f, ForceMode.VelocityChange);
				}

				int chance = Random.Range (0, 100);	
				if (!isDead && chance < painChance && CurHealth > 0f) {
					if (curAcumulatedPain >= maxAcumulatedPain && genericAI.StartComboBreaker ()) {
						curAcumulatedPain = 0f;
					} else {
						genericAI.StartPainState (attacker, stunTime, overthrows);
					}
				}
				genericAI.ReceivedAttackFrom (attacker);
                inPain = true;
				return true;
			}
		} else if (adj > 0f) {
			CurHealth += adj;
            inPain = true;
            return true;
		} else if (cantBeHitMode)
			return false;
		else
			return false;
	}

	public override bool BulletDamage(Vector3 hitPoint, Transform shooter, Transform bullet, float adj) {

		if (godMode) {
			if (adj < 0)
				adj = 0;
		}

		if (!cantBeHitMode && adj < 0f && DiplomacyManager.AreEnemies(references.team, shooter.GetComponent<References>().team)) {
			CurHealth += adj;
			BloodSplatObject = Instantiate (BloodSplatObjectBullet, transform.position, transform.rotation, gameObject.transform);
			justGotHurt = true;
			//FROM BULLET TO ENEMY
			/*
			Vector3 bloodsplatPos = Physics.ClosestPoint (shooter.transform.position,
				                        GetComponent<CapsuleCollider> (),
				                        transform.position,
				                        transform.rotation);
				                        */
			BloodSplatObject.transform.SetParent (this.gameObject);
			BloodSplatObject.transform.position = bullet.position;
			BloodSplatObject.transform.localScale = BloodSplatReference.transform.localScale * 3 / 5;
			BloodSplatObject.transform.rotation = Quaternion.LookRotation (shooter.position - hitPoint);
			int chance = Random.Range (0, 100);	

			if (!genericAI.target) {
				genericAI.target = shooter.gameObject;
			}

			rigidbody.velocity = Vector3.zero;
			rigidbody.AddForce ((transform.position - shooter.position).normalized * 0.1f, ForceMode.VelocityChange);

			if (!isDead && chance <= painChance) {
				if (genericAI.isGrounded)
					animationScript.anim.Play ("Pain", -1, 0f);
				else
					animationScript.anim.Play ("Pain Air", -1, 0f);
				//if (!inPain)
					genericAI.StartBulletPainState();
			}
			StartCoroutine (genericAI.ReceivedAttackFrom (shooter));
			return true;
		} else if (adj > 0f) {
			CurHealth += adj;
			return true;
		} else if (cantBeHitMode)
			return false;
		else
			return false;
	}

	public void StartDeathState(){
		genericAI.StartDeathState ();
	}

	public void StartPainState(Transform attacker, float stunTime, bool overthrows){
		genericAI.StartPainState (attacker, stunTime, overthrows);
	}

	public void StartBulletPainState(){
		genericAI.StartBulletPainState ();
	}

	public void StartSwordCrashState(Transform other){
		genericAI.StartSwordCrashState (other);
	}

}
