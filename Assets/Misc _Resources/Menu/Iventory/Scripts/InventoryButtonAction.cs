using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryButtonAction : MonoBehaviour {

	public GameObject owner;
	public GameObject inventoryItem;

	public List<GameObject> inventoryItems;
	public int index;

	public int counterGunSlot;
	public int counterSwordSlot;

	private GameObject ownersInventory;
	private GameObject cameraObject;
	private MonoBehaviour[] ownerScripts;

	private bool coroRunning;
	private bool ownerScriptsListed;
	private bool deactivatedScripts;
	private bool reactivatedScripts;
	public bool inventoryPauseActivated;

	// Use this for initialization
	void Start () {
		owner = transform.parent.GetComponent<NewInventory> ().Player;
		ownersInventory = owner.GetComponent<References> ().Inventory;
		ownerScripts = owner.GetComponents<MonoBehaviour> ();
		cameraObject = owner.GetComponent<References> ().Camera;

//		foreach (Transform child in ownersInventory.transform) {		
//			inventoryItems.Add (child.gameObject);
//		}	
	}

	void FixedUpdate () {
		//if (!coroRunning)
		//	StartCoroutine (UpdateInventory ());

		//para desactivar el comportamiento del jugador mientras este activo el inventario
		if (inventoryPauseActivated) {
			if (!deactivatedScripts) {
				deactivatedScripts = true;
				reactivatedScripts = false;
				cameraObject.GetComponent<Third_Person_Camera> ().noInputControls = true;
				foreach (MonoBehaviour script in ownerScripts) {
					script.enabled = false;
				}
			}
		} else {
			if (!reactivatedScripts) {
				reactivatedScripts = true;
				deactivatedScripts = false;
				cameraObject.GetComponent<Third_Person_Camera> ().noInputControls = false;
				foreach (MonoBehaviour script in ownerScripts) {
					script.enabled = true;
				}
			}
		}
	}
	
	void InstaUpdateInventory(){
//		inventoryItems.Clear ();
//		foreach (Transform child in ownersInventory.transform) {		
//			inventoryItems.Add (child.gameObject);
//		}
	}

	public void PerformButtonAction(){
		print ("selected " + inventoryItem.name);
		//Move weapon to slot in player Hands.

		/*
		Vector3 position = inventoryItem.transform.localPosition;
		Quaternion rotation = inventoryItem.transform.localRotation;
		Vector3 localScale = inventoryItem.transform.localScale;
		*/

		if (inventoryItem.tag == "EquipedWeaponGun" && !owner.GetComponent<CycleGuns>().cantSwitch) {

			//Aca se determina que armas grandes van a la mano derecha, pistolas a la izquierda;
			if (inventoryItem.GetComponent<GenericGun> ().weaponType > 2)
				inventoryItem.transform.SetParent (owner.GetComponent<References> ().RightHand.transform);
			else
				inventoryItem.transform.SetParent (owner.GetComponent<References> ().LeftHand.transform);
			
			inventoryItem.SetActive (false);
			CycleGuns gunSlots = owner.GetComponent<CycleGuns> ();

			if (gunSlots.slot2 == null)
				gunSlots.slot2 = inventoryItem;
			else if (gunSlots.slot3 == null)
				gunSlots.slot3 = inventoryItem;
			else if (gunSlots.slot4 == null)
				gunSlots.slot4 = inventoryItem;
			else {	
				//Counter gun lo que hace es reemplazar el siguiente "slot" con el arma seleccionada. Para no estancarse en reemplazar solo el 1er slot;
				transform.parent.GetComponent<NewInventory>().counterGunSlot =
					transform.parent.GetComponent<NewInventory>().counterGunSlot + 1;
				if (transform.parent.GetComponent<NewInventory>().counterGunSlot > 4)
					transform.parent.GetComponent<NewInventory> ().counterGunSlot = 2;

				counterGunSlot = transform.parent.GetComponent<NewInventory> ().counterGunSlot;
				print ("hand full");

				if (counterGunSlot == 2) {
					//store transform data
					Vector3 position2 = gunSlots.slot2.transform.localPosition;
					Quaternion rotation2 = gunSlots.slot2.transform.localRotation;
					Vector3 localScale2 = gunSlots.slot2.transform.localScale;
					//move to inventory
					gunSlots.slot2.transform.SetParent (ownersInventory.transform);
					//reapply transform data and activate.
					//gunSlots.slot2.transform.localPosition = position2;
					//gunSlots.slot2.transform.localRotation = rotation2;
					//gunSlots.slot2.transform.localScale = localScale2;

					gunSlots.slot2.transform.GetComponent<GenericGun> ().Reinitialize ();
					gunSlots.slot2.SetActive (false);

					gunSlots.slot2 = inventoryItem;

					if (gunSlots.slotSelected == 1) {
						owner.GetComponent<CycleGuns> ().InstaSwitch ();
						gunSlots.slotSelected = 0;
					}

				} else if (counterGunSlot == 3) {
					//store transform data
					Vector3 position2 = gunSlots.slot3.transform.localPosition;
					Quaternion rotation2 = gunSlots.slot3.transform.localRotation;
					Vector3 localScale2 = gunSlots.slot3.transform.localScale;
					//move to inventory
					gunSlots.slot3.transform.SetParent (ownersInventory.transform);
					//reapply transform data and activate.
					//gunSlots.slot3.transform.localPosition = position2;
					//gunSlots.slot3.transform.localRotation = rotation2;
					//gunSlots.slot3.transform.localScale = localScale2;

					gunSlots.slot3.transform.GetComponent<GenericGun> ().Reinitialize ();
					gunSlots.slot3.SetActive (false);

					gunSlots.slot3 = inventoryItem;

					if (gunSlots.slotSelected == 2) {
						owner.GetComponent<CycleGuns> ().InstaSwitch ();
						gunSlots.slotSelected = 0;
					}

				} else if (counterGunSlot == 4) {
					//store transform data
					Vector3 position2 = gunSlots.slot3.transform.localPosition;
					Quaternion rotation2 = gunSlots.slot3.transform.localRotation;
					Vector3 localScale2 = gunSlots.slot3.transform.localScale;
					//move to inventory
					gunSlots.slot4.transform.SetParent (ownersInventory.transform);
					//reapply transform data and activate.
					//gunSlots.slot4.transform.localPosition = position2;
					//gunSlots.slot4.transform.localRotation = rotation2;
					//gunSlots.slot4.transform.localScale = localScale2;

					gunSlots.slot4.transform.GetComponent<GenericGun> ().Reinitialize ();
					gunSlots.slot4.SetActive (false);

					gunSlots.slot4 = inventoryItem;

					if (gunSlots.slotSelected == 3) {
						owner.GetComponent<CycleGuns> ().InstaSwitch ();
						gunSlots.slotSelected = 0;
					}

				}									
			}
				
			owner.GetComponent<CycleGuns> ().inventoryUpdated = true;
			inventoryItem.GetComponent<GenericGun> ().Reinitialize ();



		} else if (inventoryItem.tag == "EquipedWeaponSword") {

			inventoryItem.transform.SetParent (owner.GetComponent<References> ().RightHand.transform);
			inventoryItem.SetActive (false);

			CycleSwords swordSlots = owner.GetComponent<CycleSwords> ();

			if (swordSlots.slot2 == null)
				swordSlots.slot2 = inventoryItem;
			else if (swordSlots.slot3 == null)
				swordSlots.slot3 = inventoryItem;
			else if (swordSlots.slot4 == null)
				swordSlots.slot4 = inventoryItem;
			else {			

				transform.parent.GetComponent<NewInventory>().counterSwordSlot =
					transform.parent.GetComponent<NewInventory>().counterSwordSlot + 1;
				if (transform.parent.GetComponent<NewInventory>().counterSwordSlot > 4)
					transform.parent.GetComponent<NewInventory> ().counterSwordSlot = 2;

				counterSwordSlot = transform.parent.GetComponent<NewInventory> ().counterSwordSlot;
				print ("hand full");

				if (counterSwordSlot == 2) {
					//store transform data
					Vector3 position2 = swordSlots.slot2.transform.localPosition;
					Quaternion rotation2 = swordSlots.slot2.transform.localRotation;
					Vector3 localScale2 = swordSlots.slot2.transform.localScale;
					//move to inventory
					swordSlots.slot2.transform.SetParent (ownersInventory.transform);
					//reapply transform data and activate.
					//swordSlots.slot2.transform.localPosition = position2;
					//swordSlots.slot2.transform.localRotation = rotation2;
					//swordSlots.slot2.transform.localScale = localScale2;

					swordSlots.slot2.transform.GetComponent<NGS_NewCPU> ().Reinitialize ();
					swordSlots.slot2.SetActive (false);

					swordSlots.slot2 = inventoryItem;

					if (swordSlots.slotSelected == 1) {
						owner.GetComponent<CycleSwords> ().InstaSwitch ();
						swordSlots.slotSelected = 0;
					}

				} else if (counterSwordSlot == 3) {
					//store transform data
					Vector3 position2 = swordSlots.slot3.transform.localPosition;
					Quaternion rotation2 = swordSlots.slot3.transform.localRotation;
					Vector3 localScale2 = swordSlots.slot3.transform.localScale;
					//move to inventory
					swordSlots.slot3.transform.SetParent (ownersInventory.transform);
					//reapply transform data and activate.
					//swordSlots.slot3.transform.localPosition = position2;
					//swordSlots.slot3.transform.localRotation = rotation2;
					//swordSlots.slot3.transform.localScale = localScale2;

					swordSlots.slot3.transform.GetComponent<NGS_NewCPU> ().Reinitialize ();
					swordSlots.slot3.SetActive (false);

					swordSlots.slot3 = inventoryItem;

					if (swordSlots.slotSelected == 2) {
						owner.GetComponent<CycleSwords> ().InstaSwitch ();
						swordSlots.slotSelected = 0;
					}

				} else if (counterSwordSlot == 4) {
					//store transform data
					Vector3 position2 = swordSlots.slot3.transform.localPosition;
					Quaternion rotation2 = swordSlots.slot3.transform.localRotation;
					Vector3 localScale2 = swordSlots.slot3.transform.localScale;
					//move to inventory
					swordSlots.slot4.transform.SetParent (ownersInventory.transform);
					//reapply transform data and activate.
					//swordSlots.slot4.transform.localPosition = position2;
					//swordSlots.slot4.transform.localRotation = rotation2;
					//swordSlots.slot4.transform.localScale = localScale2;

					swordSlots.slot4.transform.GetComponent<NGS_NewCPU> ().Reinitialize ();
					swordSlots.slot4.SetActive (false);

					swordSlots.slot4 = inventoryItem;

					if (swordSlots.slotSelected == 3) {
						owner.GetComponent<CycleSwords> ().InstaSwitch ();
						swordSlots.slotSelected = 0;
					}

				}									
			}

			owner.GetComponent<CycleSwords> ().inventoryUpdated = true;
			//inventoryItem.GetComponent<NGS_CPU> ().Reinitialize ();
		}
		/*
		inventoryItem.transform.localPosition = position;
		inventoryItem.transform.localRotation = rotation;
		inventoryItem.transform.localScale = localScale;
		*/

		InstaUpdateInventory ();
		transform.parent.GetComponent<NewInventory> ().OnEnable ();
	}
}

