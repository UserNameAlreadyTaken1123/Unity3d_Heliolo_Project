using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Luminosity.IO;

public class NextScene : MonoBehaviour {

	public  string destiny;
	private int users;


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider user){
		if (user.gameObject.tag == "Player") {
			users = users + 1;				
			}
	}

	void OnTriggerStay(Collider user){
		if (user.gameObject.tag == "Player") {
			if (InputManager.GetButtonDown ("Magic")) {
				StartCoroutine (LoadScene ());			
			}
		}
	}

	IEnumerator LoadScene(){
		yield return new WaitForEndOfFrame ();
		PlayerPrefs.SetString ("NextScene", destiny);
		SceneManager.LoadScene ("LoadingScene");
	}

	void OnTriggerExit(Collider user){
		if (user.gameObject.tag == "Player") {
			users = users - 1;
		}
	}

	void OnGUI(){
		if (users > 0) {
			GUI.Box (new Rect(Screen.width / 2 - 75, Screen.height - 65, 150, 25), "To " + destiny);
		}
	}
}
