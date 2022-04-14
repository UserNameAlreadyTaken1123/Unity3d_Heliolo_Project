using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ToMainMenu : MonoBehaviour {

	private float globalTimeScale;

	// Use this for initialization
	void Start () {
		globalTimeScale = Time.timeScale;
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void GoToMainMenu(){
		Time.timeScale = globalTimeScale;
		SceneManager.LoadScene ("NewMainMenu");
	}
}
