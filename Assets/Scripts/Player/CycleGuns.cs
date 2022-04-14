using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Luminosity.IO;

public class CycleGuns : MonoBehaviour {

	public bool automationButtonFreeHands;
	public bool cantSwitch;
	public bool _cantSwitch;

	public playerGUI playerGUI;
	public GunsActivatorDeactivator GADscript;
	public CycleSwords cycleSwords;
	public References references;

	public GameObject slot1;
	public GameObject slot2;
	public GameObject slot3;
	public GameObject slot4;

	public List<GameObject> gunsCarried;
	public int slotSelected = 0;

	public GameObject currentGun;
	private GameObject selectedGun;
	public GameObject previousGun;

	private bool buttonPressed;
	public bool hasToChangeWeapons;
	public bool inventoryUpdated;

	public float timeToWait = 0.35f;
	public float timerWaiting;

	// Use this for initialization
	void Awake () {

		timerWaiting = 0.0f;
		currentGun = slot1;
		previousGun = slot1;
		
		if (slot1 != null)
			gunsCarried.Add (slot1);
		if (slot2 != null)
			gunsCarried.Add (slot2);
		if (slot3 != null)
			gunsCarried.Add (slot3);
		if (slot4 != null)
			gunsCarried.Add (slot4);
	}
/*
	void OnGUI(){
		if (isActiveAndEnabled) {
//			if (slot1 != null)
//				GUI.Label (new Rect(10, Screen.height * 3/4, 100, 20), slot1.ToString());
			if (slot2 != null)
				GUI.Label (new Rect(10, Screen.height * 3/4 + 15, 100, 20), slot2.ToString());
			if (slot3 != null)
				GUI.Label (new Rect(10, Screen.height * 3/4 + 30, 100, 20), slot3.ToString());
			if (slot4 != null)
				GUI.Label (new Rect(10, Screen.height * 3/4 + 45, 100, 20), slot4.ToString());
		}
	}
*/

	void Start(){

		playerGUI = GetComponent<playerGUI> ();
		GADscript = GetComponent<GunsActivatorDeactivator> ();
		references = GetComponent<References> ();

		cycleSwords = GetComponent<CycleSwords>();

		foreach (GameObject gun in gunsCarried) {
			if (gunsCarried.IndexOf (gun) == slotSelected) {
				gun.SetActive (true);
			} else {
				gun.SetActive (false);
			}
		}

	}

	public void InstaSwitch(){
		if (!_cantSwitch && !cantSwitch && !cycleSwords.cantSwitch && !cycleSwords._cantSwitch) {
			automationButtonFreeHands = false;
			//selected slot, this variable is kind of a manual input.
			slotSelected = 0;

			//automated update, this is not manual.
			foreach (GameObject gun in gunsCarried) {
				if (gunsCarried.IndexOf (gun) == slotSelected) {
					previousGun = currentGun;
					selectedGun = gun;
				}
			}

			currentGun = selectedGun;
			GetComponent<GunsActivatorDeactivator> ().InstaDoGuns ();
		}
	}

	public void SaveGuns(){
		if (!_cantSwitch && !cantSwitch && !cycleSwords.cantSwitch && !cycleSwords._cantSwitch) {
			automationButtonFreeHands = false;
			//selected slot, this variable is kind of a manual input.
			slotSelected = 0;

			//automated update, this is not manual.
			foreach (GameObject gun in gunsCarried) {
				if (gunsCarried.IndexOf (gun) == slotSelected) {
					previousGun = currentGun;
					selectedGun = gun;
					GetComponent<References> ().LeftHandWeapon = gun;
				}
			}

			currentGun = selectedGun;
			GetComponent<GunsActivatorDeactivator> ().DoGuns ();
		}
	}

	// Update is called once per frame
	void Update () {
		if (GADscript.doingStuff || (currentGun.GetComponent<GenericGun>() && currentGun.GetComponent<GenericGun>().isReloading))
			_cantSwitch = true;
		else
			_cantSwitch = false;	

		if (!_cantSwitch && !cantSwitch && !cycleSwords.cantSwitch && !cycleSwords._cantSwitch && Time.deltaTime > 0f) {
			if (automationButtonFreeHands) {
				InstaSwitch ();
			}
			UpdateList ();
			playerGUI.gunsSlot2 = slot2;
			playerGUI.gunsSlot3 = slot3;
			playerGUI.gunsSlot4 = slot4;

			if (!GetComponent<HealthBar> ().isDead) {
				if (InputManager.GetAxisRaw ("Direction Horizontal") < 0.0f) {
					if (!buttonPressed) {
						slotSelected = slotSelected + 1;
						buttonPressed = true;
						timerWaiting = timeToWait;
						hasToChangeWeapons = true;
					} else
						timerWaiting = timeToWait;
				} else {
					buttonPressed = false;
				}   

				int listLenght = gunsCarried.Count - 1;
				if (slotSelected > listLenght)
					slotSelected = 0;
			}

			//necesito que no se actualice constantemente, sino solamente al apretar el boton
			//de cambiar de arma. En caso de actualizarse constantemente, no se podría distinguir
			//entre el arma previa y la nueva.
			if (buttonPressed == true && timerWaiting == timeToWait) {
				foreach (GameObject gun in gunsCarried) {
					if (gunsCarried.IndexOf (gun) == slotSelected) {
						previousGun = currentGun;
						selectedGun = gun;
						if (selectedGun.GetComponent<GenericGun> ())
							selectedGun.GetComponent<GenericGun> ().Player = this.gameObject;
						GetComponent<References> ().LeftHandWeapon = gun;
					}
				}
			}


			if (!_cantSwitch && timerWaiting <= 0f && hasToChangeWeapons && !GetComponent<GunsActivatorDeactivator> ().doingStuff) {
				currentGun = selectedGun;
				if (currentGun.GetComponent<GenericGun> ()) {
					//GetComponent<Player_Animation> ().gunWeaponType = 0f;
					if (currentGun.GetComponent<GenericGun> ().weaponType > 2) {
						GetComponent<CycleSwords> ().SaveSword ();
					}
				}
				GetComponent<GunsActivatorDeactivator> ().DoGuns ();
				hasToChangeWeapons = false;
			} else {
				timerWaiting = timerWaiting - Time.deltaTime;
			}
			if (timerWaiting < 0f) {
				timerWaiting = 0f;
			}
		}
	}

	public void UpdateList () {
		gunsCarried.Clear();
		if (slot1 != null)
			gunsCarried.Add (slot1);
		if (slot2 != null)
			gunsCarried.Add (slot2);
		if (slot3 != null)
			gunsCarried.Add (slot3);
		if (slot4 != null)
			gunsCarried.Add (slot4);
	}

	/*
	void Inputs(){

		if (InputManager.GetAxisRaw ("Direction Horizontal") < 0.0f)
		{
			if(!buttonPressed)
			{
				slotSelected = slotSelected + 1;
				buttonPressed = true;
				timerWaiting = 0.0f;
				hasToChangeWeapons = true;
			}
		}

		else if (InputManager.GetAxisRaw ("Direction Horizontal") == 0){
			buttonPressed = false;
		}   

		int listLenght = gunsCarried.Count - 1;
		if (slotSelected > listLenght)
			slotSelected = 0;
	}
	*/
}
