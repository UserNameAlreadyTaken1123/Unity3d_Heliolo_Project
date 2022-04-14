using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;

public class AI_StrongAtkState : State<AI> {

	private AI owner;

	public float returnedChance;
	
	private MeleeWeaponTrail meleeWeaponTrail;
	private NGS_NewCPU swordCPU;
	private BoxCollider boxCollider;

	public float chance = 15f;
	public float stunTime = 0.3f;
	public bool unstoppable = false;
	public bool overthrows = false;
	public bool shouldWait = false;

	public int attackLevel;
	public float attackBaseDamage;
	private float attackDamage;

	public AudioClip swordSlash;
	public AudioClip defenseBreak;

	private bool coroRunning;
	private float SpeedMultiplier;

	private bool rageMode;
	private float updateChanceTimer;
	private bool goOn = false;

	void Start(){
		int difficulty = PlayerPrefs.GetInt ("Difficulty", 2);
		switch (difficulty) {
		case 1:
			chance = chance * 0.75f;
			break;
		case 2:
			chance = chance * 1f;
			break;
		case 3:
			chance = chance * 1.5f;
			break;
		case 4:
			chance = chance * 2f;
			break;
		}
	}

	public override bool EnterState(AI _owner){
		_owner.currentState = this.ToString();
		owner = _owner;
		meleeWeaponTrail = _owner.references.RightHand.transform.GetComponent<MeleeWeaponTrail> ();
		boxCollider = _owner.Sword1.GetComponent<BoxCollider> ();
		swordCPU = _owner.Sword1.GetComponent<NGS_NewCPU> ();
		_owner.references.animationScript.meleeWeaponType = 2;
		attackDamage = attackBaseDamage;

		returnedChance = Random.Range (0f, 100f);

		if (returnedChance <= chance && _owner.visible) {
			goOn = true;
			if (!shouldWait) {
				currentCoroutine = _owner.StartCoroutine (ExecuteStrongAttack (_owner));
			} else {
				shouldWait = false;
				currentCoroutine = _owner.StartCoroutine (WaitTime (_owner));
			}
			return true;
		} 
		/*
		if (!_owner.coroRunning && _owner.GetComponent<AI_MoveTowardsEnemy> ()) {
			goOn = false;
			_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
			return false;
		}
		*/

		goOn = false;
		_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
		return false;
	}

	public override void ExitState(AI _owner){
	}

	public override void UpdateState(AI _owner){
		if (goOn) {
			if (!_owner.doNotMove) {
				if (!_owner.coroRunning && _owner.GetComponent<AI_MoveTowardsEnemy> ()) {
					_owner.stateMachine.ChangeState (_owner.GetComponent<AI_MoveTowardsEnemy> ());
				}
			}
		}

		if (!rageMode && _owner.healthBar.CurHealth < _owner.healthBar.Maxhealth / 2f) {
			rageMode = true;
			chance = chance * 2f;
		}			

	}
	
	public override void ForceInterruption(AI _owner){
		if (currentCoroutine != null)
			StopCoroutine (currentCoroutine);
		_owner.coroRunning = false;
		_owner.references.animationScript.isAttackingMelee = false;
	}

	public override void OnWeaponTriggerEnter(AI _owner, Collider slashed){
		slashed.gameObject.GetComponent<Rigidbody> ().AddForce (transform.forward * 4f, ForceMode.VelocityChange);
		HealthBar damage = slashed.gameObject.GetComponent<HealthBar> ();
		if (damage && !damage.isDead && damage.AddjustCurrentHealth (transform, -attackDamage, stunTime, unstoppable, overthrows)) {
			CustomMethods.PlayClipAt (swordSlash, transform.position);
		} else {
			damage.curFocus -= 5f;
			if (damage.curFocus < 1f) {
				CustomMethods.PlayClipAt (defenseBreak, damage.transform.position).transform.SetParent(damage.transform);
				damage.AddjustCurrentHealth(transform, -attackDamage * 1.15f, stunTime * 1.15f, true, true);
			}
		}
	}

	IEnumerator WaitTime(AI _owner){
		_owner.coroRunning = true;
		yield return new WaitForSeconds (0.8f);
		currentCoroutine = _owner.StartCoroutine (ExecuteStrongAttack (_owner));
	}

