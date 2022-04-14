using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class FrameCancels : MonoBehaviour {

	private HealthBar healthScript;
	private References references;
	private Hero_Movement movementScript;
	private GameObject Player;

	public bool ActivateTimerToReset;
	public float currentComboTimer;
	public float origTimer = 0.1f;

	// Use this for initialization
	void Start () {
		Player = gameObject;
		healthScript = Player.GetComponent<HealthBar> ();
		references = Player.GetComponent<References> ();
		movementScript = Player.GetComponent<Hero_Movement> ();
		origTimer = Time.fixedDeltaTime * 2f;
	}

	void Update () {
		if (!healthScript.isDead) {
			Input ();
			ResetComboStateTimer (ActivateTimerToReset);
		}
	}

	void ResetComboStateTimer(bool resetTimer){
		if (resetTimer) {
			currentComboTimer -= Time.unscaledDeltaTime;
			if (currentComboTimer <= 0) {
				ForcedReset ();
			}
		} else
			currentComboTimer = 0f;
	}

	void ForcedReset(){
		ActivateTimerToReset = false;
		origTimer = Time.fixedDeltaTime * 2f;
		currentComboTimer = origTimer;
	}

	void Input(){
		if (!ActivateTimerToReset) {
			if ((InputManager.GetButtonDown ("R2") && InputManager.GetButtonDown ("Melee"))) {
				ForcedReset ();
				movementScript.ResetStates ();
				references.SwordAttackCancelation ();
				references.frameCancel = true;
				if (movementScript.isGrounded)
					references.rigidbody.velocity = Vector3.zero;

			}

			else if (InputManager.GetButtonDown ("R2") || InputManager.GetButtonDown ("Melee")) {
				ActivateTimerToReset = true;
				currentComboTimer = origTimer;
			}
		} else if (ActivateTimerToReset) {
			if ((InputManager.GetButtonDown ("R2") && InputManager.GetButton ("Melee")) || 
				(InputManager.GetButton ("R2") && InputManager.GetButtonDown ("Melee"))) {
				ForcedReset ();
				movementScript.ResetStates ();
				references.SwordAttackCancelation ();
				references.frameCancel = true;
				if (movementScript.isGrounded)
					references.rigidbody.velocity = Vector3.zero;
			}
		}
	}
}
