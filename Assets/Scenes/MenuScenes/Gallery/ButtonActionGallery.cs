using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Luminosity.IO;

public class ButtonActionGallery : MonoBehaviour {
	public int actionToPerform;
	public GameObject MenuToActivate;
	public GameObject SomethingElseToActivate;
	public GameObject ElementToDisplay;

	public GameObject camerasContainer;
	private bool actuallyInUse = false;
	private bool hideControls = false;

	void Start (){
		if (camerasContainer == null && GameObject.Find ("Menu Main Cameras Container"))
			camerasContainer = GameObject.Find ("Menu Main Cameras Container");
	}

	void OnMouseEnter() {
		//el parent es quien se encarga de scrollear los botones, es entonces que al ingresar el mouse
		//al collider de un boton, este boton debe anunciarse a su parent para actuar en consecuencia.
		transform.parent.gameObject.GetComponent<ButtonScrollingGallery> ().signalFrom = gameObject;
		transform.parent.gameObject.GetComponent<ButtonScrollingGallery> ().GameobjectWhereMouseEntered();
	}

	void OnMouseUp(){
		transform.parent.gameObject.GetComponent<ButtonScrollingGallery> ().hasSelected = true;
	}

	public void PerformButtonAction () {

		switch (actionToPerform) {
		case 0:
			break;
		case 1:
			MenuToActivate.SetActive (true);
			transform.parent.gameObject.SetActive (false);
			break;
		case 2:
			if (!actuallyInUse) {
				actuallyInUse = true;
				transform.parent.GetComponent<ButtonScrollingGallery> ().CanScroll = false;
				camerasContainer.SetActive (false);
				MenuToActivate.SetActive (true);
				SomethingElseToActivate.SetActive (true);
			} else {
				actuallyInUse = false;
				transform.parent.GetComponent<ButtonScrollingGallery> ().CanScroll = true;
				camerasContainer.SetActive (true);
				MenuToActivate.SetActive (false);
				SomethingElseToActivate.SetActive (false);
			}
			break;
		case 3:
			break;
		case 4:
			break;
		case 5:
			SceneManager.LoadScene ("NewMainMenuGeneric");
			break;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (actionToPerform == 3) {
			if (InputManager.GetButtonDown ("Magic")) {
				if (hideControls) {
					hideControls = false;
					GetComponent<Renderer> ().enabled = true;
				} else {
					hideControls = true;
					GetComponent<Renderer> ().enabled = false;
				}		
			}
		}
	}
}
