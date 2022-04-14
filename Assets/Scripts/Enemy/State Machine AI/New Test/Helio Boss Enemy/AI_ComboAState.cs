using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;

public class AI_ComboAState : State<AI> {

	private AI owner;
	private References targetRefs;


	public float returnedChance;
	public bool useFlash = true;

	private MeleeWeaponTrail meleeWeaponTrail;

	public float chance = 20f;
	public float coolDown = 1f;
	public float stunTime = 0.3f;

	public int comboLenght;
	public float attackBaseDamage;
	private float attackDamage;

	public AudioClip swordSlash;

	private bool coroRunning;
	private int comboIndex;
	private float SpeedMultiplier;

	private bool rageMode;

	public float _coolDown;
	public float timerCoolDown;
	private float updateChanceTimer;

	public bool canAttack = false;
	private bool goOn = false;
	private bool isAttacking = false;

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
	}

	public override bool EnterState(AI _owner){
		_owner.currentState = this.ToString ();
		owner = _owner;
		meleeWeaponTrail = _owner.references.RightHand.transform.GetComponent<MeleeWeaponTrail> ();
		comboIndex = 0;
		_owner.references.animationScript.meleeWeaponType = 2;
		attackDamage = attackBaseDamage;

		targetRefs = _owner.target.GetComponent<References> ();
		//_owner.StartCoroutine (ExecuteComboA (_owner));

		returnedChance = Random.Range (0f, 100f);
		if (returnedChance <= chance && canAttack && _owner.visible) {
			goOn = true;
			//currentCoroutine = _owner.StartCoroutine (ExecuteComboA (_owner));
			return true;
		}

		if (_owner.GetComponent<AI_MoveTowardsEnemy> ()) {
			goOn = false;
			_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
			return false;
		}

		goOn = false;
		_owner.stateMachine.ChangeState (_owner.GetComponent<AI_IdleState> ());
		return false;
	}

	public override void ExitState(AI _owner){
	}

	public override void UpdateState(AI _owner){
		if (goOn == true) {
			if (!_owner.doNotMove && canAttack && _owner.target) {
				if (!_owner.coroRunning && _owner.target && _owner.GetComponent<AI_MoveTowardsEnemy> () && _owner.distance > _owner.GetComponent<AI_MoveTowardsEnemy> ().closeDistance &&
				   _owner.distance > _owner.GetComponent<AI_MoveTowardsEnemy> ().closeDistance + 2f) {
					_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
				} else if (!_owner.coroRunning && comboIndex > 0 && _owner.target && _owner.GetComponent<AI_MoveTowardsEnemy> () && _owner.distance > _owner.GetComponent<AI_MoveTowardsEnemy> ().closeDistance + 1f) {
					_owner.GetComponent<AI_StrongAtkState> ().shouldWait = true;
					_owner.stateMachine.ChangeState (_owner.GetComponent<AI_StrongAtkState> ());
				} else if (!_owner.coroRunning) {
					currentCoroutine = StartCoroutine (ExecuteComboA (_owner));
				}
			} else {
				_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
			}
		}

		isAttacking = _owner.coroRunning;

		if (!rageMode && _owner.healthBar.CurHealth < _owner.healthBar.Maxhealth / 2f) {
			rageMode = true;
			chance = chance * 2f;
		}
	}

	void Update(){
		if (owner != null && targetRefs.enemiesDetector && targetRefs.enemiesDetector.foundTarget) {
			_coolDown = coolDown + (Random.Range (0.25f, 0.5f) * targetRefs.enemiesDetector.enemiesInRange.Count);
		}else
			_coolDown = coolDown;

		if (!isAttacking) {
			if (timerCoolDown < _coolDown) {
				canAttack = false;
				timerCoolDown += Time.deltaTime + Time.deltaTime * Random.Range (0.5f, 1.5f);
			} else
				canAttack = true;
		}
	}

	public override void ForceInterruption(AI _owner){
		if (currentCoroutine != null)
			StopCoroutine (currentCoroutine);
		_owner.coroRunning = false;
		_owner.references.animationScript.isAttackingMelee = false;
		isAttacking = false;
		canAttack = false;
		timerCoolDown = 0f;
		comboIndex = 0;
	}

	public override void OnWeaponTriggerEnter(AI _owner, Collider slashed){
		HealthBar damage = slashed.gameObject.GetComponent<HealthBar> ();
		if (damage && !damage.isDead && damage.AddjustCurrentHealth (transform, -attackBaseDamage, stunTime, false, false)) {
			CustomMethods.PlayClipAt (swordSlash, transform.position);
		}
	}

	IEnumerator ExecuteComboA (AI _owner){
		_owner.coroRunning = true;
		timerCoolDown = 0f;
		SpeedMultiplier = _owner.references.animationScript.finalSpeedMultiplier;

		if (_owner.target != null)
			StartCoroutine (CustomMethods.SmoothRotateTowards (_owner.gameObject, _owner.target.transform, 0.125f));
		
		if (useFlash) {
			_owner.references.AttackFlash.GetComponent<ParticleSystem> ().Play ();
			yield return new WaitForSeconds (_owner.references.AttackFlash.GetComponent<ParticleSystem> ().main.duration);
		}

		yield return new WaitForSeconds (0.2f);

		switch (comboIndex) {
		case 0:
			if (comboIndex + 1 <= comboLenght) {
				CustomMethods.CheckCurrentAnimatorState (_owner.references.animationScript.anim);	
				comboIndex += 1;
				_owner.references.animationScript.meleeAttack = 1;
				_owner.references.animationScript.isAttackingMelee = true;
				_owner.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
				_owner.references.rigidbody.velocity = Vector3.zero;
				_owner.references.rigidbody.AddForce (_owner.transform.forward * 3.5f, ForceMode.VelocityChange);

				yield return new WaitForSeconds (0.2f / SpeedMultiplier);

				_owner.references.rigidbody.velocity = Vector3.zero;
				//swordCPU.ToggleSwordDamage (0);
				//swordCPU.AttackDone ();

				if (_owner.healthBar.CurHealth < _owner.healthBar.Maxhealth * 2 / 3)
					yield return new WaitForSeconds (0.125f / SpeedMultiplier);
			
				yield return new WaitForSeconds (0.1f / SpeedMultiplier);
				_owner.coroRunning = false;

			} else {
				timerCoolDown = 0f;
				_owner.coroRunning = false;
				_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
			}			
			_owner.references.animationScript.isAttackingMelee = false;
			break;

		case 1:
			if (comboIndex + 1 <= comboLenght) {
				CustomMethods.CheckCurrentAnimatorState (_owner.references.animationScript.anim);
				comboIndex += 1;
				_owner.references.animationScript.meleeAttack = 2;
				_owner.references.animationScript.isAttackingMelee = true;
				_owner.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
				_owner.references.rigidbody.velocity = Vector3.zero;
				_owner.references.rigidbody.AddForce (_owner.transform.forward * 2f, ForceMode.VelocityChange);
				yield return new WaitForSeconds (0.1f / SpeedMultiplier);

				_owner.references.rigidbody.AddForce (_owner.transform.forward * 2f, ForceMode.VelocityChange);
				yield return new WaitForSeconds (0.2f / SpeedMultiplier);

				_owner.references.rigidbody.velocity = Vector3.zero;
				//swordCPU.ToggleSwordDamage (0);
				//swordCPU.AttackDone ();

				if (_owner.healthBar.CurHealth < _owner.healthBar.Maxhealth * 2 / 3)
					yield return new WaitForSeconds (0.125f / SpeedMultiplier);

				yield return new WaitForSeconds (0.1f / SpeedMultiplier);

				_owner.coroRunning = false;
			} else {
				timerCoolDown = 0f;
				_owner.coroRunning = false;
				_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
			}
				_owner.references.animationScript.isAttackingMelee = false;
				break;

		case 2:
			if (comboIndex + 1 <= comboLenght) {
				CustomMethods.CheckCurrentAnimatorState (_owner.references.animationScript.anim);	
				comboIndex += 1;
				_owner.references.animationScript.meleeAttack = 3;
				_owner.references.animationScript.isAttackingMelee = true;
				_owner.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
				_owner.references.rigidbody.velocity = Vector3.zero;
				_owner.references.rigidbody.AddForce (_owner.transform.forward * 1.5f, ForceMode.VelocityChange);
				yield return new WaitForSeconds (0.2f / SpeedMultiplier);
				_owner.references.rigidbody.AddForce (_owner.transform.forward * 2f, ForceMode.VelocityChange);
				yield return new WaitForSeconds (0.1f / SpeedMultiplier);
				yield return new WaitForSeconds (0.2f / SpeedMultiplier);

				_owner.references.rigidbody.velocity = Vector3.zero;
				//swordCPU.ToggleSwordDamage (0);
				//swordCPU.AttackDone ();

				if (_owner.healthBar.CurHealth < _owner.healthBar.Maxhealth * 2 / 3)
					yield return new WaitForSeconds (0.125f / SpeedMultiplier);

				yield return new WaitForSeconds (0.1f / SpeedMultiplier);
				_owner.coroRunning = false;
			} else {
				timerCoolDown = 0f;
				_owner.coroRunning = false;
				_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
			}
				_owner.references.animationScript.isAttackingMelee = false;
				break;

		case 3:
			if (comboIndex + 1 <= comboLenght) {
				CustomMethods.CheckCurrentAnimatorState (_owner.references.animationScript.anim);
				comboIndex = 0;
				attackDamage = attackBaseDamage * 1.25f;
				_owner.references.animationScript.meleeAttack = 4;
				_owner.references.animationScript.isAttackingMelee = true;
				_owner.GetComponent<Collider> ().material.dynamicFriction = 1.0f;
				_owner.references.rigidbody.velocity = Vector3.zero;
				yield return new WaitForSeconds (0.1f / SpeedMultiplier);
				_owner.references.rigidbody.AddForce (_owner.transform.forward * 4f, ForceMode.VelocityChange);

				yield return new WaitForSeconds (0.3f / SpeedMultiplier);
				_owner.references.rigidbody.velocity = Vector3.zero;
				yield return new WaitForSeconds (0.2f / SpeedMultiplier);
				//rigidbody.AddForce (Player.transform.forward * 2f, ForceMode.VelocityChange);
				yield return new WaitForSeconds (0.2f / SpeedMultiplier);
				_owner.references.rigidbody.velocity = Vector3.zero;
				//swordCPU.ToggleSwordDamage (0);
				//swordCPU.AttackDone ();
				yield return new WaitForSeconds (0.1f / SpeedMultiplier);
				yield return StartCoroutine (BounceBack (_owner));
			} else {
				timerCoolDown = 0f;
				_owner.coroRunning = false;
				_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
			}
				_owner.references.animationScript.isAttackingMelee = false;
				break;
		}
	}

	IEnumerator BounceBack(AI _owner){
		//StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 0.5f, 0.8f));
		_owner.navMeshAgent.enabled = false;

		//_owner.references.animationScript.isBouncing = true;
		//_owner.references.animationScript.isJumping = true;
		_owner.references.animationScript.anim.Play ("Bouncing", 0);
		//_coolDown = coolDown;
		yield return new WaitForSeconds (0.1f);

		_owner.references.animationScript.isBouncing = true;
		_owner.references.animationScript.isJumping = true;

		GetComponent<Rigidbody> ().AddForce (Vector3.up * GetComponent<AI_MoveTowardsEnemy>().avoidTargetPower, ForceMode.VelocityChange);
		GetComponent<Rigidbody> ().AddForce (-transform.forward * GetComponent<AI_MoveTowardsEnemy>().avoidTargetPower * 2.125f, ForceMode.VelocityChange);

		while (!_owner.isGrounded) {
			//_owner.references.animationScript.isBouncing = true;
			//_owner.references.animationScript.isJumping = true;
			yield return null;
		}

		GetComponent<Rigidbody> ().velocity = Vector3.zero;
		_owner.references.animationScript.isBouncing = false;
		_owner.references.animationScript.isJumping = false;
		yield return new WaitForSeconds (0.5f);
		_owner.navMeshAgent.enabled = true;				
		_owner.coroRunning = false;
	}
}