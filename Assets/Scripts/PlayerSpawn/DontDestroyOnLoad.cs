using UnityEngine;
using System.Collections;
//using UnityEditor;

public class DontDestroyOnLoad : MonoBehaviour {

	public static DontDestroyOnLoad PlayerSpawn;
	public GameObject startingCoordinates;
	public GameObject Canvas;


	void Awake (){

//		GameObject startingPoint = (GameObject) PrefabUtility.InstantiatePrefab (startingCoordinates);
		GameObject startingPoint = (GameObject)Instantiate (startingCoordinates, transform.position, transform.rotation) as GameObject;
		startingPoint.name = "startingCoordinates";
		startingPoint.transform.position = transform.position;
		startingPoint.transform.rotation = transform.rotation;
		startingPoint.transform.localScale = transform.localScale;

		if (PlayerSpawn == null){
			DontDestroyOnLoad(gameObject);
			PlayerSpawn = this;
		}
		else if (PlayerSpawn != this){
			Destroy (gameObject);
		}
	}

	void Start(){
		GameObject pauseMenuCanvas = (GameObject)Instantiate (Canvas, transform.position, transform.rotation) as GameObject;
		pauseMenuCanvas.transform.SetParent (PlayerSpawn.gameObject.transform);
		pauseMenuCanvas.name = Canvas.name;
	}

	// Update is called once per frame
	void Update () {	
	}
}
