using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using StateStuff;

public class AI : MonoBehaviour{

	[HideInInspector] public StateMachine<AI> stateMachine { get; set; }
	public string currentState;

	[HideInInspector] public References references;
	[HideInInspector] public HealthBar healthBar;
	[HideInInspector] public AvoidAllies avoidAlliesScript;

	[Header("")]
	public bool hasLeader = false;
	public GameObject leader;
	public float maxDistanceFromLead = 10f;

	[Header("")]
	public bool isBlader;
	public bool isGunner;
	public bool isCaster;

	public bool keepItCool;

	public float chanceToChangeTarget = 50f;
	[HideInInspector] public float changeTargetRefresh;


	[Header("Status")]
	public bool doNotMove;
	public bool checkForGround = true;
	public bool isGrounded;
	public bool stopGroundCheck;
	public bool targetIsInSight;
	public bool isAlerted = false;
	public float distance;
	public float velocityLimitXZ;

	/*[HideInInspector]*/ public bool visible;

	[HideInInspector] public float extraGravity;

	public List <GameObject> listOfEnemies;
	public GameObject target;
	/*[HideInInspector]*/ public HealthBar targetHealthBar;


	public LayerMask searchingForTargetIgnoreLayers;
	public LayerMask isGroundedIgnoreLayers;

	public GameObject Gun1;
	public GameObject Gun2;
	public GameObject Gun3;

	public GameObject Sword1;
	public GameObject Sword2;
	public GameObject Sword3;

	[HideInInspector] public NavMeshAgent navMeshAgent;
	[HideInInspector] public bool coroRunning;

	[HideInInspector] public RaycastHit hit2;
	public State<AI>[] statesList;

	[HideInInspector] public float alertDistance = 0f;

	[HideInInspector] public Coroutine settingEnemies;
	[HideInInspector] public Coroutine updatingTarget;


	public virtual void Start() {
		extraGravity = PlayerPrefs.GetFloat("Extra Gravity", 0.15f);

		//listOfEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag ("Player"));
		//listOfEnemies.AddRange (GameObject.FindGameObjectsWithTag ("Enemy"));

		stateMachine = new StateMachine<AI>(this);
		stateMachine.ChangeState(GetComponent<AI_IdleState>());

		references = GetComponent<References>();
		avoidAlliesScript = transform.Find("AlliesDetector").GetComponent<AvoidAllies>();
		healthBar = GetComponent<HealthBar>();

		navMeshAgent = GetComponent<NavMeshAgent>();

		searchingForTargetIgnoreLayers = ~searchingForTargetIgnoreLayers;
		isGroundedIgnoreLayers = ~isGroundedIgnoreLayers;

		//DeactivateAllGuns ();

		if (Sword1) {
			Sword1.GetComponent<NGS_NewCPU>().StartupAI(gameObject);
			references.RightHandWeapon = Sword1;
			Sword1.SetActive(true);
		}
		if (Sword2) {
			Sword2.GetComponent<NGS_NewCPU>().StartupAI(gameObject);
			Sword2.SetActive(false);
		}
		if (Sword3) {
			Sword3.GetComponent<NGS_NewCPU>().StartupAI(gameObject);
			Sword3.SetActive(false);
		}

		if (Gun1) {
			Gun1.GetComponent<GenericGun>().Player = this.gameObject;
			references.LeftHandWeapon = Gun1;
			Gun1.SetActive(true);
		}
		if (Gun2) {
			Gun2.GetComponent<GenericGun>().Player = this.gameObject;
			Gun2.SetActive(false);
		}
		if (Gun3) {
			Gun3.GetComponent<GenericGun>().Player = this.gameObject;
			Gun3.SetActive(false);
		}

		statesList = GetComponents<State<AI>>();

		if (hasLeader && leader == null)
			leader = GameObject.FindWithTag("Player");

		//InvokeRepeating("SetEnemies", Random.Range(0.01f, 0.02f), Random.Range(0.875f, 0.125f));
		//InvokeRepeating("UpdateTarget", Random.Range(0.01f, 0.02f), Random.Range(0.875f, 0.125f));
	}

