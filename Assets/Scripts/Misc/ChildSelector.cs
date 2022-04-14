using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildSelector : MonoBehaviour {

	public int randomInt;
	public GameObject selectedChild;

	// Use this for initialization
	void Awake () {
		randomInt = Random.Range (0, transform.childCount + 1);
		selectedChild = transform.GetChild (randomInt).gameObject;
		foreach (Transform child in transform) {
			if (child.gameObject != selectedChild)
				child.gameObject.SetActive (false);
			else
				child.gameObject.SetActive (true);
		}
	}
}
