using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoatJoints : MonoBehaviour {

	public List<Transform> inmediateChilds;

	public Collider[] Joints1;
	public Collider[] Joints2;
	public Collider[] Joints3;
	public Collider[] Joints4;
	public Collider[] Joints5;
	public Collider[] Joints6;

	// Use this for initialization
	void Start () {

		foreach (Transform child in transform) {
			inmediateChilds.Add (child);
		}
			
		Joints1 = inmediateChilds [0].GetComponentsInChildren<Collider> ();
		Joints2 = inmediateChilds [1].GetComponentsInChildren<Collider> ();
		Joints3 = inmediateChilds [2].GetComponentsInChildren<Collider> ();
		Joints4 = inmediateChilds [3].GetComponentsInChildren<Collider> ();
		Joints5 = inmediateChilds [4].GetComponentsInChildren<Collider> ();
		Joints6 = inmediateChilds [5].GetComponentsInChildren<Collider> ();

		foreach (Collider joint in Joints1) {
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints2[i]);
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints3[i]);
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints4[i]);
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints5[i]);
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints6[i]);
		}

		foreach (Collider joint in Joints2) {
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints3[i]);
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints4[i]);
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints5[i]);
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints6[i]);
		}

		foreach (Collider joint in Joints3) {
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints4[i]);
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints5[i]);
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints6[i]);
		}

		foreach (Collider joint in Joints4) {
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints5[i]);
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints6[i]);
		}

		foreach (Collider joint in Joints5) {
			for (int i = 0; i <= 4; i++)
				Physics.IgnoreCollision(joint, Joints6[i]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
