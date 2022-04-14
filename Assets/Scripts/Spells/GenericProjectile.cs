using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericProjectile : MonoBehaviour {

	public GameObject caster;
	public AudioClip crashSound;
	public GameObject deathEffect;
	public float damage = 10f;
	public float forwardSpeed = 25f;
	public float lifeTime = 3f;
	public float explosionRadius = 4f;
	public float explosionThrust = 7.5f;
	public float stunTime = 0.15f;
	public bool unstoppable = true;
	public bool overthrows = true;

	private float explosionMaxDamage = 15f;
	private AudioSource audio;
	private Vector3 prevPos;
	private RaycastHit Hit;

	// Use this for initialization
	void Awake(){
	}

	void Start () {
		Physics.IgnoreCollision(GetComponent<Collider>(), caster.GetComponent<CapsuleCollider>());
		GetComponent<Rigidbody>().AddForce (transform.forward * forwardSpeed, ForceMode.VelocityChange);
		if (deathEffect)
			deathEffect.SetActive (false);
		Destroy (gameObject, lifeTime);
		prevPos = transform.position;
        explosionMaxDamage = damage;
	}

	void OnDestroy(){
		audio = CustomMethods.PlayClipAt (crashSound, transform.position);
		audio.volume = 1f;
		audio.maxDistance = 100f;
		audio.minDistance = 30f;

		if (deathEffect != null){
			deathEffect.transform.parent = null;
			deathEffect.SetActive (true);
			//CameraShake.Shake (1f,  1f / Vector3.Distance (deathEffect.transform.position, Camera.main.transform.position) * explosionRadius / 2f );
			CameraShake.Shake (0.025f, 1f / Vector3.Distance (deathEffect.transform.position, Camera.main.transform.position) * explosionRadius * 2f);
			Destroy (deathEffect, 2.5f);
		}

		Collider[] hitColliders = Physics.OverlapSphere (transform.position, explosionRadius, LayerMask.GetMask ("Enemy", "Player", "Debris"));
		foreach (Collider collided in hitColliders) {
			/*
			if (collided.gameObject != caster) {
				if (collided.gameObject.GetComponent<HealthBar> ())
					collided.gameObject.GetComponent<HealthBar> ().AddjustCurrentHealth (caster.transform, -explosionMaxDamage / Vector3.Distance (transform.position, collided.transform.position) / 1f);
				collided.gameObject.GetComponent<Rigidbody> ().AddForce ((collided.transform.position - transform.position).normalized * explosionThrust / Vector3.Distance (transform.position, collided.transform.position) / 1f, ForceMode.VelocityChange);
				collided.gameObject.GetComponent<Rigidbody> ().AddForce (Vector3.up * Vector3.Distance (transform.position, collided.transform.position) / 2f, ForceMode.VelocityChange);
			}
			*/
			RaycastHit obstacleHit;
			Physics.Raycast (transform.position, collided.transform.position - transform.position, out obstacleHit, explosionRadius + 0.1f, LayerMask.GetMask ("Scenario", "Default"), QueryTriggerInteraction.Ignore);
			if (!obstacleHit.collider && collided.gameObject != caster) {
				float effect = 1 - (transform.position - collided.transform.position).magnitude / explosionRadius;
				if (collided.gameObject.GetComponent<HealthBar> ()) {
					if (Vector3.Distance (transform.position, collided.gameObject.transform.position) > explosionRadius * 2/3) 
						collided.gameObject.GetComponent<HealthBar> ().AddjustCurrentHealth (caster.transform, -explosionMaxDamage * effect, stunTime, unstoppable, false);
					else
						collided.gameObject.GetComponent<HealthBar> ().AddjustCurrentHealth (caster.transform, -explosionMaxDamage * effect, stunTime, unstoppable, overthrows);
					if (collided.gameObject.GetComponent<References> () && collided.gameObject.GetComponent<References> ().RightHandWeapon && collided.gameObject.GetComponent<References> ().RightHandWeapon.GetComponent<NGS_NewCPU> () && collided.gameObject.GetComponent<References> ().RightHandWeapon.GetComponent<NGS_NewCPU> ().blocking) {
						collided.gameObject.GetComponent<HealthBar> ().CurHealth += -damage / 2f / Vector3.Distance (collided.transform.position, transform.position);
						return;
					}
				}
				collided.gameObject.GetComponent<Rigidbody> ().AddForce ((collided.transform.position - transform.position).normalized * explosionThrust / explosionRadius, ForceMode.VelocityChange);
				collided.gameObject.GetComponent<Rigidbody> ().AddForce (Vector3.up * 2.5f * explosionThrust / explosionRadius, ForceMode.VelocityChange);
			}
		}
	}
		
	private void OnTriggerEnter(Collider checker){
		transform.position = prevPos;
		if (checker.gameObject != caster && caster.tag != checker.tag && checker.GetComponent<HealthBar>()) {
			HealthBar health = checker.GetComponent<HealthBar> ();
			health.AddjustCurrentHealth (caster.transform, -damage, stunTime, unstoppable, overthrows);
			Destroy (gameObject);
		} else
			Destroy (gameObject);
	}

	void Update(){
		prevPos = transform.position;
	}
}
