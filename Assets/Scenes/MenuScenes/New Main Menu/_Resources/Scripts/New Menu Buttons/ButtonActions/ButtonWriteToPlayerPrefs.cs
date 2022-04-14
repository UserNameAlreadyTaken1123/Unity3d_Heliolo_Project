using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonWriteToPlayerPrefs : ButtonClass {
	
	[Header("For Ints")]
	public List<string> playerPrefsKeyA;
	public List<int> playerPrefsValueA;

	[Header("For floats")]
	public List<string> playerPrefsKeyB;
	public List<float> playerPrefsValueB;

	[Header("For Strings")]
	public List<string> playerPrefsKeyC;
	public List<string> playerPrefsValueC;

	private int index;

	public void WriteValues(){
		index = 0;
		foreach (string key in playerPrefsKeyA) {
			PlayerPrefs.SetInt (playerPrefsKeyA [index], playerPrefsValueA [index]);
			index += 1; 
		}

		index = 0;
		foreach (string key in playerPrefsKeyB) {
			PlayerPrefs.SetFloat (playerPrefsKeyB [index], playerPrefsValueB [index]);
			index += 1;
		}

		index = 0;
		foreach (string key in playerPrefsKeyC) {
			PlayerPrefs.SetString (playerPrefsKeyC [index], playerPrefsValueC [index]);
			index += 1;
		}
	}
}
