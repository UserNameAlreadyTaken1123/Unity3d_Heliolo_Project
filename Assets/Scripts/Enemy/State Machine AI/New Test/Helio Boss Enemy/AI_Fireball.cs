using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;

public class AI_Fireball : State<AI> {

	private AI owner;
	private References targetRefs;

	public float returnedChance;
	public bool useFlash = true;

	public bool canShoot = true;
	public float chance = 20f;

	public GameObject projectile;

	public float minAttackDistance = 10f;
	public float maxAttackDistance = 20f;


	public int shotsAmount = 3;
	public float aimTime = 0.5f;
	public float coolDown = 0.1f;
	public float baseDamage = 7.5f;
	public float projectileBaseSpeed = 10f;

	public float castDelay = 4f;

	public AudioClip castSound;

	private RaycastHit hitInfo1;
	private GameObject projectileAttack;
	private float _coolDown;
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
	}

	public override bool EnterState(AI _owner){

		_owner.currentState = this.ToString();
		owner = _owner;
		targetRefs = _owner.target.GetComponent<References> ();

		//currentCoroutine = _owner.StartCoroutine (Shoot (_owner));

		returnedChance = Random.Range (0f, 100f);
		if (canShoot && returnedChance <= chance && _owner.visible) {
			//goOn = true;
			//currentCoroutine = _owner.StartCoroutine (ExecuteComboA (_owner));
			currentCoroutine = _owner.StartCoroutine (Shoot (_owner));
			return true;
		}

		if (_owner.GetComponent<AI_MoveTowardsEnemy> ()) {
			//goOn = false;
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
			_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
		}

		if (!rageMode && _owner.healthBar.CurHealth < _owner.healthBar.Maxhealth / 2f) {
			rageMode = true;
			chance = chance * 2f;
			shotsAmount = shotsAmount * 1/2;
			aimTime = aimTime / 2f;
			coolDown = coolDown / 2f;
		}
	}

	void Update(){
		if (owner != null && targetRefs.enemiesDetector && targetRefs.enemiesDetector.foundTarget) {
			_coolDown = coolDown + (Random.Range (0.25f, 0.5f) * targetRefs.enemiesDetector.enemiesInRange.Count);
		}else
			_coolDown = coolDown;

		if (_coolDown < coolDown) {
			canShoot = false;
			_coolDown += Time.deltaTime;
		} else if (_coolDown >= coolDown)
			canShoot = true;

		if (updateChanceTimer >= 0.5f) {
			returnedChance = Random.Range (0f, 100f);
			updateChanceTimer = 0f;
		}else {
			updateChanceTimer += Time.deltaTime;
		}
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
		_owner.DeactivateAllGuns ();

		_coolDown = 0f;
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

		_owner.references.animationScript.ResetValues ();
		_owner.references.animationScript.rise = true;
		_owner.references.animationScript.isAiming = true;

		//_owner.doNotMove = true;

		StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 2f, aimTime));

		for (float t = aimTime; t > 0; t -= Time.deltaTime) {
			//_owner.references.animationScript.isAiming = true;
			//transform.localRotation = Quaternion.Euler (0f, transform.localRotation.eulerAngles.y, 0f);
			yield return null;
		}

		if (useFlash) {
			_owner.references.AttackFlash.GetComponent<ParticleSystem> ().Play ();
			yield return new WaitForSeconds (0.5f);
		}

		for (int i = shotsAmount; i > 0; i--) {
			_owner.references.animationScript.anim.Play ("SpellCast");
			AudioSource audioSource = CustomMethods.PlayClipAt (castSound, transform.position);
			audioSource.volume = 0.8f;
			audioSource.maxDistance = 60f;
			projectileAttack = (GameObject)Instantiate (projectile, _owner.references.LeftHand.transform.position + transform.forward * 0.5f, Quaternion.LookRotation (_owner.target.transform.position - (_owner.references.LeftHand.transform.position + transform.forward * 0.5f)));
			projectileAttack.name = projectile.name;
			projectileAttack.GetComponent<GenericProjectile> ().caster = transform.gameObject;
			projectileAttack.GetComponent<GenericProjectile> ().damage = baseDamage;
			projectileAttack.GetComponent<GenericProjectile> ().forwardSpeed = projectileBaseSpeed;
			//projectileAttack.transform.GetChild (0).GetComponent<TrailRenderer> ().material = bulletMaterial;
			//projectileAttack.transform.GetChild (0).GetComponent<MeshRenderer> ().material = bulletMaterial;
			Physics.IgnoreCollision (projectileAttack.GetComponent<Collider> (), _owner.references.collider);
			yield return new WaitForSeconds (castDelay * Random.Range (0.5f, 1.5f));
			_owner.references.animationScript.fire = true;
			StartCoroutine (CustomMethods.SmoothRotateTowards (gameObject, _owner.target.transform, castDelay));
			//transform.localRotation = Quaternion.Euler (0f, transform.localRotation.eulerAngles.y, 0f);
			yield return new WaitForFixedUpdate ();
		}

		yield return new WaitForSeconds (0.2f);

		_coolDown = 0f;
		_owner.references.animationScript.fire = false;
		_owner.references.animationScript.isAiming = false;
		_owner.references.animationScript.rise = false;
		//_owner.doNotMove = false;
		_owner.coroRunning = false;
		yield return null;
	}
}