	public virtual void OnBecameVisible() {
		visible = true;
	}

	public virtual void OnBecameInvisible() {
		visible = false;
	}

	public virtual void Update() {
		stateMachine.Update();
		SetEnemies();
		UpdateTarget();
	}

	public virtual void LateUpdate() {
		changeTargetRefresh -= Time.deltaTime;
		if (changeTargetRefresh < 0f)
			changeTargetRefresh = 0f;

		if (!isGrounded)
			navMeshAgent.enabled = false;
	}

	public virtual void FixedUpdate() {
		//SetEnemies ();
		//UpdateTarget ();	
		UpdateInfo();
		//stateMachine.Update ();
		LimitVelocityXZ();

		if (!isGrounded) {
			if (references.rigidbody.velocity.y < -0.125f) {
				references.rigidbody.AddForce(Vector3.down * extraGravity, ForceMode.VelocityChange);
				references.animationScript.isFalling = true;
			}
		} else
			references.animationScript.isFalling = false;
	}

	public virtual void UpdateInfo() {

		if (references.renderer)
			visible = references._renderer.isVisible;

		if (healthBar.isDead) {
			stateMachine.stopBehavior = true;
		}

		if (!navMeshAgent.enabled || navMeshAgent.isStopped)
			references.animationScript.isMoving = false;


		if (checkForGround) {
			if (Physics.SphereCast(transform.position, 0.2f, Vector3.down, out hit2, references.collider.bounds.extents.y + 0.01f, isGroundedIgnoreLayers)) {
				isGrounded = true;
				references.animationScript.isGrounded = true;
			} else {
				isGrounded = false;
				references.animationScript.isGrounded = false;
			}
		}

		if (target) {
			if (targetHealthBar == null)
				targetHealthBar = target.GetComponent<HealthBar>();

			distance = Vector3.Distance(transform.position, target.transform.position);
			Physics.Raycast(references.Head.transform.position, (target.transform.position - references.Head.transform.position).normalized, out hit2, Mathf.Infinity, searchingForTargetIgnoreLayers);
			if (hit2.collider && hit2.collider.gameObject == target)
				targetIsInSight = true;
			else
				targetIsInSight = false;
		} else if (hasLeader) {
			targetIsInSight = false;
			distance = Vector3.Distance(transform.position, leader.transform.position);
		}

		if (!keepItCool && target && distance < 10f)
			references.animationScript.comboMode = true;
		else
			references.animationScript.comboMode = false;
	}

	public virtual void SetEnemies() {
		if (settingEnemies == null && Random.Range (0f, 1f) < 0.5f)
			settingEnemies = StartCoroutine(CoroSetEnemies());
	}

	public virtual IEnumerator CoroSetEnemies() {
		if (listOfEnemies.Count == 0) {
			listOfEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
			listOfEnemies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
			if (listOfEnemies.Count > 0) {
				for (int i = listOfEnemies.Count - 1; i >= 0; i--) { //
					if (listOfEnemies[i] == null || listOfEnemies[i] == gameObject || listOfEnemies[i].GetComponent<HealthBar>().isDead ||
						!DiplomacyManager.AreEnemies(references.team, listOfEnemies[i].GetComponent<References>().team))
						listOfEnemies.RemoveAt(i);
					yield return null;
				}
			}
		}
		yield return null;
	}

	public virtual void UpdateTarget() {
		if (updatingTarget == null && Random.Range(0f, 1f) < 0.5f)
			updatingTarget = StartCoroutine(CoroUpdateTarget());
	}

