using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionToCycleValues : MonoBehaviour, MenuButtonInterface {

	public List<string> playerprefsFields;

	public int ppIsStringIntOrFloat;

	public List<string> ppValueStrings;
	public List<int> ppValueInts;
	public List<float> ppValueFloats;

	public void PerformButtonAction (){
	}
	public bool ScrollButtonOptions (){
		return false;
	}
	public void ScrollButtonOptions (int scroll){
	}

	public void PerformButtonOption (){
		switch (ppIsStringIntOrFloat) {
		case 0:
			for (int i = 0; i < playerprefsFields.Count; i++) {
				print (playerprefsFields [i] + ": " + ppValueStrings [i]);
				PlayerPrefs.SetString (playerprefsFields[i], ppValueStrings[i]);
			}
			break;

		case 1:
			for (int i = 0; i < playerprefsFields.Count; i++) {
				print (playerprefsFields[i] + ": " +  ppValueInts[i]);
				PlayerPrefs.SetInt (playerprefsFields[i], ppValueInts[i]);
			}
			break;
		case 2:
			for (int i = 0; i < playerprefsFields.Count; i++) {
				print (playerprefsFields[i] + ": " +  ppValueFloats[i]);
				PlayerPrefs.SetFloat (playerprefsFields[i], ppValueFloats[i]);
			}
			break;
		}
	}
}
