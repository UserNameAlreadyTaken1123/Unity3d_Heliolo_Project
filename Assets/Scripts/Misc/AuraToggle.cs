using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class AuraToggle : MonoBehaviour {

	//Código de prueba. El effecto en sí funciona, quedaría reprogramar todo para optimizarlo.

	private ManaBar manaScript;
	private HealthBar healthScript;
	private References references;
	private Hero_Movement movementScript;
	private GameObject Player;

	public bool auraActivated;
	public float explosionRadius = 4f;
	public float explosionMaxDamage = 20f;
	public float explosionThrust = 4f;
	private bool prevValue;

	public float stunTime = 0.3f;
	public bool unstoppable = true;
	public bool overthrows = false;

	public float SpeedMultiplier = 1.5f;
	public float damageMultiplier = 1.5f;

	public GameObject ActivationEffect;
	public GameObject DeactivationEffect;
	public List<GameObject> auraReferences;
	public List<GameObject> auraMeshes;
	public AudioClip activationSound;
	public AudioClip activeSound;
	private AudioSource activeSoundAS;

	public bool ActivateTimerToReset;
	public float currentComboTimer;
	public float manaOnActivation = 20f;
	public float manaRate = 4f;
	public float origTimer = 0.2f;

	public float coolDown = 2f;

	// Use this for initialization
	void Start () {
		Player = transform.parent.parent.gameObject;
		manaScript = Player.GetComponent<ManaBar> ();
		healthScript = Player.GetComponent<HealthBar> ();
		references = Player.GetComponent<References> ();
		movementScript = Player.GetComponent<Hero_Movement> ();
		prevValue = auraActivated;
	}
	
	// Update is called once per frame
	void Update () {
		if (!healthScript.isDead) {
			Input ();
			ResetComboStateTimer (ActivateTimerToReset);

			if (auraActivated && manaScript.CurMana >= (manaRate * Time.deltaTime))
				manaScript.CurMana -= manaRate * Time.deltaTime;
			else if (manaScript.CurMana < (manaRate * Time.deltaTime)) {
				auraActivated = false;
				ToggleAura (auraActivated);
			}
			if (coolDown < 1f)
				coolDown += Time.deltaTime;
		} else {
			auraActivated = false;
			ToggleAura (auraActivated);
		}
	}

	void FixedUpdate(){
		if (auraActivated) {
			foreach (GameObject auraPart in auraReferences) {
				auraPart.transform.rotation = Quaternion.LookRotation (Vector3.up);
			}
		}
	}

	void Input(){
		if (!ActivateTimerToReset) {
			if (InputManager.GetButtonDown ("Melee") || InputManager.GetButtonDown ("Launcher") || InputManager.GetButtonDown ("Jump") || InputManager.GetButtonDown ("Magic")) {
				ActivateTimerToReset = true;
				currentComboTimer = origTimer;
			}
		}

		if (ActivateTimerToReset && coolDown >= 1f) {
			if (InputManager.GetButton ("Melee") && InputManager.GetButton ("Launcher") && InputManager.GetButton ("Jump") && InputManager.GetButton ("Magic")) {
				ForcedReset ();
				if (!auraActivated && manaScript.CurMana > manaOnActivation) {
					auraActivated = true;
					ToggleAura (auraActivated);
				} else if (auraActivated) {
					auraActivated = false;
					ToggleAura (auraActivated);
				}
			}
		}

	}

	void ResetComboStateTimer(bool resetTimer){
		if (resetTimer){
			currentComboTimer -= Time.deltaTime;
			if (currentComboTimer <= 0){
				ForcedReset ();
			}
		}
	}

	void ForcedReset(){
		ActivateTimerToReset = false;
		currentComboTimer = origTimer;
	}

	void ToggleAura(bool activated){
		coolDown = 0f;

		foreach (GameObject auraMesh in auraMeshes) {
			auraMesh.SetActive (activated);
		}

		if (activated) {
			references.triggered = true;
			references.rigidbody.velocity = Vector3.zero;
			manaScript.CurMana -= manaOnActivation;
			ActivationEffect.GetComponent<ParticleSystem> ().Play ();
			CustomMethods.PlayClipAt (activationSound, transform.position);
			activeSoundAS = CustomMethods.PlayLoopClipAt (activeSound, transform.position);
			activeSoundAS.GetComponent<AudioSource> ().loop = true;
			activeSoundAS.transform.parent = this.transform;
			references.SwordAttackCancelation ();

			if (Camera.main.GetComponent<MorePPEffects.RadialBlur> ())
				Camera.main.GetComponent<MorePPEffects.RadialBlur> ().RadialBlurFx (0.75f, 1f);

			Collider[] hitColliders = Physics.OverlapSphere (Player.GetComponent<References> ().RightFoot.transform.position, explosionRadius, LayerMask.GetMask ("Enemy", "Player"));
			foreach (Collider collided in hitColliders) {
				if (movementScript.isGrounded) {
					if (collided.gameObject != Player) {
						float effect = 1 - (transform.position - collided.transform.position).magnitude / explosionRadius;
						collided.gameObject.GetComponent<HealthBar> ().AddjustCurrentHealth (Player.transform, -explosionMaxDamage * effect, stunTime, unstoppable, overthrows);
						collided.gameObject.GetComponent<Rigidbody> ().velocity = Vector3.zero;
						collided.gameObject.GetComponent<Rigidbody> ().AddForce ((collided.transform.position - transform.position).normalized * explosionThrust / explosionRadius, ForceMode.VelocityChange);
						collided.gameObject.GetComponent<Rigidbody> ().AddForce (Vector3.up * 1.75f * explosionThrust / explosionRadius, ForceMode.VelocityChange);
					}
				} else if (!movementScript.isGrounded) {
					if (collided.gameObject != Player) {
						float effect = 1 - (transform.position - collided.transform.position).magnitude / explosionRadius;
						collided.gameObject.GetComponent<HealthBar> ().AddjustCurrentHealth (Player.transform, -explosionMaxDamage * effect, stunTime, unstoppable, overthrows);
						collided.gameObject.GetComponent<Rigidbody> ().velocity = Vector3.zero;
						collided.gameObject.GetComponent<Rigidbody> ().AddForce (Vector3.up * 3f, ForceMode.VelocityChange);
					}
				}
			}

			StartCoroutine (AuraBlast ());
			foreach (GameObject auraPart in auraReferences) {
				auraPart.GetComponent<ParticleSystem> ().Play ();
			}

		} else {
			references.SwordAttackCancelation();
			StartCoroutine (Timer ());
			//references.SwordAttackCancelation ();
			references.triggered = false;
			references.animationScript.SpeedMultiplierA = 1f;
			references.damageMultiplier = 1f;

			if (activeSoundAS)
				StartCoroutine (CustomMethods.FadeAndDestroyAudiosource (activeSoundAS, 0.5f));
			
			foreach (GameObject auraPart in auraReferences) {
				auraPart.GetComponent<ParticleSystem> ().Stop ();
			}
		}
	}

	IEnumerator AuraBlast(){
		references.animationScript.ResetValues();
		references.animationScript.anim.Play ("Triggered");
		references.animationScript.SpeedMultiplierA = SpeedMultiplier;
		movementScript.ResetStates ();
		movementScript.doNotMove = true;
		movementScript.cantStab = true;
		movementScript.cantShoot = true;
		references.triggering = true;
		references.damageMultiplier = damageMultiplier;
		healthScript.ResetValues ();
		healthScript.CantBeHitMode(0.5f);
		CameraShake.Shake (1f, 10f / ((Camera.main.transform.position - transform.position).sqrMagnitude * 4f));

		float normalizedTimer = 1f;
		float duration = 0.3f;
		bool isGrounded = true;

		if (!movementScript.isGrounded) {
			isGrounded = false;
			duration = 0.15f;
			references.rigidbody.velocity = Vector3.zero;
			references.rigidbody.AddForce (Vector3.up * 4f, ForceMode.VelocityChange);
		}

		while (normalizedTimer > 0) {
			if (isGrounded)
				references.rigidbody.velocity = Vector3.Lerp (Vector3.zero, references.rigidbody.velocity, normalizedTimer * 2f);
			Camera.main.transform.position = Camera.main.transform.position + (new Vector3 (Random.Range (-2, 2), Random.Range (-2, 2), Random.Range (-2, 2)) / Vector3.Distance (transform.position, Camera.main.transform.position)) / 2 * normalizedTimer;
			Camera.main.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler (new Vector3 (Random.Range (-2, 2), Random.Range (-2, 2), Random.Range (-2, 2)) / Vector3.Distance (transform.position, Camera.main.transform.position) * normalizedTimer);
			yield return null;
			normalizedTimer -= Time.deltaTime / duration;
		}
		movementScript.doNotMove = false;
		movementScript.cantStab = false;
		movementScript.cantShoot = false;
		references.triggering = false;
	}
	IEnumerator Timer(){
		references.animationScript.ResetValues();
		movementScript.doNotMove = true;
		movementScript.cantStab = true;
		movementScript.cantShoot = true;
		references.triggering = true;
		yield return new WaitForSeconds (0.2f);
		movementScript.doNotMove = false;
		movementScript.cantStab = false;
		movementScript.cantShoot = false;
		references.triggering = false;
	}
}
	