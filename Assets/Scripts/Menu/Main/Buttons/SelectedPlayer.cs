using UnityEngine;
using System.Collections;

public class SelectedPlayer : MonoBehaviour {
	public int currentPlayer;

	// Use this for initialization
	void Start () {
		currentPlayer = 1;
		StopCoroutine ("Lighten");
		StopCoroutine ("Darken");
	}

	// Update is called once per frame
	void Update () {	
		switch (currentPlayer) {
		case 1:
			GetComponent<TextMesh> ().text = "Helio";
			break;
		case 2:
			GetComponent<TextMesh> ().text = "Duncan";
			break;
		}
		PlayerPrefs.SetInt ("selectedCharacter", currentPlayer);
	}
}
