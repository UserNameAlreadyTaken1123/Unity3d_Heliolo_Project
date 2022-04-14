using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiResolutionMenu : MonoBehaviour {

	public Transform cameraReference;
	private Camera camera;

	public bool moveCamera;

	public bool resize;
	public float resizeTo = 1.5f;

	public float width;
	public float height;
	private float zDistance;
	public float ratio;

	private Vector3 newPosition;
	private Vector3 originalPosition;
	private Vector3 originalSize;


	void OnEnable(){
		Start ();
	}

	void Awake (){
		originalPosition = transform.position;
		originalSize = transform.localScale;
	}

	// Use this for initialization
	public void Start () {

		width = Screen.width;
		height = Screen.height;
		zDistance = Vector3.Distance (cameraReference.position, new Vector3 (cameraReference.position.x, cameraReference.position.y, transform.position.z));
		camera = cameraReference.GetComponent<Camera> ();
		ratio = width / height;

		if (moveCamera) {
			PauseGUIAdaptation ();
		} else {
			if (ratio >= 1.7f) { 			// 16:9			
				transform.position = originalPosition;
				transform.localScale = originalSize;

			} else if (ratio >= 1.6f) { 	// 16:10
				newPosition = camera.ScreenToWorldPoint (new Vector3 (Screen.width / 10, Screen.height - Screen.height / 8, zDistance));
				transform.position = newPosition;

			} else if (ratio >= 1.5f) {		// 3:2
				newPosition = camera.ScreenToWorldPoint (new Vector3 (Screen.width / 10, Screen.height - Screen.height / 8, zDistance));
				transform.position = newPosition;

			} else if (ratio >= 1.3f) {		// 4:3
				newPosition = camera.ScreenToWorldPoint (new Vector3 (Screen.width / 20, Screen.height - Screen.height / 16, zDistance));
				transform.position = newPosition;
				if (resize)
					transform.localScale = Vector3.one * resizeTo;
//				else
//					transform.localScale = Vector3.one * 1.5f;
				
			} else if (ratio >= 1.24f) {	// 5:4
				newPosition = camera.ScreenToWorldPoint (new Vector3 (Screen.width / 10, Screen.height - Screen.height / 8, zDistance));
				transform.position = newPosition;
			}

		}
	}

	void PauseGUIAdaptation(){
		if (ratio >= 1.7f) { 			// 16:9			
			cameraReference.localPosition = new Vector3 (0f, 0f, -10f);
		} else if (ratio >= 1.6f) { 	// 16:10
			newPosition = camera.ScreenToWorldPoint (new Vector3 (Screen.width / 10, Screen.height - Screen.height / 8, zDistance));
			transform.position = newPosition;
		} else if (ratio >= 1.5f) {		// 3:2
			newPosition = camera.ScreenToWorldPoint (new Vector3 (Screen.width / 10, Screen.height - Screen.height / 8, zDistance));
			transform.position = newPosition;
		} else if (ratio >= 1.3f) {		// 4:3
//			newPosition = camera.ScreenToWorldPoint (new Vector3 (Screen.width / 2 +  Screen.width /8, Screen.height / 2, zDistance));
			cameraReference.localPosition = new Vector3 (-1.77f, 0f, -10f);
			if (resize)
				transform.localScale = Vector3.one * resizeTo;
		} else if (ratio >= 1.24f) {	// 5:4
			newPosition = camera.ScreenToWorldPoint (new Vector3 (Screen.width / 10, Screen.height - Screen.height / 8, zDistance));
			transform.position = newPosition;
		}
	}
}
