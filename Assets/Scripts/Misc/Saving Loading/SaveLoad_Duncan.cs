using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SaveLoad_Duncan : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.F5)) {
			PlayerState.Instance.localPlayerData.Duncan_SceneID   = SceneManager.GetActiveScene().buildIndex;
			PlayerState.Instance.localPlayerData.Duncan_PositionX = transform.position.x;
			PlayerState.Instance.localPlayerData.Duncan_PositionY = transform.position.y;
			PlayerState.Instance.localPlayerData.Duncan_PositionZ = transform.position.z;

			PlayerState.Instance.localPlayerData.Duncan_Maxhealth = transform.GetComponent<HealthBar>().Maxhealth;
			PlayerState.Instance.localPlayerData.Duncan_CurHealth = transform.GetComponent<HealthBar>().CurHealth;
			PlayerState.Instance.localPlayerData.Duncan_MaxMana = transform.GetComponent<ManaBar>().MaxMana;
			PlayerState.Instance.localPlayerData.Duncan_CurMana = transform.GetComponent<ManaBar>().CurMana;

			PlayerState.Instance.localPlayerData.Duncan_runSpeed = transform.GetComponent<Hero_Movement>().runSpeed;
			PlayerState.Instance.localPlayerData.Duncan_sprintSpeed = transform.GetComponent<Hero_Movement>().sprintSpeed;
			PlayerState.Instance.localPlayerData.Duncan_walkSpeed = transform.GetComponent<Hero_Movement>().walkSpeed;

			PlayerState.Instance.localPlayerData.Duncan_jumpBaseForce = transform.GetComponent<Hero_Movement>().jumpBaseForce;
			PlayerState.Instance.localPlayerData.Duncan_jumpsAmount = transform.GetComponent<Hero_Movement>().jumpsAmount;
			PlayerState.Instance.localPlayerData.Duncan_airSpeed = transform.GetComponent<Hero_Movement>().airSpeed;

			GlobalControl.Instance.SaveData ();
		}

		if (Input.GetKeyDown (KeyCode.F9)) {
			GlobalControl.Instance.LoadData ();
			GlobalControl.Instance.IsSceneBeingLoaded = true;

			int whichScene = GlobalControl.Instance.LocalCopyOfData.Duncan_SceneID;
			if (whichScene != 0) {
				SceneManager.LoadScene (whichScene);

				transform.position = new Vector3 (
					GlobalControl.Instance.LocalCopyOfData.Duncan_PositionX,
					GlobalControl.Instance.LocalCopyOfData.Duncan_PositionY,
					GlobalControl.Instance.LocalCopyOfData.Duncan_PositionZ + 0.01f);

				transform.GetComponent<HealthBar> ().Maxhealth = PlayerState.Instance.localPlayerData.Duncan_Maxhealth;
				transform.GetComponent<HealthBar> ().CurHealth = PlayerState.Instance.localPlayerData.Duncan_CurHealth;
				transform.GetComponent<ManaBar> ().MaxMana = PlayerState.Instance.localPlayerData.Duncan_MaxMana;
				transform.GetComponent<ManaBar> ().CurMana = PlayerState.Instance.localPlayerData.Duncan_CurMana;

				transform.GetComponent<Hero_Movement> ().runSpeed = PlayerState.Instance.localPlayerData.Duncan_runSpeed;
				transform.GetComponent<Hero_Movement> ().sprintSpeed = PlayerState.Instance.localPlayerData.Duncan_sprintSpeed;
				transform.GetComponent<Hero_Movement> ().walkSpeed = PlayerState.Instance.localPlayerData.Duncan_walkSpeed;

				transform.GetComponent<Hero_Movement> ().jumpBaseForce = PlayerState.Instance.localPlayerData.Duncan_jumpBaseForce;
				transform.GetComponent<Hero_Movement> ().jumpsAmount = PlayerState.Instance.localPlayerData.Duncan_jumpsAmount;
				transform.GetComponent<Hero_Movement> ().airSpeed = PlayerState.Instance.localPlayerData.Duncan_airSpeed;
			}

		}
	}
}
