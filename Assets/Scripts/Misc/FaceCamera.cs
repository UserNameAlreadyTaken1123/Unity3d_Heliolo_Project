using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {

	public Vector3 angleCorrection;

	// Use this for initialization
	void Start () {
		if (Camera.main) {
			transform.LookAt (Camera.main.transform.position);
			transform.rotation = transform.rotation * Quaternion.Euler (angleCorrection);
		}
	}

	void LateUpdate(){
		if (Camera.main) {
			transform.LookAt (Camera.main.transform.position);
			transform.rotation = transform.rotation * Quaternion.Euler (angleCorrection);
		}
	}
}
