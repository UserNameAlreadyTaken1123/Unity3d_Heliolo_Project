using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonApply : ButtonClass {

	public bool isVideoOption;
	public bool restartScene;
	private bool shouldRestart;

	private Transform menu;
	private List<Transform> MenuChilds = new List<Transform>();
	private Camera[] AllCameras;
	private ButtonClass[] menuChildrenButtonScripts;

	public override void Start(){
		menu = transform.parent;
		foreach (Transform child in menu) {
			if (child.GetComponent<ButtonDelegateForChildren> ()) {
				MenuChilds.Add (child);
			}
		}
	}
	
	public void ApplySettings(){

		menu = transform.parent;
		foreach (Transform child in menu) {
			if (child.GetComponent<ButtonDelegateForChildren>()) {
				MenuChilds.Add(child);
			}
		}

		Transform parent = transform.parent;
		//GameObject optionInCycle;
		ButtonClass[] optionInCycleScripts;

		foreach (Transform child in MenuChilds) {
			foreach (Transform option in child) {
				if (option.gameObject.activeSelf) {
					print (option.gameObject.name);
					optionInCycleScripts = option.GetComponents<ButtonClass> ();
					foreach (ButtonClass script in optionInCycleScripts) {
						script.PerformButtonAction ();
					}
				}
			}
		}

		if (isVideoOption) {
            int temp1;
            int temp2;
            bool temp3;
			if (PlayerPrefs.GetInt ("Fullscreen", 1) == 1)
				temp3 = true;
			else
				temp3 = false;


			QualitySettings.SetQualityLevel (PlayerPrefs.GetInt ("QualityLevel", 1));
						
			if (Screen.width != PlayerPrefs.GetInt ("ScreenW", Screen.currentResolution.width) ||
				Screen.height != PlayerPrefs.GetInt ("ScreenH", Screen.currentResolution.height) ||
				Screen.currentResolution.refreshRate != PlayerPrefs.GetInt("ScreenH", Screen.currentResolution.refreshRate) ||
				Screen.fullScreen != temp3) {
                temp1 = PlayerPrefs.GetInt("ScreenW");
                temp2 = PlayerPrefs.GetInt("ScreenH");
                Screen.SetResolution (temp1, temp2, temp3);
				print ("A");
				if (restartScene)
					shouldRestart = true;
				else
					shouldRestart = false;
			} else {
                print("B");
                temp1 = Screen.width;
                temp2 = Screen.height;
                Screen.SetResolution(temp1, temp2, temp3);
            }

			if (shouldRestart) {
				shouldRestart = false;
				SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
			}else {
				AllCameras = Camera.allCameras;
				foreach (Camera camera in AllCameras) {
					if (camera.gameObject.activeSelf) {
						camera.gameObject.SetActive (false);
						camera.gameObject.SetActive (true);
					}
				}
			}            
			return;
		}
	}
}
