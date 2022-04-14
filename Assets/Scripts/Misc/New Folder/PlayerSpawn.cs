using UnityEngine;
using System.Collections;
//using UnityEditor;

public class PlayerSpawn : MonoBehaviour {

	public static PlayerSpawn Instance;
	public GameObject PauseMenu;
	public GameObject InventoryMenu;
	public GameObject Player;

	public GameObject instanceSpawnPauseMenu;
	public GameObject instanceSpawnInventoryMenu;

	void Start (){
		if (Instance == null) {
			Instance = this;
			instanceSpawnPauseMenu = (GameObject)Instantiate (PauseMenu, transform.position, transform.rotation) as GameObject;
			instanceSpawnPauseMenu.transform.SetParent (Instance.gameObject.transform);
			instanceSpawnPauseMenu.name = PauseMenu.name;
			instanceSpawnPauseMenu.transform.position = new Vector3 (-1000f, -1000f, -1000f);
			instanceSpawnPauseMenu.GetComponent<NewPauseMenu> ().Player = Player;
			instanceSpawnPauseMenu.transform.SetParent (null);

			instanceSpawnInventoryMenu = (GameObject)Instantiate (InventoryMenu, transform.position, transform.rotation) as GameObject;
			instanceSpawnInventoryMenu.transform.SetParent (Instance.gameObject.transform);
			instanceSpawnInventoryMenu.name = InventoryMenu.name;
			instanceSpawnInventoryMenu.transform.position = new Vector3 (-1000f, -1000f, -1000f);
		} else if (Instance != this) {
			Destroy (gameObject);
		}		 
	}
	/*
	void Start(){
		if (NewGameControl.IsSceneBeingLoaded) {
			if (transform.Find ("Helio_PlayerSpawn")) {
				Player = transform.Find ("Helio_PlayerSpawn/Player").gameObject;
			} else if (transform.Find ("Duncan_PlayerSpawn")) {
				Player = transform.Find ("Duncan_PlayerSpawn/Player").gameObject;
			}
			
			Player.transform.position = new Vector3 (
				NewGameControl.Instance.PositionX,
				NewGameControl.Instance.PositionY,
				NewGameControl.Instance.PositionZ + 0.005f);

			Player.transform.rotation = Quaternion.Euler (
				NewGameControl.Instance.EulerX,
				NewGameControl.Instance.EulerY,
				NewGameControl.Instance.EulerZ);

			Player.transform.GetComponent<HealthBar> ().Maxhealth = NewGameControl.Instance.Maxhealth;
			Player.transform.GetComponent<HealthBar> ().CurHealth = NewGameControl.Instance.CurHealth;
			Player.transform.GetComponent<ManaBar> ().MaxMana = NewGameControl.Instance.MaxMana;
			Player.transform.GetComponent<ManaBar> ().CurMana = NewGameControl.Instance.CurMana;

			Player.transform.GetComponent<Hero_Movement> ().runSpeed = NewGameControl.Instance.runSpeed;
			Player.transform.GetComponent<Hero_Movement> ().sprintSpeed = NewGameControl.Instance.sprintSpeed;
			Player.transform.GetComponent<Hero_Movement> ().walkSpeed = NewGameControl.Instance.walkSpeed;

			Player.transform.GetComponent<Hero_Movement> ().jumpBaseForce = NewGameControl.Instance.jumpBaseForce;
			Player.transform.GetComponent<Hero_Movement> ().jumpsAmount = NewGameControl.Instance.jumpsAmount;
			Player.transform.GetComponent<Hero_Movement> ().airSpeed = NewGameControl.Instance.airSpeed;
			NewGameControl.IsSceneBeingLoaded = false;
		}		
	}
	*/
}
