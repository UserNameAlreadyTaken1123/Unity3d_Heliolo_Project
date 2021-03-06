using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.IO;
using Luminosity.IO;

public class StartupLoadingScreen : MonoBehaviour {

	public Transform loadingList;

	public GUISkin defaultGUISkin;
	private Texture2D texture;
	private AsyncOperation async = null;
	private float value;

	public TextAsset xmlResourceFile;
	private string xmlFileContent;

	//Always start this coroutine in the start function
	private void Start(){

		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.0133f;
		Time.maximumDeltaTime = 0.0399f;
		PlayerPrefs.SetFloat ("GameFixedUpdateSpeed", Time.fixedDeltaTime);
		PlayerPrefs.SetFloat ("GameMaximumFixedTimestep", Time.maximumDeltaTime);
		PlayerPrefs.SetFloat ("Extra Gravity", 0.15f);

		System.GC.Collect();
		StartCoroutine(StartLoad("NewMainMenuGeneric"));
	}
	//CoRoutine to return async progress, and trigger level load.
	private IEnumerator StartLoad(string Level){
		
		yield return new WaitForFixedUpdate();

		/*
		foreach (Transform toActivate in loadingList) {
			toActivate.gameObject.SetActive (true);
			yield return null;
		}
		*/

		yield return new WaitForSeconds(0.25f);
		if(!Directory.Exists(Application.dataPath + "/config/")){ 
			Directory.CreateDirectory(Application.dataPath + "/config/");
		}

		LoadInputConfigurations ();
		yield return new WaitForFixedUpdate();
		async = SceneManager.LoadSceneAsync(Level);
		//GUI.skin.box.normal.background = defaultGUISkin.GetStyle ("textfield").normal.background;
		while (!async.isDone) {
			value = Mathf.Lerp (value, async.progress, 0.25f);
			if (value == 0.9f) {
				value = 1f;
				yield return new WaitForSeconds (0.5f);
				async.allowSceneActivation = true;
			}
			yield return null;
		}
		yield return null;
	}

	private void LoadInputConfigurations(){
		if (File.Exists (Application.dataPath + "/config/CustomInputConfig"))
			InputManager.Load (Application.dataPath + "/config/CustomInputConfig");
		else if (File.Exists (Application.dataPath + "/config/DefaultConfig"))
			InputManager.Load (Application.dataPath + "/config/DefaultConfig");
		else {
			if (xmlResourceFile == null)
				xmlResourceFile = Resources.Load<TextAsset>("DefaultConfig");
			string newXmlFile = Application.dataPath + "/config/DefaultConfig.xml";
			FileStream fileStream = new FileStream(newXmlFile, FileMode.Create);
			StreamWriter streamWriter = new StreamWriter(fileStream);
			streamWriter.Write(xmlResourceFile.text);
			streamWriter.Flush();
			streamWriter.Close();

			InputManager.Load (Application.dataPath + "/config/DefaultConfig.xml");
			}			

		InputManager.CreateAnalogAxis("DefaultJoystickMap", "Mouse ScrollWheel", 1, 24, 0.0f, 1.0f);
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