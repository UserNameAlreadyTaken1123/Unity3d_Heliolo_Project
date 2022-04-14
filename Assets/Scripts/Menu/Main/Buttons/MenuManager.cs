using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour {

	public List<GameObject> menuButtons;
	public int buttonSelectedAt = 0;
	private GameObject buttonSelected;
	private GameObject buttonNotSelected;

	public GameObject slot1;
	public GameObject slot2;
	public GameObject slot3;
	public GameObject slot4;
	public GameObject slot5;
	public GameObject slot6;
	public GameObject slot7;
	public GameObject slot8;
	public GameObject slot9;
	public GameObject slot10;

	private bool buttonPressed;
	private float glow = 0f;

	void Awake(){
		glow = 0f;	
		StopCoroutine ("Lighten");
		StopCoroutine ("Darken");
	}

	void Start () {
		if (slot1 == true)
			menuButtons.Add (slot1);
		if (slot2 == true)
			menuButtons.Add (slot2);
		if (slot3 == true)
			menuButtons.Add (slot3);
		if (slot4 == true)
			menuButtons.Add (slot4);
		if (slot5 == true)
			menuButtons.Add (slot5);
		if (slot6 == true)
			menuButtons.Add (slot6);
		if (slot7 == true)
			menuButtons.Add (slot7);
		if (slot8 == true)
			menuButtons.Add (slot8);
		if (slot9 == true)
			menuButtons.Add (slot9);
		if (slot10 == true)
			menuButtons.Add (slot10);
	
	}
	
	// Update is called once per frame
	void Update () {
		print (buttonSelectedAt);
		foreach (GameObject button in menuButtons) {
			if (menuButtons.IndexOf (button) == buttonSelectedAt) {
				buttonSelected = button;
				StartCoroutine (Lighten ());
			} else {
				buttonNotSelected = button;
				StartCoroutine (Darken ());
			}
		}

		int listLenght = menuButtons.Count - 1;
		if (buttonSelectedAt > listLenght)
			buttonSelectedAt = 0;
	}

	void LateUpdate(){
		Inputs ();
	}

	void Inputs(){

		if (Input.GetAxisRaw ("Vertical") < 0.0f)
		{
			if(!buttonPressed)
			{
				buttonSelectedAt = buttonSelectedAt + 1;
				buttonPressed = true;
			}
		}

		if (Input.GetAxisRaw ("Vertical") > 0.0f)
		{
			if(!buttonPressed)
			{
				buttonSelectedAt = buttonSelectedAt - 1;
				buttonPressed = true;
			}
		}

		else if (Input.GetAxisRaw ("Vertical") == 0){
			buttonPressed = false;
		}   

		int listLenght = menuButtons.Count - 1;
		if (buttonSelectedAt > listLenght)
			buttonSelectedAt = 0;
		if (buttonSelectedAt < 0)
			buttonSelectedAt = listLenght;

		if (Input.GetButtonDown ("Jump")){
		}
	}

	IEnumerator Lighten (){
		glow = glow + 0.1f;
		buttonSelected.transform.GetChild (0).gameObject.GetComponent<Renderer> ().material.SetFloat("_MKGlowPower", glow);
		yield return new WaitForSeconds (0.05f);
	}

	IEnumerator Darken (){
		while (glow > 0) {
			glow = glow - 0.1f;
			buttonNotSelected.transform.GetChild (0).gameObject.GetComponent<Renderer> ().material.SetFloat("_MKGlowPower", glow);
			yield return new WaitForSeconds (0.05f);
		}
	}
}
