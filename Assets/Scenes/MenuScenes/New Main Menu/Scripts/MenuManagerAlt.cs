using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManagerAlt : MonoBehaviour {

	public GameObject mainMenu;
	public GameObject newGameMenu;
	public GameObject selectSceneMenu;
	public GameObject selectCharacterMenu;
	public GameObject optionsMenu;
	public GameObject gameplaySettingsMenu;
	public GameObject graphicSettingsMenu;
	public GameObject audioSettingsMenu;

	public List<GameObject> SelectablePlayers;

	// Use this for initialization
	void Awake () {
		mainMenu = GameObject.Find ("MainMenu");
		newGameMenu = GameObject.Find ("NewGameMenu");
		selectSceneMenu = GameObject.Find ("SelectSceneMenu");
		selectCharacterMenu = GameObject.Find ("SelectCharacterMenu");
		optionsMenu = GameObject.Find ("OptionsMenu");
		gameplaySettingsMenu = GameObject.Find ("GameplaySettingsMenu");
		graphicSettingsMenu = GameObject.Find ("GraphicSettingsMenu");
		audioSettingsMenu = GameObject.Find ("AudioSettingsMenu");

		//this weir thing because list is not array (Findgameobjects returns array)
		SelectablePlayers.AddRange (GameObject.FindGameObjectsWithTag ("Player"));
		foreach (GameObject player in SelectablePlayers) {
			player.SetActive (false);
		}

//		mainMenu.SetActive (true);

	}

	void Start(){
		StartCoroutine (Delayer ());
	}

	IEnumerator Delayer(){
		yield return new WaitForEndOfFrame ();
		newGameMenu.SetActive (false);
		selectSceneMenu.SetActive (false);
		selectCharacterMenu.SetActive (false);
		optionsMenu.SetActive (false);
		gameplaySettingsMenu.SetActive (false);
		graphicSettingsMenu.SetActive (false);
		audioSettingsMenu.SetActive (false);
	}
}
