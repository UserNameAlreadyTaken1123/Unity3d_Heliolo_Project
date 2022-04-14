using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollIntToChild : MonoBehaviour {

	public string playerPrefsKey;
	public int returnedInt;

	[Header("Min allowed Value")]
	public int minValue = 0;
	[Header("Default Value if null")]
	public int defValue = 0;

	private List<Transform> listOfChilds = new List<Transform> ();

	// Use this for initialization
	void OnEnable () {
		returnedInt = PlayerPrefs.GetInt (playerPrefsKey, defValue);
		foreach (Transform child in transform) {
			listOfChilds.Add (child);
		}

		foreach (Transform child in listOfChilds) {
			if (listOfChilds.IndexOf (child) == returnedInt - minValue)
				child.gameObject.SetActive (true);
			else
				child.gameObject.SetActive (false);
		}

		GetComponent<ButtonDelegateForChildren> ().optionIndex = returnedInt - minValue;
	}
}
