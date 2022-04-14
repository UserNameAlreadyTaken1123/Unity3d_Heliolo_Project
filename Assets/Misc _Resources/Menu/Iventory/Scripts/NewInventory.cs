using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NewInventory : MonoBehaviour {
	public float lineSpacing;
	public bool activated;

	private GameObject PlayerSpawn;
	public GameObject Player;
	public GameObject Inventory;
	public GameObject EquippedSwords;
	public GameObject EquippedGuns;
	public GameObject Reference;

	public GameObject MenuItem;

	private float offset;
	private bool coroDone = false;

	public int counterGunSlot = 1;
	public int counterSwordSlot = 1;

	void Start(){
	}

	IEnumerator DelayedStart(){
		yield return new WaitForEndOfFrame ();
		PlayerSpawn = transform.parent.parent.gameObject;
		Player = PlayerSpawn.GetComponent<PlayerSpawn> ().Player;
		Inventory = Player.GetComponent<References> ().Inventory;
		UpdateMenu ();
		yield return new WaitForEndOfFrame ();
		GetComponent<InventoryScroller> ().Activated ();
	}

	public void OnEnable(){
		StartCoroutine (DelayedStart ());
	}

	void UpdateMenu(){
		List<GameObject> Buttons = GetComponent<InventoryScroller>().Buttons;
		foreach (GameObject button in Buttons) {
			Destroy (button);
		}

		Buttons.RemoveAll (GameObject => GameObject != null || GameObject == null);

		foreach (Transform item in Inventory.transform) {
			MenuItem = (GameObject) Instantiate(Reference, Reference.transform.position, Reference.transform.rotation, this.transform);
			MenuItem.transform.position = new Vector3 (MenuItem.transform.position.x,
			MenuItem.transform.position.y + offset, MenuItem.transform.position.z);
			MenuItem.GetComponent<TextMeshPro> ().text = item.name;
			MenuItem.name = item.name;
			MenuItem.layer = 5;
			MenuItem.AddComponent<InventoryButtonAction>();
			MenuItem.GetComponent<InventoryButtonAction> ().inventoryItem = item.gameObject;
			offset = offset - lineSpacing;
		}
		offset = 0;
	}
}
