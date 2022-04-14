using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOffsetFixing : MonoBehaviour {

	public Vector3 offset;
	//public Animator anim;

	private bool alreadyDone;

	void Update () {
		transform.localPosition = offset;
	}
	/*
	void FixedUpdate () {
		transform.localPosition = offset;
	}

	void LateUpdate () {
		transform.localPosition = offset;
	}
	*/
}
