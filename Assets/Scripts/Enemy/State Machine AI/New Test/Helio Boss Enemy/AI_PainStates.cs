using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;

public class AI_PainStates : State<AI>{

	public AudioClip[] painSounds;
	private AudioClip currentAudioClip;

	public Coroutine currentPainCoro;

	private AI_HealthBar healthBar;
	private bool coroPlaying;
	private State<AI> targetState;

	private AudioClip swordClash;

	void Start(){
		healthBar = GetComponent<AI_HealthBar> ();
	}

	public override bool EnterState(AI _owner){
		_owner.currentState = this.ToString();
		targetState = _owner.GetComponent<AI_IdleState>();
		return true;
	}

	public override void ExitState(AI _owner){
	}

	public override void UpdateState(AI _owner){
		if (!coroPlaying)
			_owner.stateMachine.ChangeState(targetState);
	}

	public override void ForceInterruption(AI _owner) {
		if (currentCoroutine != null)
			StopCoroutine(currentCoroutine);
		_owner.coroRunning = false;
		coroPlaying = false;
		_owner.references.rigidbody.mass = _owner.references._Mass;
	}

	public IEnumerator Pain(AI _owner, Transform attacker, float stunTime, bool overthrows){
		coroPlaying = true;
		_owner.InterrupAllBehavior ();
		_owner.doNotMove = true;
		_owner.stateMachine.ChangeState(GetComponent<AI_PainStates>());
		_owner.references.rigidbody.mass = 1f;
		_owner.navMeshAgent.enabled = false;
		_owner.references.animationScript.inPain = true;


		if (!overthrows && _owner.isGrounded && !_owner.references.animationScript.isStandingUp) {
			_owner.references.capsuleCollider.radius = _owner.references.capsuleColliderRadius;
			_owner.references.animationScript.anim.Play ("Pain", -1, normalizedTime: 0.0f);

			if (painSounds.Length > 0) {
				currentAudioClip = painSounds[Random.Range(0, painSounds.Length)];
				CustomMethods.PlayClipAt(currentAudioClip, transform.position);
			}


			GetComponent<Player_Animation> ().isStandingUp = false;
			yield return new WaitForFixedUpdate ();
	
			while (stunTime > 0f) {
				if (_owner.isGrounded)
					stunTime -= Time.deltaTime;
				_owner.navMeshAgent.enabled = false;
				yield return null;
			}

			yield return new WaitForSeconds(healthBar.standUpDelay);
			_owner.references.animationScript.isStandingUp = true;
			yield return new WaitForFixedUpdate ();
			_owner.references.animationScript.isStandingUp = false;

			_owner.references.rigidbody.mass = 50f;
			GetComponent<Collider> ().material.dynamicFriction = 0.75f; 
			GetComponent<Collider> ().material.staticFriction = 0.75f;
			_owner.references.rigidbody.angularDrag = 0.05f;

			_owner.references.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
			_owner.references.rigidbody.isKinematic = false;
			_owner.navMeshAgent.enabled = true;
			_owner.navMeshAgent.updateRotation = true;

			_owner.doNotMove = false;
			_owner.references.animationScript.inPain = false;
			healthBar.inPain = false;
			coroPlaying = false;
			currentPainCoro = null;
			yield break;
		} else if (overthrows || !_owner.isGrounded || _owner.references.animationScript.isStandingUp) {
			//_owner.references.animationScript.inPain = true;
			//Debug.Break();
			StartCoroutine (CustomMethods.SmoothRotateTowards (gameObject, attacker, 0.1f));
			//GetComponent<CapsuleCollider> ().radius = _owner.references.capsuleColliderRadius * 2.25f;
			_owner.references.capsuleCollider.radius = _owner.references.capsuleColliderRadius * 2.25f;
			_owner.references.animationScript.anim.Play("Pain Air", -1, normalizedTime: 0.0f);

			if (painSounds.Length > 0) {
				currentAudioClip = painSounds[Random.Range(0, painSounds.Length)];
				CustomMethods.PlayClipAt(currentAudioClip, transform.position);
			}

			_owner.references.animationScript.isStandingUp = false;
			_owner.references.animationScript.isGrounded = false;
            _owner.stopGroundCheck = true;
			_owner.isGrounded = false;
			_owner.transform.position = _owner.transform.position + Vector3.up * 0.1f;

			float t = 0.2f;
			while (t > 0f) {
				_owner.isGrounded = false;
				_owner.references.animationScript.isGrounded = false;
				t -= Time.deltaTime;
				yield return null;
            }

			//yield return new WaitForFixedUpdate ();
			_owner.stopGroundCheck = false;
			_owner.isGrounded = false;
			_owner.references.animationScript.isGrounded = false;

			while (!_owner.isGrounded) {
				//_owner.references.capsuleCollider.radius = _owner.references.capsuleColliderRadius * 2.25f;
				yield return new WaitForFixedUpdate ();
			}

			//el personaje vuelve en sí
			_owner.references.Landing();
			_owner.references.animationScript.anim.Play ("Lay On Ground", -1, normalizedTime: 0.0f);
			//GetComponent<CapsuleCollider> ().radius = _owner.references.capsuleColliderRadius;
			while (stunTime > 0f || _owner.references.rigidbody.velocity.magnitude > 0.75f) {
				//_owner.references.capsuleCollider.radius = _owner.references.capsuleColliderRadius * 2.25f;
				if (_owner.isGrounded) {
					//_owner.references.animationScript.anim.Play("Lay On Ground", -1, normalizedTime: 1.0f); 
					_owner.references.rigidbody.mass = Mathf.Lerp (_owner.references.rigidbody.mass, 50f, 10f * Time.deltaTime); //De esta forma impido que el pj se vea arrastrado excesivamente, es como si simulara fricción.
					stunTime -= Time.deltaTime;
				}
				//_owner.navMeshAgent.enabled = false;
				yield return null;
			}

			//_owner.references.animationScript.anim.Play("Lay On Ground", -1, normalizedTime: 1.0f); //Refuerzo para evitar el bug, en que el pj se levanta aunque aun no está listo para continunar.
			//GetComponent<Player_Animation> ().isStandingUp = false;
			//yield return new WaitForFixedUpdate ();
			//healthBar.inPain = false;

			yield return new WaitForSeconds(healthBar.standUpDelay);

			_owner.references.animationScript.inPain = false;
			_owner.references.animationScript.isStandingUp = true;

			_owner.references.rigidbody.mass = 50f;
			_owner.references.rigidbody.angularDrag = 0.05f;

			_owner.references.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
			_owner.references.rigidbody.isKinematic = false;

			while (_owner.references.animationScript.anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.00f) {
				yield return null;
            }

			yield return new WaitForSeconds (1f);
			_owner.references.animationScript.isStandingUp = false;
			_owner.doNotMove = false;
			//_owner.navMeshAgent.enabled = true; //por ahora lo apago, porque no depende del pain state definir esto, sino del move towards enemy state
			//_owner.navMeshAgent.updateRotation = true; //lo mismo que arriba
			coroPlaying = false;
			currentPainCoro = null;
			yield break;
		}
	}

