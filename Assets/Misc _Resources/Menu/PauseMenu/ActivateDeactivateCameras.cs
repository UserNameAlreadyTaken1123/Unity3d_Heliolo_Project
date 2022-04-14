using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateDeactivateCameras : MonoBehaviour {

	public bool deactivateCameras;
	public bool activateCameras;

	public void Action () {
		if (deactivateCameras)
			transform.root.GetComponent<PlayerSpawn>().Player.GetComponent<References> ().Camera.GetComponent<Camera> ().enabled = false;
		if (activateCameras)
			transform.root.GetComponent<PlayerSpawn>().Player.GetComponent<References> ().Camera.GetComponent<Camera> ().enabled = true;
	}
}