	public virtual IEnumerator CoroUpdateTarget() {
		if (!hasLeader) {
			if (target && targetHealthBar && targetHealthBar.isDead) {
				target = null;
				targetHealthBar = null;
				listOfEnemies.Remove(target);
				//Al buscar el siguiente objetivo el código es el mismo que en PlayIdleState, sólo que no revisa si el target está delante o no.
				//Al tratarse ya de un estado de alerta, tiene sentido pensar que mirará a su alrededor en busca de un nuevo target.
				foreach (GameObject enemy in listOfEnemies) {
					if (enemy != null && Physics.Raycast(references.Head.transform.position, (enemy.transform.position - references.Head.transform.position).normalized, out hit2, alertDistance, searchingForTargetIgnoreLayers)) {
						if (hit2.collider.gameObject == enemy) {
							target = enemy;
							targetHealthBar = target.GetComponent<HealthBar>();
							//yield return StartCoroutine (CustomMethods.SmoothRotateTowards (gameObject, target.transform, 0.25f));
							break;
						}
					}
					yield return null;
				}
			}
		} else {
			if (target && targetHealthBar.isDead) {
				target = null;
				targetHealthBar = null;
				listOfEnemies.Remove(target);
				//Al buscar el siguiente objetivo el código es el mismo que en PlayIdleState, sólo que no revisa si el target está delante o no.
				//Al tratarse ya de un estado de alerta, tiene sentido pensar que mirará a su alrededor en busca de un nuevo target.
				foreach (GameObject enemy in listOfEnemies) {
					if (enemy != null && Physics.Raycast(references.Head.transform.position, (enemy.transform.position - references.Head.transform.position).normalized, out hit2, alertDistance, searchingForTargetIgnoreLayers) &&
						Vector3.SqrMagnitude(enemy.transform.position - leader.transform.position) > maxDistanceFromLead * maxDistanceFromLead) {
						if (hit2.collider.gameObject == enemy) {
							target = enemy;
							targetHealthBar = target.GetComponent<HealthBar>();
							//yield return StartCoroutine (CustomMethods.SmoothRotateTowards (gameObject, target.transform, 0.25f));
							break;
						}
					}
					yield return null;
				}
			}
		}

		if (changeTargetRefresh <= 0f && Random.Range(0f, 100f) < chanceToChangeTarget) {
			changeTargetRefresh = 1f;
			for (int i = listOfEnemies.Count - 1; i >= 0; i--) { //
				if (listOfEnemies[i] == null || listOfEnemies[i] == gameObject || listOfEnemies[i].GetComponent<HealthBar>().isDead ||
					!DiplomacyManager.AreEnemies(GetComponent<References>().team, listOfEnemies[i].GetComponent<References>().team))
					listOfEnemies.RemoveAt(i);
				else if (distance > Vector3.Distance(transform.position, listOfEnemies[i].transform.position) + distance * Random.Range(-0.25f, 0.25f) &&
					Physics.Raycast(references.Head.transform.position, (listOfEnemies[i].transform.position - references.Head.transform.position).normalized, out hit2, 10f, searchingForTargetIgnoreLayers)) {
					if (hit2.collider.gameObject == listOfEnemies[i]) {
						target = listOfEnemies[i];
						targetHealthBar = target.GetComponent<HealthBar>();
						break;
					}
				}
				yield return null;
			}
		}
	}



	public virtual IEnumerator ReceivedAttackFrom(Transform attacker) {
		/*
		float timer = healthBar.painTimer;
		while (timer > 0f) {
			timer -= Time.deltaTime;
			yield return null;
		}
		*/

		if (target && Random.Range(0f, 100f) < chanceToChangeTarget) {
			if (distance * Random.Range(10f, 15f) < Vector3.Distance(attacker.position, target.transform.position) + distance * Random.Range(-0.25f, 0.25f) &&
				Physics.Raycast(references.Head.transform.position, (attacker.transform.position - references.Head.transform.position).normalized, out hit2, alertDistance, searchingForTargetIgnoreLayers)) {
				if (hit2.collider.gameObject == attacker.gameObject) {
					changeTargetRefresh = 1f;
					target = attacker.gameObject;
					targetHealthBar = target.GetComponent<HealthBar>();
				}
			}
		} else if (!target) {
			if (Physics.Raycast(references.Head.transform.position, (attacker.transform.position - references.Head.transform.position).normalized, out hit2, alertDistance, searchingForTargetIgnoreLayers)) {
				if (hit2.collider.gameObject == attacker.gameObject) {
					target = attacker.gameObject;
					targetHealthBar = target.GetComponent<HealthBar>();
				}
			}
		}
		yield return null;
	}

