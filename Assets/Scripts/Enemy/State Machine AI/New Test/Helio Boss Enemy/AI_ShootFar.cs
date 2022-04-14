using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;

public class AI_ShootFar : State<AI> {

	private AI owner;
	private References targetRefs;

	public float returnedChance;
	public bool useFlash = true;

	public bool canShoot = true;
	public float chance = 20f;
	private float _chance;

	public GameObject projectile;

	public float minAttackDistance = 10f;
	public float maxAttackDistance = 15f;


	public int shotsAmount = 3;
	public float aimTime = 0.5f;
	public float gunCoolDown = 0.1f;
	public float bulletDamage = 7.5f;
	public float bulletSpeed = 10f;
	public bool bulletIsRipper = false;
	public float weaponSpread = 2.0f;

	public float coolDown = 4f;

	public Material bulletMaterial;

	public AudioClip shootSound;
	public AudioClip reloadSounds;

	private RaycastHit hitInfo1;
	private GameObject projectileAttack;
	[HideInInspector] public float _coolDown;
	private float timerCoolDown;
	private float updateChanceTimer;

	private bool rageMode = false;

	public void Start(){
		int difficulty = PlayerPrefs.GetInt ("Difficulty", 2);
		switch (difficulty) {
		case 1:
			coolDown = coolDown / 0.75f;
			chance = chance * 0.75f;
			break;
		case 2:
			coolDown = coolDown / 1f;
			chance = chance * 1f;
			break;
		case 3:
			coolDown = coolDown / 1.5f;
			chance = chance * 1.5f;
			break;
		case 4:
			coolDown = coolDown / 2f;
			chance = chance * 2f;
			break;
		}

		_coolDown = coolDown;
		timerCoolDown = _coolDown;
		_chance = chance;
	}

	public override bool EnterState(AI _owner){
		_owner.currentState = this.ToString();
		owner = _owner;
		currentCoroutine = _owner.StartCoroutine (Shoot (_owner));
		targetRefs = _owner.target.GetComponent<References> ();

		returnedChance = Random.Range (0f, 100f);
		if (returnedChance <= chance && _owner.visible) {
			//goOn = true;
			//currentCoroutine = _owner.StartCoroutine (ExecuteComboA (_owner));
			return true;
		}

		if (_owner.GetComponent<AI_MoveTowardsEnemy> ()) {
			//goOn = false;
			timerCoolDown = 0f;
			_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
			
			return false;
		}

		//goOn = false;
		_owner.stateMachine.ChangeState (_owner.GetComponent<AI_IdleState> ());
		return false;
	}

	public override void ExitState(AI _owner){
	}

	public override void UpdateState(AI _owner){
		if (!_owner.coroRunning) {
			if (_owner.targetIsInSight)
				chance = _chance;
			else
				chance = _chance / 2f;
			_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
			StopAllCoroutines();
		}

		if (!rageMode && _owner.healthBar.CurHealth < _owner.healthBar.Maxhealth / 2f) {
			rageMode = true;
			_chance = _chance * 2f;
			shotsAmount = shotsAmount * 2;
			aimTime = aimTime / 2f;
			weaponSpread = weaponSpread * 1.5f;
			coolDown = coolDown / 2f;
		}
	}

	void Update(){
		if (owner != null && targetRefs.enemiesDetector && targetRefs.enemiesDetector.foundTarget) {
			_coolDown = coolDown + (Random.Range (0.25f, 0.5f) * targetRefs.enemiesDetector.enemiesInRange.Count);
		}else
			coolDown = _coolDown;

		if (timerCoolDown < coolDown) {
			canShoot = false;
			timerCoolDown += Time.deltaTime;
		} else if (timerCoolDown >= coolDown)
			canShoot = true;
	}

	public override void ForceInterruption(AI _owner){
		if (currentCoroutine != null)
			StopCoroutine (currentCoroutine);
		_owner.references.animationScript.isAiming = false;
		_owner.references.animationScript.rise = false;
		_owner.references.animationScript.fire = false;
	}

