using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingesStarter : MonoBehaviour {

	// Use this for initialization
	void Start () {
		List<Transform> toUnparent = new List<Transform> (transform.childCount);

		foreach (Transform child in transform) {
			toUnparent.Add (child);
		}
		foreach (Transform child in toUnparent) {
			child.parent = null;
		}
	}
}
