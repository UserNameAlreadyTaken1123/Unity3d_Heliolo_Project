using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class PauseMenuButtonAction : MonoBehaviour {
	public int actionToPerform;

	public string character;
	public string scene;

	public GameObject menuToActivate;
	public GameObject menuToDeactivate;

	private GameObject PauseMenu;
	private GameObject CharacterPlayerSpawn;
	private GameObject Player;
	private GameObject PlayerCamera;
	private MonoBehaviour[] scriptsList;
	private List<MonoBehaviour> scriptsListedToReactivate = new List<MonoBehaviour>();

	static public Resolution[] resolutionsList;
	static public Resolution resolutionsCurrentlySelected;
	static public int resolutionListIndex;
	static private bool fullscreen;

	static private int qualityLevel;
	static private bool simpleConfig;

	private bool initialized;
	private bool autoAimActivated;

	void DelayedStart (){
		PauseMenu = transform.parent.parent.gameObject;
		CharacterPlayerSpawn = GameObject.Find ("PlayerSpawn").GetComponent<PlayerSpawn> ().Player.transform.parent.gameObject;
		Player = GameObject.Find ("PlayerSpawn").GetComponent<PlayerSpawn> ().Player;
		PlayerCamera = GameObject.Find ("PlayerSpawn").GetComponent<PlayerSpawn> ().Player.GetComponent<References> ().Camera;
		qualityLevel = QualitySettings.GetQualityLevel();
		resolutionsList = Screen.resolutions;
		fullscreen = Screen.fullScreen;

		autoAimActivated = Player.GetComponent<References> ().AutoAim.activeSelf;

		initialized = true;

		if (actionToPerform == 11) {
			GetComponent<TextMeshPro> ().text = "Resolution... " + Screen.currentResolution;
			resolutionsCurrentlySelected = Screen.currentResolution;
			resolutionListIndex = resolutionsList.IndexOf(resolutionsCurrentlySelected);
		}
	}

	void OnMouseEnter() {
		//el parent es quien se encarga de scrollear los botones, es entonces que al ingresar el mouse
		//al collider de un boton, este boton debe anunciarse a su parent para actuar en consecuencia.
		transform.parent.gameObject.GetComponent<ButtonScrolling> ().signalFrom = gameObject;
		transform.parent.gameObject.GetComponent<ButtonScrolling> ().GameobjectWhereMouseEntered();
	}

	void OnMouseUp(){
		transform.parent.gameObject.GetComponent<ButtonScrolling> ().hasSelected = true;
	}

	void OnEnable(){
		if (initialized && actionToPerform == 1) {
			StartCoroutine (WaitForPhotogram ());
			}
		else if (initialized && actionToPerform == 11) {
			GetComponent<TextMeshPro> ().text = "Resolution... " + Screen.currentResolution;
			resolutionsCurrentlySelected = Screen.currentResolution;
			resolutionListIndex = resolutionsList.IndexOf(resolutionsCurrentlySelected);
		}
	}

	IEnumerator WaitForPhotogram(){
		scriptsListedToReactivate = new List<MonoBehaviour> ();
		scriptsList = CharacterPlayerSpawn.transform.parent.GetComponent<PlayerSpawn> ().Player.GetComponents<MonoBehaviour> ();
		yield return new WaitForFixedUpdate ();
		foreach (MonoBehaviour script in scriptsList) {
			if (script.isActiveAndEnabled) {
				scriptsListedToReactivate.Add (script);
				script.enabled = false;
			}
		}
	}

	private void Update(){
		if (!initialized) {
			DelayedStart ();
		} else {

			if (actionToPerform == 10)
				GetComponent<TextMeshPro> ().text = "Quality... " + qualityLevel;
		
			if (actionToPerform == 12)
				GetComponent<TextMeshPro> ().text = "Fullscreen: " + Screen.fullScreen;

			if (actionToPerform == 14)
				GetComponent<TextMeshPro> ().text = "Autoaim in Combo Mode: " + autoAimActivated;
		}
	}

	public void PerformButtonAction () {
		CharacterPlayerSpawn = transform.parent.GetComponent<PauseMenuScrollButtons> ().CharacterPlayerSpawn;
		PlayerCamera = GameObject.Find ("PlayerSpawn").GetComponent<PlayerSpawn> ().Player.GetComponent<References> ().Camera;
		switch (actionToPerform) {
		case 0:
			break;
		case 1:
			ResumeGame ();
			break;
		case 2:
			menuToDeactivate.SetActive (false);
			menuToActivate.SetActive (true);
			if (GetComponent<ActivateDeactivateCameras> ())
				GetComponent<ActivateDeactivateCameras> ().Action ();
			break;
		case 3:
			BackToMainMenu ();
			break;

		case 10:
			PresetQualityLevel ();
		break;

		case 11:
			CycleResolutions ();
			break;

		case 12:
			FullscreenToggle ();
			break;

		case 13:
			ApplyGraphics ();
			break;

		case 14:
			if (Player.GetComponent<References> ().AutoAim.activeSelf) {
				autoAimActivated = false;
				Player.GetComponent<References> ().AutoAim.SetActive (false);
			} else {
				autoAimActivated = true;
				Player.GetComponent<References> ().AutoAim.SetActive (true);
			}
			break;
		}
	}

	void ResumeGame(){
		scriptsList = CharacterPlayerSpawn.transform.parent.GetComponent<PlayerSpawn> ().Player.GetComponents<MonoBehaviour> ();
		foreach (MonoBehaviour script in scriptsList) {
			if (scriptsListedToReactivate.Contains (script))
				script.enabled = true;
		}
		CharacterPlayerSpawn.GetComponent<PauseActivatorDeactivator>().pauseMenuActivated = false;
		CharacterPlayerSpawn.GetComponent<PauseActivatorDeactivator> ().ActivateDeactivate ();
	}

	void BackToMainMenu(){
		PlayerPrefs.SetString ("NextScene", "NewMainMenuGeneric");
		Time.timeScale = PlayerPrefs.GetFloat ("GameSpeed", 1f);
		Time.fixedDeltaTime = PlayerPrefs.GetFloat ("GameFixedUpdateSpeed", 0.01f);
		SceneManager.LoadScene ("LoadingScene");
	}

	void PresetQualityLevel(){
		qualityLevel = qualityLevel + 1;
		if (qualityLevel > 4)
			qualityLevel = 0;
	}

	void CycleResolutions(){
		resolutionListIndex = resolutionListIndex + 1;
		if (resolutionListIndex > resolutionsList.Length)
			resolutionListIndex = 0;
		GetComponent<TextMeshPro> ().text = "Resolution... " + resolutionsList[resolutionListIndex];

		transform.root.Find ("Pause Camera").GetComponent<MultiResolutionMenu> ().Start ();
	}	

	void FullscreenToggle(){
		Screen.fullScreen = !Screen.fullScreen;
	}

	void ApplyGraphics(){
		QualitySettings.SetQualityLevel(qualityLevel, true);
		Screen.SetResolution (resolutionsList [resolutionListIndex].width, resolutionsList [resolutionListIndex].height, fullscreen);
		if (PlayerCamera.GetComponent<GraphicsQuality> ())
			PlayerCamera.GetComponent<GraphicsQuality> ().Start();
		transform.parent.parent.parent.parent.GetChild(0).GetComponent<MultiResolutionMenu> ().Start();

	}


}