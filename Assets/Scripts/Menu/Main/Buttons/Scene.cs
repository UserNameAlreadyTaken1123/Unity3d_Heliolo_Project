using UnityEngine;
using System.Collections;

public class Scene : MonoBehaviour {
	private int selectedScene;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {	
		selectedScene = GameObject.Find ("Select").GetComponent<Select>().scene;
		switch (selectedScene) {
		case 1:
			GetComponent<TextMesh> ().text = "Tutorial";
			break;

		case 2:
			GetComponent<TextMesh> ().text = "Campo Verde";
			break;

		case 3:
			GetComponent<TextMesh> ().text = "Proto Jungle";
			break;

		case 4:
			GetComponent<TextMesh> ().text = "Treasure Temple";
			break;
		case 5:
			GetComponent<TextMesh> ().text = "fy_iceworld";
			break;
		case 6:
			GetComponent<TextMesh> ().text = "cs_assault";
			break;

		}
		
	}

}
