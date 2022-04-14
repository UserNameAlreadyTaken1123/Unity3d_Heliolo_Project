using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Luminosity.IO;

public class EnemiesDetector : MonoBehaviour {


	private bool gottaFindTarget;
	public bool foundTarget;
	public bool foundAlly;
	public bool lookingAtAnEnemy = false;
	private GameObject Player;
	public GameObject target;
	private Vector3 lookTarget;
	private bool comboMode;
//	public int numberOfEnemies;

	public List<GameObject> enemiesInRange;
	public List<GameObject> enemiesInDirection;
	public List<GameObject> alliesInRange;
	private GameObject closestEnemy;
	private GameObject closestAlly;
	private float radius;

	private Vector3 forwardDirection;
	private Vector3 rightDirection;
	public Vector3 inputDirection;

	private Material[] targetOriginalMaterials;
	private Material[] targetCurrentMaterials;

	private RaycastHit hitInfo;
	bool initialized = false;

	// Use this for initialization

	void Start(){
		radius = GetComponent<SphereCollider> ().radius;
		Player = transform.parent.gameObject;
		Physics.IgnoreCollision(transform.GetComponent<Collider>(), Player.transform.GetComponent<Collider>());
		Physics.IgnoreCollision(transform.GetComponent<Collider>(), Player.transform.GetComponent<Collider>());
		enemiesInRange.RemoveAll(item => item == null);
		alliesInRange.RemoveAll(item => item == null);
		Player.GetComponent<References> ().enemiesDetector = this;
		Invoke ("Initialize", 0.01f);
	}

	void Initialize () {
		initialized = true;
		enemiesInRange.RemoveAll(item => item == null);
		alliesInRange.RemoveAll(item => item == null);
	}

	void Update (){
		if (target && lookingAtAnEnemy) {
			if (target.GetComponent<HealthBar> () && target.GetComponent<HealthBar> ().isDead) {
				enemiesInRange.Remove (target);
				if (InputManager.GetButton ("Combo Mode"))
					gottaFindTarget = true;
			}				
		}

		for (var index = enemiesInRange.Count - 1; index > -1; index--) {
			if (enemiesInRange [index] == null)
				enemiesInRange.RemoveAt (index);
			if (enemiesInRange [index].GetComponent<HealthBar> () && enemiesInRange [index].GetComponent<HealthBar> ().isDead)
				enemiesInRange.RemoveAt (index);
		}

		if (enemiesInRange.Count > 0) {
			GetComponent<SphereCollider> ().radius = radius + 1f;
		} else {
			target = null;
			Player.GetComponent<References> ().currentAutoaimTarget = null;
			Player.GetComponent<References> ().currentAutoaimTargetLastLong = null;
			foundTarget = false;
		}

		if (alliesInRange.Count == 0) {
			foundAlly = false;
		}

		if (gottaFindTarget) {
			forwardDirection = Player.GetComponent<References> ().Camera.transform.forward * InputManager.GetAxis ("Vertical");
			rightDirection = Player.GetComponent<References> ().Camera.transform.right * InputManager.GetAxis ("Horizontal");
			inputDirection = (forwardDirection + rightDirection) * 100000;
			inputDirection.y = 0f;

			//Comparo si los controles "apuntan" hacia algún enemigo
			enemiesInDirection.Clear ();
			foreach (GameObject enemyInRange in enemiesInRange) {
				if (Vector3.Angle (inputDirection, enemyInRange.transform.position - Player.transform.position) < 20f)
					enemiesInDirection.Add (enemyInRange);
			}

			//comparo qué enemigos fueron apuntados para ver cual es el más cercano, sólo si alguno fue apuntado.
			//Si "enemiesInDirection" == 0, entonces sólo girar al enemigo más cercano.
			if (enemiesInDirection.Count > 0) {
				closestEnemy = enemiesInDirection [0];
				foreach (GameObject enemyInDir in enemiesInDirection) {
					if (Vector3.Distance (Player.transform.position, enemyInDir.transform.position) <
					    Vector3.Distance (Player.transform.position, closestEnemy.transform.position)) {
						closestEnemy = enemyInDir;
					}
				}

				target = closestEnemy;
				Player.GetComponent<References> ().currentAutoaimTarget = target;
				Player.GetComponent<References> ().currentAutoaimTargetLastLong = target;

			} else if (enemiesInRange.Count > 0) {
				closestEnemy = enemiesInRange [0];
				foreach (GameObject enemyInRange in enemiesInRange) {
					if (Vector3.Distance (Player.transform.position, enemyInRange.transform.position) <
						Vector3.Distance (Player.transform.position, closestEnemy.transform.position)){
						closestEnemy = enemyInRange;
					}
				}
				target = closestEnemy;
				Player.GetComponent<References> ().currentAutoaimTarget = target;
				Player.GetComponent<References> ().currentAutoaimTargetLastLong = target;

			}

			//Comparo si los controles "apuntan" hacia algún ALIADO
			//comparo qué enemigos fueron apuntados para ver cual es el más cercano, sólo si alguno fue apuntado.
			//Si "enemiesInDirection" == 0, entonces sólo girar al enemigo más cercano.
			if (alliesInRange.Count > 0) {
				closestAlly = alliesInRange [0];
				foreach (GameObject allyInDir in alliesInRange) {
					if (Vector3.Distance (Player.transform.position,
						allyInDir.transform.position) <
						Vector3.Distance (Player.transform.position, closestAlly.transform.position)) {
						closestAlly = allyInDir;
					}
				}

			} else {
				closestAlly = null;
				foundAlly = false;
			}
		}

		if (InputManager.GetButtonDown ("Reset Camera") && comboMode)
			gottaFindTarget = true;
		else if (InputManager.GetButtonDown ("Combo Mode"))
			gottaFindTarget = true;
		//else if (InputManager.GetButtonDown ("Melee") && (Mathf.Abs(InputManager.GetAxis ("Vertical")) > 0.1f || Mathf.Abs(InputManager.GetAxis ("Horizontal")) > 0.1f))
		//	gottaFindTarget = true;
		else
			gottaFindTarget = false;
	}


