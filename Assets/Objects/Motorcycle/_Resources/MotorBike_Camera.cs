using UnityEngine;
using System.Collections;
using Luminosity.IO;
using MorePPEffects;

public class MotorBike_Camera : Third_Person_Camera {

	public Transform pivot;

	private Headache headacheEffect;
	private References references;

	private Transform playersHead;
	public Transform _overShoulderTarget;
	//distance between character and camera
	private float _distance = 5.0f;
	//x and y position of camera
	private float x = 0.0f;
	private float y = 0.0f;
	private float tempX = 0.0f;
	private float tempY = 0.0f;
	private float inputT;

	private float speedFactorOnColision = 1f;

	//ESTO ES PARA CAMBIAR A FPS APRETANDO MIDDLEMOUSEBUTTON
	/*	//checks if first person mode is on
	private bool click = false;
	*/	//stores cameras distance from player
	private float newDist;
	private Vector3 camera1Position;
	private Vector3 camera2Position;
	private float lerpingTime;

	private float horizontalSum;

	private Vector3 currentPosition;
	private Vector3 targetPositionRaw;
	private Vector3 targetPosition;
	private Vector3 comboPosition;
	private float currentX;
	private float currentY;
	private float currentZ;

	private float resetTimerProgress;
	private float resetTimer;

	private float cameraAngleLimiter;

	//utilería
	Vector3 negDistance;
	Vector3 newNegDistance;
	Vector3 position;
	float newHeight;

	Vector3 midPoint;
	float distanceFromFighters;
	Vector3 combatDistance;

	float playerRotationY;
	Quaternion targetRotation;

	//lock aim mode
	private bool aimPressedA;
	private float lockAimTimer;
	private float aimAutoaimTimer = 0f;
	private bool aimButtonPressed;
	private float lerpValueA;
	private float lerpValueB;
	private float aimModeDistance;

	//lock combo mode
	private bool comboPressedA;
	private float lockComboTimer;
	private bool comboButtonPressed;

	// Use this for initialization
	private void Start(){
		if (GetComponent<Headache> ()) {
			headacheEffect = GetComponent<Headache> ();
			headacheEffect.enabled = false;
		}

		GetComponent<FadeIn> ().StartFade (new Color(0,0,0,0), 1f);

		if (player == null)
			player = GameObject.FindGameObjectWithTag ("Player").transform;

		references = player.GetComponent<References> ();
		playersHead = references.Head.transform;

		//make variable from our euler angles
		Vector3 angles = transform.eulerAngles;
		//and store y and x angles to different values
		x = angles.y;
		y = angles.x;
		//sets this camera to main camera
		mainCamera = Camera.main;
		//mouseSensitivity = PlayerPrefs.GetFloat ("MouseSens", 2.0f);
		mouseSensitivity = 1.0f;

		//isGroundedIgnoreLayers = heroMovement.isGroundedIgnoreLayers;
		y = y + 0.001f;
		x = x + 0.001f;

		_overShoulderTarget = overShoulderTarget;

		Invoke ("Unparent", Time.deltaTime);
	}

	void Unparent(){
		transform.parent = null;
	}
		

	void Update(){
	}

	void LateUpdate(){
	}

