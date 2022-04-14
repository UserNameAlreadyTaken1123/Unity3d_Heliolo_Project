using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayCurrentCharacter : MonoBehaviour {
	
	// Use this for initialization
	public void Start () {
		GetComponent<TextMeshPro> ().text = PlayerPrefs.GetString ("NewPlayer", "None");
	}

}
