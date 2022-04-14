using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;
using TMPro;

public class InventoryScroller : MonoBehaviour {
	public float scrollCoolDown = 0.2f;

	public AudioClip ButtonScrollSound;
	public AudioClip ButtonClickSound;

	public GameObject signalFrom;
	public GameObject CharacterPlayerSpawn;
	public Camera PlayerCamera;

	public List<GameObject> Buttons;

	public int listIndex;
	private float buttonDelay;
	public bool buttonPressed;
	public bool cycle;
	public bool updateList;
	public bool hasSelected;

	static private Color unselected = new Vector4 (0.6f, 0.6f, 0.6f, 1f);

	// Use this for initialization
	void Start () {
		foreach (Transform childButton in transform) {
			if (childButton != null)
				Buttons.Add (childButton.gameObject);
		}

		foreach (GameObject button in Buttons) {
			if (Buttons.IndexOf (button) == listIndex) {
				button.GetComponent<TextMeshPro> ().color = Color.white;
			} else {
				button.GetComponent<TextMeshPro> ().color = Color.gray;
			}
		}

		updateList = false;
	}

	public void GameobjectWhereMouseEntered(){
		foreach (GameObject button in Buttons) {
			if (signalFrom == button) {
				listIndex = Buttons.IndexOf (button);
				cycle = true;
			}
		}
	}

	public void OnDisable(){
		buttonDelay = 0;
		listIndex = 0;
	}

	public void Activated(){
		Start ();
		updateList = true;
	}

	// Update is called once per frame
	void Update () {

		Inputs ();

		if (buttonDelay <= 0) {
			buttonDelay = 0;
		} else
			buttonDelay = buttonDelay - Time.deltaTime;		

		if (cycle) {
			foreach (GameObject button in Buttons) {
				if (Buttons.IndexOf (button) == listIndex) {
					button.GetComponent<TextMeshPro> ().color = Color.white;
				} else {
					button.GetComponent<TextMeshPro> ().color = Color.gray;
				}
			}
			PlayClipAt (ButtonScrollSound, Camera.main.transform.position);
			cycle = false;
		}

		if (hasSelected) {
			hasSelected = false;
			PlayClipAt (ButtonClickSound, Camera.main.transform.position);
			Buttons[listIndex].gameObject.GetComponent<InventoryButtonAction>().PerformButtonAction();
		}
	}

	void Inputs(){
		if (InputManager.GetAxis ("Vertical") > 0.1f){
			if (!buttonPressed) {
				listIndex = listIndex - 1;
				buttonDelay = scrollCoolDown;
				buttonPressed = true;
				cycle = true;
			}
		}

		if (InputManager.GetAxis ("Vertical") < -0.1f){
			if(!buttonPressed){
				listIndex = listIndex + 1;
				buttonDelay = scrollCoolDown;
				buttonPressed = true;
				cycle = true;
			}
		}

		if (buttonDelay == 0 || InputManager.GetAxis ("Vertical") == 0f){
			buttonPressed = false;
			buttonDelay = 0;
		}   

		int listLenght = Buttons.Count - 1;
		if (cycle && listIndex > listLenght)
			listIndex = 0;
		if (cycle && listIndex < 0)
			listIndex = Buttons.Count - 1;

		if (InputManager.GetButtonDown ("Jump"))
			hasSelected = true;
	}

	AudioSource PlayClipAt(AudioClip clip, Vector3 pos){
		GameObject tempGO = new GameObject("TempAudio"); // create the temp object
		tempGO.transform.position = pos; // set its position
		AudioSource aSource = tempGO.AddComponent<AudioSource>(); // add an audio source
		aSource.clip = clip; // define the clip
		// set other aSource properties here, if desired
		aSource.Play(); // start the sound
		Destroy(tempGO, clip.length); // destroy object after clip duration
		return aSource; // return the AudioSource reference
	}
}