	// Update is called once per frame
	void FixedUpdate (){
		if (!noInputControls && !resetingCamPlzWait) {
			float speedDistance = (_distance - distanceMax*2) / (distanceMin - distanceMax*2);
			if (InputManager.GetButton ("Aim")) { //MODO APUNTAR
				if (InputManager.GetButton ("Aim")) { // MODO APUNTAR APRETANDO EL BOTON APUNTAR
					if (InputManager.GetButton ("Reset Camera")) { // MODO APUNTAR RAPIDO CON RESET CAMERA
						tempX += InputManager.GetAxis ("Mouse X") * speedDistance * mouseSensitivity;
						tempY += InputManager.GetAxis ("Mouse Y") * speedDistance * mouseSensitivity;
						tempX += (InputManager.GetAxis ("RightAnalogX") * xSpeed);
						tempY += (InputManager.GetAxis ("RightAnalogY") * ySpeed);
					} else {  // MODO APUNTAR RAPIDO SIN RESET CAMERA
						tempX += InputManager.GetAxis ("Mouse X") * speedDistance * mouseSensitivity;
						tempY += InputManager.GetAxis ("Mouse Y") * speedDistance * mouseSensitivity;
						tempX += (InputManager.GetAxis ("RightAnalogX") * xSpeed * 0.5f);
						tempY += (InputManager.GetAxis ("RightAnalogY") * ySpeed * 0.5f);
					}
				} else { // MODO APUNTAR FIJO, SIN EL BOTON APUNTAR
					if (InputManager.GetButton ("Reset Camera")) { // MODO APUNTAR RAPIDO CON RESET CAMERA
						tempX += InputManager.GetAxis ("Mouse X") * speedDistance * mouseSensitivity;
						tempY += InputManager.GetAxis ("Mouse Y") * speedDistance * mouseSensitivity;
						tempX += (InputManager.GetAxis ("RightAnalogX") * xSpeed * 3f);
						tempY += (InputManager.GetAxis ("RightAnalogY") * ySpeed * 3f);
					} else {  // MODO APUNTAR RAPIDO SIN RESET CAMERA
						tempX += InputManager.GetAxis ("Mouse X") * speedDistance * mouseSensitivity;
						tempY += InputManager.GetAxis ("Mouse Y") * speedDistance * mouseSensitivity;
						tempX += (InputManager.GetAxis ("RightAnalogX") * xSpeed);
						tempY += (InputManager.GetAxis ("RightAnalogY") * ySpeed);
					}
				}
			} else if (!InputManager.GetButton ("Reset Camera")) { //MODO NORMAL
				tempX += InputManager.GetAxis ("Mouse X") * speedDistance * mouseSensitivity;
				tempX += InputManager.GetAxis ("RightAnalogX") * xSpeed * speedDistance;
				tempY += InputManager.GetAxis ("Mouse Y") * speedDistance * mouseSensitivity;
				tempY -= InputManager.GetAxis ("RightAnalogY") * ySpeed * speedDistance;
			}
			if (InputManager.GetButton ("Aim")) {
				x =  Mathf.Lerp (x, tempX, 0.5f);
				y =  Mathf.Lerp (y, tempY, 0.5f);
			} else {
				x =  Mathf.Lerp (x, tempX, 0.5f);
				y =  Mathf.Lerp (y, tempY, 0.5f);
			}
		}

		if (!noInputControls) {
			if (!InputManager.GetButton ("Aim")) {
				aimAutoaimTimer = 0f;
			}
		}

		//////////////////////////////////////////////////////////////////////////

		if (targetEnemy != prevTargetEnemy) {
			targetEnemyHead = targetEnemy.GetComponent<References> ().Head;
		}

		if (targetEnemy) {
			prevTargetEnemy = targetEnemy;
		}

		//gets mouse movement x and y and multiplies them with speeds and moves camera with them
		if (!noInputControls && !InputManager.GetButton ("Reset Camera")) {
			float speedDistance = (_distance - distanceMax * 2) / (distanceMin - distanceMax * 2);
		}

		if (!noInputControls && x != 0 || y != 0) {
			rotation = transform.rotation * Quaternion.Euler (y, x, 0f);
		}

		//changes distance between max and min distancy by mouse scroll
		if (!noInputControls) {
			if (InputManager.GetButton ("Reset Camera"))
				distance = Mathf.Clamp (distance - InputManager.GetAxis ("RightAnalogY") / 5f * -1, distanceMin, distanceMax);
			distance = Mathf.Clamp (distance - InputManager.GetAxis ("Mouse ScrollWheel") * 5, distanceMin, distanceMax);
		}

		float separation = (distance - 8f) / (distanceMin - 8f);
		separation = Mathf.Clamp (separation, 0.0f, 1.0f);
		negDistance = new Vector3 (0.0f, cameraHeight, -_distance + zDistanceOffset) + new Vector3 (separation / 2.5f, 0.0f, 0.0f);
		//cameras postion
		position = rotation * negDistance + player.position;

		newNegDistance = new Vector3 (0.0f, 0.0f, overShoulderDistance);
		camera1Position = transform.position;
		camera2Position = rotation * newNegDistance;

		if (!noInputControls && !InputManager.GetButton ("Aim") && InputManager.GetButtonDown ("Reset Camera") || resetingCamPlzWait == true) {
			ResetCamera ();
		}

		//checks if Aim button is pressed or lockedAimMode is on /*and if player is on ground*/
		if (noInputControls || !InputManager.GetButton ("Aim") /* && isGrounded == true */) {
			lerpValueA = 5f;
			lerpValueB = 20f;
		}

		//CAMERA AIM MODE
		if (!noInputControls && InputManager.GetButton ("Aim") /* && isGrounded == true */) {
			CameraAimMode ();
		} else if (!InputManager.GetButton ("Aim") && !InputManager.GetButton ("Reset Camera") || !resetingCamPlzWait) {
			targetPosition = position + player.transform.forward / 10;
			targetPositionRaw = (rotation * (new Vector3 (0.0f, cameraHeight, -distance) + new Vector3 (separation / 2.5f, 0.0f, 0.0f)) + player.position) + player.transform.forward / 10;
			transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * 10f);
			//transform.position = Vector3.Slerp (transform.position, targetPosition, 10f * Time.deltaTime * speedFactorOnColision);
			currentX = Mathf.Lerp (transform.position.x, targetPosition.x, 10f * Time.deltaTime);
			currentY = Mathf.Lerp (transform.position.y, targetPosition.y, 10f * Time.deltaTime);
			currentZ = Mathf.Lerp (transform.position.z, targetPosition.z, 10f * Time.deltaTime * speedFactorOnColision);
			currentPosition = new Vector3 (currentX, currentY, currentZ);
			transform.position = Vector3.Slerp (transform.position, currentPosition + player.forward * 0.5f, 1.000f);
		}


