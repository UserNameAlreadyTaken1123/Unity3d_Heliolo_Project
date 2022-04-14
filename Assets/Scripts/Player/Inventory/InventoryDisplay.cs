using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryDisplay : MonoBehaviour {

	public GameObject owner;
	public GameObject cameraObject;
	public bool inventoryPauseActivated;
	public List<GameObject> inventoryItems;
	public int slotSelected = 0;

	public MonoBehaviour[] ownerScripts;

	private bool coroRunning;
	private bool ownerScriptsListed;
	private bool deactivatedScripts;
	private bool reactivatedScripts;

	private int counterGun = 1;
	private int counterSword = 1;

	private WaitForSeconds quarterSecond = new WaitForSeconds (0.25f);

	// Use this for initialization
	void Start () {

		foreach (Transform child in transform) {		
			inventoryItems.Add (child.gameObject);
		}		
	}
	
	// Update is called once per frame
	void Update () {
		if (!coroRunning)
			StartCoroutine (UpdateInventory ());

		if (owner && !ownerScriptsListed) {
			ownerScripts = owner.GetComponents<MonoBehaviour> ();
			cameraObject = owner.GetComponent<References> ().Camera;
			ownerScriptsListed = true;
		}
		
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

	IEnumerator UpdateInventory(){
		coroRunning = true;
		inventoryItems.Clear ();

		foreach (Transform child in transform) {		
			inventoryItems.Add (child.gameObject);
		}

		yield return quarterSecond;
		coroRunning = false;
	}

	void InstaUpdateInventory(){
		inventoryItems.Clear ();
		foreach (Transform child in transform) {		
			inventoryItems.Add (child.gameObject);
		}
	}

	public void OnGUI()
	{
		if (inventoryPauseActivated) {
			
			//Display Inventory
			for (int x = 0; x < (inventoryItems.Count - 1) / 4 + 1; x++) { // rows
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("  Row " + (x + 1), GUILayout.Width (50));
				for (int y = 0; y < 4; y++) { // columns
					int index = x * 4 + y;
					if (index >= inventoryItems.Count)
						break;
					if (GUILayout.Button (inventoryItems [index].name, GUILayout.Width (100))) {
						print ("pressed " + inventoryItems [index].name);
						//Move weapon to slot in player Hands.

						Vector3 position = inventoryItems [index].transform.localPosition;
						Quaternion rotation = inventoryItems [index].transform.localRotation;
						Vector3 localScale = inventoryItems [index].transform.localScale;

						if (inventoryItems [index].tag == "EquipedWeaponGun") {

							inventoryItems [index].transform.SetParent (owner.GetComponent<References> ().LeftHand.transform);
							inventoryItems [index].SetActive (false);

							CycleGuns gunSlots = owner.GetComponent<CycleGuns> ();
							if (gunSlots.slot2 == null)
								gunSlots.slot2 = inventoryItems [index].gameObject;
							else if (gunSlots.slot3 == null)
								gunSlots.slot3 = inventoryItems [index].gameObject;
							else if (gunSlots.slot4 == null)
								gunSlots.slot4 = inventoryItems [index].gameObject;
							else {			

								counterGun = counterGun + 1;
								if (counterGun > 4)
									counterGun = 2;

								print ("hand full");

								if (counterGun == 2) {
									//store transform data
									Vector3 position2 = gunSlots.slot2.transform.localPosition;
									Quaternion rotation2 = gunSlots.slot2.transform.localRotation;
									Vector3 localScale2 = gunSlots.slot2.transform.localScale;
									//move to inventory
									gunSlots.slot2.transform.SetParent (this.transform);
									//reapply transform data and activate.
									gunSlots.slot2.transform.localPosition = position2;
									gunSlots.slot2.transform.localRotation = rotation2;
									gunSlots.slot2.transform.localScale = localScale2;

									gunSlots.slot2.transform.GetComponent<GenericGun> ().Reinitialize ();
									gunSlots.slot2.SetActive (true);

									gunSlots.slot2 = inventoryItems [index].gameObject;

									if (gunSlots.slotSelected == 1)
										gunSlots.slotSelected = 0;
									
								} else if (counterGun == 3) {
									//store transform data
									Vector3 position2 = gunSlots.slot3.transform.localPosition;
									Quaternion rotation2 = gunSlots.slot3.transform.localRotation;
									Vector3 localScale2 = gunSlots.slot3.transform.localScale;
									//move to inventory
									gunSlots.slot3.transform.SetParent (this.transform);
									//reapply transform data and activate.
									gunSlots.slot3.transform.localPosition = position2;
									gunSlots.slot3.transform.localRotation = rotation2;
									gunSlots.slot3.transform.localScale = localScale2;

									gunSlots.slot3.transform.GetComponent<GenericGun> ().Reinitialize ();
									gunSlots.slot3.SetActive (true);

									gunSlots.slot3 = inventoryItems [index].gameObject;

									if (gunSlots.slotSelected == 2)
										gunSlots.slotSelected = 0;

								} else if (counterGun == 4) {
									//store transform data
									Vector3 position2 = gunSlots.slot3.transform.localPosition;
									Quaternion rotation2 = gunSlots.slot3.transform.localRotation;
									Vector3 localScale2 = gunSlots.slot3.transform.localScale;
									//move to inventory
									gunSlots.slot4.transform.SetParent (this.transform);
									//reapply transform data and activate.
									gunSlots.slot4.transform.localPosition = position2;
									gunSlots.slot4.transform.localRotation = rotation2;
									gunSlots.slot4.transform.localScale = localScale2;

									gunSlots.slot4.transform.GetComponent<GenericGun> ().Reinitialize ();
									gunSlots.slot4.SetActive (true);

									gunSlots.slot4 = inventoryItems [index].gameObject;

									if (gunSlots.slotSelected == 3)
										gunSlots.slotSelected = 0;

								}									
							}
								
							owner.GetComponent<CycleGuns> ().inventoryUpdated = true;
							inventoryItems [index].GetComponent<GenericGun> ().Reinitialize ();

						}
						else if (inventoryItems [index].tag == "EquipedWeaponSword") {

							inventoryItems [index].transform.SetParent (owner.GetComponent<References> ().RightHand.transform);
							inventoryItems [index].SetActive (false);

							CycleSwords swordSlots = owner.GetComponent<CycleSwords> ();
							if (swordSlots.slot2 == null)
								swordSlots.slot2 = inventoryItems [index].gameObject;
							else if (swordSlots.slot3 == null)
								swordSlots.slot3 = inventoryItems [index].gameObject;
							else if (swordSlots.slot4 == null)
								swordSlots.slot4 = inventoryItems [index].gameObject;
							else {
								
								counterSword = counterSword + 1;
								if (counterSword > 4)
									counterSword = 2;

								print ("hand full, using slot number " + counterSword);

								if (counterSword == 2) {									
									//store transform data
									Vector3 position2 = swordSlots.slot2.transform.localPosition;
									Quaternion rotation2 = swordSlots.slot2.transform.localRotation;
									Vector3 localScale2 = swordSlots.slot2.transform.localScale;
									//move to inventory
									swordSlots.slot2.transform.SetParent (this.transform);
									//reapply transform data and activate.
									swordSlots.slot2.transform.localPosition = position2;
									swordSlots.slot2.transform.localRotation = rotation2;
									swordSlots.slot2.transform.localScale = localScale2;

									swordSlots.slot2.transform.GetComponent<NGS_NewCPU> ().Reinitialize ();
									swordSlots.slot2.SetActive (true);

									swordSlots.slot2 = inventoryItems [index].gameObject;

									if (swordSlots.slotSelected == 1)
										swordSlots.slotSelected = 0;

								} else if (counterSword == 3) {
									//store transform data
									Vector3 position2 = swordSlots.slot3.transform.localPosition;
									Quaternion rotation2 = swordSlots.slot3.transform.localRotation;
									Vector3 localScale2 = swordSlots.slot3.transform.localScale;
									//move to inventory
									swordSlots.slot3.transform.SetParent (this.transform);
									//reapply transform data and activate.
									swordSlots.slot3.transform.localPosition = position2;
									swordSlots.slot3.transform.localRotation = rotation2;
									swordSlots.slot3.transform.localScale = localScale2;

									swordSlots.slot3.transform.GetComponent<NGS_NewCPU> ().Reinitialize ();
									swordSlots.slot3.SetActive (true);

									swordSlots.slot3 = inventoryItems [index].gameObject;

									if (swordSlots.slotSelected == 2)
										swordSlots.slotSelected = 0;
									
								} else if (counterSword == 4) {
									//store transform data
									Vector3 position2 = swordSlots.slot3.transform.localPosition;
									Quaternion rotation2 = swordSlots.slot3.transform.localRotation;
									Vector3 localScale2 = swordSlots.slot3.transform.localScale;
									//move to inventory
									swordSlots.slot4.transform.SetParent (this.transform);
									//reapply transform data and activate.
									swordSlots.slot4.transform.localPosition = position2;
									swordSlots.slot4.transform.localRotation = rotation2;
									swordSlots.slot4.transform.localScale = localScale2;

									swordSlots.slot4.transform.GetComponent<NGS_NewCPU> ().Reinitialize ();
									swordSlots.slot4.SetActive (true);

									swordSlots.slot4 = inventoryItems [index].gameObject;

									if (swordSlots.slotSelected == 3)
										swordSlots.slotSelected = 0;
									
								}

							}

							owner.GetComponent<CycleSwords> ().inventoryUpdated = true;
							inventoryItems [index].GetComponent<NGS_NewCPU> ().Reinitialize ();
						}

						inventoryItems [index].transform.localPosition = position;
						inventoryItems [index].transform.localRotation = rotation;
						inventoryItems [index].transform.localScale = localScale;
						InstaUpdateInventory ();
					}
				}
				GUILayout.EndHorizontal ();
			}
		}
	}
}