using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class MBAOpenNewMenu : MonoBehaviour, MenuButtonInterface {

	public UnityEvent myUnityEvent;
	public bool canCycleOptions = false;

	[Header("If ActivateDeactivateMenu()")]

	public List<GameObject> thingsToDeactivate;
	public List<GameObject> thingsToActivate;

	[Header("If SelectScene()")]
	public string sceneName;

	[Header("If SelectCharacter()")]
	public string characterName;

	[Header("If ApplySettings()")]
	public bool isVideoMenu;

	public List<GameObject> optionsToCycle = new List<GameObject>();
	public int optionIndex;

	//hidden values

	private int qualityLevel;

	private bool firstTime = true;
	private List<Resolution> resolutionsArray;
	private Resolution[] resolutionsList;
	private Resolution resolutionSelected;
	private GameObject[] alreadyExists;

	private string[] settingsToApply;

	private float gameSpeed;

	private bool firstTimeMouseSens = true;
	private float mouseSens;

	public void Start(){
		if (canCycleOptions) {
			optionsToCycle.Clear ();
			foreach (Transform child in transform) {
				optionsToCycle.Add (child.gameObject);
			}
			ScrollButtonOptions (0);
		}			
	}

	public void OnEnable(){
		Start ();
	}

	public void PerformButtonAction (){
		myUnityEvent.Invoke();
	}

	void OnMouseEnter(){
		transform.parent.GetComponent<MenuButtonsScroller> ().signalFromThisBrutton = this.gameObject;
		transform.parent.GetComponent<MenuButtonsScroller> ().GameobjectWhereMouseEntered ();
	}

	public bool ScrollButtonOptions(){
		if (canCycleOptions)
			return true;
		else
			return false;
	}

	public void ScrollButtonOptions(int scroll){

		optionIndex = optionIndex + scroll;

		if (optionIndex > optionsToCycle.Count - 1) {
			optionIndex = 0;
		}
		if (optionIndex < 0) {
			optionIndex = optionsToCycle.Count - 1;
		}

		foreach (GameObject option in optionsToCycle) {
			if (optionsToCycle.IndexOf (option) == optionIndex) {
				option.SetActive(true);
			} else {
				option.SetActive(false);
			}
		}
	}

	public void PerformButtonOption(){
	}




	//Button commands



	public void ActivateDeactivateMenu(){
		foreach (GameObject thingToDeactivate in thingsToDeactivate) {
			thingToDeactivate.SetActive (false);
		}

		foreach (GameObject thingToActivate in thingsToActivate) {
			thingToActivate.SetActive (true);
		}
	}

	public void StartNewGame(){
		PlayerPrefs.SetInt ("NewGame?", 1);
		SceneManager.LoadScene ("LoadingScene");
	}

	public void SelectScene(){
		PlayerPrefs.SetString ("NewScene", sceneName);
	}

	public void GoToScene(){
		SceneManager.LoadScene (sceneName);
	}

	public void SelectCharacter(){
		PlayerPrefs.SetString ("NewPlayer", characterName);
	}

	public void SelectQuality(){
		qualityLevel = QualitySettings.GetQualityLevel();
		foreach (Transform quality in transform) {
			if (quality.GetSiblingIndex () == qualityLevel) {
				quality.gameObject.SetActive (true);
				optionIndex = quality.GetSiblingIndex ();
			} else {
				quality.gameObject.SetActive (false);
			}
		}
	}

	public void SelectResolution(){
		if (firstTime) {
			firstTime = false;
			resolutionSelected = Screen.currentResolution;
			resolutionsList = Screen.resolutions;

			Resolution tempResolution = resolutionsList [0];
			GameObject reference = transform.GetChild (0).gameObject;
			reference.GetComponent<TextMeshPro> ().text = resolutionsList [0].width + "x" + resolutionsList [0].height + " @" + resolutionsList [0].refreshRate;

			foreach (Resolution resolution in resolutionsList) {
				if (tempResolution.width != resolution.width || tempResolution.height != resolution.height || tempResolution.refreshRate != resolution.refreshRate) {
					GameObject resolutionObject = Instantiate (reference, transform);
					resolutionObject.name = resolution.width + "x" + resolution.height + " @" + resolution.refreshRate;
					resolutionObject.GetComponent<TextMeshPro> ().text = resolutionObject.name;
					resolutionObject.GetComponent<OptionToCycleValues> ().ppIsStringIntOrFloat = 1; 
					//resolutionObject.GetComponent<OptionToCycleValues> ().playerprefsFields.Add ("ScreenW"); 
					//resolutionObject.GetComponent<OptionToCycleValues> ().ppValueInts.Add (resolution.width); 
					//resolutionObject.GetComponent<OptionToCycleValues> ().playerprefsFields.Add ("ScreenH"); 
					//resolutionObject.GetComponent<OptionToCycleValues> ().ppValueInts.Add (resolution.height); 
					resolutionObject.GetComponent<OptionToCycleValues> ().ppValueInts[0] = resolution.width; 
					resolutionObject.GetComponent<OptionToCycleValues> ().ppValueInts[1] = resolution.height; 
					tempResolution = resolution;
				}
			}

			optionsToCycle.Remove(transform.GetChild(0).gameObject);
		}

		//Until Unity fixes the duplicated elements bug in Screen.resolutions;
		// Have to finda a way to remove doubles

		foreach (Transform resolution in transform) {
			if (resolution.GetSiblingIndex () == resolutionsList.IndexOf (resolutionSelected)) {
				resolution.gameObject.SetActive (true);
				optionIndex = resolution.GetSiblingIndex ();
			} else {
				resolution.gameObject.SetActive (false);
			}
		}	
	}

	public void SelectMouseSens(){
		
		mouseSens = PlayerPrefs.GetFloat ("MouseSens", 1.0f);

		if (firstTimeMouseSens) {
			print ("entered");
			firstTimeMouseSens = false;

			GameObject reference = transform.GetChild (0).gameObject;
			reference.GetComponent<TextMeshPro> ().text = "0.0f";
			//float tempValues = 0.1f;

			for (float tempValues = 0.1f; tempValues <= 3f; tempValues = Mathf.Round((tempValues + 0.1f)*10)/10) {
				GameObject valueObject = Instantiate (reference, transform);
				valueObject.name = tempValues.ToString ("f1");
				valueObject.GetComponent<TextMeshPro> ().text = valueObject.name;
				valueObject.GetComponent<OptionToCycleValues> ().ppIsStringIntOrFloat = 2;
				valueObject.GetComponent<OptionToCycleValues> ().ppValueFloats [0] = tempValues;
			}

			optionsToCycle.Remove(reference);
		}

		foreach (Transform value in transform) {
			if (value.GetComponent<OptionToCycleValues> ().ppValueFloats [0] == mouseSens) {
				value.gameObject.SetActive (true);
				optionIndex = value.GetSiblingIndex ();
			} else {
				value.gameObject.SetActive (false);
			}
		}	
	}

	//para cada boton, ver si tiene opciones para scrollear
	//si las tiene, acceder y aplicar las variables del hijo activado de cada botón.
	//si no las tiene, no hacer nada
	public void ApplySettings(){
		Transform parent = transform.parent;
		Transform childOption;
		Transform childOptionValue;

		//cuenta los hijos del menu.
		for (int i = 0; i < parent.childCount; i++) {
			childOption = parent.GetChild (i);
			//si este hijo tiene opciones...
			if (childOption.GetComponent<MenuButtonInterface> ().ScrollButtonOptions ()) {
				//...encontrar el primer hijo activo y acceder a aplicar sus valores.
				for (int ii = 0; ii < childOption.childCount; ii++) {
					if (childOption.GetChild (ii).gameObject.activeSelf == true) {
						childOptionValue = childOption.GetChild (ii);
						childOptionValue.GetComponent<MenuButtonInterface> ().PerformButtonOption ();
					}
				}
			}
		}

		if (isVideoMenu) {
			QualitySettings.SetQualityLevel (PlayerPrefs.GetInt ("QualityLevel", 2));

			bool temp;
			if (PlayerPrefs.GetInt ("Fullscreen", 1) > 0)
				temp = true;
			else
				temp = false;

			if (Screen.currentResolution.width != PlayerPrefs.GetInt ("ScreenW", Screen.currentResolution.width) &&
				Screen.currentResolution.height != PlayerPrefs.GetInt ("ScreenH", Screen.currentResolution.height)) {
				Screen.SetResolution (PlayerPrefs.GetInt ("ScreenW"), PlayerPrefs.GetInt ("ScreenH"), temp);
				SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
			}
		}
	}

	public void ExitGame(){
		System.GC.Collect();
		Application.Quit ();
	}

	public void GameSpeed(){
		gameSpeed = PlayerPrefs.GetFloat ("GameSpeed", 1.0f);
		int optionNumber;

		if (gameSpeed == 0.8f)
			optionNumber = 0;
		else if (gameSpeed == 1f)
			optionNumber = 1;
		else //(gameSpeed == 1.2f)
			optionNumber = 2;

		foreach (Transform gameSpeedOption in transform) {
			if (gameSpeedOption.GetSiblingIndex () == optionNumber) {
				gameSpeedOption.gameObject.SetActive (true);
				optionIndex = gameSpeedOption.GetSiblingIndex ();
			} else {
				gameSpeedOption.gameObject.SetActive (false);
			}
		}
	}
}

