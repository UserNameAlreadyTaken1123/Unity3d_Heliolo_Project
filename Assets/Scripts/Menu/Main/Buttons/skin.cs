using UnityEngine;
using System.Collections;

public class skin : MonoBehaviour {
	public int skinSelected;
	private int characterSelected;

	// Use this for initialization
	void Start () {
		skinSelected = 1;
	}
	
	// Update is called once per frame
	void Update () {
		characterSelected = GameObject.Find ("Player").GetComponent<SelectedPlayer> ().currentPlayer;
	
		//Helio
		if (GameObject.Find("Player").GetComponent<SelectedPlayer> ().currentPlayer == 1) {
			switch (skinSelected) {
			case 0:
				GetComponent<TextMesh> ().text = "Standard";
				break;
			case 1:
				GetComponent<TextMesh> ().text = "Standard";
				break;
			case 2:
				GetComponent<TextMesh> ().text = "Sulfur";
				break;

			}
		}

		//Duncan
		if (GameObject.Find("Player").GetComponent<SelectedPlayer> ().currentPlayer == 2) {
			switch (skinSelected) {
			case 0:
				GetComponent<TextMesh> ().text = "Standard";
				break;
			case 1:
				GetComponent<TextMesh> ().text = "Standard";
				break;
			case 2:
				GetComponent<TextMesh> ().text = "Reptile";
				break;
			case 3:
				GetComponent<TextMesh> ().text = "Hey, Donche!";
				break;

			}
		}

		PlayerPrefs.SetInt ("skinSelected", skinSelected);
	}

	void OnMouseDown (){

		//Helio Selected
		if (characterSelected == 0 || characterSelected == 1) {
			skinSelected = skinSelected + 1;
			if (skinSelected > 2)
				skinSelected = 1;
		}

		//Duncan selected
		if (characterSelected == 2) {
			skinSelected = skinSelected + 1;
			if (skinSelected > 3)
				skinSelected = 1;
		}
	}

}
