using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverShoulderCamera : MonoBehaviour {
	
	private Vector3 origPosition;
	private Coroutine coroutine;

	[HideInInspector] public Vector3 lastAsignedRelPosition;
	[HideInInspector] public float heightModifier = 0f;

	// Use this for initialization
	void Awake () {
		origPosition = transform.localPosition;
		lastAsignedRelPosition = origPosition;
	}

	public void SetRelativePosition(Vector3 newPosition, float totalDuration){
		lastAsignedRelPosition = newPosition;
		if (coroutine != null)
			StopCoroutine (coroutine);
		coroutine = StartCoroutine (SetPosition(newPosition, totalDuration));
	}

	public void ResetPosition(float totalDuration){
		if (coroutine != null)
			StopCoroutine (coroutine);
		coroutine = StartCoroutine (SetPosition(origPosition, totalDuration));
	}

	IEnumerator SetPosition(Vector3 setPosition, float totalDuration){
		float normalizedTime = 0f;
		while (normalizedTime < 1f) {
			normalizedTime += Time.deltaTime / totalDuration;
			transform.localPosition = Vector3.Slerp (transform.localPosition, setPosition + Vector3.up * heightModifier, normalizedTime);
			yield return null;
		}
	}
}
