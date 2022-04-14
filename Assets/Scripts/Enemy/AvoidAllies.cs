using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AvoidAllies : MonoBehaviour {


	public bool foundAlly;
	private GameObject thisBadGuy;
	private SphereCollider sphereCollider;
	public GameObject ally;

	public List<GameObject> alliesInRange;
	private int selectedAllyInArray;
	//private float radius;
	private bool initialized = false;

	// Use this for initialization
	void Start () {
		sphereCollider = GetComponent<SphereCollider> ();
		//radius = sphereCollider.radius;
		thisBadGuy = transform.parent.gameObject;
		Physics.IgnoreCollision(transform.GetComponent<Collider>(), thisBadGuy.transform.GetComponent<Collider>());
		selectedAllyInArray = 0;
		initialized = true;
	}
	
	// Update is called once per frame
	void FixedUpdate (){
//		Physics.IgnoreCollision(transform.GetComponent<Collider>(), thisBadGuy.transform.GetComponent<Collider>());
		if (initialized) {
			if (alliesInRange.Count > 0) {
				if (!alliesInRange [selectedAllyInArray]) {
					alliesInRange.RemoveAt (selectedAllyInArray);
				}
				if (alliesInRange [selectedAllyInArray].gameObject.tag == thisBadGuy.tag && alliesInRange [selectedAllyInArray].gameObject.GetComponent<HealthBar> ().isDead) {
					alliesInRange.RemoveAt (selectedAllyInArray);
				}
			}

			if (selectedAllyInArray > alliesInRange.Count - 1)
				selectedAllyInArray = 0;

			if (alliesInRange.Count == 0) {
				//sphereCollider.radius = radius;
				ally = null;
				foundAlly = false;
			}
		}
	}

	void OnTriggerEnter (Collider other){

		//revisar si aún quedan enemigos cerca
		if (initialized && other.gameObject.tag == thisBadGuy.tag) {
			foundAlly = true;
			alliesInRange.Add (other.gameObject);
		}
	}

	void OnTriggerExit (Collider other){
		if (initialized && other.gameObject.tag == thisBadGuy.tag) {
			//			numberOfEnemies = numberOfEnemies - 1;
			alliesInRange.Remove (other.gameObject);
		}
	}

}
