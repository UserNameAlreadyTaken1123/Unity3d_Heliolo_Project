using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonDelegateForChildren : ButtonClass {

	private ButtonClass[] childrenButtonScripts;

	// Use this for initialization
	public void ExecuteSelectedChildAction () {
		childrenButtonScripts = optionsToCycle[optionIndex].GetComponents<ButtonClass>();
		foreach (ButtonClass script in childrenButtonScripts) {
			script.PerformButtonAction();
		}
	}

	public void ExecuteSelectedChildHighlitAction () {
		childrenButtonScripts = optionsToCycle[optionIndex].GetComponents<ButtonClass>();
		foreach (ButtonClass script in childrenButtonScripts) {
			script.PerformHighlightedAction();
		}
	}
}