	public IEnumerator Shoot(AI _owner){
		_owner.coroRunning = true;
		_owner.doNotMove = true;
		_owner.DeactivateAllGuns ();
		_owner.Gun1.SetActive (true);
        _owner.references.animationScript.ResetValues();
        _owner.references.animationScript.gunWeaponType = _owner.Gun1.GetComponent<GenericGun> ().weaponType;
        _owner.navMeshAgent.enabled = false;
		if (useFlash) {
			_owner.references.AttackFlash.GetComponent<ParticleSystem> ().Play ();
			yield return new WaitForSeconds (_owner.references.AttackFlash.GetComponent<ParticleSystem> ().main.duration);
		}

		timerCoolDown = 0f;
		if (_owner.targetIsInSight) {
			if (_owner.distance <= maxAttackDistance && _owner.distance <= minAttackDistance) {
				if (!_owner.target.GetComponent<HealthBar> ().isDead) {
					yield return null;
				} else {
					_owner.coroRunning = false;
					yield break;
				}
			}
		} else {
			_owner.coroRunning = false;
			yield break;
		}
        
		_owner.references.animationScript.rise = true;
		_owner.references.animationScript.isAiming = true;

		StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 2f, aimTime));

		for (float t = aimTime; t > 0; t -= Time.deltaTime) {
			yield return null;
		}


		if (_owner.targetIsInSight) {
			if (_owner.distance <= maxAttackDistance && _owner.distance <= minAttackDistance) {
				if (!_owner.target.GetComponent<HealthBar> ().isDead) {
					yield return null;
				} else {
					_owner.coroRunning = false;
					yield break;
				}
			}
		} else {
			_owner.coroRunning = false;
			yield break;
		}

        float aimToHeadTimer = 0f;
        Transform targetsChest = _owner.target.GetComponent<References>().Chest.transform;
        Transform targetsHead = _owner.target.GetComponent<References>().Head.transform;        
        Vector3 aimToHeadDirection = targetsChest.position;
        BulletBehavior bulletBehavior;

        for (int i = shotsAmount; i > 0; i--) {
            
			_owner.references.animationScript.fire = true;
			AudioSource audioSource = CustomMethods.PlayClipAt (shootSound, transform.position);
			audioSource.volume = 0.8f;
			audioSource.maxDistance = 80f;
			_owner.Gun1.GetComponent<GenericGun>().muzzleFlash.GetComponent<ParticleSystem>().Play ();

            aimToHeadDirection = Vector3.Lerp(targetsChest.position, targetsHead.position, aimToHeadTimer);
            aimToHeadTimer += gunCoolDown * aimTime / weaponSpread / 2f;

            projectileAttack = (GameObject)Instantiate(projectile, _owner.references.LeftHand.transform.position, Quaternion.LookRotation(aimToHeadDirection - _owner.references.LeftHand.transform.position));
            projectileAttack.name = projectile.name;
            bulletBehavior = projectileAttack.GetComponent<BulletBehavior>();

            bulletBehavior.shooter = transform;
            bulletBehavior.AIShoter = true;
            bulletBehavior.baseDamage = bulletDamage;
            bulletBehavior.spread = weaponSpread;
            bulletBehavior.speed = bulletSpeed;
            bulletBehavior.isRipper = bulletIsRipper;
			projectileAttack.transform.GetChild (0).GetComponent<TrailRenderer> ().material = bulletMaterial;
			projectileAttack.transform.GetChild (0).GetComponent<MeshRenderer> ().material = bulletMaterial;
            if (i == shotsAmount){
                bulletBehavior.isFirstShot = true;
            }

			//Physics.IgnoreCollision (projectileAttack.GetComponent<Collider> (), collider);
			yield return new WaitForSeconds (gunCoolDown * Random.Range (0.5f, 1.5f));
			_owner.references.animationScript.fire = true;
			StartCoroutine (CustomMethods.SmoothRotateTowards (gameObject, _owner.target.transform, gunCoolDown));
			//transform.localRotation = Quaternion.Euler (0f, transform.localRotation.eulerAngles.y, 0f);
			yield return new WaitForFixedUpdate ();
		}

		yield return new WaitForSeconds (0.2f);

		timerCoolDown = 0f;
		_owner.doNotMove = false;
		_owner.references.animationScript.fire = false;
		_owner.references.animationScript.isAiming = false;
		_owner.references.animationScript.rise = false;
		//_owner.doNotMove = false;
		_owner.coroRunning = false;
		AudioSource audioSource2 = CustomMethods.PlayClipAt (reloadSounds, transform.position);
		audioSource2.maxDistance = 20;
		yield return null;
	}
}

