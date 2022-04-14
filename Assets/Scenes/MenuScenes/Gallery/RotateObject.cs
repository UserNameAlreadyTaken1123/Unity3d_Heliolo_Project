using UnityEngine;
using System.Collections;
using Luminosity.IO;

public class RotateObject : MonoBehaviour {

	public float maxRotationSpeed;
	private Quaternion rotation;
	private float size;

	private void Start(){	
		rotation = transform.rotation;
	}

	void Update (){	
	
	}

	void OnDisable(){
		transform.rotation = rotation;
	}

	private void FixedUpdate (){
		transform.rotation = Quaternion.Euler (0f, transform.rotation.eulerAngles.y, 0f);

		if (InputManager.GetAxis ("RightAnalogX") > 0.2f) {
			transform.Rotate(Vector3.down * 50f * Time.deltaTime);
		}

		if (InputManager.GetAxis ("RightAnalogX") < -0.2f) {
			transform.Rotate(Vector3.up * 50f * Time.deltaTime);
		}
	}
}
