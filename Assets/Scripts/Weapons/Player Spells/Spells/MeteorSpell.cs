using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class MeteorSpell : MagicSpellClass {

	private MagicSpellCPU MagicSpellCPUScript;
	private ManaBar manaScript;

	public int attackLevel = 1;
	public float level1ManaRequirement;
	public float level2ManaRequirement;
	public float level3ManaRequirement;

	[Header(" ")]

	public float baseDamage = 10f;
	public float baseForwardSpeed = 25f;
	public float baseLifeTime = 3f;
	public float baseExplosionRadius = 4f;
	public float baseExplosionThrust = 7.5f;
	public float stunTime = 0.15f;

	[Header(" ")]

	public bool meteorSpell = false;
	public bool ActivateTimerToReset = false;
	//The more bools, the less readibility, try to stick with the essentials.
	//If you were to press 10 buttons in a row
	//having 10 booleans to check for those would be confusing

	public float currentSpellTimer;
	public int currentSpellState = 0;

	public GameObject Projectile;
	public AudioClip FireSound;

	public GameObject Level1ParticlesFX;
	public GameObject Level2ParticlesFX;
	public GameObject Level3ParticlesFX;

	float origTimer = 1f;
    float randomWait;

	void Start(){
		if (level2ManaRequirement == 0f)
			level2ManaRequirement = level1ManaRequirement * 1.25f;
		if (level3ManaRequirement == 0f)
			level3ManaRequirement = level1ManaRequirement * 1.5f;

		MagicSpellCPUScript = GetComponent<MagicSpellCPU> ();
		Player = MagicSpellCPUScript.Player;
		manaScript = Player.GetComponent<ManaBar> ();
	}

	void Update(){
		NewComboSystem();
		ResetComboStateTimer(ActivateTimerToReset);
	}

	void ResetComboStateTimer(bool resetTimer){
		if (resetTimer){
			currentSpellTimer -= Time.deltaTime;
			if (currentSpellTimer <= 0){
				ForcedReset ();
			}
		}
	}

	void OnEnable(){
		ForcedReset ();
	}

	public override void ForcedReset(){
		currentSpellState = 0;
		ActivateTimerToReset = false;
		currentSpellTimer = origTimer;
		meteorSpell = false;
	}

	void NewComboSystem(){
		switch (currentSpellState) {
		case 0:
			if (InputManager.GetAxis ("Horizontal") < -0.25f && InputManager.GetButton ("Magic")) {
				ActivateTimerToReset = true;
				currentSpellTimer = origTimer;
				currentSpellState++;
			}
			break;
		case 1:
			if (InputManager.GetAxis ("Vertical") < -0.25f && InputManager.GetButton ("Magic") && currentSpellTimer != 0f)
				currentSpellState++;
			break;
		case 2:
			if (InputManager.GetAxis ("Horizontal") > 0.25f && InputManager.GetButton ("Magic") && currentSpellTimer != 0f)
				currentSpellState++;
			break;
		case 3:
			if (InputManager.GetButtonUp ("Magic") && currentSpellTimer != 0f) {
				meteorSpell = true;
				currentSpellState = 0;
				GetComponent<MagicSpellCPU> ().StartSpell (this);
			}
			break;
		}
	}

	public override IEnumerator DoSomeMagic(){
		MagicSpellCPUScript.references.rigidbody.velocity = Vector3.zero;
		if (attackLevel == 1 && manaScript.CurMana > level1ManaRequirement) {
			MagicSpellCPUScript.ForcedReset ();
			CameraShake.Shake (0.025f, 1f / Vector3.Distance (Camera.main.transform.position, transform.position) * 5f);
			manaScript.CurMana -= level1ManaRequirement;
			Player.GetComponent<Player_Animation> ().ResetValues ();
			Player.GetComponent<Player_Animation> ().anim.Play ("SpellCast");
			yield return (0.2f);
			for (int i = 5; i > 0; i--) {
                GameObject projectileInstance = Instantiate(Projectile,

                    Player.transform.position +
                    Player.transform.forward * Random.Range(4f, 15f) +
                    Player.transform.right * Random.Range(-5f, 5f) +
                    Vector3.up * Random.Range(10f, 25f),

                    Quaternion.LookRotation(Vector3.down +
                        Vector3.forward * Random.Range(-0.25f, 0.25f) +
                        Vector3.right * Random.Range(-0.25f, 0.25f)));

                projectileInstance.GetComponent<GenericProjectile>().caster = Player;
                projectileInstance.GetComponent<GenericProjectile>().damage = baseDamage + attackLevel * attackLevel * 2f;
                projectileInstance.GetComponent<GenericProjectile> ().forwardSpeed = baseForwardSpeed;
				projectileInstance.GetComponent<GenericProjectile> ().lifeTime = baseLifeTime;
				projectileInstance.GetComponent<GenericProjectile> ().explosionRadius = baseExplosionRadius;
				projectileInstance.GetComponent<GenericProjectile> ().explosionThrust = baseExplosionThrust;
				projectileInstance.GetComponent<GenericProjectile> ().stunTime = stunTime;
				projectileInstance.GetComponent<SphereCollider> ().radius = 2f;
				yield return new WaitForFixedUpdate();
			}
			CustomMethods.PlayClipAt (FireSound, transform.position);
			yield return (0.2f);
			MagicSpellCPUScript.Player.GetComponent<Player_Animation> ().isAiming = false;
			yield return null;
		} else if (attackLevel == 2 && manaScript.CurMana > level2ManaRequirement) {
			MagicSpellCPUScript.ForcedReset ();
			CameraShake.Shake (0.025f, 1f / Vector3.Distance (Camera.main.transform.position, transform.position) * 10f);
			manaScript.CurMana -= level2ManaRequirement;
			MagicSpellCPUScript.Player.GetComponent<Player_Animation> ().ResetValues ();
			Player.GetComponent<Player_Animation> ().anim.Play ("SpellCast");
			yield return (0.2f);
			for (int i = 8; i > 0; i--) {
				GameObject projectileInstance = Instantiate (Projectile,

					Player.transform.position +
                    Player.transform.forward * Random.Range(4f, 15f) +
                    Player.transform.right * Random.Range (-5f, 5f) +
					Vector3.up * Random.Range (10f, 25f),

					Quaternion.LookRotation (Vector3.down +
						Vector3.forward * Random.Range (-0.25f, 0.25f) +
						Vector3.right * Random.Range (-0.25f, 0.25f)));

				projectileInstance.GetComponent<GenericProjectile> ().caster = Player;
                projectileInstance.GetComponent<GenericProjectile>().damage = baseDamage + attackLevel * attackLevel * 2.25f;
                projectileInstance.GetComponent<GenericProjectile> ().forwardSpeed = baseForwardSpeed * 1.175f * Random.Range (0.9f, 1.1f);
				projectileInstance.GetComponent<GenericProjectile> ().lifeTime = baseLifeTime * 1.175f;
				projectileInstance.GetComponent<GenericProjectile> ().explosionRadius = baseExplosionRadius * 1.175f;
				projectileInstance.GetComponent<GenericProjectile> ().explosionThrust = baseExplosionThrust * 1.175f;
				projectileInstance.GetComponent<GenericProjectile> ().stunTime = stunTime;
                projectileInstance.GetComponent<SphereCollider>().radius = 2.25f;

                randomWait = Random.Range(0.04f, 0.1f);
                yield return new WaitForSeconds(randomWait);
			}
			CustomMethods.PlayClipAt (FireSound, transform.position);
			yield return (0.2f);
			MagicSpellCPUScript.Player.GetComponent<Player_Animation> ().isAiming = false;
			yield return null;
		}  else if (attackLevel == 3 && manaScript.CurMana > level3ManaRequirement) {
			MagicSpellCPUScript.ForcedReset ();
			CameraShake.Shake (0.025f, 1f / Vector3.Distance (Camera.main.transform.position, transform.position) * 15f);
			manaScript.CurMana -= level3ManaRequirement;
			MagicSpellCPUScript.Player.GetComponent<Player_Animation> ().ResetValues ();
			Player.GetComponent<Player_Animation> ().anim.Play ("SpellCast");
			yield return (0.2f);
			for (int i = 10; i > 0; i--) {
                GameObject projectileInstance = Instantiate(Projectile,

                    Player.transform.position +
                    Player.transform.forward * Random.Range(4f, 15f) +
                    Player.transform.right * Random.Range(-5f, 5f) +
                    Vector3.up * Random.Range(10f, 25f),

                    Quaternion.LookRotation(Vector3.down +
                        Vector3.forward * Random.Range(-0.25f, 0.25f) +
                        Vector3.right * Random.Range(-0.25f, 0.25f)));

                projectileInstance.GetComponent<GenericProjectile>().caster = Player;
                projectileInstance.GetComponent<GenericProjectile>().damage = baseDamage + attackLevel * attackLevel * 2.5f;
                projectileInstance.GetComponent<GenericProjectile> ().forwardSpeed = baseForwardSpeed * 1.25f;
				projectileInstance.GetComponent<GenericProjectile> ().lifeTime = baseLifeTime * 1.4f;
				projectileInstance.GetComponent<GenericProjectile> ().explosionRadius = baseExplosionRadius * 1.4f;
				projectileInstance.GetComponent<GenericProjectile> ().explosionThrust = baseExplosionThrust * 1.4f;
				projectileInstance.GetComponent<GenericProjectile> ().stunTime = stunTime;
				projectileInstance.GetComponent<SphereCollider> ().radius = 2.5f;
				yield return new WaitForFixedUpdate();
			}
			CustomMethods.PlayClipAt (FireSound, transform.position);
			yield return (0.2f);
			MagicSpellCPUScript.Player.GetComponent<Player_Animation> ().isAiming = false;
			yield return null;
		} 
		MagicSpellCPUScript.SpellTerminated ();
	}
}
