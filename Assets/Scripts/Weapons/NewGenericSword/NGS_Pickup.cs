using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;

public class NGS_Pickup : MonoBehaviour {

	private List<GameObject> swordsCarried;
	public GameObject SwordGiven;
	public AudioClip PickUpSound;
	public GameObject PickupFlash;
	private GameObject weaponMeshObject;
	private Transform inventory;
	private bool alreadyOwned;
	private bool alreadyEquiped;
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
			swordsCarried = newOwner.GetComponent<CycleSwords> ().swordsCarried;
			inventory = newOwner.GetComponent<References> ().Inventory.transform;

			//Revisa si está equipada
			foreach (GameObject sword in swordsCarried) {
				if (sword.name == SwordGiven.name) {
					alreadyOwned = true;
				} else {
					//					sword.SetActive (false);
				}			
			}

			//Revisa si está en el inventario
			if (!alreadyOwned) {
				foreach (Transform sword in inventory) {
					if (sword.name == SwordGiven.name) {
						alreadyOwned = true;
					} else {
						//sword.SetActive (false);
					}			
				}
			}

			if (!alreadyOwned) {
				rotate = false;
				GiveItem (newOwner.gameObject);
				//				GameObject Spawn = (GameObject) PrefabUtility.InstantiatePrefab (SwordGiven);
				/*				GameObject Spawn = (GameObject)Instantiate (SwordGiven, newOwner.GetComponent<References> ().LeftHand.transform.position, Quaternion.identity) as GameObject;
				Spawn.name = SwordGiven.name;
				Spawn.transform.SetParent (newOwner.GetComponent<References> ().LeftHand.transform);
				Spawn.transform.localPosition = SwordGiven.transform.localPosition;
				Spawn.transform.localRotation = SwordGiven.transform.localRotation;
				Spawn.transform.localScale = SwordGiven.transform.localScale;
				Spawn.SetActive (false);
*/
			}
		}
	}

	private void GiveItem(GameObject newOwner){
		GameObject Spawn2 = (GameObject)Instantiate (SwordGiven, SwordGiven.transform.position, SwordGiven.transform.rotation) as GameObject;
		Spawn2.transform.localScale = SwordGiven.transform.localScale;
		Spawn2.name = SwordGiven.name;

		if (newOwner.GetComponent<CycleSwords> ().slot1 == null) {
			newOwner.GetComponent<CycleSwords> ().slot1 = Spawn2;
			Spawn2.transform.SetParent (newOwner.GetComponent<References> ().RightHand.transform);
			Spawn2.SetActive (false);
		} else if (newOwner.GetComponent<CycleSwords> ().slot2 == null) {
			newOwner.GetComponent<CycleSwords> ().slot2 = Spawn2;
			Spawn2.transform.SetParent (newOwner.GetComponent<References> ().RightHand.transform);
			Spawn2.SetActive (false);
		} else if (newOwner.GetComponent<CycleSwords> ().slot3 == null) {
			newOwner.GetComponent<CycleSwords> ().slot3 = Spawn2;
			Spawn2.transform.SetParent (newOwner.GetComponent<References> ().RightHand.transform);
			Spawn2.SetActive (false);
		} else if (newOwner.GetComponent<CycleSwords> ().slot4 == null) {
			newOwner.GetComponent<CycleSwords> ().slot4 = Spawn2;
			Spawn2.transform.SetParent (newOwner.GetComponent<References> ().RightHand.transform);
			Spawn2.SetActive (false);
		} else {
			print ("no slots available! This goes to the inventory!");
			Spawn2.transform.SetParent (newOwner.GetComponent<References> ().Inventory.transform);
		}

		Spawn2.transform.localPosition = SwordGiven.transform.localPosition;
		Spawn2.transform.localRotation = SwordGiven.transform.localRotation;
		Spawn2.transform.localScale = SwordGiven.transform.localScale;

		newOwner.GetComponent<CycleSwords> ().UpdateList ();

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
			weaponMeshObject.transform.Rotate (0f, 0f, rotationSpeed * 100 * Time.deltaTime);
	}
}
