using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDisplaySelector : MonoBehaviour {

	private string currentCharacter;
	private GameObject currentGO;

	// Use this for initialization
	public void Start () {
		currentCharacter = PlayerPrefs.GetString ("NewPlayer", "Helio");
		foreach (Transform character in transform) {
			if (character.name == currentCharacter){
				character.gameObject.SetActive (true);
				currentGO = character.gameObject;
			} else
				character.gameObject.SetActive (false);
		}

		if (currentGO == null) {
			transform.GetChild (0).gameObject.SetActive(true);
			transform.GetChild (0).name = currentCharacter;
			currentGO = transform.GetChild (0).gameObject;
			PlayerPrefs.SetString ("NewPlayer", currentCharacter);

		}

//		currentGO.GetComponent<Animator> ().SetFloat ("MeleeWeaponType", Random.Range (0f, 4f));
//		currentGO.GetComponent<Animator> ().SetFloat ("IdleAnimation", Random.Range (0f, 4f));
	}

	void OnEnable(){
		Start ();
	}
}
