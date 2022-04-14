using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Luminosity.IO;

public class CycleSwords : MonoBehaviour {


	public bool automationButtonFreeHands;
	public bool cantSwitch;
	public bool _cantSwitch;

	public playerGUI playerGUI;
	private SwordsActivatorDeactivator SADscript;
	public CycleGuns cycleGuns;

	public GameObject slot1;
	public GameObject slot2;
	public GameObject slot3;
	public GameObject slot4;

	public List<GameObject> swordsCarried;
	public int slotSelected = 0;

	public GameObject currentSword;
	private GameObject selectedSword;
	public GameObject previousSword;

	private bool buttonPressed;
	public bool hasToChangeWeapons;
	public bool inventoryUpdated;

	public float timeToWait = 0.35f;
	public float timerWaiting;

	// Use this for initialization
	void Awake () {

		timerWaiting = 0.0f;
		currentSword = slot1;
		previousSword = slot1;

		if (slot1 != null)
			swordsCarried.Add (slot1);
		if (slot2 != null)
			swordsCarried.Add (slot2);
		if (slot3 != null)
			swordsCarried.Add (slot3);
		if (slot4 != null)
			swordsCarried.Add (slot4);
		
	}

	void Start(){

		playerGUI = GetComponent<playerGUI> ();
		SADscript = GetComponent<SwordsActivatorDeactivator> ();

		cycleGuns = GetComponent<CycleGuns>();

		foreach (GameObject sword in swordsCarried) {
			if (swordsCarried.IndexOf (sword) == slotSelected) {
				sword.SetActive (true);
			} else {
				sword.SetActive (false);
			}
		}			
	}

/*
	void OnGUI(){
		if (isActiveAndEnabled) {
//			if (slot1 != null)
//				GUI.Label (new Rect(Screen.width - 110, Screen.height * 3/4, 100, 20), slot1.ToString());
			if (slot2 != null)
				GUI.Label (new Rect(Screen.width - 110, Screen.height * 3/4 + 15, 100, 20), slot2.ToString());
			if (slot3 != null)
				GUI.Label (new Rect(Screen.width - 110, Screen.height * 3/4 + 30, 100, 20), slot3.ToString());
			if (slot4 != null)
				GUI.Label (new Rect(Screen.width - 110, Screen.height * 3/4 + 45, 100, 20), slot4.ToString());
		}
	}
*/

	public void InstaSwitch(){
		if (!_cantSwitch && !cantSwitch && !cycleGuns.cantSwitch && !cycleGuns._cantSwitch) {
			automationButtonFreeHands = false;
			//selected slot, this variable is kind of a manual input.
			slotSelected = 0;

			//automated update, this is not manual.
			foreach (GameObject sword in swordsCarried) {
				if (swordsCarried.IndexOf (sword) == slotSelected) {
					previousSword = currentSword;
					selectedSword = sword;
				}
			}

			currentSword = selectedSword;
			GetComponent<SwordsActivatorDeactivator> ().InstaDoSwords ();
		}
	}

	public void SaveSword(){
		if (!_cantSwitch && !cantSwitch && !cycleGuns.cantSwitch && !cycleGuns._cantSwitch) {
			automationButtonFreeHands = false;
			//selected slot, this variable is kind of a manual input.
			slotSelected = 0;

			//automated update, this is not manual.
			foreach (GameObject sword in swordsCarried) {
				if (swordsCarried.IndexOf (sword) == slotSelected) {
					previousSword = currentSword;
					selectedSword = sword;
					GetComponent<References> ().RightHandWeapon = sword;
				}
			}

			currentSword = selectedSword;
			GetComponent<SwordsActivatorDeactivator> ().DoSwords ();
		}
	}

	// Update is called once per frame
	void Update () {
		if (SADscript.doingStuff || GetComponent<Hero_Movement>().cantStab || (GetComponent<CycleGuns>().currentGun.GetComponent<GenericGun>() && GetComponent<CycleGuns>().currentGun.GetComponent<GenericGun>().isReloading))
			_cantSwitch = true;
		else
			_cantSwitch = false;		

		if (!_cantSwitch && !cantSwitch && !cycleGuns.cantSwitch && !cycleGuns._cantSwitch && Time.deltaTime > 0f) {
			if (automationButtonFreeHands) {
				InstaSwitch ();
			}
			UpdateList ();
			playerGUI.swordsSlot2 = slot2;
			playerGUI.swordsSlot3 = slot3;
			playerGUI.swordsSlot4 = slot4;

			if (!GetComponent<HealthBar> ().isDead) {
				if (InputManager.GetAxisRaw ("Direction Horizontal") > 0.0f) {
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

				int listLenght = swordsCarried.Count - 1;
				if (slotSelected > listLenght)
					slotSelected = 0;
			}

			//////////////////////////////////////////////////
			//necesito que no se actualice constantemente, sino solamente al apretar el boton
			//de cambiar de arma. En caso de actualizarse constantemente, no se podría distinguir
			//entre el arma previa y la nueva.
			if (buttonPressed == true && timerWaiting == timeToWait && !GetComponent<SwordsActivatorDeactivator> ().doingStuff) {
				if (currentSword.GetComponent<NGS_NewCPU> ()) {
					currentSword.GetComponent<NGS_NewCPU> ().deactivateAllBehavior = true;
					currentSword.GetComponent<NGS_NewCPU> ().ForcedReset ();
				}
				foreach (GameObject sword in swordsCarried) {
					if (swordsCarried.IndexOf (sword) == slotSelected) {
						previousSword = currentSword;
						selectedSword = sword;
						GetComponent<References> ().RightHandWeapon = sword;
					} 
				}
			}

			if (timerWaiting <= 0f && hasToChangeWeapons) {
				currentSword = selectedSword;
				GetComponent<SwordsActivatorDeactivator> ().DoSwords ();

				if (GetComponent<CycleGuns> ().currentGun.GetComponent<GenericGun> () && GetComponent<CycleGuns> ().currentGun.GetComponent<GenericGun> ().weaponType > 2) {
					GetComponent<CycleGuns> ().SaveGuns ();
				}

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
		swordsCarried.Clear();
		if (slot1 != null)
			swordsCarried.Add (slot1);
		if (slot2 != null)
			swordsCarried.Add (slot2);
		if (slot3 != null)
			swordsCarried.Add (slot3);
		if (slot4 != null)
			swordsCarried.Add (slot4);
	}

	/*

	void Inputs(){
		if (InputManager.GetAxisRaw ("Direction Horizontal") < 0.0f){
			if(!buttonPressed){
				slotSelected = slotSelected + 1;
				buttonPressed = true;
				timerWaiting = 0.0f;
				hasToChangeWeapons = true;
			}
		}

		else if (InputManager.GetAxisRaw ("Direction Horizontal") == 0){
			buttonPressed = false;
		}   

		int listLenght = swordsCarried.Count - 1;
		if (slotSelected > listLenght)
			slotSelected = 0;
	}

*/
}
