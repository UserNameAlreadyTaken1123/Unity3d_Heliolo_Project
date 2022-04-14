using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderChild : MonoBehaviour {

	public GameObject Player;
	public ColliderManager colliderManagerScript;
	public float damageReceivedBase;
	public float damageMultiplier;


	// Use this for initialization
	void Start () {
		if (GetComponent<ColliderManager> ()) {
			Player = this.gameObject;
			colliderManagerScript = GetComponent<ColliderManager> ();
		}

		Player.GetComponent<ColliderManager> ().collidersList.Add (this.gameObject.GetComponent<Collider> ());
		colliderManagerScript = Player.GetComponent<ColliderManager> ();

		GetComponent<Collider> ().isTrigger = true;

		if (!GetComponent<Rigidbody>())
			gameObject.AddComponent<Rigidbody> ().isKinematic = true;
		else if (!GetComponent<Rigidbody>().isKinematic)
			gameObject.GetComponent<Rigidbody> ().isKinematic = true;
	}
}
