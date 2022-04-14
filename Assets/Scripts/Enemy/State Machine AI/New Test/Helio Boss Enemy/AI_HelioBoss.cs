using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using StateStuff;

public class AI_HelioBoss : AI{	

	private float extraGravity;
	private RaycastHit hit2;

    private void Start(){

		extraGravity = PlayerPrefs.GetFloat ("Extra Gravity", 0.15f);

		listOfEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag ("Player"));
		listOfEnemies.AddRange (GameObject.FindGameObjectsWithTag ("Enemy"));
		/*
		for (int i = listOfEnemies.Count - 1; i >= 0; i--){ //
			if (listOfEnemies[i] == null || listOfEnemies[i] == gameObject  || listOfEnemies[i].GetComponent<HealthBar>().isDead ||
				!DiplomacyManager.AreEnemies(listOfEnemies[i].GetComponent<References>().team, GetComponent<References>().team))
				listOfEnemies.RemoveAt(i);
		}
		*/

		stateMachine = new StateMachine<AI> (this);
		stateMachine.ChangeState (GetComponent<AI_IdleState>());

		references = GetComponent<References> ();
		avoidAlliesScript = transform.Find("AlliesDetector").GetComponent<AvoidAllies> ();
		healthBar = GetComponent<HealthBar> ();

		navMeshAgent = GetComponent<NavMeshAgent> ();

		searchingForTargetIgnoreLayers = ~searchingForTargetIgnoreLayers;
		isGroundedIgnoreLayers = ~isGroundedIgnoreLayers;

		DeactivateAllGuns ();

		if (Sword1) {
			Sword1.GetComponent<NGS_NewCPU> ().StartupAI (gameObject);
			references.RightHandWeapon = Sword1;
		}
		if (Sword2) {
			Sword2.GetComponent<NGS_NewCPU> ().StartupAI (gameObject);
			Sword2.SetActive (false);
		}
		if (Sword3) {
			Sword3.GetComponent<NGS_NewCPU> ().StartupAI (gameObject);
			Sword3.SetActive (false);
		}

		if (Gun1) {
			Gun1.GetComponent<GenericGun> ().Player = this.gameObject;
			references.LeftHandWeapon = Gun1;
		}
		if (Gun2) {
			Gun2.GetComponent<GenericGun> ().Player = this.gameObject;
			Gun2.SetActive (false);
		}
		if (Gun3) {
			Gun3.GetComponent<GenericGun> ().Player = this.gameObject;
			Gun3.SetActive (false);
		}

