using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutodestroyParticles : MonoBehaviour {

	public float autodestroyTimer;

	// Use this for initialization
	void Start () {
		Destroy (gameObject, autodestroyTimer);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
