using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;
using TMPro;

public class MenuButtonsScroller : MonoBehaviour {
	public float scrollCoolDown = 0.2f;

	public GameObject signalFromThisBrutton;

	public List<GameObject> Buttons;

	public int listIndex;
	private float buttonDelay;
	private float horizontalButtonDelay;
	public bool buttonPressed;
	public bool buttonPressedLong;

	public bool cycle;
	public bool horizontalCycle;
	public bool hasSelected;

	public bool pressedHorizontal;
	public bool buttonHorizontalPressedLong;
	public int buttonOptionIndexAdd;

	public Color selected = new Vector4 (1.0f, 1.0f, 1.0f, 1f);
	public Color unselected = new Vector4 (0.6f, 0.6f, 0.6f, 1f);

	public AudioClip ButtonScrollSound;
	public AudioClip ButtonClickSound;

	private bool initialized = false;

	// Use this for initialization
	void Start () {
		foreach (Transform child in transform) {
			if (child.GetComponent<MenuButtonInterface> () != null)
				Buttons.Add (child.gameObject);
		}

		foreach (GameObject button in Buttons) {
			if (Buttons.IndexOf (button) == listIndex) {
				button.GetComponent<TextMeshPro> ().color = selected;
			} else {
				button.GetComponent<TextMeshPro> ().color = unselected;
			}
		}
		initialized = true;

		//Activation of button options, without this they wont activate unless user manually activates "cycle" bool

		foreach (GameObject button in Buttons) {
			if (Buttons.IndexOf (button) == listIndex) {
				button.GetComponent<TextMeshPro> ().color = selected;
				if (button.GetComponent<MBAOpenNewMenu> ().canCycleOptions) {
					foreach (Transform option in button.transform){
						option.GetComponent<TextMeshPro> ().color = selected;
					}
				}
			} else {
				button.GetComponent<TextMeshPro> ().color = unselected;
				if (button.GetComponent<MBAOpenNewMenu> ().canCycleOptions) {
					foreach (Transform option in button.transform){
						option.GetComponent<TextMeshPro> ().color = unselected;
					}
				}
			}
		}

	}

	public void GameobjectWhereMouseEntered(){
		foreach (GameObject button in Buttons) {
			if (signalFromThisBrutton == button) {
				listIndex = Buttons.IndexOf (button);
				cycle = true;
			}
		}
	}
	

	void OnEnable(){
		if (initialized) {
			buttonDelay = 0;
			Buttons.Clear ();
			Start ();
		}
	}

	// Update is called once per frame
	void Update () {

		Inputs ();

		if (buttonDelay <= 0) {
			buttonDelay = 0;
		} else
			buttonDelay = buttonDelay - Time.deltaTime;	

		if (horizontalButtonDelay <= 0) {
			horizontalButtonDelay = 0;
		} else
			horizontalButtonDelay = horizontalButtonDelay - Time.deltaTime;

		if (cycle) {
			foreach (GameObject button in Buttons) {
				if (Buttons.IndexOf (button) == listIndex) {
					button.GetComponent<TextMeshPro> ().color = selected;
					if (button.GetComponent<MBAOpenNewMenu> ().canCycleOptions) {
						foreach (Transform option in button.transform){
							option.GetComponent<TextMeshPro> ().color = selected;
						}
					}
				} else {
					button.GetComponent<TextMeshPro> ().color = unselected;
					if (button.GetComponent<MBAOpenNewMenu> ().canCycleOptions) {
						foreach (Transform option in button.transform){
							option.GetComponent<TextMeshPro> ().color = unselected;
						}
					}
				}
			}
			CustomMethods.PlayClipAt (ButtonScrollSound, Camera.main.transform.position);
			cycle = false;
		}

		if (hasSelected) {
			hasSelected = false;
			CustomMethods.PlayClipAt (ButtonClickSound, Camera.main.transform.position);
			Buttons[listIndex].gameObject.GetComponent<MenuButtonInterface>().PerformButtonAction();
		}

		if (horizontalCycle) {
			if (Buttons[listIndex].gameObject.GetComponent<MenuButtonInterface>().ScrollButtonOptions() == true){
				CustomMethods.PlayClipAt (ButtonClickSound, Camera.main.transform.position);
			Buttons[listIndex].gameObject.GetComponent<MenuButtonInterface>().ScrollButtonOptions(buttonOptionIndexAdd);
			}
			horizontalCycle = false;
			buttonOptionIndexAdd = 0;
		}

	}

	void Inputs(){
		if (InputManager.GetAxis ("Vertical") > 0.1f || InputManager.GetAxis ("Direction Vertical") > 0.5f){
			if (!buttonPressed) {
				listIndex = listIndex - 1;
				if (buttonPressedLong)
					buttonDelay = scrollCoolDown / 2f;
				else
					buttonDelay = scrollCoolDown;
				buttonPressed = true;
				cycle = true;
			}
		}

		else if (InputManager.GetAxis ("Vertical") < -0.1f || InputManager.GetAxis ("Direction Vertical") < -0.5f){
			if(!buttonPressed){
				listIndex = listIndex + 1;
				if (buttonPressedLong)
					buttonDelay = scrollCoolDown / 2f;
				else
					buttonDelay = scrollCoolDown;
				buttonPressed = true;
				cycle = true;
			}
		}

		if (buttonDelay == 0 || InputManager.GetAxis ("Vertical") == 0f && InputManager.GetAxis ("Direction Vertical") == 0f){
			buttonPressed = false;
			buttonDelay = 0;
			if (InputManager.GetAxis ("Vertical") != 0f || InputManager.GetAxis ("Direction Vertical") != 0f)
				buttonPressedLong = true;
			else
				buttonPressedLong = false;
		}   

		int listLenght = Buttons.Count - 1;
		if (cycle && listIndex > listLenght)
			listIndex = 0;
		if (cycle && listIndex < 0)
			listIndex = Buttons.Count - 1;

		if (InputManager.GetButtonDown ("Jump"))
			hasSelected = true;

		if (InputManager.GetAxis ("Horizontal") > 0.1f || InputManager.GetAxis ("Direction Horizontal") > 0.5f){
			if (!pressedHorizontal) {
				buttonOptionIndexAdd = buttonOptionIndexAdd + 1;
				if (buttonHorizontalPressedLong)
					horizontalButtonDelay = scrollCoolDown / 2f;
				else
					horizontalButtonDelay = scrollCoolDown;
				pressedHorizontal = true;
				horizontalCycle = true;
			}
		}

		else if (InputManager.GetAxis ("Horizontal") < -0.1f || InputManager.GetAxis ("Direction Horizontal") < -0.5f){
			if(!pressedHorizontal){
				buttonOptionIndexAdd = buttonOptionIndexAdd - 1;
				if (buttonHorizontalPressedLong)
					horizontalButtonDelay = scrollCoolDown / 2f;
				else
					horizontalButtonDelay = scrollCoolDown;
				pressedHorizontal = true;
				horizontalCycle = true;
			}
		}

		if (horizontalButtonDelay == 0 || InputManager.GetAxis ("Horizontal") == 0f && InputManager.GetAxis ("Direction Horizontal") == 0f){
			pressedHorizontal = false;
			horizontalButtonDelay = 0;
			if (InputManager.GetAxis ("Horizontal") != 0f || InputManager.GetAxis ("Direction Horizontal") != 0f)
				buttonHorizontalPressedLong = true;
			else
				buttonHorizontalPressedLong = false;
		}
	}
}
