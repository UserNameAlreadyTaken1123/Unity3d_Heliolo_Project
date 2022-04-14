using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class GalleryCameraMovement : MonoBehaviour {

	private Vector3 position;
	private float size;

	// Use this for initialization
	void Start () {
		position = transform.position;
		size = GetComponent<Camera> ().orthographicSize;
	}

	void OnDisable(){
		transform.position = position;
		GetComponent<Camera> ().orthographicSize = size;
	}
	
	// Update is called once per frame
	void Update () {

		if (InputManager.GetAxis ("Horizontal") > 0.2f || InputManager.GetAxis ("Direction Horizontal") > 0.2f) {
			transform.position += Vector3.right * 1.5f * Time.deltaTime;
		}

		if (InputManager.GetAxis ("Horizontal") < -0.2f || InputManager.GetAxis ("Direction Horizontal") < -0.2f) {
			transform.position -= Vector3.right * 1.5f * Time.deltaTime;
		}

		if (InputManager.GetAxis ("Vertical") > 0.2f || InputManager.GetAxis ("Direction Vertical") > 0.2f) {
			transform.position += Vector3.up * 1.5f * Time.deltaTime;
		}

		if (InputManager.GetAxis ("Vertical") < -0.2f || InputManager.GetAxis ("Direction Vertical") < -0.2f) {
			transform.position -= Vector3.up * 1.5f * Time.deltaTime;
		}

		if (InputManager.GetAxis ("RightAnalogY") > 0.2f && GetComponent<Camera>().orthographicSize < 5f) {
			GetComponent<Camera>().orthographicSize += 1.5f * Time.deltaTime;
		}

		if (InputManager.GetAxis ("RightAnalogY") < -0.2f && GetComponent<Camera>().orthographicSize > 1f) {
			GetComponent<Camera>().orthographicSize -= 1.5f * Time.deltaTime;
		}
	}
}
