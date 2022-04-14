using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorSetValues : MonoBehaviour {

	private Animator animator;
	public List<string> parameters;
	public List<float> values;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();

		for (int count = 0; count <= parameters.Count; count = count) {
			animator.SetFloat (parameters[count], values[count]);
			count++;
		}
			
	}
	
	// Update is called once per frame
	void OnEnable () {
		Start ();
	}
}