		if (InputManager.GetAxisRaw ("Mouse X") < 0.05f &&
			InputManager.GetAxisRaw ("Mouse X") > -0.05f &&
			InputManager.GetAxisRaw ("Mouse Y") < 0.05f &&
			InputManager.GetAxisRaw ("Mouse Y") > -0.05f &&
			InputManager.GetAxisRaw ("RightAnalogX") < 0.05f &&
			InputManager.GetAxisRaw ("RightAnalogX") > -0.05f &&
			InputManager.GetAxisRaw ("RightAnalogY") < 0.05f &&
			InputManager.GetAxisRaw ("RightAnalogY") > -0.05f) {
			if (InputManager.GetButton ("Aim")) {
				tempX =  Mathf.Lerp (tempX, 0f, Time.deltaTime * 10f);
				tempY =  Mathf.Lerp (tempY, 0f, Time.deltaTime * 10f);
			} else {
				tempX =  Mathf.Lerp (tempX, 0f, Time.deltaTime * 5f);
				tempY =  Mathf.Lerp (tempY, 0f, Time.deltaTime * 5f);
			}
		} else {
			tempX = 0f;
			tempY = 0f;
		}
		//////////////////////////

		camYRotation = transform.localEulerAngles.x;
		camYRotation = (camYRotation > 180) ? camYRotation - 360 : camYRotation;
		camYRotation = Mathf.Clamp (camYRotation, -70, 70);
		transform.localEulerAngles = new Vector3 (camYRotation, transform.eulerAngles.y, 0f);

