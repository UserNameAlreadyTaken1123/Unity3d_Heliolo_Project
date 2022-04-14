using UnityEngine;
using System.Collections;

public class StartingCoordinates : MonoBehaviour {

	public GameObject PlayerSpawn;

	// Use this for initialization
	void Start () {
		StartCoroutine (LateStart ());	
	}

	IEnumerator LateStart(){
		yield return new WaitForEndOfFrame ();
		if (GameObject.Find ("Helio"))
			PlayerSpawn = GameObject.Find ("Helio");
		if (GameObject.Find ("Duncan"))
			PlayerSpawn = GameObject.Find ("Duncan");

		PlayerSpawn.transform.position = transform.position;
		PlayerSpawn.transform.rotation = transform.rotation;
		PlayerSpawn.transform.localScale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
	}
}
