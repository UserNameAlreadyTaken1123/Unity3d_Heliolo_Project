using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ButtonAction : MonoBehaviour {
	public int actionToPerform;

	public string character;
	public string scene;

	private GameObject menuManager;
	private GameObject mainMenu;
	private GameObject newGameMenu;
	private GameObject selectSceneMenu;
	private GameObject selectCharacterMenu;
	private GameObject optionsMenu;
	private GameObject graphicSettingsMenu;
	private GameObject audioSettingsMenu;

	static public Resolution[] resolutionsList;
	static public Resolution resolutionsCurrentlySelected;
	static public int resolutionListIndex;
	static private bool fullscreen;

	static private int qualityLevel;
	static private bool simpleConfig;

	void Start (){
		qualityLevel = QualitySettings.GetQualityLevel();
		resolutionsList = Screen.resolutions;
		fullscreen = Screen.fullScreen;

		menuManager = GameObject.Find ("MenuManager");
		mainMenu = menuManager.GetComponent<MenuManagerAlt>().mainMenu;
		newGameMenu = menuManager.GetComponent<MenuManagerAlt>().newGameMenu;
		selectSceneMenu = menuManager.GetComponent<MenuManagerAlt>().selectSceneMenu;
		selectCharacterMenu = menuManager.GetComponent<MenuManagerAlt>().selectCharacterMenu;
		optionsMenu = menuManager.GetComponent<MenuManagerAlt>().optionsMenu;
		graphicSettingsMenu = menuManager.GetComponent<MenuManagerAlt>().graphicSettingsMenu;
		audioSettingsMenu = menuManager.GetComponent<MenuManagerAlt>().audioSettingsMenu;


		
	}

	void OnEnable(){
		Start ();
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

	public void PerformButtonAction () {

		switch (actionToPerform) {
		case 0:
			break;

		case 1:
			NewGame ();
			break;
		case 2:
			LoadGame ();
			break;
		case 3:
			Gallery ();
			break;
		case 4:
			OptionsMenu ();
			break;
		case 5:
			Help ();
			break;
		case 6:
			Exit ();
			break;

		case 11:
			StartNewGame ();
			break;
		case 12:
			SelectSceneMenu ();
			break;
		case 13:
			SelectCharacterMenu ();
			break;
		case 14:
			BackToMain ();
			break;
		case 15:
			BackToNewGame ();
			break;
		case 16:
			SelectCharacter ();
			break;
		case 17:
			SelectScene ();
			break;

		case 21:
			GraphicSettingsMenu ();
			break;

		case 22:
			AudioSettingsMenu ();
			break;

		case 23:
			InputSettingsMenu ();
			break;

		case 24:
			BackToOptionsMenu ();
			break;

		case 25:
			AdvancedSettings ();
			break;

		case 26:
			RendererConfig ();
			break;
		case 27:
			AAConfig ();
			break;
		case 28:
			BlendWeightsConfig ();
			break;
		case 29:
			MaxLod ();
			break;
		case 30:
			MaxLights ();
			break;
		case 31:
			RealtimeReflection ();
			break;
		case 32:
			AdvancedSettings ();
			break;
		case 33:
			AdvancedSettings ();
			break;
		case 34:
			AdvancedSettings ();
			break;
		case 35:
			AdvancedSettings ();
			break;
		case 36:
			AdvancedSettings ();
			break;
		case 37:
			AdvancedSettings ();
			break;


		case 38:
			PresetQualityLevel ();
			break;

		case 39:
			ApplyGraphics ();
			break;

		case 40:
			CycleResolutions ();
			break;

		case 41:
			FullscreenToggle ();
			break;
		}
			
	}

	void NewGame(){
		mainMenu.SetActive (false);
		newGameMenu.SetActive (true);
		List<GameObject> SelectablePlayers = menuManager.GetComponent<MenuManagerAlt> ().SelectablePlayers;
		foreach (GameObject player in SelectablePlayers) {
			if (player.name == PlayerPrefs.GetString ("NewPlayer"))
				player.SetActive (true);
			else
				player.SetActive (false);
		}
	}
	void LoadGame(){

	}
	void Gallery(){
		SceneManager.LoadScene ("Gallery");
	}
	void Options(){

	}
	void Help(){

	}
	void Exit(){
		Application.Quit();
	}

	void StartNewGame(){
		PlayerPrefs.SetInt ("NewGame?", 1);
		SceneManager.LoadScene ("LoadingScene");
	}

	void SelectSceneMenu(){
		newGameMenu.SetActive (false);
		selectSceneMenu.SetActive (true);
	}

	void SelectScene(){
		PlayerPrefs.SetString ("NewScene", scene);
		List<GameObject> SelectablePlayers = menuManager.GetComponent<MenuManagerAlt> ().SelectablePlayers;
		foreach (GameObject player in SelectablePlayers) {
			player.SetActive (false);
		}
		BackToNewGame ();
	}

	void SelectCharacterMenu(){
		selectCharacterMenu.SetActive (true);
		newGameMenu.SetActive (false);
	}

	void SelectCharacter(){
		PlayerPrefs.SetString ("NewPlayer", character);
		List<GameObject> SelectablePlayers = menuManager.GetComponent<MenuManagerAlt> ().SelectablePlayers;
		foreach (GameObject player in SelectablePlayers) {
			if (player.name == PlayerPrefs.GetString ("NewPlayer"))
				player.SetActive (true);
			else
				player.SetActive (false);
		}
		BackToNewGame ();
	}

	void BackToMain(){
		mainMenu.SetActive (true);
		newGameMenu.SetActive (false);
		optionsMenu.SetActive (false);
		List<GameObject> SelectablePlayers = menuManager.GetComponent<MenuManagerAlt> ().SelectablePlayers;
		foreach (GameObject player in SelectablePlayers) {
			player.SetActive (false);
		}
	}

	void BackToNewGame(){
		newGameMenu.SetActive (true);
		selectSceneMenu.SetActive (false);
		selectCharacterMenu.SetActive (false);
		List<GameObject> SelectablePlayers = menuManager.GetComponent<MenuManagerAlt> ().SelectablePlayers;
		foreach (GameObject player in SelectablePlayers) {
			if (player.name == PlayerPrefs.GetString ("NewPlayer"))
				player.SetActive (true);
			else
				player.SetActive (false);
		}
	}

	void OptionsMenu(){
		mainMenu.SetActive (false);
		optionsMenu.SetActive (true);
	}

	void GraphicSettingsMenu(){
		graphicSettingsMenu.SetActive (true);
		optionsMenu.SetActive (false);

		Transform qualityObject = menuManager.GetComponent<MenuManagerAlt> ().graphicSettingsMenu.transform.Find("Quality Level");
		qualityObject.GetComponent<TextMesh> ().text = "Quality Level... " + QualitySettings.GetQualityLevel();
		qualityLevel = QualitySettings.GetQualityLevel ();

		Transform resolutionObject = menuManager.GetComponent<MenuManagerAlt> ().graphicSettingsMenu.transform.Find("Resolution");
		resolutionObject.GetComponent<TextMesh> ().text = "Resolution... " + Screen.currentResolution;
		resolutionsCurrentlySelected = Screen.currentResolution;
		resolutionListIndex = resolutionsList.IndexOf(resolutionsCurrentlySelected);
	}

	void PresetQualityLevel(){
		qualityLevel = qualityLevel + 1;
		if (qualityLevel > 4)
			qualityLevel = 0;
		Transform qualityObject = menuManager.GetComponent<MenuManagerAlt> ().graphicSettingsMenu.transform.Find("Quality Level");
		qualityObject.GetComponent<TextMesh> ().text = "Quality Level... " + qualityLevel;
		simpleConfig = true;
	}

	void CycleResolutions(){

		resolutionListIndex = resolutionListIndex + 1;
		if (resolutionListIndex > resolutionsList.Length)
			resolutionListIndex = 0;

		Transform resolutionObject = menuManager.GetComponent<MenuManagerAlt> ().graphicSettingsMenu.transform.Find("Resolution");
		resolutionObject.GetComponent<TextMesh> ().text = "Resolution... " + resolutionsList[resolutionListIndex];

		transform.parent.GetComponent<MultiResolutionMenu> ().Start ();
	}	

	void FullscreenToggle(){
		Screen.fullScreen = !Screen.fullScreen;

		Transform qualityObject = menuManager.GetComponent<MenuManagerAlt> ().graphicSettingsMenu.transform.Find("Fullscreen");
		qualityObject.GetComponent<TextMesh> ().text = "Fullscreen... " + Screen.fullScreen;
	}

	void AdvancedSettings(){
		simpleConfig = false;
		Transform qualityObject = menuManager.GetComponent<MenuManagerAlt> ().graphicSettingsMenu.transform.Find("Quality Level");
		qualityObject.GetComponent<TextMesh> ().text = "Quality Level... ";
	}
		
	void ApplyGraphics(){
		if (simpleConfig) {
			QualitySettings.SetQualityLevel(qualityLevel, true);
		} else {
			
		}

		Screen.SetResolution (resolutionsList [resolutionListIndex].width, resolutionsList [resolutionListIndex].height, fullscreen);
		transform.parent.GetComponent<MultiResolutionMenu> ().Start ();
	}

	void RendererConfig(){
	}

	void AAConfig(){
	}

	void BlendWeightsConfig(){
	}

	void MaxLod(){
	}

	void MaxLights(){
	}

	void RealtimeReflection(){
	}

















	void AudioSettingsMenu(){
		optionsMenu.SetActive (false);
		audioSettingsMenu.SetActive (true);
	}

	void InputSettingsMenu(){
//		optionsMenu.SetActive (false);
//		inputSettingsMenu.SetActive (true);
		SceneManager.LoadScene ("KeyMappingScene");
	}

	void BackToOptionsMenu(){
		optionsMenu.SetActive (true);
		graphicSettingsMenu.SetActive (false);
		audioSettingsMenu.SetActive (false);
	}
		
}
