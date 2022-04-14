using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class NewPauseMenu : MonoBehaviour {

	public GameObject Player;
	private PlayerPrefsManager ppmanager;
	public NewMenuButtonScroller[] subMenus;
	public GameObject[] moreToDeactivate;

	public AudioClip ButtonClickSound;

	static AudioSource buttonClickAS;


	void Start(){
		ppmanager = GameObject.Find ("PlayerPrefsManager").GetComponent<PlayerPrefsManager>();
		subMenus = transform.GetComponentsInChildren<NewMenuButtonScroller>(true);
		subMenus[0].gameObject.SetActive(true);
	}

	// Use this for initialization
	void OnEnable () {
		if (Player) {
			System.GC.Collect ();
			Player.transform.parent.GetComponent<PauseActivatorDeactivator> ().pauseMenuActivated = true;
			Player.GetComponent<Hero_Movement> ().enabled = false;
			subMenus[0].gameObject.SetActive(true);
			Time.timeScale = 0f;
		}
	}
	
	// Update is called once per frame
	void OnDisable () {
		if (Player) {
			Player.transform.parent.GetComponent<PauseActivatorDeactivator> ().pauseMenuActivated = false;
			if (!Player.GetComponent<HealthBar>().isDead)
				Player.GetComponent<Hero_Movement> ().enabled = true;
			ppmanager.Awake ();
		}
	}

    private void Update() {
		if (InputManager.GetButtonDown("Submit")) {
			int index = 0;
			foreach (NewMenuButtonScroller subMenu in subMenus) {
				if (index != 0)
					subMenu.gameObject.SetActive(false);
				else
					subMenu.gameObject.SetActive(true);
				index += 1;
            }
			foreach (GameObject gO in moreToDeactivate) {
				gO.SetActive(false);
			}

			if (buttonClickAS == null || Time.timeScale == 0f) {
				buttonClickAS = CustomMethods.PlayClipAt(ButtonClickSound, Camera.main.transform.position);
			}
			gameObject.SetActive(false);
		}
	}
}
