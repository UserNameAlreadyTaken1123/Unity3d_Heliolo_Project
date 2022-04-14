using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class GG_SinperZoom : MonoBehaviour {

	public GenericGun gunScript;
	public Third_Person_Camera cameraScript;
	public OverShoulderCamera overShCam;
	public References ownerReferences;
	public Camera cameraComponent;
	public float animationProgress = 0f;
	public float originalFov;

	public bool canZoom = true;
	public bool isZooming = false;
	private bool wasZooming = false;
	public float fov;
	public Vector3 cameraScopeRelativeCoords;

	private float xSpeedOnEnable;
	private float ySpeedOnEnable;

	// Use this for initialization
	void Start () {
		gunScript = GetComponent<GenericGun> ();
		if (!gunScript.isInventory && gunScript.Player) {
			ownerReferences = gunScript.Player.GetComponent<References> ();
			overShCam = ownerReferences.OverShoulderCameraPos.GetComponent<OverShoulderCamera> ();
			cameraComponent = ownerReferences.Camera.GetComponent<Camera> ();
			cameraScript = cameraComponent.GetComponent<Third_Person_Camera> ();

			xSpeedOnEnable = cameraScript.xSpeed;
			ySpeedOnEnable = cameraScript.ySpeed;
		}
	}

	void OnEnable(){
		Start ();
	}

	void OnDisable(){
		StartCoroutine (RestartFov ());
	}

	// Update is called once per frame
	void LateUpdate () {
		if (!gunScript.Player)
			Start ();

		if (gunScript.initialized && canZoom && !gunScript.isInventory) {
			if (InputManager.GetButton ("Aim")) {
				if (!isZooming) {
					originalFov = cameraComponent.fieldOfView;
					xSpeedOnEnable = cameraScript.xSpeed;
					ySpeedOnEnable = cameraScript.ySpeed;
				}
				isZooming = true;
				if (animationProgress < 1f) {
					animationProgress += Time.deltaTime / 0.5f;
					//ownerReferences.Camera.transform.localPosition = Vector3.Slerp (ownerReferences.Camera.transform.position, cameraScopeRelativeCoords, animationProgress);
					overShCam.SetRelativePosition (cameraScopeRelativeCoords, 0.1f);
					cameraComponent.fieldOfView = Mathf.Lerp (cameraComponent.fieldOfView, fov, animationProgress);
				} else {
					animationProgress = 1f;
					GetComponent<Renderer>().enabled = false;
				}
			} else {
				isZooming = false;
				if (animationProgress > 0f) {
					if (animationProgress == 1f) {
						ownerReferences.OverShoulderCameraPos.GetComponent<OverShoulderCamera> ().SetRelativePosition (gunScript.overShoulderRelativePos, 2f);
						GetComponent<Renderer>().enabled = true;
					}
					animationProgress -= Time.deltaTime / 0.5f;
					//ownerReferences.Camera.transform.localPosition = Vector3.Slerp (ownerReferences.Camera.transform.position, gunScript.overShoulderRelativePos, animationProgress);
					cameraComponent.fieldOfView = Mathf.Lerp (cameraComponent.fieldOfView, originalFov, animationProgress);
					//overShCam.ResetPosition (0.25f);
				} else {
					animationProgress = 0f;
				}
			}
		}

		if (wasZooming && !isZooming) {
			ownerReferences.GetComponent<CycleGuns> ().cantSwitch = false;
			cameraScript.xSpeed = xSpeedOnEnable;
			cameraScript.ySpeed = ySpeedOnEnable;
		} else if (!wasZooming && isZooming) {
			ownerReferences.GetComponent<CycleGuns> ().cantSwitch = true;
			cameraScript.xSpeed = xSpeedOnEnable * 0.3f;
			cameraScript.ySpeed = ySpeedOnEnable * 0.3f;
		}

		wasZooming = isZooming;
	}

	IEnumerator RestartFov(){
		cameraScript.xSpeed = xSpeedOnEnable;
		cameraScript.ySpeed = ySpeedOnEnable;
		float duration = 0.5f;
		float normalized = 1f;
		while (normalized > 0f) {
			cameraComponent.fieldOfView = Mathf.Lerp (cameraComponent.fieldOfView, fov, normalized);
			normalized -= Time.deltaTime / duration;
			yield return null;
		}
	}
}
