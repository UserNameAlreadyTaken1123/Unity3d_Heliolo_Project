using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StainedGlassDistortionChanger : MonoBehaviour {
	
	private Material glassMat;
	public float startValue;
	public float finalValue;
	public float duration;
	public float startDelay;
	private float normalizedTime;
	private float tempValue;

	void Start(){
		glassMat = GetComponent<Renderer> ().material;
	}

	// Update is called once per frame
	void LateUpdate () {
		if (startDelay > 0)
			startDelay -= Time.deltaTime;
		
		else if (normalizedTime < 1) {
			tempValue = Mathf.Lerp (startValue, finalValue, normalizedTime);
			GetComponent<Renderer>().material.SetFloat("_BumpAmt", tempValue);
			normalizedTime += Time.deltaTime / duration;
		}
	}
}
