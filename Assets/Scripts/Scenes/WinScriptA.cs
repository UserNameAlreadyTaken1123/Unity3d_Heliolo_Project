using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.PostProcessing;

public class WinScriptA : MonoBehaviour {

	public AudioClip WinSound;

	public List<GameObject> targetsToKill;
	public List<GameObject> targetsToKillTemp;

	private GameObject Player;

	// Use this for initialization
	void Awake () {
		targetsToKillTemp = new List<GameObject> (targetsToKill);
		Invoke ("DelayedStart", 0.01f);
	}

	void DelayedStart(){
		Player = GameObject.FindGameObjectWithTag ("Player");
		StartCoroutine (Checker ());
	}
	
	IEnumerator Checker(){
		yield return new WaitForFixedUpdate ();
		while (targetsToKillTemp.Count > 0) {
			yield return null;
			foreach (GameObject Target in targetsToKill) {				
				if (Target == null || Target.GetComponent<HealthBar> ().isDead)
					targetsToKillTemp.Remove (Target);
			}
		}
		StartCoroutine (Congrats ());
	}

	IEnumerator Congrats(){
		float currentTimeScale = Time.timeScale;
		float currentFixedDeltaTime = Time.fixedDeltaTime;
		float currentMaximumDelta = Time.maximumDeltaTime;

		StartCoroutine (Bloom ());
		StartCoroutine (MusicChange ());

		float t = 3f;
		while (t > 0) {
			t -= Time.deltaTime / Time.timeScale * 2f;
			Time.timeScale = 0.05f;
			Time.fixedDeltaTime = 0.0025f;
			yield return null;
		}

		/*
		Time.timeScale = currentTimeScale;
		Time.fixedDeltaTime = currentFixedDeltaTime;
		Time.maximumDeltaTime = currentMaximumDelta;

		GameObject.Find ("PlayerPrefsManager").GetComponent<PlayerPrefsManager> ().Awake ();
		yield return new WaitForSecondsRealtime (3f);

		Player.GetComponent<Hero_Movement> ().doNotMove = true;
		Player.GetComponent<Hero_Movement> ().cantStab = true;
		Player.GetComponent<Hero_Movement> ().cantShoot = true;
		Player.GetComponent<References> ().Camera.GetComponent<Third_Person_Camera>().noInputControls = true;
		Player.GetComponent<playerGUI> ().youWin = true;
		*/

		/*
		AudioSource oldMusic = GameObject.Find ("SoundManager").GetComponent<AudioSource>();
		StartCoroutine (Bloom ());
		while (oldMusic.volume > 0.1f) {
			yield return new WaitForFixedUpdate ();
			oldMusic.volume -= 0.01f;
			Player.GetComponent<Hero_Movement> ().doNotMove = true;
			Player.GetComponent<Hero_Movement> ().cantStab = true;
			Player.GetComponent<Hero_Movement> ().cantShoot = true;
		}

		oldMusic.gameObject.SetActive (false);
		AudioSource audio = CustomMethods.PlayClipAt (WinSound, Camera.main.transform.position);
		audio.spatialBlend = 0f;
		audio.spatialize = false;
		audio.dopplerLevel = 0f;
		audio.pitch = 1f;
		audio.reverbZoneMix = 0f;
		//StartCoroutine (Bloom ());


		while (audio.isPlaying) {
			//Time.timeScale = Mathf.Lerp (Time.timeScale, 0f, 0.01f);
			yield return new WaitForFixedUpdate();
		}

		yield return new WaitForSecondsRealtime (0.5f);

		PlayerPrefs.SetString ("NextScene", "NewMainMenuGeneric");
		SceneManager.LoadScene ("LoadingScene");
		*/
	}

	IEnumerator MusicChange(){

		yield return new WaitForSecondsRealtime (0.5f);
		AudioSource oldMusic = GameObject.Find ("SoundManager").GetComponent<AudioSource>();
		while (oldMusic.volume > 0.1f) {
			yield return new WaitForFixedUpdate ();
			oldMusic.volume -= 0.01f;
			Player.GetComponent<Hero_Movement> ().doNotMove = true;
			Player.GetComponent<Hero_Movement> ().cantStab = true;
			Player.GetComponent<Hero_Movement> ().cantShoot = true;
		}

		oldMusic.gameObject.SetActive (false);
		AudioSource audio = CustomMethods.PlayClipAt (WinSound, Camera.main.transform.position);
		audio.volume = 0.5f;
		audio.spatialBlend = 0f;
		audio.spatialize = false;
		audio.dopplerLevel = 0f;
		audio.pitch = 1f;
		audio.reverbZoneMix = 0f;
		//StartCoroutine (Bloom ());

		GameObject.Find ("Player").GetComponent<playerGUI>().youWin = true;

		while (audio.isPlaying) {
			//Time.timeScale = Mathf.Lerp (Time.timeScale, 0f, 0.01f);
			yield return new WaitForFixedUpdate();
		}

		yield return new WaitForSecondsRealtime (2f);

		PlayerPrefs.SetString ("NextScene", "NewMainMenuGeneric");
		SceneManager.LoadScene ("LoadingScene");
	}

	IEnumerator Bloom(){
		PostProcessingBehaviour postProcessingBehavior = Player.GetComponent<References> ().Camera.GetComponent<PostProcessingBehaviour> ();
		PostProcessingProfile oldProfile = postProcessingBehavior.profile;
		PostProcessingProfile newProfile = Instantiate(postProcessingBehavior.profile);
		BloomModel.Settings bloomSettings = newProfile.bloom.settings;
		float startingIntensity = newProfile.bloom.settings.bloom.intensity;
		float startingThreshold = newProfile.bloom.settings.bloom.threshold;
		float finalIntensity = startingIntensity;
		float finalThreshold;

		startingIntensity = newProfile.bloom.settings.bloom.intensity;
		startingThreshold = newProfile.bloom.settings.bloom.threshold;
		finalIntensity = oldProfile.bloom.settings.bloom.intensity + 0.25f;
		finalThreshold = oldProfile.bloom.settings.bloom.threshold / 4f;

		float timer = 0f;
		while (timer < 1f) {
			timer += Time.deltaTime / (0.8f * Time.timeScale);
			bloomSettings.bloom.intensity = Mathf.Lerp (startingIntensity, finalIntensity, timer);
			bloomSettings.bloom.threshold = Mathf.Lerp (startingThreshold, finalThreshold, timer);
			newProfile.bloom.settings = bloomSettings;
			postProcessingBehavior.profile = newProfile;
			yield return null;
		}
	}
}
