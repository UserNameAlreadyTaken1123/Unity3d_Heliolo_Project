using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SaveLoad_Helio : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.F5)) {
			PlayerState.Instance.localPlayerData.Helio_SceneID   = SceneManager.GetActiveScene().buildIndex;
			PlayerState.Instance.localPlayerData.Helio_PositionX = transform.position.x;
			PlayerState.Instance.localPlayerData.Helio_PositionY = transform.position.y;
			PlayerState.Instance.localPlayerData.Helio_PositionZ = transform.position.z;

			PlayerState.Instance.localPlayerData.Helio_Maxhealth = transform.GetComponent<HealthBar>().Maxhealth;
			PlayerState.Instance.localPlayerData.Helio_CurHealth = transform.GetComponent<HealthBar>().CurHealth;
			PlayerState.Instance.localPlayerData.Helio_MaxMana = transform.GetComponent<ManaBar>().MaxMana;
			PlayerState.Instance.localPlayerData.Helio_CurMana = transform.GetComponent<ManaBar>().CurMana;

			PlayerState.Instance.localPlayerData.Helio_runSpeed = transform.GetComponent<Hero_Movement>().runSpeed;
			PlayerState.Instance.localPlayerData.Helio_sprintSpeed = transform.GetComponent<Hero_Movement>().sprintSpeed;
			PlayerState.Instance.localPlayerData.Helio_walkSpeed = transform.GetComponent<Hero_Movement>().walkSpeed;

			PlayerState.Instance.localPlayerData.Helio_jumpBaseForce = transform.GetComponent<Hero_Movement>().jumpBaseForce;
			PlayerState.Instance.localPlayerData.Helio_jumpsAmount = transform.GetComponent<Hero_Movement>().jumpsAmount;
			PlayerState.Instance.localPlayerData.Helio_airSpeed = transform.GetComponent<Hero_Movement>().airSpeed;

			GlobalControl.Instance.SaveData ();
		}

		if (Input.GetKeyDown (KeyCode.F9)) {
			GlobalControl.Instance.LoadData ();
			GlobalControl.Instance.IsSceneBeingLoaded = true;

			int whichScene = GlobalControl.Instance.LocalCopyOfData.Helio_SceneID;

			if (whichScene != 0) {
				SceneManager.LoadScene (whichScene);
				transform.position = new Vector3 (
					GlobalControl.Instance.LocalCopyOfData.Helio_PositionX,
					GlobalControl.Instance.LocalCopyOfData.Helio_PositionY,
					GlobalControl.Instance.LocalCopyOfData.Helio_PositionZ + 0.01f);

				transform.GetComponent<HealthBar> ().Maxhealth = PlayerState.Instance.localPlayerData.Helio_Maxhealth;
				transform.GetComponent<HealthBar> ().CurHealth = PlayerState.Instance.localPlayerData.Helio_CurHealth;
				transform.GetComponent<ManaBar> ().MaxMana = PlayerState.Instance.localPlayerData.Helio_MaxMana;
				transform.GetComponent<ManaBar> ().CurMana = PlayerState.Instance.localPlayerData.Helio_CurMana;

				transform.GetComponent<Hero_Movement> ().runSpeed = PlayerState.Instance.localPlayerData.Helio_runSpeed;
				transform.GetComponent<Hero_Movement> ().sprintSpeed = PlayerState.Instance.localPlayerData.Helio_sprintSpeed;
				transform.GetComponent<Hero_Movement> ().walkSpeed = PlayerState.Instance.localPlayerData.Helio_walkSpeed;

				transform.GetComponent<Hero_Movement> ().jumpBaseForce = PlayerState.Instance.localPlayerData.Helio_jumpBaseForce;
				transform.GetComponent<Hero_Movement> ().jumpsAmount = PlayerState.Instance.localPlayerData.Helio_jumpsAmount;
				transform.GetComponent<Hero_Movement> ().airSpeed = PlayerState.Instance.localPlayerData.Helio_airSpeed;
			}
		}
	}
}
