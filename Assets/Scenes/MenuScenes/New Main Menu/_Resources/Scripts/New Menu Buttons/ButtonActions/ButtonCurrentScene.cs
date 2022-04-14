using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonCurrentScene : ButtonClass {

	// Use this for initialization
	public void RestartScene () {
		PlayerPrefs.SetString ("NextScene", SceneManager.GetActiveScene().name);
	}
}
