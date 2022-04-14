using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {	
		if (GameObject.Find ("PlayerSpawn")) {
			Transform playerSpawn = GameObject.Find ("PlayerSpawn").transform;
			foreach( Transform child in playerSpawn ){
//				child.gameObject.SetActive( false );
				Destroy (playerSpawn.gameObject);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
