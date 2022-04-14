using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour {

	public bool resetToDefaults;

	// Use this for initialization
	/*
	public void Awake () {
		if (!resetToDefaults) {
			Time.timeScale = PlayerPrefs.GetFloat ("GameSpeed", 1f);
			if (Time.timeScale == 0.8f){
				Time.fixedDeltaTime = 0.0133f;
				//Time.maximumDeltaTime = 0.0266f;
				Time.maximumDeltaTime = 0.0399f;
				PlayerPrefs.SetFloat ("GameFixedUpdateSpeed", Time.fixedDeltaTime);
				PlayerPrefs.SetFloat ("GameMaximumFixedTimestep", Time.maximumDeltaTime);
			}
			if (Time.timeScale == 1.0f){
				Time.fixedDeltaTime = 0.0133f;
				Time.maximumDeltaTime = 0.0399f;
				PlayerPrefs.SetFloat ("GameFixedUpdateSpeed", Time.fixedDeltaTime);
				PlayerPrefs.SetFloat ("GameMaximumFixedTimestep", Time.maximumDeltaTime);
			}
			if (Time.timeScale == 1.2f){
				Time.fixedDeltaTime = 0.0133f;
				Time.maximumDeltaTime = 0.0399f;
				PlayerPrefs.SetFloat ("GameFixedUpdateSpeed", Time.fixedDeltaTime);
				PlayerPrefs.SetFloat ("GameMaximumFixedTimestep", Time.maximumDeltaTime);
			}
			if (Time.timeScale == 1.5f){
				Time.fixedDeltaTime = 0.00833f;
				Time.maximumDeltaTime = 0.02499f;
				PlayerPrefs.SetFloat ("GameFixedUpdateSpeed", Time.fixedDeltaTime);
				PlayerPrefs.SetFloat ("GameMaximumFixedTimestep", Time.maximumDeltaTime);
			}
		} else {
			Time.timeScale = 1.0f;
			Time.fixedDeltaTime = 0.0133f;
			Time.maximumDeltaTime = 0.0399f;
			PlayerPrefs.SetFloat ("GameFixedUpdateSpeed", Time.fixedDeltaTime);
			PlayerPrefs.SetFloat ("GameMaximumFixedTimestep", Time.maximumDeltaTime);
			PlayerPrefs.SetFloat ("Extra Gravity", 0.15f);
		}
	}
	*/

	public void Awake(){
		#if UNITY_EDITOR
		Debug.unityLogger.logEnabled=true;
		#else
		Debug.unityLogger.logEnabled=false;
		#endif

		if (!resetToDefaults) {
			Time.timeScale = PlayerPrefs.GetFloat ("GameSpeed", 1f) - 0.1f;
			Time.fixedDeltaTime = 0.01333f / Time.timeScale;
			Time.maximumDeltaTime = 0.02666f / Time.timeScale;
			PlayerPrefs.SetFloat ("GameFixedUpdateSpeed", Time.fixedDeltaTime);
			PlayerPrefs.SetFloat ("GameMaximumFixedTimestep", Time.maximumDeltaTime);
			PlayerPrefs.SetFloat ("Extra Gravity", 0.15f);
		} else {
			Time.timeScale = 1.0f;
			Time.fixedDeltaTime = 0.01333f;
			Time.maximumDeltaTime = 0.02666f;
			PlayerPrefs.SetFloat ("GameFixedUpdateSpeed", Time.fixedDeltaTime);
			PlayerPrefs.SetFloat ("GameMaximumFixedTimestep", Time.maximumDeltaTime);
			PlayerPrefs.SetFloat ("Extra Gravity", 0.15f);
		}
	}
}
