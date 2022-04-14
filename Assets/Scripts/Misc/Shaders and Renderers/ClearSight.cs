using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearSight : MonoBehaviour {

	public float distanceToPlayer;
	public float transparencyAmount;
	public LayerMask transparentableLayers;

	// Use this for initialization
	void Start () {
		transparentableLayers = ~transparentableLayers;
	}
	
	// Update is called once per frame

}