	public IEnumerator BulletPain(AI _owner){
		coroPlaying = true;
		_owner.InterrupAllBehavior ();
		_owner.stateMachine.ChangeState (GetComponent<AI_PainStates> ());
		_owner.references.animationScript.ResetValues ();

		///////////////////////////////////////////////////////
		//_owner.animationScript.inPain = true;
		GetComponent<Animator> ().Play ("Pain", -1, normalizedTime: 0.0f);
		///////////////////////////////////////////////////////

		float painTimer = healthBar.painTimer * 0.5f;
		while (painTimer > 0f || !_owner.isGrounded) {
			painTimer -= Time.deltaTime;
			_owner.navMeshAgent.enabled = false;
			yield return null;
		}

		//el personaje vuelve en sí
		yield return new WaitForSeconds(1.0f);
		_owner.references.collider.material.dynamicFriction = 0.75f; 
		_owner.references.collider.material.staticFriction = 0.75f;

		_owner.references.animationScript.inPain = false;
		_owner.doNotMove = false;
		healthBar.inPain = false;
		coroPlaying = false;
		yield return null;
	}

	public IEnumerator SwordCrash(AI _owner, Transform other){
		coroPlaying = true;
		_owner.InterrupAllBehavior ();
		_owner.stateMachine.ChangeState (GetComponent<AI_PainStates> ());
		_owner.references.animationScript.ResetValues ();
		_owner.references.animationScript.anim.Play ("SwordCrash", -1);

		if (healthBar.SwordSpark != null && GetComponent<References> ().RightHandWeapon) {
			GameObject ImpactSpark = Instantiate (healthBar.SwordSpark, transform.position + transform.forward * 0.35f + transform.up * 0.35f, transform.rotation);
			ImpactSpark.GetComponent<Renderer> ().material.SetColor ("_Color", GetComponent<References> ().RightHandWeapon.GetComponent<NGS_NewCPU> ().SwordSparkColor);
		}
			
		//_owner.animationScript.Sword001 = false;
		_owner.references.animationScript.fire = false;

		/**********************************************************
			//animator.Play ("SwordCrash", -1, normalizedTime: 0.0f);
			**********************************************************/
		_owner.references.rigidbody.velocity = Vector3.zero;
		if (_owner.isGrounded)
			_owner.references.rigidbody.AddForce ((transform.position - other.position).normalized * 3f, ForceMode.VelocityChange);
		else
			_owner.references.rigidbody.AddForce ((transform.position - other.position).normalized * 8f, ForceMode.VelocityChange);

		yield return new WaitForFixedUpdate ();
		CustomMethods.PlayClipAt (GetComponent<References> ().RightHandWeapon.GetComponent<NGS_NewCPU> ().swordClash, transform.position);

		yield return new WaitForSeconds (1f);
		if (_owner.healthBar.CurHealth < _owner.healthBar.Maxhealth * 2/3) {
			targetState = _owner.GetComponent<AI_StrongAtkState>();
			_owner.stateMachine.ChangeState(targetState);
		}
		healthBar.swordsCrashed = false;
		coroPlaying = false;
	}