		CamDetectCollision ();
	}



	public void ResetCamera(){
		transform.rotation = Quaternion.Euler (new Vector3 (player.transform.rotation.eulerAngles.x + 15f, player.transform.rotation.eulerAngles.y, 0f));
		transform.position = player.transform.position + new Vector3 (0f, cameraHeight, -_distance);

		rotation = transform.rotation;
		targetPosition = transform.position;
	}

	public void CameraAimMode(){
		//Ver si resetear o no en modo apuntar.
		rotation = transform.rotation * Quaternion.Euler (y, x, 0f);
		targetPosition = _overShoulderTarget.position - (_overShoulderTarget.forward * (-zDistanceOffset));
		targetPositionRaw = _overShoulderTarget.position - (_overShoulderTarget.forward);
		//Velocidad con la cual la camara se acerca al hombro.
		lerpValueA =  Mathf.Lerp (lerpValueA, 60f, 0.1f);
		lerpValueB =  Mathf.Lerp (lerpValueA, 100f, 0.1f);
		//targetEnemy = player.GetComponent<References> ().currentAutoaimTarget;
		if (InputManager.GetButton ("Aim")) {
			transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * 5f);
			//transform.position = Vector3.Slerp (transform.position, targetPosition, Time.deltaTime * lerpValueA * speedFactorOnColision);
			currentX = Mathf.Lerp (transform.position.x, targetPosition.x, Time.deltaTime * lerpValueA);
			currentY = Mathf.Lerp (transform.position.y, targetPosition.y, Time.deltaTime * lerpValueA);
			currentZ = Mathf.Lerp (transform.position.z, targetPosition.z, Time.deltaTime * lerpValueA * speedFactorOnColision);
			currentPosition = new Vector3 (currentX, currentY, currentZ);
			transform.position = Vector3.Slerp (transform.position, currentPosition, 1.000f);
		//} else if (InputManager.GetButton("Combo Mode") && aimAutoaimTimer < 1f /*&& autoAim.target*/){
		/*	targetEnemy = references.currentAutoaimTargetLastLong;
			midPoint = (targetEnemyHead.transform.position + targetEnemy.transform.position) * 0.5f;
			transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (midPoint - transform.position), aimAutoaimTimer * 2f);
			//transform.position = Vector3.Slerp (transform.position, targetPosition, Time.deltaTime * lerpValueA * 2f * speedFactorOnColision);
			currentX = Mathf.Lerp (transform.position.x, targetPosition.x, Time.deltaTime * lerpValueA * 2f);
			currentY = Mathf.Lerp (transform.position.y, targetPosition.y, Time.deltaTime * lerpValueA * 2f);
			currentZ = Mathf.Lerp (transform.position.z, targetPosition.z, Time.deltaTime * lerpValueA * 2f * speedFactorOnColision);
			currentPosition = new Vector3 (currentX, currentY, currentZ);
			transform.position = Vector3.Slerp (transform.position, currentPosition, 200f * Time.deltaTime);

			if (InputManager.GetAxis ("RightAnalogY") > 0.2 || InputManager.GetAxis ("RightAnalogY") < -0.2 ||
				InputManager.GetAxis ("RightAnalogX") > 0.2 || InputManager.GetAxis ("RightAnalogX") < -0.2) {
				aimAutoaimTimer = 1f;
			}

			aimAutoaimTimer += Time.deltaTime / 1.5f;*/
		}else {
			transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * 5f);
			//transform.position = Vector3.Slerp (transform.position, targetPosition, Time.deltaTime * lerpValueB * speedFactorOnColision);
			currentX = Mathf.Lerp (transform.position.x, targetPosition.x, Time.deltaTime * lerpValueB);
			currentY = Mathf.Lerp (transform.position.y, targetPosition.y, Time.deltaTime * lerpValueB);
			currentZ = Mathf.Lerp (transform.position.z, targetPosition.z, Time.deltaTime * lerpValueB * speedFactorOnColision);
			currentPosition = new Vector3 (currentX, currentY, currentZ);
			transform.position = Vector3.Slerp (transform.position, currentPosition, 1.000f);
		}
	}

	private void CamDetectCollision(){
		//store raycast hit
		RaycastHit hit;
		LayerMask layerMask = LayerMask.GetMask ("Scenario", "Objects");
		//en modo apuntar
		if (InputManager.GetButton ("Aim")) {
			if (Physics.SphereCast (player.position + Vector3.up, cameraRadius, (targetPositionRaw - playersHead.position).normalized, out hit, aimModeDistance + cameraRadius, layerMask, QueryTriggerInteraction.Ignore)) {
				zDistanceOffset = Mathf.Lerp (zDistanceOffset, (aimModeDistance - hit.distance) + cameraRadius, Time.deltaTime * 10f); 
				speedFactorOnColision = 10000f;
			} else {
				zDistanceOffset = 0f;
				speedFactorOnColision = 1f;
			}
		}
		//Normal
		else if (!InputManager.GetButton ("Aim")) {
			if (Physics.SphereCast
				(player.position + Vector3.up, cameraRadius, (targetPositionRaw - playersHead.position).normalized, out hit, distance + cameraRadius, layerMask, QueryTriggerInteraction.Ignore)) {	
				zDistanceOffset = Mathf.Lerp (zDistanceOffset, (distance - hit.distance) + cameraRadius, Time.deltaTime * 10f); 
				//zDistanceOffset = Mathf.Clamp (zDistanceOffset, 0, distance);
				speedFactorOnColision = 10000f;
			} else {
				zDistanceOffset = 0f;
				speedFactorOnColision = 1f;
			}
		}
	}
}