	public virtual void DeactivateAllGuns() {
		if (Gun1)
			Gun1.SetActive(false);
		if (Gun2)
			Gun2.SetActive(false);
		if (Gun3)
			Gun3.SetActive(false);
	}

	public virtual void StartDeathState() {
		InterrupAllBehavior();
		stateMachine.ChangeState(GetComponent<AI_PainStates>());
		StartCoroutine(GetComponent<AI_PainStates>().Death(this));
	}

	public virtual void StartPainState(Transform attacker, float stunTime, bool overthrows) {
		if (GetComponent<AI_PainStates>().currentPainCoro != null) {
			StopCoroutine(GetComponent<AI_PainStates>().currentPainCoro);
		}
		InterrupAllBehavior();
		stateMachine.ChangeState(GetComponent<AI_PainStates>());
		GetComponent<AI_PainStates>().currentPainCoro = StartCoroutine(GetComponent<AI_PainStates>().Pain(this, attacker, stunTime, overthrows));
	}

	public virtual void StartBulletPainState() {
		if (GetComponent<AI_PainStates>().currentPainCoro != null) {
			StopCoroutine(GetComponent<AI_PainStates>().currentPainCoro);
		}
		InterrupAllBehavior();
		stateMachine.ChangeState(GetComponent<AI_PainStates>());
		GetComponent<AI_PainStates>().currentPainCoro = StartCoroutine(GetComponent<AI_PainStates>().BulletPain(this));
	}

	public virtual void StartSwordCrashState(Transform other) {
		InterrupAllBehavior();
		stateMachine.ChangeState(GetComponent<AI_PainStates>());
		StartCoroutine(GetComponent<AI_PainStates>().SwordCrash(this, other));
	}

	public virtual bool StartComboBreaker() {
		if (GetComponent<AI_ComboBreaker>()) {
			InterrupAllBehavior();
			stateMachine.ChangeState(GetComponent<AI_ComboBreaker>());
			GetComponent<AI_ComboBreaker>().currentCoroutine = StartCoroutine(GetComponent<AI_ComboBreaker>().ComboBreaker(this));
			return true;
		}
		return false;
	}

	public virtual void InterrupAllBehavior() {
		references.animationScript.ResetValues();
		references.ToggleSwordDamage(0);
		if (Sword1) {
			//Sword1.GetComponent<BoxCollider> ().size = Sword1.GetComponent<NGS_NewCPU> ().boxColliderOrigSize;
			Sword1.GetComponent<NGS_NewCPU>().ForcedReset();
		}
		velocityLimitXZ = 0f;
		foreach (State<AI> state in statesList) {
			state.ForceInterruption(this);
		}
	}

	public virtual void LimitVelocityXZ() {
		float velocityY = references.rigidbody.velocity.y;
		references.rigidbody.velocity = new Vector3(references.rigidbody.velocity.x, 0f, references.rigidbody.velocity.z);

		if (velocityLimitXZ > 0f) {
			if (references.rigidbody.velocity.magnitude > velocityLimitXZ) {
				references.rigidbody.velocity = references.rigidbody.velocity.normalized * velocityLimitXZ;
			}
		}
		references.rigidbody.velocity = new Vector3(references.rigidbody.velocity.x, velocityY, references.rigidbody.velocity.z);
	}
}
