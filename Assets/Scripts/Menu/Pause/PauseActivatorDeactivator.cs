using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Luminosity.IO;


public class PauseActivatorDeactivator : MonoBehaviour {

	private InventoryKeysInput canOpenInventory;
	public GameObject Inventory;
	public GameObject PauseGUI;
	public bool pauseMenuActivated = false;
	private bool delayedStart;
	public float globalTimeScale;

	// Use this for initialization
	void Start () {
	}

	void DelayedStart () {
		canOpenInventory = Inventory.GetComponent<InventoryKeysInput> ();
		PauseGUI = transform.parent.GetComponent<PlayerSpawn>().instanceSpawnPauseMenu;
		pauseMenuActivated = false;
		PauseGUI.SetActive (false);
		delayedStart = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (!delayedStart)
			DelayedStart ();

		else{
			if (!pauseMenuActivated && InputManager.GetButtonDown ("Submit") && !GetComponent<InventoryActivatorDeactivator>().inventoryMenuActivated) {
				ActivateDeactivate ();
			}
		}
	}

	public void ActivateDeactivate(){
		if (!pauseMenuActivated) {
			canOpenInventory.pauseMenuActivated = true;
			PauseGUI.SetActive (true);
		}

		else if (pauseMenuActivated) {
			canOpenInventory.pauseMenuActivated = false;
			pauseMenuActivated = false;
			PauseGUI.SetActive (false);
		}
	}
}
