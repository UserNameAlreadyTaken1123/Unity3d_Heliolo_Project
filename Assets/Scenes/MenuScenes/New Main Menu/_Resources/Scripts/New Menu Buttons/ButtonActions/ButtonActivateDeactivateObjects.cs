using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonActivateDeactivateObjects : ButtonClass {

	[Header("On Execute")]
	public List<GameObject> thingsToDeactivate;
	public List<GameObject> thingsToActivate;

	[Header("On Highlight")]
	public List<GameObject> thingsToDeactivateB;
	public List<GameObject> thingsToActivateB;

	public void ActivateDeactivateObjects(){
		foreach (GameObject thingToActivate in thingsToActivate) {
			thingToActivate.SetActive (true);						//Para que se active el objeto, y en OnEnable() haga "lo que quiera".
		}

		foreach (GameObject thingToDeactivate in thingsToDeactivate) {
			if (thingToDeactivate.GetComponent<ButtonClass> ()) { 		//Para que se desactive de la forma que el botón "prefiera".
				thingToDeactivate.GetComponent<ButtonClass> ().CallDeactivation ();
				return;
			} else {
				thingToDeactivate.SetActive (false);
			}
		}
	}

	public void OnHighlightActivateDeactivateObjects (){
		foreach (GameObject thingToActivate in thingsToActivateB) {
			thingToActivate.SetActive (true);						//Para que se active el objeto, y en OnEnable() haga "lo que quiera".
		}

		foreach (GameObject thingToDeactivate in thingsToDeactivateB) {
			if (thingToDeactivate.GetComponent<ButtonClass> ()) { 		//Para que se desactive de la forma que el botón "prefiera".
				thingToDeactivate.GetComponent<ButtonClass> ().CallDeactivation ();
				return;
			} else {
				thingToDeactivate.SetActive (false);
			}
		}
	}
}