	// Update is called once per frame
	void LateUpdate () {
		if (InputManager.GetButtonDown ("Combo Mode")) {
			GetComponent<SphereCollider> ().radius = radius;
		} else if (InputManager.GetButtonUp ("Combo Mode")) {
			GetComponent<SphereCollider> ().radius = radius;
		}
		
		comboMode = InputManager.GetButton ("Combo Mode");
		if (target && comboMode) {
			lookingAtAnEnemy = true;

		} else if (!target && comboMode) {
			gottaFindTarget = true;
			lookingAtAnEnemy = false;
		}
		else {
			gottaFindTarget = false;
			lookingAtAnEnemy = false;
			if (target) { //si hubiera un target, hacerlo null. Si no hay target, no hacerlo null. Para evitar hacerlo en cada fotograma.
				target = null;
				Player.GetComponent<References> ().currentAutoaimTarget = null;
			}
		}
	
	}

	void OnTriggerEnter (Collider other){
		if (initialized) {
			if (other.gameObject != Player && other.GetComponent<HealthBar> ()) {
				if (DiplomacyManager.AreEnemies (Player.GetComponent<References> ().team, other.GetComponent<References> ().team)) {
					foundTarget = true;
					enemiesInRange.Add (other.gameObject);
				} else {
					foundAlly = true;
					alliesInRange.Add (other.gameObject);
				}
			}
		}
	}

	void OnTriggerExit (Collider other){
		if (initialized) {
			if (enemiesInRange.Contains (other.gameObject)) {
				enemiesInRange.Remove (other.gameObject);
			} else if (alliesInRange.Contains (other.gameObject)) {
				alliesInRange.Remove (other.gameObject);
			}

			if (InputManager.GetButton ("Combo Mode")) {
				if (enemiesInRange.Count == 0)
					Camera.main.GetComponent<Third_Person_Camera> ().rotation = Quaternion.Euler (Camera.main.transform.forward) * Camera.main.transform.rotation;
			}
		}
	}
}