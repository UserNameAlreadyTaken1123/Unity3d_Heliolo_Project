using UnityEngine;
using System.Collections;

public class BackgroundHero : MonoBehaviour {

	public GameObject Helio;
	public GameObject Duncan;
	public int selectedPlayer;

	void Start () {
		selectedPlayer = PlayerPrefs.GetInt ("selectedCharacter");
	}
	
	// Update is called once per frame
	void Update () {
		selectedPlayer = PlayerPrefs.GetInt ("selectedCharacter");	
		switch (selectedPlayer) {
		case 0:
			Helio.SetActive (true);	
			Duncan.SetActive (false);	
			break;
		case 1:
			Helio.SetActive (true);	
			Duncan.SetActive (false);
			break;
		case 2:
			Helio.SetActive (false);	
			Duncan.SetActive (true);
			break;
		}
	}
}
