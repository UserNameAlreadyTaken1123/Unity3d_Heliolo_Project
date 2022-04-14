using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportOnContact : MonoBehaviour {

	public Transform destination;
	public GameObject particlesEffect;
	private GameObject EffectA;
	private GameObject EffectB;

	// Use this for initialization
	void Start () {
		EffectA = Instantiate (particlesEffect);
		EffectB = Instantiate (particlesEffect);
	}
	
	void OnCollisionEnter(Collision collision){
		if (collision.gameObject.CompareTag ("Enemy") || collision.gameObject.CompareTag ("Player")) {
			EffectA.transform.position = collision.gameObject.transform.position;
			EffectA.GetComponent<ParticleSystem> ().Play();
			collision.gameObject.GetComponent<Rigidbody> ().velocity = Vector3.zero;
			collision.gameObject.transform.position = destination.position + Vector3.up * 2f;
			collision.gameObject.GetComponent<Rigidbody> ().AddForce (Vector3.right * Random.Range (-1, 1), ForceMode.VelocityChange);
			collision.gameObject.GetComponent<Rigidbody> ().AddForce (Vector3.forward * Random.Range (-1, 1), ForceMode.VelocityChange);
			EffectB.transform.position = destination.position;
			EffectB.GetComponent<ParticleSystem> ().Play();

			if (collision.gameObject.GetComponent<Hero_Movement> ()) {
				collision.gameObject.GetComponent<Hero_Movement> ().ResetStates ();
			}
		}		
	}
}
