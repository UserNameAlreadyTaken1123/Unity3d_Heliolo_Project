using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;
using TMPro;

public class NewMenuButtonScroller : MonoBehaviour {
	public float scrollCoolDown = 0.1f;

	public List<GameObject> Buttons;

	public int listIndex;
	private float buttonDelay;
	private float horizontalButtonDelay;
	public bool buttonPressed;
	public bool buttonPressedLong;

	public bool cycle;
	public bool horizontalCycle;
	public bool hasSelected;
	public bool hasReturned;

	public bool pressedHorizontal;
	public bool buttonHorizontalPressedLong;
	public int buttonOptionIndexAdd;

	public Color selected = new Vector4 (1.0f, 1.0f, 1.0f, 1f);
	public Color unselected = new Vector4 (0.6f, 0.6f, 0.6f, 1f);

	public AudioClip ButtonScrollSound;
	public AudioClip ButtonClickSound;

	private bool initialized = false;
	private bool restart = false;
	private ButtonClass[] buttonScripts;

	public GameObject backButton;
	static AudioSource buttonClickAS; 

	// Use this for initialization
	void Start () {
		restart = false;
		Buttons.Clear ();
		foreach (Transform child in transform) {
			if (child.GetComponent<ButtonClass> () != null && child.gameObject.activeSelf)
				Buttons.Add (child.gameObject);
		}

		if (backButton == null) {
			if (transform.FindChildIncludingDeactivated ("Back")) {
				backButton = transform.FindChildIncludingDeactivated ("Back").gameObject;
			} else if (transform.FindChildIncludingDeactivated("No")) {
				backButton = transform.FindChildIncludingDeactivated ("No").gameObject;
			}
		}

		foreach (GameObject button in Buttons) {
			if (Buttons.IndexOf (button) == listIndex) {
				if (button.GetComponent<ButtonClass>().usesCustomColors)
					button.GetComponent<TextMeshPro> ().color = button.GetComponent<ButtonClass>().customSelected;
				else
					button.GetComponent<TextMeshPro> ().color = selected;
			} else {
				if (button.GetComponent<ButtonClass>().usesCustomColors)
					button.GetComponent<TextMeshPro> ().color = button.GetComponent<ButtonClass>().customUnselected;
				else
					button.GetComponent<TextMeshPro> ().color = unselected;
			}
		}

		//Activation of button options, without this they wont activate unless user manually activates "cycle" bool
		foreach (GameObject button in Buttons) {
			if (Buttons.IndexOf (button) == listIndex) { //if selected
				if (button.GetComponent<ButtonClass>().usesCustomColors)
					button.GetComponent<TextMeshPro> ().color = button.GetComponent<ButtonClass>().customSelected;
				else
					button.GetComponent<TextMeshPro> ().color = selected;
				
				if (button.GetComponent<ButtonClass> ().canCycleOptions) {
					foreach (Transform option in button.transform){
						if (option.GetComponent<ButtonClass>().usesCustomColors)
							option.GetComponent<TextMeshPro> ().color = option.GetComponent<ButtonClass>().customSelected;
						else
							option.GetComponent<TextMeshPro> ().color = selected;
					}
				}
			} else {
				if (button.GetComponent<ButtonClass>().usesCustomColors)
					button.GetComponent<TextMeshPro> ().color = button.GetComponent<ButtonClass>().customUnselected;
				else
					button.GetComponent<TextMeshPro> ().color = unselected;
				
				if (button.GetComponent<ButtonClass> ().canCycleOptions) {
					foreach (Transform option in button.transform){
						if (option.GetComponent<ButtonClass>().usesCustomColors)
							option.GetComponent<TextMeshPro> ().color = option.GetComponent<ButtonClass>().customUnselected;
						else
							option.GetComponent<TextMeshPro> ().color = unselected;
					}
				}
			}
		}
		initialized = true;
		cycle = true;
	}

