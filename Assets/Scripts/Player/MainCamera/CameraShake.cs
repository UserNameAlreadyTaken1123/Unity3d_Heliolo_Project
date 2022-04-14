using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraShake : MonoBehaviour {

	public static CameraShake instance;
	public static Third_Person_Camera camScript;
	public static bool debugMode = false;//Test-run/Call ShakeCamera() on start

	public static float shakeAmount;//The amount to shake this frame.shakePercentage
	public static float shakeDuration;//The duration this frame.

	//Readonly values...
	static float shakePercentage;//A percentage (0-1) representing the amount of shake to be applied when setting rotation.
	static float startAmount;//The initial shake amount (to determine percentage), set when ShakeCamera is called.
	static float startDuration;//The initial shake duration, set when ShakeCamera is called.

	static bool isRunning = false;	//Is the coroutine running right now?

	static public bool smooth;//Smooth rotation?
	static public float smoothAmount = 5f;//Amount to smooth

	void Start () {
		Invoke ("SetInstance", 0.01f);
		if(debugMode) ShakeCamera ();
	}

	void SetInstance(){
		instance = this;
	}

	void ShakeCamera() {

		startAmount = shakeAmount;//Set default (start) values
		startDuration = shakeDuration;//Set default (start) values

		if (!isRunning) instance.StartCoroutine (CoroShake());//Only call the coroutine if it isn't currently running. Otherwise, just set the variables.
	}

	public static void Shake(float duration, float amount) {

		/*
		shakeAmount += amount;//Add to the current amount.
		startAmount = shakeAmount;//Reset the start amount, to determine percentage.
		shakeDuration += duration;//Add to the current time.
		startDuration = shakeDuration;//Reset the start time.
		*/

		shakeAmount += amount;//Add to the current amount.
		startAmount = shakeAmount;//Reset the start amount, to determine percentage.
		shakeDuration = duration;
		startDuration = shakeDuration;//Reset the start time.

		if(!isRunning) instance.StartCoroutine (instance.CoroShake());//Only call the coroutine if it isn't currently running. Otherwise, just set the variables.
	}


	IEnumerator CoroShake() {
		isRunning = true;
		Quaternion origRotation = Camera.main.transform.rotation;
		Vector3 origPosition = Camera.main.transform.position;
		bool yes = false;

		if (Camera.main.transform.GetComponent<Third_Person_Camera> ()) {
			camScript = Camera.main.transform.GetComponent<Third_Person_Camera> ();
			yes = true;
		}

		while (shakeDuration > 0.01f) {
			if (Time.timeScale > 0f) {
				Vector3 rotationAmount = Random.insideUnitSphere * shakeAmount;//A Vector3 to add to the Local Rotation
				rotationAmount.z = 0;//Don't change the Z; it looks funny.

				shakePercentage = shakeDuration / startDuration;//Used to set the amount of shake (% * startAmount).

				shakeAmount = startAmount * shakePercentage;//Set the amount of shake (% * startAmount).
				shakeDuration = Mathf.Lerp (shakeDuration, 0, Time.deltaTime * 5f);//Lerp the time, so it is less and tapers off towards the end.

				if (smooth)
					Camera.main.transform.localRotation = Quaternion.Lerp (Camera.main.transform.localRotation, Quaternion.Euler (rotationAmount), Time.deltaTime * smoothAmount);
				else
					Camera.main.transform.localRotation = Camera.main.transform.localRotation * Quaternion.Euler (rotationAmount);//Set the local rotation the be the rotation amount.
			}
			if (yes) {
				origRotation = camScript.rotation;
				origPosition = camScript.targetPositionRaw;
			}
			yield return null;
		}

		float t = 1;
		while (t > 0) {
			if (yes && camScript) {
				camScript.transform.rotation = Quaternion.Lerp (camScript.transform.localRotation, origRotation, Time.deltaTime * 10f);
				camScript.transform.position = Vector3.Lerp (camScript.transform.position, origPosition, Time.deltaTime * 10f);
			}
			t -= Time.deltaTime * 10f;
			yield return null;
		}
		//Camera.main.transform.localRotation = Quaternion.Euler(0.0f, Camera.main.transform.localRotation.eulerAngles.y, 0.0f);//Set the local rotation to 0 when done, just to get rid of any fudging stuff.
		isRunning = false;
	}

}