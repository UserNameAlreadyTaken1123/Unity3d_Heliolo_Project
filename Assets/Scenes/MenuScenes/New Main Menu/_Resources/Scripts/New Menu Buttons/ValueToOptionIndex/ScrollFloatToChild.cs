using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollFloatToChild : MonoBehaviour {

	public string playerPrefsKey;
	public float returnedFloat;

	private List<Transform> listOfChilds = new List<Transform> ();
	private Transform reference;

	// Use this for initialization
	void OnEnable () {
		returnedFloat = PlayerPrefs.GetFloat (playerPrefsKey, 0f);
		foreach (Transform child in transform) {
			listOfChilds.Add (child);
		}

		foreach (Transform child in listOfChilds) {
			if (child.GetComponent<ButtonWriteToPlayerPrefs> ().playerPrefsValueB [0] == returnedFloat) {
				child.gameObject.SetActive (true);
				reference = child;
			}
			else
				child.gameObject.SetActive (false);
		}
		GetComponent<ButtonDelegateForChildren> ().optionIndex = listOfChilds.IndexOf(reference);
	}
}