	public void GameobjectWhereMouseEntered(GameObject signalFrom){
		foreach (GameObject button in Buttons) {
			if (signalFrom == button) {
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
		if (restart)
			Start ();

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
			horizontalCycle = true;
			foreach (GameObject button in Buttons) {
				if (Buttons.IndexOf (button) == listIndex) {
					
					if (button.GetComponent<ButtonClass>().usesCustomColors)
						button.GetComponent<TextMeshPro> ().color = button.GetComponent<ButtonClass>().customSelected;
					else
						button.GetComponent<TextMeshPro> ().color = selected;
					
					button.GetComponent<ButtonClass> ().thisOneHighlighted = true;
					buttonScripts = Buttons[listIndex].gameObject.GetComponents<ButtonClass>();
					foreach (ButtonClass script in buttonScripts) {
						script.PerformHighlightedAction();
					}
					if (button.GetComponent<ButtonClass> ().canCycleOptions) {
						foreach (Transform option in button.transform){
							if (option.GetComponent<ButtonClass>().usesCustomColors)
								option.GetComponent<TextMeshPro> ().color = option.GetComponent<ButtonClass>().customSelected;
							else
								option.GetComponent<TextMeshPro> ().color = selected;
						}
					}
				} else {
					
					if (button.GetComponent<ButtonClass>().usesCustomColors)
						button.GetComponent<TextMeshPro> ().color = button.GetComponent<ButtonClass>().customUnselected;
					else
						button.GetComponent<TextMeshPro> ().color = unselected;
					
					button.GetComponent<ButtonClass> ().thisOneHighlighted = false;
					if (button.GetComponent<ButtonClass> ().canCycleOptions) {
						foreach (Transform option in button.transform){
							if (option.GetComponent<ButtonClass>().usesCustomColors)
								option.GetComponent<TextMeshPro> ().color = option.GetComponent<ButtonClass>().customUnselected;
							else
								option.GetComponent<TextMeshPro> ().color = unselected;
						}
					}
				}
			}

			if (buttonClickAS == null || Time.timeScale == 0f) {
				buttonClickAS = CustomMethods.PlayClipAt(ButtonScrollSound, Camera.main.transform.position);
			}
			cycle = false;
		}

		if (hasSelected) {
			hasSelected = false;
			if (buttonClickAS == null || Time.timeScale == 0f) {
				buttonClickAS = CustomMethods.PlayClipAt(ButtonScrollSound, Camera.main.transform.position);
			}
			buttonScripts = Buttons[listIndex].gameObject.GetComponents<ButtonClass>();
			foreach (ButtonClass script in buttonScripts) {
				script.PerformButtonAction();
				restart = true;
			}
		}

		if (hasReturned) {
			hasReturned = false;
			if (buttonClickAS == null || Time.timeScale == 0f) {
				buttonClickAS = CustomMethods.PlayClipAt(ButtonScrollSound, Camera.main.transform.position);
			}
			buttonScripts = backButton.GetComponents<ButtonClass>();
			foreach (ButtonClass script in buttonScripts) {
				script.PerformButtonAction();
				restart = true;
			}
		}

		if (horizontalCycle) {
			if (Buttons[listIndex].gameObject.GetComponent<ButtonClass>().canCycleOptions == true){
				if (buttonClickAS == null || Time.timeScale == 0f) {
					buttonClickAS = CustomMethods.PlayClipAt(ButtonScrollSound, Camera.main.transform.position);
				}
				Buttons[listIndex].gameObject.GetComponent<ButtonClass>().ScrollButtonOptions(buttonOptionIndexAdd);
				buttonScripts = Buttons[listIndex].gameObject.GetComponents<ButtonClass>();
				foreach (ButtonClass script in buttonScripts) {
					script.PerformHighlightedAction();
				}
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

		if (InputManager.GetButtonDown ("Jump") || InputManager.GetMouseButtonUp(0))
			hasSelected = true;

		if ((InputManager.GetButtonDown ("Melee") || InputManager.GetMouseButtonUp(1)) && backButton != null)
			hasReturned = true;

		if (InputManager.GetAxis ("Horizontal") > 0.1f || InputManager.GetAxis ("Direction Horizontal") > 0.5f || InputManager.GetAxis ("Mouse ScrollWheel") > 0f){
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

		else if (InputManager.GetAxis ("Horizontal") < -0.1f || InputManager.GetAxis ("Direction Horizontal") < -0.5f || InputManager.GetAxis("Mouse ScrollWheel") < 0f) {
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
