using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LoadingScreen : MonoBehaviour {
	
	public GUISkin defaultGUISkin;
	private Texture2D texture;
	private AsyncOperation async = null;
	private float value;

	//Always start this coroutine in the start function
	private void Start(){

		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.0133f;
		Time.maximumDeltaTime = 0.0399f;
		PlayerPrefs.SetFloat ("GameFixedUpdateSpeed", Time.fixedDeltaTime);
		PlayerPrefs.SetFloat ("GameMaximumFixedTimestep", Time.maximumDeltaTime);
		PlayerPrefs.SetFloat ("Extra Gravity", 0.15f);

		System.GC.Collect();
		//Cursor.visible = false;

		int newGame = PlayerPrefs.GetInt ("NewGame?");
		string LoadScene;

		if (newGame == 1) {
			LoadScene = PlayerPrefs.GetString ("NewScene", "CampoVerde");

		} else {
			LoadScene = PlayerPrefs.GetString ("NextScene", "CampoVerde");
		}
		StartCoroutine (SoundRaise ());
		StartCoroutine(LoadLevel(LoadScene));
	}

	private IEnumerator SoundRaise(){		
		AudioSource aS = GameObject.Find ("SoundManager").GetComponent<AudioSource> ();
		aS.volume = 0f;
		yield return new WaitForSeconds (0.025f);
		while (aS.volume < 0.5f) {
			aS.volume += 0.01f;
			yield return new WaitForSeconds (0.01f);
		}
	}

	private IEnumerator SoundLower(){		
		AudioSource aS = GameObject.Find ("SoundManager").GetComponent<AudioSource> ();
		while (aS.volume < 0) {
			aS.volume -= 0.01f;
			yield return new WaitForSeconds (0.01f);
		}
	}

	//CoRoutine to return async progress, and trigger level load.
	private IEnumerator LoadLevel(string Level){
		
		yield return new WaitForEndOfFrame();
		PlayerPrefs.SetInt ("NewGame?", 0);
		async = SceneManager.LoadSceneAsync(Level);
		//async.allowSceneActivation = false;
		//GUI.skin.box.normal.background = defaultGUISkin.GetStyle ("textfield").normal.background;
		while (!async.isDone) {
			value = Mathf.Lerp (value, async.progress, 0.25f);
			if (value >= 0.9f) {
				value = 1f;
				StartCoroutine (SoundLower ());
				yield return new WaitForSeconds (1.0f);
				async.allowSceneActivation = true;
			}
			yield return new WaitForFixedUpdate();
		}
		yield return new WaitForFixedUpdate();
	}

	private void OnGUI(){
		//Progress Bar Background
		texture = new Texture2D (1, 1);
		texture.SetPixel (0, 0, new Color (0f, 0f, 0f, 0.25f));
		texture.Apply ();
		GUI.skin.box.normal.background = texture;
		GUI.Box (new Rect (Screen.width / 4, Screen.height - 45 - 2, Screen.width / 2, 12f), "");
		GUI.skin.box.normal.background = defaultGUISkin.GetStyle ("textfield").normal.background;

		//Progress Bar Fill
		if (async != null) {
			texture = new Texture2D (1, 1);
			texture.SetPixel (0, 0, new Color (1f, 0.6f, 0f, 0.375f));
			texture.Apply ();
			GUI.skin.box.normal.background = texture;
			GUI.Box (new Rect (Screen.width / 4 + 2f, Screen.height - 45, (Screen.width / 2 - 4f) * value, 8f), "");
			GUI.skin.box.normal.background = defaultGUISkin.GetStyle ("textfield").normal.background;
		}
	}
}