	public IEnumerator Death(AI _owner){
		coroPlaying = true;
		_owner.InterrupAllBehavior ();
		_owner.stateMachine.ChangeState(GetComponent<AI_PainStates>());
		//_owner.references.rigidbody.constraints = RigidbodyConstraints.None;
		_owner.references.rigidbody.drag = 1f;
		_owner.references.rigidbody.AddForce (-transform.forward * 2f, ForceMode.VelocityChange);
		_owner.GetComponent<CapsuleCollider> ().height = _owner.references.capsuleColliderHeight * 0.5f;
		_owner.GetComponent<CapsuleCollider> ().radius = _owner.references.capsuleColliderRadius * 2f;
		_owner.GetComponent<CapsuleCollider> ().center = _owner.references.capsuleColliderCenter - Vector3.up * _owner.references.capsuleColliderHeight/4.5f;
		_owner.GetComponent<CapsuleCollider> ().material.dynamicFriction = 1.0f;
		//_owner.GetComponent<CapsuleCollider> ().enabled = false;
		List<Collider> colliders = _owner.GetComponent<ColliderManager> ().collidersList;
		foreach (Collider col in colliders) {
			col.gameObject.layer = 17;
			col.isTrigger = false;
		}

		_owner.transform.rotation = Quaternion.Euler (0f, transform.rotation.eulerAngles.y, 0f);

		GetComponent<CapsuleCollider> ().material.dynamicFriction = 0.65f;
		gameObject.layer = 17;

		_owner.references.animationScript.ResetValues();
		_owner.references.animationScript.isDead = true;
		yield return new WaitForEndOfFrame ();

		if (_owner.references.RightHandWeapon != null) {
			_owner.references.RightHandWeapon.transform.parent = null;
			_owner.references.RightHandWeapon.layer = 17;
			_owner.references.RightHandWeapon.GetComponent<Collider> ().enabled = true;
			_owner.references.RightHandWeapon.GetComponent<Collider> ().isTrigger = false;
			_owner.references.RightHandWeapon.AddComponent<Rigidbody> ();
			Rigidbody enemyWeaponRigidbody = _owner.references.RightHandWeapon.GetComponent<Rigidbody>();
			enemyWeaponRigidbody.isKinematic = false;
			enemyWeaponRigidbody.useGravity = true;
			enemyWeaponRigidbody.GetComponent<Rigidbody>().maxAngularVelocity = 10f;
			enemyWeaponRigidbody.mass = 1f;
			enemyWeaponRigidbody.drag = 0f;
			enemyWeaponRigidbody.AddForce (transform.forward * Random.Range (0.5f, 1.0f), ForceMode.VelocityChange);
			enemyWeaponRigidbody.AddForce (Vector3.up * (Random.Range (1f, 2.5f)), ForceMode.VelocityChange);
			enemyWeaponRigidbody.AddTorque (new Vector3 (Random.Range (-5f, 5f ), Random.Range (-5f /1f, 5f /1f ), Random.Range (-5f, 5f )));
		}

		if (_owner.references.LeftHandWeapon != null) {
			_owner.references.LeftHandWeapon.transform.parent = null;
			_owner.references.LeftHandWeapon.layer = 17;
			_owner.references.LeftHandWeapon.GetComponent<Collider> ().enabled = true;
			_owner.references.LeftHandWeapon.GetComponent<Collider> ().isTrigger = false;
			_owner.references.LeftHandWeapon.AddComponent<Rigidbody> ();
			Rigidbody enemyWeaponRigidbody = _owner.references.LeftHandWeapon.GetComponent<Rigidbody>();
			enemyWeaponRigidbody.isKinematic = false;
			enemyWeaponRigidbody.useGravity = true;
			enemyWeaponRigidbody.GetComponent<Rigidbody>().maxAngularVelocity = 10f;
			enemyWeaponRigidbody.mass = 1f;
			enemyWeaponRigidbody.drag = 0f;
			enemyWeaponRigidbody.AddForce (transform.forward * Random.Range (0.5f, 1.0f), ForceMode.VelocityChange);
			enemyWeaponRigidbody.AddForce (Vector3.up * (Random.Range (1f, 2.5f)), ForceMode.VelocityChange);
			enemyWeaponRigidbody.AddTorque (new Vector3 (Random.Range (-5f, 5f ), Random.Range (-5f /1f, 5f /1f ), Random.Range (-5f, 5f )));
		}

		coroPlaying = false;

		if (!_owner.isGrounded) {
			_owner.references.animationScript.anim.Play ("Pain Air", -1, normalizedTime: 0.0f);
			while (!_owner.isGrounded) {
				yield return null;
			}
			_owner.references.Landing();
			_owner.references.animationScript.anim.Play ("Lay On Ground", -1, normalizedTime: 0.0f);
		}

		yield return new WaitForSeconds (30f);
		if (healthBar.removedEffect) {
			GameObject removedEffectGO = Instantiate (healthBar.removedEffect);
			removedEffectGO.transform.position = transform.position;
			removedEffectGO.GetComponent<ParticleSystem> ().Play();
			Destroy (removedEffectGO, 2f);
		}	
		Destroy (gameObject, 0.1f);
		Destroy (_owner.references.LeftHandWeapon, 0.1f);
		Destroy (_owner.references.RightHandWeapon, 0.1f);
		yield return null;
	}
}


