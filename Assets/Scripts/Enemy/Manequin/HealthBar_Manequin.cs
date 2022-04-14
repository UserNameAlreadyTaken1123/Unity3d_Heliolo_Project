using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar_Manequin : HealthBar {

	public AudioClip hitSound;
	private bool coroDeath;

	// Use this for initialization
	void Start () {
		animationScript = GetComponent<Player_Animation> ();
		animationScript = GetComponent<Player_Animation> ();
		collider = GetComponent<Collider> ();
		rigidbody = GetComponent<Rigidbody> ();
		transform = GetComponent<Transform> ();
		references = GetComponent<References> ();
	}

	void HealOverTime (){
		if (!isDead && CurHealth < Maxhealth)
			AddjustCurrentHealth (this.transform, healAmountPerSecond * Time.deltaTime, 0f, true, false);
	}

	// Update is called once per frame
	void Update () {	
		HealOverTime ();

		if (cheatGodMode) {
			godMode = true;
			cantBeHitMode = true;
		}

		if (CurHealth > Maxhealth)
			CurHealth = Maxhealth;
	}

	public override bool AddjustCurrentHealth (Transform attacker, float adj, float stunTime, bool unstoppable, bool overthrows) {
		if (godMode) {
			if (adj < 0)
				adj = 0;
		}

		if (!cantBeHitMode && adj < 0) {
			CurHealth += adj;
			rigidbody.velocity = Vector3.zero;
			StartCoroutine (Pain ());
			return true;

		} else if (adj > 0f) {
			CurHealth += adj;
			return true;
		} else if (cantBeHitMode)
			return false;
		else
			return false;
	}

	public override bool BulletDamage(Vector3 hitPoint, Transform shooter, Transform bullet, float adj) {
		if (godMode) {
			if (adj < 0)
				adj = 0;
		}

		if (!cantBeHitMode && adj < 0) {
			CurHealth += adj;

			rigidbody.velocity = Vector3.zero;
			StartCoroutine (Pain ());
			return true;

		} else if (adj > 0f) {
			CurHealth += adj;
			return true;
		} else if (cantBeHitMode)
			return false;
		else
			return false;
	}

	public IEnumerator Pain(){
		if (!inPain) {
			inPain = true;

			if (hitSound != null)
				CustomMethods.PlayClipAt (hitSound, transform.position);

			Color origColor;
			if (transform.GetComponentInChildren<Light> ()) {
				origColor = transform.GetComponentInChildren<Light> ().color;
				transform.GetComponentInChildren<Light> ().color = Color.red;
				yield return new WaitForSeconds (0.5f);
				transform.GetComponentInChildren<Light> ().color = origColor;
			}
				
			inPain = false;
		}
		yield return null;
	}

	IEnumerator BulletPain(){
		if (!inPain) {
			inPain = true;

			if (hitSound != null)
				CustomMethods.PlayClipAt (hitSound, transform.position);

			Color origColor;
			if (transform.GetComponentInChildren<Light> ()) {
				origColor = transform.GetComponentInChildren<Light> ().color;
				transform.GetComponentInChildren<Light> ().color = Color.red;
				yield return new WaitForSeconds (0.5f);
				transform.GetComponentInChildren<Light> ().color = origColor;
			}

			inPain = false;
		}
		yield return null;
	}
}
