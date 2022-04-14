using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonResolutionsCycle : ButtonClass {

	private bool firstTime = true;
	private List<Resolution> resolutionsArray;
	private Resolution[] resolutionsList;
	private Resolution resolutionSelected;
	private int currentW;
	private int currentH;
	private GameObject resolutionSelectedGO;
	private GameObject[] alreadyExists;

	// Use this for initialization
	public override void Start () {
		if (firstTime) {
			firstTime = false;
			resolutionSelected = Screen.currentResolution;
			currentW = resolutionSelected.width;
			currentH = resolutionSelected.height;

			resolutionsList = Screen.resolutions;

			Resolution tempResolution = resolutionsList [0];
			GameObject reference = transform.GetChild (0).gameObject;
			reference.GetComponent<TextMeshPro> ().text = resolutionsList [0].width + "x" + resolutionsList [0].height + " @" + resolutionsList [0].refreshRate;

			foreach (Resolution resolution in resolutionsList) {
				if (tempResolution.width != resolution.width || tempResolution.height != resolution.height || tempResolution.refreshRate != resolution.refreshRate) {
					GameObject resolutionObject = Instantiate (reference, transform);
					resolutionObject.name = resolution.width + "x" + resolution.height + " @" + resolution.refreshRate;
					resolutionObject.GetComponent<TextMeshPro> ().text = resolutionObject.name;
					//resolutionObject.GetComponent<OptionToCycleValues> ().ppIsStringIntOrFloat = 1; 
					//resolutionObject.GetComponent<OptionToCycleValues> ().playerprefsFields.Add ("ScreenW"); 
					//resolutionObject.GetComponent<OptionToCycleValues> ().ppValueInts.Add (resolution.width); 
					//resolutionObject.GetComponent<OptionToCycleValues> ().playerprefsFields.Add ("ScreenH"); 
					//resolutionObject.GetComponent<OptionToCycleValues> ().ppValueInts.Add (resolution.height); 
					resolutionObject.GetComponent<ButtonWriteToPlayerPrefs> ().playerPrefsKeyA[0] = "ScreenW";
					resolutionObject.GetComponent<ButtonWriteToPlayerPrefs> ().playerPrefsKeyA[1] = "ScreenH";
					resolutionObject.GetComponent<ButtonWriteToPlayerPrefs> ().playerPrefsValueA[0] = resolution.width;
					resolutionObject.GetComponent<ButtonWriteToPlayerPrefs> ().playerPrefsValueA[1] = resolution.height;
					tempResolution = resolution;

					if (resolution.width == currentW && resolution.height == currentH)
						resolutionSelectedGO = resolutionObject;
					else
						resolutionObject.SetActive(false);
				}
			}
			reference.SetActive (false);
		}

		//Until Unity fixes the duplicated elements bug in Screen.resolutions;
		//Have to find a way to remove doubles.

		foreach (Transform resolution in transform) {
			resolution.gameObject.SetActive (false);
			if (resolution.gameObject == resolutionSelectedGO) {
				resolution.gameObject.SetActive (true);
				optionIndex = resolution.GetSiblingIndex ();
                GetComponent<ButtonDelegateForChildren>().optionIndex = resolution.GetSiblingIndex();
            } else {
				resolution.gameObject.SetActive (false);
			}
		}	
	}
}
