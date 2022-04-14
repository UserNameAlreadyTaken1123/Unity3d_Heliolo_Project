using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuProperties : MonoBehaviour {

	public GameObject cameraToUse;
	private Camera cameraCoponent;
	public float xPositionFixed;
	public float yPositionFixed;
	public float zDistanceFromCamera;
	public Vector3 buttonScale;

	// Use this for initialization
	void Start () {
		cameraCoponent = cameraToUse.GetComponent<Camera>();

		transform.position = cameraCoponent.ViewportToWorldPoint (new Vector3 (xPositionFixed, yPositionFixed, cameraCoponent.transform.position.z + zDistanceFromCamera));
		transform.localScale = buttonScale;
	}

	void OnEnable(){
		Start ();
	}
}
