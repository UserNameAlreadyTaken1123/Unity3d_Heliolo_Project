using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Luminosity.IO;

public class InventoryActivatorDeactivator : MonoBehaviour {

//	private InventoryKeysInput canPauseGame;
	public GameObject InventoryScreen;
	public GameObject Player;
	public Hero_Movement movementScript;
	public bool inventoryMenuActivated = false;
	public float globalTimeScale;

	public bool buttonPressed;

	private bool delayedStart;
	private bool activateInventory;

	// Use this for initialization
	void Start () {
	}

	void DelayedStart () {
		globalTimeScale = Time.timeScale;
		delayedStart = true;
		//		PauseGUI = GameObject.Find ("PlayerSpawn/Canvas/GUI/PauseGUI");
		InventoryScreen = transform.parent.GetComponent<PlayerSpawn>().instanceSpawnInventoryMenu;
		Player = transform.FindChildIncludingDeactivated("Player").gameObject;
		transform.parent.GetComponent<PlayerSpawn> ().Player = Player;
		movementScript = Player.GetComponent<Hero_Movement> ();
		InventoryScreen.transform.FindChildIncludingDeactivated ("InventoryMenu").GetComponent<NewInventory> ().Player = Player;
			
		inventoryMenuActivated = false;
		InventoryScreen.SetActive (false);
	}

	// Update is called once per frame
	void Update () {
		if (!delayedStart)
			DelayedStart ();
		else {
			if (!buttonPressed && InputManager.GetAxisRaw ("Direction Vertical") < 0.0f && !GetComponent<PauseActivatorDeactivator>().pauseMenuActivated) {
				buttonPressed = true;
				if (!inventoryMenuActivated) {
					inventoryMenuActivated = true;
					ActivateDeactivate ();
				} else {
					inventoryMenuActivated = false;
					ActivateDeactivate ();
				}

			} else if (InputManager.GetAxisRaw ("Direction Vertical") == 0.0f) {
				buttonPressed = false; 
			}
		}
	}

	public void ActivateDeactivate(){
		if (inventoryMenuActivated == true) {
			System.GC.Collect();
			globalTimeScale = Time.timeScale;
			Time.timeScale = 0f;
			InventoryScreen.SetActive (true);
			movementScript.enabled = false;
		}

		if (inventoryMenuActivated == false) {
			Time.timeScale = globalTimeScale;
			InventoryScreen.SetActive (false);
			movementScript.enabled = true;
		}
	}


	void Inputs(){
		if (InputManager.GetAxis ("Direction Vertical") < -0.1f){
			if (!buttonPressed) {
				buttonPressed = true;
				activateInventory = true;
			}
		}

		if (InputManager.GetAxis ("Direction Vertical") == 0f){
			buttonPressed = false;
		}   
	}
}
