using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnstopableShooter : MonoBehaviour {

	public GameObject bulletPrefab;
	public AudioClip ShotSound;
	public int bulletsPerBurst;
	public float cooldown;

	public float baseDamage = 1f;
	private float timerA;
	private float timerB;
	private int bulletsLeft;
	
	// Update is called once per frame
	void Update () {
		if (timerA >= cooldown) {
			if (bulletsLeft > 0) {
				if (timerB > 0.15f) {
					timerB = 0f;
					bulletsLeft -= 1;
					GameObject projectileInstance = Instantiate (bulletPrefab, transform.position, transform.rotation);
					//projectileInstance.transform.position = transform.position;
					//projectileInstance.transform.rotation = transform.rotation;
					projectileInstance.GetComponent<BulletBehavior> ().shooter = transform;
					projectileInstance.GetComponent<BulletBehavior> ().baseDamage = baseDamage;
					projectileInstance.GetComponent<BulletBehavior> ().spread = 1f;
					projectileInstance.GetComponent<BulletBehavior> ().speed = 5f;
					projectileInstance.GetComponent<BulletBehavior> ().isRipper = false;
					CustomMethods.PlayClipAt (ShotSound, transform.position);
					//projectileInstance.GetComponent<TrailRenderer> ().material = tracerMaterial;
					//projectileInstance.GetComponent<MeshRenderer> ().material = tracerMaterial;
					//Physics.IgnoreCollision (projectileInstance.transform.GetComponent<Collider> (), transform.GetComponent<Collider> ());
				} else
					timerB += Time.deltaTime;
			} else {
				bulletsLeft = bulletsPerBurst;
				timerA = 0f;
				timerB = 0f;
			}

		} else
			timerA += Time.deltaTime;
	}
}
