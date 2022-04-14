using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DisplayCurrentScene : MonoBehaviour {

	// Use this for initialization
	public void Start () {
		GetComponent<TextMeshPro> ().text = PlayerPrefs.GetString ("NewScene", "None");
		if (SceneManager.GetActiveScene().name.Contains ("Load") && PlayerPrefs.GetInt ("NewGame?", 0) == 0)
			transform.parent.gameObject.SetActive (false);
	}
}
