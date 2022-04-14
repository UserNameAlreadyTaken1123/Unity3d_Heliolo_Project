using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class ButtonScrolling : MonoBehaviour {
	public float scrollCoolDown = 0.2f;

	public AudioClip ButtonScrollSound;
	public AudioClip ButtonClickSound;

	public GameObject signalFrom;

	public List<GameObject> Buttons;

	public int listIndex;
	public float buttonDelay;
	public bool buttonPressed;
	public bool update;
	public bool hasSelected;

	static private Color unselected = new Vector4 (0.6f, 0.6f, 0.6f, 1f);

	// Use this for initialization
	void Start () {

		foreach (Transform childButtons in transform) {
			if (childButtons != null)
				Buttons.Add (childButtons.gameObject);
		}

		foreach (GameObject buttonSelected in Buttons) {
			buttonSelected.GetComponent<TextMesh> ().fontSize = 64;
			buttonSelected.GetComponent<TextMesh> ().characterSize = 0.225f;
			if (Buttons.IndexOf (buttonSelected) == listIndex) {
				buttonSelected.transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);
				buttonSelected.GetComponent<TextMesh> ().color = Color.white;
			} else {
				buttonSelected.transform.localScale = new Vector3 (1f, 1f, 1f);
				buttonSelected.GetComponent<TextMesh>().color = unselected;
			}
		}
	}

	public void GameobjectWhereMouseEntered(){
		foreach (GameObject button in Buttons) {
			if (signalFrom == button) {
				listIndex = Buttons.IndexOf (button);
				update = true;
			}
		}
	}

	public void OnDisable(){
		buttonDelay = 0;
		listIndex = 1;

		foreach (GameObject buttonSelected in Buttons) {
			if (Buttons.IndexOf (buttonSelected) == listIndex) {
				buttonSelected.transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);
				buttonSelected.GetComponent<TextMesh> ().color = Color.white;
			} else {
				buttonSelected.transform.localScale = new Vector3 (1f, 1f, 1f);
				buttonSelected.GetComponent<TextMesh>().color = unselected;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		Inputs ();

		if (buttonDelay <= 0) {
			buttonDelay = 0;
		} else
			buttonDelay = buttonDelay - Time.deltaTime;		

		if (update) {
			foreach (GameObject buttonSelected in Buttons) {
				if (Buttons.IndexOf (buttonSelected) == listIndex) {
					buttonSelected.transform.localScale = new Vector3 (1.2f, 1.2f, 1.2f);
					buttonSelected.GetComponent<TextMesh> ().color = Color.white;
				} else {
					buttonSelected.transform.localScale = new Vector3 (1f, 1f, 1f);
					buttonSelected.GetComponent<TextMesh>().color = unselected;
				}
			}
			PlayClipAt (ButtonScrollSound, Camera.main.transform.position);
			update = false;
		}

		if (hasSelected) {
			hasSelected = false;
			PlayClipAt (ButtonClickSound, Camera.main.transform.position);
			Buttons[listIndex].gameObject.GetComponent<ButtonAction>().PerformButtonAction();
		}
	}

	void Inputs(){
		if (InputManager.GetAxis ("Direction Vertical") > 0.1f || InputManager.GetAxis ("Vertical") > 0.1f){
			if (!buttonPressed) {
				listIndex = listIndex - 1;
				buttonDelay = scrollCoolDown;
				buttonPressed = true;
				update = true;
			}
		}

		if (InputManager.GetAxis ("Direction Vertical") < -0.1f || InputManager.GetAxis ("Vertical") < -0.1f){
			if(!buttonPressed){
				listIndex = listIndex + 1;
				buttonDelay = scrollCoolDown;
				buttonPressed = true;
				update = true;
			}
		}

		if (buttonDelay == 0 || InputManager.GetAxis ("Direction Vertical") == 0f && InputManager.GetAxis ("Vertical") == 0f){
			buttonPressed = false;
			buttonDelay = 0;
		}   

		int listLenght = Buttons.Count - 1;
		if (update && listIndex > listLenght)
			listIndex = 1;
		if (update && listIndex < 1)
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