		statesList = GetComponents<State<AI>> ();
	}

	void LateUpdate(){
		changeTargetRefresh -= Time.deltaTime;
		if (changeTargetRefresh < 0f)
			changeTargetRefresh = 0f;

		if (!isGrounded)
			navMeshAgent.enabled = false;
	}

    private void FixedUpdate(){
		SetEnemies ();
		UpdateTarget ();
		UpdateInfo ();
		stateMachine.Update ();
		LimitVelocityXZ ();

		if (!isGrounded) {
			if (references.rigidbody.velocity.y < -0.125f) {
				references.rigidbody.AddForce (Vector3.down * extraGravity, ForceMode.VelocityChange);
				references.animationScript.isFalling = true;
			}
		} else
			references.animationScript.isFalling = false;
	}

	public void UpdateInfo(){

		if (healthBar.isDead) {
			stateMachine.stopBehavior = true;
		}

		if (!navMeshAgent.enabled || navMeshAgent.isStopped)
			references.animationScript.isMoving = false;
		
		if (Physics.SphereCast (transform.position, 0.2f, Vector3.down, out hit2, references.collider.bounds.extents.y + 0.01f, isGroundedIgnoreLayers)) {
			isGrounded = true;
			references.animationScript.isGrounded = true;
		} else {
			isGrounded = false;
			references.animationScript.isGrounded = false;
		}

		if (target) {
			distance = Vector3.Distance (transform.position, target.transform.position);
			targetIsInSight = Physics.Raycast (references.Head.transform.position, (target.transform.position - references.Head.transform.position).normalized, out hit2, Mathf.Infinity, searchingForTargetIgnoreLayers);
		}

		if (!keepItCool && isAlerted && distance < 10f)
			references.animationScript.comboMode = true;
		else
			references.animationScript.comboMode = false;
	}

	void SetEnemies(){
		if (listOfEnemies.Count == 0) {
			listOfEnemies = new List<GameObject> (GameObject.FindGameObjectsWithTag ("Player"));
			listOfEnemies.AddRange (GameObject.FindGameObjectsWithTag ("Enemy"));
			if (listOfEnemies.Count > 0) {
				for (int i = listOfEnemies.Count - 1; i >= 0; i--) { //
					if (listOfEnemies [i] == null || listOfEnemies [i] == gameObject || listOfEnemies [i].GetComponent<HealthBar> ().isDead ||
						!DiplomacyManager.AreEnemies (references.team, listOfEnemies [i].GetComponent<References> ().team))
						listOfEnemies.RemoveAt (i);
				}
			}
		}
	}

	public IEnumerator ReceivedAttackFrom (Transform attacker){
		float timer = healthBar.painTimer;
		while (timer > 0f) {
			timer -= Time.deltaTime;
			yield return null;
		}
		if (target && Random.Range (0f, 100f) < chanceToChangeTarget) {
			if (distance * Random.Range (10f, 15f) < Vector3.Distance (attacker.position, target.transform.position) + distance * Random.Range (-0.25f, 0.25f) &&
				Physics.Raycast (references.Head.transform.position, (attacker.transform.position - references.Head.transform.position).normalized, out hit2, alertDistance, searchingForTargetIgnoreLayers)) {
				if (hit2.collider.gameObject == attacker.gameObject) {
					changeTargetRefresh = 1f;
					target = attacker.gameObject;
				}
			}
		} else if (!target) {
			if (Physics.Raycast (references.Head.transform.position, (attacker.transform.position - references.Head.transform.position).normalized, out hit2, alertDistance, searchingForTargetIgnoreLayers)) {
				if (hit2.collider.gameObject == attacker.gameObject) {
					target = attacker.gameObject;
				}
			}
		}
	}

	void UpdateTarget(){

		if (target && target.GetComponent<HealthBar> ().isDead) {
			target = null;
			listOfEnemies.Remove (target);
			//Al buscar el siguiente objetivo el código es el mismo que en PlayIdleState, sólo que no revisa si el target está delante o no.
			//Al tratarse ya de un estado de alerta, tiene sentido pensar que mirará a su alrededor en busca de un nuevo target.
			foreach (GameObject enemy in listOfEnemies) {
				if (enemy != null && Physics.Raycast (references.Head.transform.position, (enemy.transform.position - references.Head.transform.position).normalized, out hit2, alertDistance, searchingForTargetIgnoreLayers)) {
					if (hit2.collider.gameObject == enemy){
						target = enemy;
						//yield return StartCoroutine (CustomMethods.SmoothRotateTowards (gameObject, target.transform, 0.25f));
						break;
					}
				}
			}

		}

		if (changeTargetRefresh <= 0f && Random.Range (0f, 100f) < chanceToChangeTarget) {
			changeTargetRefresh = 1f;
			for (int i = listOfEnemies.Count - 1; i >= 0; i--) { //
				if (listOfEnemies[i] == null || listOfEnemies[i] == gameObject  || listOfEnemies[i].GetComponent<HealthBar>().isDead ||
				    !DiplomacyManager.AreEnemies (GetComponent<References> ().team, listOfEnemies [i].GetComponent<References> ().team))
					listOfEnemies.RemoveAt (i);
				else if (distance > Vector3.Distance (transform.position, listOfEnemies [i].transform.position) + distance * Random.Range (-0.25f , 0.25f) &&
				         Physics.Raycast (references.Head.transform.position, (listOfEnemies [i].transform.position - references.Head.transform.position).normalized, out hit2, 10f, searchingForTargetIgnoreLayers)) {
					if (hit2.collider.gameObject == listOfEnemies [i]) {
						target = listOfEnemies [i];
						break;
					}
				}
			}
		}
	}


	public void DeactivateAllGuns(){
		if (Gun1)
			Gun1.SetActive (false);
		if (Gun2)
			Gun2.SetActive (false);
		if (Gun3)
			Gun3.SetActive (false);
	}

	public void StartDeathState(){
		StartCoroutine (GetComponent<AI_PainStates> ().Death (this));
	}

	public void StartPainState(Transform attacker, float stunTime, bool overthrows){
		if (GetComponent<AI_PainStates> ().currentPainCoro != null) {
			StopCoroutine (GetComponent<AI_PainStates> ().currentPainCoro);
		}
		GetComponent<AI_PainStates>().currentPainCoro = StartCoroutine (GetComponent<AI_PainStates> ().Pain (this, attacker, stunTime, overthrows));
	}

	public void StartBulletPainState(){
		if (GetComponent<AI_PainStates> ().currentPainCoro != null) {
			StopCoroutine (GetComponent<AI_PainStates> ().currentPainCoro);
		}
		GetComponent<AI_PainStates>().currentPainCoro = StartCoroutine (GetComponent<AI_PainStates> ().BulletPain (this));
	}

	public void StartSwordCrashState(Transform other){
		StartCoroutine (GetComponent<AI_PainStates> ().SwordCrash (this, other));
	}

	public bool StartComboBreaker(){
		if (GetComponent<AI_ComboBreaker> ()) {
			GetComponent<AI_ComboBreaker> ().currentCoroutine = StartCoroutine (GetComponent<AI_ComboBreaker> ().ComboBreaker (this));
			return true;
		}
		return false;
	}

	public void InterrupAllBehavior(){
		references.animationScript.ResetValues ();
		references.ToggleSwordDamage (0);
		Sword1.GetComponent<BoxCollider> ().size = Sword1.GetComponent<NGS_NewCPU> ().boxColliderOrigSize;
		velocityLimitXZ = 0f;
		foreach (State<AI> state in statesList) {
			state.ForceInterruption (this);
		}
	}

	public void LimitVelocityXZ(){
		float velocityY = references.rigidbody.velocity.y;		
		references.rigidbody.velocity = new Vector3 (references.rigidbody.velocity.x, 0f, references.rigidbody.velocity.z);

		if (velocityLimitXZ > 0f) {
			if (references.rigidbody.velocity.magnitude > velocityLimitXZ) {
				references.rigidbody.velocity = references.rigidbody.velocity.normalized * velocityLimitXZ;
			}
		}
		references.rigidbody.velocity = new Vector3 (references.rigidbody.velocity.x, velocityY, references.rigidbody.velocity.z);
	}
}
