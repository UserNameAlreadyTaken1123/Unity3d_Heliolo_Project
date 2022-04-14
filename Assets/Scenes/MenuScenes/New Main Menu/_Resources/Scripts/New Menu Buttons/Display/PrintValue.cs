using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PrintValue : MonoBehaviour {

	public bool isInt;
	public bool isFloat;
	public bool isString;
	public string ppKey;

	// Use this for initialization
	public void Start () {
		if (isInt)
			GetComponent<TextMeshPro> ().text = " " + PlayerPrefs.GetInt (ppKey, 0);
		else if (isFloat)
			GetComponent<TextMeshPro> ().text = " " + PlayerPrefs.GetFloat (ppKey, 0f);
		else if (isString)
			GetComponent<TextMeshPro> ().text = PlayerPrefs.GetString (ppKey, "None");
		
		if (SceneManager.GetActiveScene().name.Contains ("Load") && PlayerPrefs.GetInt ("NewGame?", 0) == 0)
			transform.parent.gameObject.SetActive (false);
	}
}
