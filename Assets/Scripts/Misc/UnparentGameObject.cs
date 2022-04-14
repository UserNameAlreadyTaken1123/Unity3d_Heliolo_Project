using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnparentGameObject : MonoBehaviour {

	public Vector3 setRotation;

	// Use this for initialization
	void Start () {
		transform.SetParent (null);
		transform.parent = null;

		transform.rotation = Quaternion.Euler (setRotation);
	}
}
