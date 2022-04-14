using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkinSelector : MonoBehaviour {

	public List<GameObject> skinsList;
	public string playerPrefsKey;
	public int selectedSkin;
	private int prevValue;
	private bool firstTime = false;

	// Use this for initialization
	public void Start () {
		selectedSkin = PlayerPrefs.GetInt (playerPrefsKey, 0);
		foreach (Transform child in transform) {
			if (child.name.Contains ("_LOD") && !skinsList.Contains(child.gameObject))
				skinsList.Add (child.gameObject);
		}

		int i = 0;
		foreach (GameObject skin in skinsList) {
			if (i == selectedSkin)
				skin.SetActive (true);
			else
				skin.SetActive (false);
			if (i < skinsList.Count)
				i++;
			print (skin.name + ": " + skin.activeSelf);
		}
		firstTime = true;
	}
	
	// Update is called once per frame
	void OnEnable () {
		if (firstTime)
			Start ();
	}
}
