using UnityEngine;
using System.Collections;

public class PlayerState : MonoBehaviour {

	public static PlayerState Instance;
	public PlayerStatistics localPlayerData = new PlayerStatistics();

	public Transform playerPosition;

	void Awake(){
/*		if (Instance == null)
			Instance = this;

		if (Instance != this) {
			print (Instance);
			Destroy (gameObject);
		}
*/
	}

	// Use this for initialization
	void Start (){
		if (Instance == null)
			Instance = this;

		if (Instance != this)
			Destroy (gameObject);
//		LoadPlayer ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {	
		GlobalControl.Instance.Player = gameObject;
	}

	void LoadPlayer (){ 
		localPlayerData = GlobalControl.Instance.savedPlayerData;
	}

	public void SavePlayer()	{
		GlobalControl.Instance.savedPlayerData = localPlayerData;        
	}
}
