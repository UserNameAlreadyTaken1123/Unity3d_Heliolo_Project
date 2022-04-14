using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class InventoryKeysInput : MonoBehaviour {

	private InventoryDisplay inventoryDisplay;
	private float globalTimeScale;
	private bool inventoryPauseActivated = false;
	private bool buttonPressed;

	public bool pauseMenuActivated;

	private bool doingStuff;

	// Use this for initialization
	void Start () {
		inventoryDisplay = GetComponent<InventoryDisplay> ();
		globalTimeScale = Time.timeScale;
		inventoryPauseActivated = false;
	}
	
	// Update is called once per frame

	void FixedUpdate(){
		if (GetComponent<InventoryDisplay> ().owner.GetComponent<SwordsActivatorDeactivator> ().doingStuff ||
		    GetComponent<InventoryDisplay> ().owner.GetComponent<GunsActivatorDeactivator> ().doingStuff)
			doingStuff = true;
		else
			doingStuff = false;
	}

	void Update () {
		if (!pauseMenuActivated) {
			if (!inventoryPauseActivated)
				Time.timeScale = globalTimeScale;
			else
				Time.timeScale = 0f;

			if (!buttonPressed && !doingStuff && InputManager.GetAxisRaw ("Direction Vertical") < 0.0f) {
				buttonPressed = true;
				if (!inventoryPauseActivated) {
					inventoryPauseActivated = true;
					inventoryDisplay.inventoryPauseActivated = true;
				} else{
					inventoryPauseActivated = false;
					inventoryDisplay.inventoryPauseActivated = false;
				}

			} else if (InputManager.GetAxisRaw ("Direction Vertical") == 0.0f) {
				buttonPressed = false; 
			}
		}
	}
}




