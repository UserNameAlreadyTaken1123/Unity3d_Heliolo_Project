using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderManager : MonoBehaviour {

	public HealthBar healthBarScript;
	public float damageMultiplier;
	public float damageReceivedBase;
	public GameObject damageSource;
	public List<Collider> collidersList;
	private float damageCalculation;
	private Rigidbody rigidbody;

	// Use this for initialization
	void Start () {
		healthBarScript = GetComponent<HealthBar> ();
	}
}