	IEnumerator ExecuteStrongAttack(AI _owner) {
		if (attackLevel <= 1) {
			_owner.references.animationScript.attackLevel = 1f;
			attackDamage = attackBaseDamage * 1.3f + Mathf.Sqrt(attackBaseDamage) * 2.5f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer>().material = meleeWeaponTrail._material;
		} else if (attackLevel == 2) {
			_owner.references.animationScript.attackLevel = 2f;
			attackDamage = attackBaseDamage * 1.4f + Mathf.Sqrt(attackBaseDamage) * 3f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer>().material = meleeWeaponTrail._materialBlue;
		} else if (attackLevel >= 3) {
			_owner.references.animationScript.attackLevel = 3f;
			attackDamage = attackBaseDamage * 1.5f + Mathf.Sqrt(attackBaseDamage) * 4f;
			meleeWeaponTrail._trailObject.GetComponent<Renderer>().material = meleeWeaponTrail._materialRed;
		}

		_owner.doNotMove = true;
		_owner.coroRunning = true;
		_owner.navMeshAgent.enabled = false;
		_owner.references.animationScript.meleeAttack = 5f;
		SpeedMultiplier = _owner.references.animationScript.finalSpeedMultiplier;
		_owner.references.animationScript.isAttackingMelee = true;
		CustomMethods.CheckCurrentAnimatorState(_owner.references.animationScript.anim);
		CustomMethods.SmoothRotateTowards(_owner.gameObject, _owner.target.transform, 0.2f);
		_owner.references.animationScript.isJumping = true;
		_owner.references.rigidbody.velocity = Vector3.zero;
		_owner.GetComponent<Collider>().material.dynamicFriction = 0.0f;
		_owner.GetComponent<Collider>().material.staticFriction = 0.0f;
		_owner.references.animationScript.attackLevel = attackLevel;

		//rigidbody.AddForce (_owner.transform.forward * 2f, ForceMode.VelocityChange);
		//boxCollider.size = new Vector3 (boxCollider.size.x * 2f, boxCollider.size.y, boxCollider.size.z);
		yield return new WaitForSeconds(0.3f / SpeedMultiplier);
		_owner.references.rigidbody.AddForce(_owner.transform.up * 4f, ForceMode.VelocityChange);
		_owner.references.rigidbody.AddForce(_owner.transform.forward * 5f, ForceMode.VelocityChange);
		yield return new WaitForSeconds(0.2f / SpeedMultiplier);
		_owner.references.rigidbody.velocity = Vector3.zero;
		_owner.references.rigidbody.AddForce(_owner.transform.up * 5f * (-2), ForceMode.VelocityChange);
		_owner.velocityLimitXZ = 0.1f;
		boxCollider.size = new Vector3(swordCPU.boxColliderOrigSize.x, swordCPU.boxColliderOrigSize.y * 5f, swordCPU.boxColliderOrigSize.z);
		boxCollider.center = new Vector3(swordCPU.boxColliderOrigCenter.x, swordCPU.boxColliderOrigCenter.y * (-15f), swordCPU.boxColliderOrigCenter.z);
		yield return new WaitForSeconds(0.25f / SpeedMultiplier);
		yield return new WaitForSeconds(0.25f / SpeedMultiplier);
		//_owner.references.animationScript.meleeAttack = 0f;

		//_owner.references.animationScript.attackLevel = 0f;
		//_owner.references.animationScript.isAttackingMelee = false;
		boxCollider.size = swordCPU.boxColliderOrigSize;
		boxCollider.center = swordCPU.boxColliderOrigCenter;
		swordCPU.attackDamage = swordCPU.baseDamage;
		yield return new WaitForSeconds(0.125f / SpeedMultiplier);

		_owner.velocityLimitXZ = 0.0f;
		_owner.references.animationScript.isJumping = false;
		_owner.references.rigidbody.velocity = Vector3.zero;

		yield return new WaitForSeconds(0.5f / SpeedMultiplier);
		if (_owner.distance < 2.5f)
			yield return StartCoroutine(BounceBack(_owner));
		else {
			yield return new WaitForSeconds(0.5f / SpeedMultiplier);
			_owner.coroRunning = false;
		}

		_owner.navMeshAgent.enabled = true;
		_owner.doNotMove = false;
		_owner.references.animationScript.isAttackingMelee = false;
	}

	IEnumerator BounceBack(AI _owner){
		StartCoroutine (CustomMethods.SmoothKeepRotatingTowards (gameObject, _owner.target.transform, 0.5f, 0.8f));
		_owner.navMeshAgent.enabled = false;
		GetComponent<Rigidbody> ().AddForce (Vector3.up * GetComponent<AI_MoveTowardsEnemy>().avoidTargetPower, ForceMode.VelocityChange);
		GetComponent<Rigidbody> ().AddForce ((transform.position - _owner.target.transform.position).normalized * GetComponent<AI_MoveTowardsEnemy>().avoidTargetPower * 2.125f, ForceMode.VelocityChange);

		_owner.references.animationScript.isBouncing = true;
		yield return new WaitForSeconds (1.0f);

		while (!_owner.isGrounded) {
			yield return new WaitForFixedUpdate ();
		}

		GetComponent<Rigidbody> ().velocity = Vector3.zero;
		_owner.references.animationScript.isBouncing = false;	
		yield return new WaitForSeconds (0.5f);
		_owner.navMeshAgent.enabled = true;				
		_owner.coroRunning = false;
	}
}
