using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;

public class GG_Pickup : MonoBehaviour {

	private List<GameObject> gunsCarried;
	public GameObject GunGiven;
	public GameObject PickupFlash;
	public AudioClip PickUpSound;
	private GameObject weaponMeshObject;
	private Transform inventory;

	private bool alreadyOwned;
	//private bool alreadyEquiped;
	private bool rotate = true;

	public float rotationSpeed = 2.0f;

	private float pickupTimer = 0.125f;

	// Use this for initialization

	void Start () {
		if (PickupFlash == null) {
			if (transform.Find ("PickupFlash")) {
				PickupFlash = transform.Find ("PickupFlash").gameObject;	
			}
		}
		weaponMeshObject = transform.Find ("Pickup").gameObject;
	}

	void OnTriggerStay(Collider newOwner){
		if (pickupTimer <= 0f && newOwner.gameObject.tag == "Player") {
			gunsCarried = newOwner.GetComponent<CycleGuns> ().gunsCarried;
			inventory = newOwner.GetComponent<References> ().Inventory.transform;

			//Revisa si está equipada
			foreach (GameObject gun in gunsCarried) {
				if (gun.name == GunGiven.name) {
					alreadyOwned = true;
				} else {
					//					sword.SetActive (false);
				}			
			}

			//Revisa si está en el inventario
			if (!alreadyOwned) {
				foreach (Transform gun in inventory) {
					if (gun.name == GunGiven.name) {
						alreadyOwned = true;
					} else {
						//sword.SetActive (false);
					}			
				}
			}

			if (!alreadyOwned) {
				rotate = false;
				GiveItem (newOwner.gameObject);
//				GameObject Spawn = (GameObject) PrefabUtility.InstantiatePrefab (GunGiven);
/*				GameObject Spawn = (GameObject)Instantiate (GunGiven, newOwner.GetComponent<References> ().LeftHand.transform.position, Quaternion.identity) as GameObject;
				Spawn.name = GunGiven.name;
				Spawn.transform.SetParent (newOwner.GetComponent<References> ().LeftHand.transform);
				Spawn.transform.localPosition = GunGiven.transform.localPosition;
				Spawn.transform.localRotation = GunGiven.transform.localRotation;
				Spawn.transform.localScale = GunGiven.transform.localScale;
				Spawn.SetActive (false);
*/
			}
		}
	}

	private void GiveItem(GameObject newOwner){
		GameObject Spawn2 = (GameObject)Instantiate (GunGiven, GunGiven.transform.position, GunGiven.transform.rotation) as GameObject;
		Spawn2.transform.localScale = GunGiven.transform.localScale;
		Spawn2.name = GunGiven.name;

		if (newOwner.GetComponent<CycleGuns> ().slot1 == null) {
			newOwner.GetComponent<CycleGuns> ().slot1 = Spawn2;
			if (Spawn2.GetComponent<GenericGun>().weaponType > 2)
				Spawn2.transform.SetParent (newOwner.GetComponent<References> ().RightHand.transform);
			else
				Spawn2.transform.SetParent (newOwner.GetComponent<References> ().LeftHand.transform);
			Spawn2.SetActive (false);
		} else if (newOwner.GetComponent<CycleGuns> ().slot2 == null) {
			newOwner.GetComponent<CycleGuns> ().slot2 = Spawn2;
			if (Spawn2.GetComponent<GenericGun>().weaponType > 2)
				Spawn2.transform.SetParent (newOwner.GetComponent<References> ().RightHand.transform);
			else
				Spawn2.transform.SetParent (newOwner.GetComponent<References> ().LeftHand.transform);
			Spawn2.SetActive (false);
		} else if (newOwner.GetComponent<CycleGuns> ().slot3 == null) {
			newOwner.GetComponent<CycleGuns> ().slot3 = Spawn2;
			if (Spawn2.GetComponent<GenericGun>().weaponType > 2)
				Spawn2.transform.SetParent (newOwner.GetComponent<References> ().RightHand.transform);
			else
				Spawn2.transform.SetParent (newOwner.GetComponent<References> ().LeftHand.transform);
			Spawn2.SetActive (false);
		} else if (newOwner.GetComponent<CycleGuns> ().slot4 == null) {
			newOwner.GetComponent<CycleGuns> ().slot4 = Spawn2;
			if (Spawn2.GetComponent<GenericGun>().weaponType > 2)
				Spawn2.transform.SetParent (newOwner.GetComponent<References> ().RightHand.transform);
			else
				Spawn2.transform.SetParent (newOwner.GetComponent<References> ().LeftHand.transform);
			Spawn2.SetActive (false);
		} else {
			print ("no slots available! This goes to the inventory!");
			Spawn2.transform.SetParent (newOwner.GetComponent<References> ().Inventory.transform);
		}

		Spawn2.transform.localPosition = GunGiven.transform.localPosition;
		Spawn2.transform.localRotation = GunGiven.transform.localRotation;
		Spawn2.transform.localScale = GunGiven.transform.localScale;

		newOwner.GetComponent<CycleGuns> ().UpdateList ();

		if (PickupFlash != null) {
			Destroy (weaponMeshObject);
			GetComponent<CapsuleCollider> ().enabled = false;
			PickupFlash.GetComponent<ParticleSystem> ().Play ();
			StartCoroutine (PickingUp ());
		} else
			Destroy (this.gameObject);
	}

	IEnumerator PickingUp(){
		yield return new WaitForEndOfFrame ();
		if (PickUpSound != null) {
			AudioSource sound = CustomMethods.PlayClipAt (PickUpSound, transform.position);
			sound.volume = 0.8f;
		}

		while (PickupFlash.GetComponent<ParticleSystem> ().IsAlive ()) {
			yield return null;
		}
		Destroy (this.gameObject);
	}

	void OnTriggerExit (Collider newOwner){
		alreadyOwned = false;
		//alreadyEquiped = false;
	}

	// Update is called once per frame
	void Update () {
		if (pickupTimer >= 0f)
			pickupTimer -= Time.deltaTime;

		if (rotate)
			weaponMeshObject.transform.Rotate (0f, rotationSpeed * 100 * Time.deltaTime, 0f);
	}
}
