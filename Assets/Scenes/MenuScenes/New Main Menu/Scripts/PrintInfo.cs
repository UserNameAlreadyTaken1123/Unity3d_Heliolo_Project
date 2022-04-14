using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintInfo : MonoBehaviour {

	private GameObject menuManager;

	private bool activate;
	public bool character;
	public bool skin;
	public bool scene;


	// Use this for initialization
	void Awake () {
		menuManager = GameObject.Find ("MenuManager");
		GetComponent<TextMesh> ().fontSize = 64;
		GetComponent<TextMesh> ().characterSize = 0.225f;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (menuManager.GetComponent<MenuManagerAlt> ().newGameMenu.activeInHierarchy) {
			if (character)
				GetComponent<TextMesh> ().text = "Character: " + PlayerPrefs.GetString ("NewPlayer");
			if (scene)
				GetComponent<TextMesh> ().text = "Scene: " + PlayerPrefs.GetString ("NewScene");
		} else {
			GetComponent<TextMesh> ().text = " ";
		}
	}
}
