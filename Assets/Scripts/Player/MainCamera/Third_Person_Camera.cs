using UnityEngine;
using System.Collections;
using Luminosity.IO;
using MorePPEffects;

public class Third_Person_Camera : MonoBehaviour {
	private Hero_Movement heroMovement;
	private Headache headacheEffect;
	private EnemiesDetector autoAim;
	private References references;

	//This camera
	public Camera mainCamera;
	//Our character
	public Transform player;
	private Transform playersHead;
	public Transform overShoulderTarget;
	public float cameraFov = 60f;
	public float cameraRadius = 0.2f;
	//distance between character and camera
	public float distance = 5.0f;
	private float _distance = 5.0f;
	public float overShoulderDistance = 3.0f;
	//x and y position of camera
	private float cameraAccel = 0f;
	private float x = 0.0f;
	private float y = 0.0f;
	private float tempX = 0.0f;
	private float tempY = 0.0f;
	private float inputT;

	public float mouseSensitivity = 1.0f;
	private float speedFactorOnColision = 1f;

	private float prevX;
	private float prevY;
	public Quaternion rotation;

	public float cameraHeight = 0.525f;
	private float _cameraHeight = 0.525f;
	//x and y side speed, how fast your camera moves in x way and in y way
	public float xSpeed = 120.0f;
	public float ySpeed = 210.0f;
	//Minium and maximum distance between player and camera
	public float distanceMin = 1.0f;
	public float distanceMax = 10f;



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
	public Vector3 targetPositionRaw;
	private Vector3 targetPosition;
	private Vector3 comboPosition;
	private float currentX;
	private float currentY;
	private float currentZ;
	public float zDistanceOffset;
	public bool resetingCamPlzWait;
	private float resetTimerProgress;
	private float resetTimer;
	private bool oneTimeCoro = false;

	private float cameraAngleLimiter;
	public float camYRotation;
	public bool noInputControls;

	//utilería
	Vector3 negDistance;
	Vector3 newNegDistance;
	Vector3 position;
	float newHeight;

	public GameObject targetEnemy;
	public GameObject targetEnemyHead;
	public GameObject prevTargetEnemy;

	Vector3 midPoint;
	float distanceFromFighters;
	Vector3 combatDistance;

	float playerRotationY;
	Quaternion targetRotation;

	//lock aim mode
	public bool lockedAimMode;
	private bool aimPressedA;
	private float lockAimTimer;
	private float aimAutoaimTimer = 0f;
	private bool aimButtonPressed;
	private float lerpValueA;
	private float lerpValueB;
	private float aimModeDistance;

	//lock combo mode
	public bool lockComboMode;
	private bool comboPressedA;
	private float lockComboTimer;
	private bool comboButtonPressed;

	//normal mode aux
	private int n = 0;
	private float _n = 0;

	//public LayerMask isGroundedIgnoreLayers;

	private void Start(){
		if (GetComponent<Headache> ()) {
			headacheEffect = GetComponent<Headache> ();
			headacheEffect.enabled = false;
		}

		GetComponent<FadeIn> ().StartFade (new Color(0,0,0,0), 1f);

		if (player == null)
			player = GameObject.FindGameObjectWithTag ("Player").transform;

		heroMovement = player.gameObject.GetComponent<Hero_Movement> ();
		playersHead = player.GetComponent<References> ().Head.transform;
		autoAim = transform.parent.GetChild (1).GetChild (1).GetComponent<EnemiesDetector> ();
		references = player.GetComponent<References> ();

		_cameraHeight = cameraHeight;

		//make variable from our euler angles
		Vector3 angles = transform.eulerAngles;
		//and store y and x angles to different values
		x = angles.y;
		y = angles.x;
		//sets this camera to main camera
		mainCamera = Camera.main;
		//mouseSensitivity = PlayerPrefs.GetFloat ("MouseSens", 2.0f);
		mouseSensitivity = 2.0f;

		aimModeDistance = Vector3.Distance (overShoulderTarget.position, playersHead.position);
		//isGroundedIgnoreLayers = heroMovement.isGroundedIgnoreLayers;
		CamDetectCollision();
		y = y + 0.001f;
		x = x + 0.001f;

		Invoke ("Unparent", (Time.deltaTime / (Time.timeScale + 0.001f)));
	}

	void Unparent(){
		transform.parent = null;
	}

	void AimModeLocker(){
		if (InputManager.GetButtonDown ("Aim")){
			if(!aimButtonPressed && lockAimTimer < 0.2f){
				if (!aimPressedA) {
					aimPressedA = true;
				} else if (!lockedAimMode) {
					aimPressedA = false;
					lockedAimMode = true;
					heroMovement.lockedAimMode = true;
				} else {
					aimPressedA = false;
					lockedAimMode = false;
					heroMovement.lockedAimMode = false;
				}
			}

		}else {
			aimButtonPressed = false;
		}

		if (aimPressedA && lockAimTimer < 0.2f) {
			lockAimTimer += (Time.deltaTime / (Time.timeScale + 0.001f));
		} else {
			lockAimTimer = 0;
			aimPressedA = false;
		}
	}

	void ComboModeLocker(){
		if (InputManager.GetButtonDown ("Combo Mode")){
			if(!comboButtonPressed && lockComboTimer < 0.2f){
				if (!comboPressedA) {
					comboPressedA = true;
				} else if (!lockComboMode) {
					comboPressedA = false;
					lockComboMode = true;
					heroMovement.lockedComboMode = true;
				} else {
					comboPressedA = false;
					lockComboMode = false;
					heroMovement.lockedComboMode = false;
				}
			}

		}else {
			comboButtonPressed = false;
		}

		if (comboPressedA && lockComboTimer < 0.2f) {
			lockComboTimer += (Time.deltaTime / (Time.timeScale + 0.001f)) ;
		} else {
			lockComboTimer = 0;
			comboPressedA = false;
		}
	}

	void Update (){
		if (InputManager.GetAxis("RightAnalogX") > 0.05f || InputManager.GetAxis("RightAnalogX") < -0.05f ||
			InputManager.GetAxis("RightAnalogY") > 0.05f || InputManager.GetAxis("RightAnalogY") < -0.05f){
			cameraAccel += (Time.deltaTime / (Time.timeScale + 0.001f))  * 4f;
		} else
			cameraAccel -= (Time.deltaTime / (Time.timeScale + 0.001f))  * 8f;
		cameraAccel = Mathf.Clamp01 (cameraAccel);

		if (!noInputControls && !resetingCamPlzWait) {	//He removido time deltatime del input, supuestamente el input ya es framerate tied
			//Ejemplo de original: 						tempY += InputManager.GetAxis ("Mouse Y") * speedDistance * mouseSensitivity * (Time.deltaTime / (Time.timeScale + 0.001f))  * 25;
			float speedDistance = (_distance - distanceMax*2) / (distanceMin - distanceMax*2);
			if (InputManager.GetButton ("Aim") || lockedAimMode) { //MODO APUNTAR
				if (InputManager.GetButton ("Aim")) { // MODO APUNTAR APRETANDO EL BOTON APUNTAR
					if (InputManager.GetButton ("Reset Camera")) { // MODO APUNTAR RAPIDO CON RESET CAMERA
						tempX += InputManager.GetAxis ("Mouse X") * speedDistance * mouseSensitivity * 1.25f;
						tempY += InputManager.GetAxis ("Mouse Y") * speedDistance * mouseSensitivity * 1.25f;
						tempX += (InputManager.GetAxis ("RightAnalogX") * xSpeed * 2f * cameraAccel);
						tempY += (InputManager.GetAxis ("RightAnalogY") * ySpeed) * 2f * cameraAccel;
					} else {  // MODO APUNTAR RAPIDO SIN RESET CAMERA
						tempX += InputManager.GetAxis ("Mouse X") * speedDistance * mouseSensitivity;
						tempY += InputManager.GetAxis ("Mouse Y") * speedDistance * mouseSensitivity;
						tempX += (InputManager.GetAxis ("RightAnalogX") * xSpeed * 0.5f * cameraAccel);
						tempY += (InputManager.GetAxis ("RightAnalogY") * ySpeed * 0.5f * cameraAccel);
					}
				} else { // MODO APUNTAR FIJO, SIN EL BOTON APUNTAR
					if (InputManager.GetButton ("Reset Camera")) { // MODO APUNTAR RAPIDO CON RESET CAMERA
						tempX += InputManager.GetAxis ("Mouse X") * speedDistance * mouseSensitivity;
						tempY += InputManager.GetAxis ("Mouse Y") * speedDistance * mouseSensitivity;
						tempX += (InputManager.GetAxis ("RightAnalogX") * xSpeed * 3.5f * cameraAccel);
						tempY += (InputManager.GetAxis ("RightAnalogY") * ySpeed * 3.5f * cameraAccel);
					} else {  // MODO APUNTAR RAPIDO SIN RESET CAMERA
						tempX += InputManager.GetAxis ("Mouse X") * speedDistance * mouseSensitivity;
						tempY += InputManager.GetAxis ("Mouse Y") * speedDistance * mouseSensitivity;
						tempX += (InputManager.GetAxis ("RightAnalogX") * xSpeed * cameraAccel);
						tempY += (InputManager.GetAxis ("RightAnalogY") * ySpeed * cameraAccel);
					}
				}
			} else if (!InputManager.GetButton ("Reset Camera")) { //MODO NORMAL
				tempX += InputManager.GetAxis ("Mouse X") * speedDistance * mouseSensitivity;
				tempX += InputManager.GetAxis ("RightAnalogX") * xSpeed * speedDistance * cameraAccel;
				tempY += InputManager.GetAxis ("Mouse Y") * speedDistance * mouseSensitivity;
				tempY += InputManager.GetAxis ("RightAnalogY") * ySpeed * speedDistance * cameraAccel;
			}
			if (InputManager.GetButton ("Aim") || lockedAimMode) {
				x =  Mathf.Lerp (x, tempX, 0.5f);
				y =  Mathf.Lerp (y, tempY, 0.5f);
			} else {
				x =  Mathf.Lerp (x, tempX, 0.5f);
				y =  Mathf.Lerp (y, tempY, 0.5f);
			}
		}

		if (!noInputControls) {
			if (!InputManager.GetButton ("Combo Mode")) {
				aimAutoaimTimer = 0f;
			}

			AimModeLocker ();
			//ComboModeLocker ();
		}

		//////////////////////////////////////////////////////////////////////////

		if (targetEnemy != prevTargetEnemy) {
			targetEnemyHead = targetEnemy.GetComponent<References> ().Head;
		}

		if (targetEnemy) {
			prevTargetEnemy = targetEnemy;
		}

		bool isGrounded = heroMovement.isGrounded;
		//gets mouse movement x and y and multiplies them with speeds and moves camera with them

		if (!noInputControls && !InputManager.GetButton ("Reset Camera")) {
			float speedDistance = (_distance - distanceMax * 2) / (distanceMin - distanceMax * 2);
		}

		x = x - prevX;
		y = y - prevY;

		if (!noInputControls && x != 0 || y != 0) {
			rotation = transform.rotation * Quaternion.Euler (y, x, 0f);
		}

		//changes distance between max and min distancy by mouse scroll
		if (!noInputControls) {
			distance = Mathf.Clamp (distance - InputManager.GetAxis ("Mouse ScrollWheel") * 4, distanceMin, distanceMax);
		}

		float separation = Mathf.Pow ((distance - distanceMax) / (distanceMin - distanceMax),2);
		separation = Mathf.Clamp (separation, 0.0f, 1.0f);
		negDistance = new Vector3 (0.0f, _cameraHeight, -_distance + zDistanceOffset) + new Vector3 (separation / 1.5f, 0.0f, 0.0f);
		//cameras postion
		position = rotation * negDistance + player.position;

		newNegDistance = new Vector3 (0.0f, 0.0f, overShoulderDistance);
		camera1Position = transform.position;
		camera2Position = rotation * newNegDistance;

		//RESETING CAMERA MODE
		if ((!noInputControls && !InputManager.GetButton ("Aim") && !lockedAimMode && InputManager.GetButton ("Reset Camera")) || resetingCamPlzWait == true) {

			if (resetTimer > 0.05f && InputManager.GetButtonDown ("Reset Camera") && resetingCamPlzWait) {
				distance = 2f;
				//resetTimer = 0f;
			}
			
			if (InputManager.GetButtonDown ("Reset Camera")) {
				resetingCamPlzWait = true;
			}

			if (resetTimer > 0.5f) {
				resetingCamPlzWait = false;
				resetTimer = 0f;
			}

			distance = Mathf.Clamp (distance - InputManager.GetAxis ("RightAnalogY") / 5f * -1, distanceMin, distanceMax);
			resetTimer += Time.deltaTime / 0.25f;
			if (!InputManager.GetButton ("Combo Mode")) {
				playerRotationY = player.eulerAngles.y;
				targetPosition = (rotation * (new Vector3 (0.0f, _cameraHeight, -distance) + new Vector3 (separation / 2.5f, 0.0f, 0.0f)) + player.position) + player.transform.forward / 10;
				currentX = Mathf.Lerp (transform.position.x, targetPosition.x, resetTimer * 2f);
				currentY = Mathf.Lerp (transform.position.y, targetPosition.y, resetTimer * 2f);
				currentZ = Mathf.Lerp (transform.position.z, targetPosition.z, resetTimer * 2f * speedFactorOnColision);
				currentPosition = new Vector3 (currentX, currentY, currentZ);

				rotation = Quaternion.Euler (15f, playerRotationY, 0f);
				transform.rotation = Quaternion.Slerp (transform.rotation, rotation, (Time.deltaTime / (Time.timeScale + 0.001f)) * 15f);			
				transform.localPosition += new Vector3 (separation / 2.5f, 0.0f, 0.0f);
				transform.position = Vector3.Slerp (transform.position, currentPosition, 400f * (Time.deltaTime / (Time.timeScale + 0.001f)));
			}
		}
			
		if (noInputControls || !InputManager.GetButton ("Aim") && !lockedAimMode) {
			lerpValueA = 5f;
			lerpValueB = 20f;
		}

		//CAMERA AIM MODE
		if (!noInputControls && InputManager.GetButton ("Aim") || lockedAimMode) {
			//Ver si resetear o no en modo apuntar.
			rotation = transform.rotation * Quaternion.Euler (y, x, 0f);
			targetPosition = overShoulderTarget.position - (overShoulderTarget.forward * (-zDistanceOffset));
			targetPositionRaw = overShoulderTarget.position - (overShoulderTarget.forward);
			//Velocidad con la cual la camara se acerca al hombro.
			lerpValueA = Mathf.Lerp (lerpValueA, 60f, 0.1f);
			lerpValueB = Mathf.Lerp (lerpValueA, 100f, 0.1f);
			//targetEnemy = player.GetComponent<References> ().currentAutoaimTarget;
			if (!InputManager.GetButton ("Combo Mode")) {
				transform.rotation = Quaternion.Slerp (transform.rotation, rotation, (Time.deltaTime / (Time.timeScale + 0.001f)) * 5f);
				//transform.position = Vector3.Slerp (transform.position, targetPosition, (Time.deltaTime / (Time.timeScale + 0.001f))  * lerpValueA * speedFactorOnColision);
				currentX = Mathf.Lerp (transform.position.x, targetPosition.x, (Time.deltaTime / (Time.timeScale + 0.001f)) * lerpValueA);
				currentY = Mathf.Lerp (transform.position.y, targetPosition.y, (Time.deltaTime / (Time.timeScale + 0.001f)) * lerpValueA);
				currentZ = Mathf.Lerp (transform.position.z, targetPosition.z, (Time.deltaTime / (Time.timeScale + 0.001f)) * lerpValueA * speedFactorOnColision);
				currentPosition = new Vector3 (currentX, currentY, currentZ);
				
			} else if (InputManager.GetButton ("Combo Mode") && aimAutoaimTimer < 1f && autoAim.target) {
				targetEnemy = references.currentAutoaimTargetLastLong;
				midPoint = (targetEnemyHead.transform.position + targetEnemy.transform.position) * 0.5f;
				transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (midPoint - transform.position), aimAutoaimTimer * 2f);
				currentX = Mathf.Lerp (transform.position.x, targetPosition.x, (Time.deltaTime / (Time.timeScale + 0.001f)) * lerpValueA * 2f);
				currentY = Mathf.Lerp (transform.position.y, targetPosition.y, (Time.deltaTime / (Time.timeScale + 0.001f)) * lerpValueA * 2f);
				currentZ = Mathf.Lerp (transform.position.z, targetPosition.z, (Time.deltaTime / (Time.timeScale + 0.001f)) * lerpValueA * 2f * speedFactorOnColision);
				currentPosition = new Vector3 (currentX, currentY, currentZ);
				

				if (InputManager.GetAxis ("RightAnalogY") > 0.2 || InputManager.GetAxis ("RightAnalogY") < -0.2 ||
				    InputManager.GetAxis ("RightAnalogX") > 0.2 || InputManager.GetAxis ("RightAnalogX") < -0.2) {
					aimAutoaimTimer = 1f;
				}

				aimAutoaimTimer += (Time.deltaTime / (Time.timeScale + 0.001f)) / 1.5f;
			} else {
				transform.rotation = Quaternion.Slerp (transform.rotation, rotation, (Time.deltaTime / (Time.timeScale + 0.001f)) * 5f);
				currentX = Mathf.Lerp (transform.position.x, targetPosition.x, (Time.deltaTime / (Time.timeScale + 0.001f)) * lerpValueA);
				currentY = Mathf.Lerp (transform.position.y, targetPosition.y, (Time.deltaTime / (Time.timeScale + 0.001f)) * lerpValueA);
				currentZ = Mathf.Lerp (transform.position.z, targetPosition.z, (Time.deltaTime / (Time.timeScale + 0.001f)) * lerpValueA * speedFactorOnColision);
				currentPosition = new Vector3 (currentX, currentY, currentZ);
				
			}


			//CAMERA COMBO MODE
		} else if (!noInputControls && !lockedAimMode && !InputManager.GetButton ("Aim") && InputManager.GetButton ("Combo Mode")) {
			if (!autoAim.lookingAtAnEnemy) {
				targetPosition = position + player.transform.forward / 10;
				targetPositionRaw = targetPosition;
				newHeight = ((distance - distanceMin) / (5f - distanceMin)) / 3;
				targetPosition.y = targetPosition.y + newHeight;
				transform.rotation = Quaternion.Slerp (transform.rotation, rotation, (Time.deltaTime / (Time.timeScale + 0.001f)) * 7.5f); // original 10f
				currentX = Mathf.Lerp (transform.position.x, targetPosition.x, 10f * (Time.deltaTime / (Time.timeScale + 0.001f)));
				currentY = Mathf.Lerp (transform.position.y, targetPosition.y, 10f * (Time.deltaTime / (Time.timeScale + 0.001f)));
				currentZ = Mathf.Lerp (transform.position.z, targetPosition.z, 10f * (Time.deltaTime / (Time.timeScale + 0.001f)) * speedFactorOnColision);
				currentPosition = new Vector3 (currentX, currentY, currentZ);
				midPoint = transform.position;
				
			} else if (references.currentAutoaimTargetLastLong) {				
				targetEnemy = references.currentAutoaimTargetLastLong;
				//midPoint = player.transform.position;
				int n = 1;
				RaycastHit hitInfo;
				foreach (GameObject newTargetEnemy in autoAim.enemiesInRange) {
					if (!Physics.Raycast (transform.position, (newTargetEnemy.transform.position - transform.position).normalized, out hitInfo, Vector3.Distance (transform.position, newTargetEnemy.transform.position), LayerMask.GetMask ("Scenario", "Default"))) {
						n += 1;
						midPoint += newTargetEnemy.transform.position;
					}
				}
					
				if (midPoint == Vector3.zero) {
					midPoint = transform.position;
				}
					

				if (n > 2)
					midPoint = Vector3.Slerp (midPoint, (player.transform.position + midPoint / n) * 0.5f, 50f);
				else
					midPoint = Vector3.Slerp (midPoint, (player.transform.position + ((midPoint + player.transform.position + player.transform.forward * 2f) / (n + 1))) * 0.5f, 50f);
				
				//ORIGINAL distanceFromFighters = Vector3.Distance (midPoint, player.transform.position) + Mathf.Sqrt (distance * 1.25f) * Mathf.Sqrt (n);
				distanceFromFighters = Vector3.Distance (midPoint, player.transform.position) + Mathf.Sqrt (distance * 1.25f * n);
				combatDistance = new Vector3 (0.0f, 0.0f, -distanceFromFighters + zDistanceOffset);

				targetPosition = rotation * combatDistance + midPoint;
				targetPosition = new Vector3 (targetPosition.x, targetPosition.y + 0.5f, targetPosition.z);
				targetPositionRaw = targetPosition;
				transform.rotation = Quaternion.Slerp (transform.rotation, rotation, (Time.deltaTime / (Time.timeScale + 0.001f)) * 6f); // original 8f
				currentX = Mathf.Lerp (transform.position.x, targetPosition.x, 8f * (Time.deltaTime / (Time.timeScale + 0.001f)));
				currentY = Mathf.Lerp (transform.position.y, targetPosition.y, 8f * (Time.deltaTime / (Time.timeScale + 0.001f)));
				currentZ = Mathf.Lerp (transform.position.z, targetPosition.z, 8f * (Time.deltaTime / (Time.timeScale + 0.001f)) * speedFactorOnColision);
				currentPosition = new Vector3 (currentX, currentY, currentZ);
			}



			//CAMERA NORMAL MODE
		} else if (!InputManager.GetButton ("Aim") && !lockedAimMode /*&& !InputManager.GetButton ("Reset Camera")*/ && !InputManager.GetButton ("Combo Mode") || !resetingCamPlzWait) {
			if (autoAim.enemiesInRange.Count > 0) {
				n = 0;
				RaycastHit hitInfo;
					foreach (GameObject newTargetEnemy in autoAim.enemiesInRange) {
					if (n < 2) {
						if (!Physics.Raycast (transform.position, (newTargetEnemy.transform.position - transform.position).normalized, out hitInfo, Vector3.Distance (transform.position, newTargetEnemy.transform.position), LayerMask.GetMask ("Scenario", "Default"))) {
							n += 1;
							midPoint += newTargetEnemy.transform.position;
						}
					} else
						break;
				}
			} else
				n = 0;

			_n = Mathf.Lerp (_n, n * 0.5f, (Time.deltaTime / (Time.timeScale + 0.001f)) * 0.75f);

			targetPosition = position + player.transform.forward / 10 + transform.forward * (-_n);
			targetPositionRaw = (rotation * (new Vector3 (0.0f, _cameraHeight, -distance) + new Vector3 (separation / 2.5f, 0.0f, 0.0f)) + player.position) + player.transform.forward / 10 + transform.forward * (-_n);
			currentX = Mathf.Lerp (transform.position.x, targetPosition.x, 10f * (Time.deltaTime / (Time.timeScale + 0.001f)));
			currentY = Mathf.Lerp (transform.position.y, targetPosition.y, 10f * (Time.deltaTime / (Time.timeScale + 0.001f)));
			currentZ = Mathf.Lerp (transform.position.z, targetPosition.z, 10f * (Time.deltaTime / (Time.timeScale + 0.001f)) * speedFactorOnColision);
			currentPosition = new Vector3 (currentX, currentY, currentZ);

			if (!InputManager.GetButton ("Reset Camera"))
				transform.rotation = Quaternion.Slerp (transform.rotation, rotation, (Time.deltaTime / (Time.timeScale + 0.001f)) * 10f);	
		}

		if (InputManager.GetAxisRaw ("Mouse X") < 0.05f &&
			InputManager.GetAxisRaw ("Mouse X") > -0.05f &&
			InputManager.GetAxisRaw ("Mouse Y") < 0.05f &&
			InputManager.GetAxisRaw ("Mouse Y") > -0.05f &&
			InputManager.GetAxisRaw ("RightAnalogX") < 0.05f &&
			InputManager.GetAxisRaw ("RightAnalogX") > -0.05f &&
			InputManager.GetAxisRaw ("RightAnalogY") < 0.05f &&
			InputManager.GetAxisRaw ("RightAnalogY") > -0.05f) {
			if (InputManager.GetButton ("Aim") || lockedAimMode) {
				tempX =  Mathf.Lerp (tempX, 0f, (Time.deltaTime / (Time.timeScale + 0.001f))  * 10f);
				tempY =  Mathf.Lerp (tempY, 0f, (Time.deltaTime / (Time.timeScale + 0.001f))  * 10f);
			} else {
				tempX =  Mathf.Lerp (tempX, 0f, (Time.deltaTime / (Time.timeScale + 0.001f))  * 5f);
				tempY =  Mathf.Lerp (tempY, 0f, (Time.deltaTime / (Time.timeScale + 0.001f))  * 5f);
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

	void LateUpdate(){
		transform.position = Vector3.Slerp (transform.position, currentPosition, 200f * (Time.deltaTime / (Time.timeScale + 0.001f)) );
		//Movement speed dependant FOV

		if ((player.GetComponent<CycleGuns> ().currentGun.GetComponent<GenericGun> () && !player.GetComponent<CycleGuns> ().currentGun.GetComponent<GG_SinperZoom> ()) || (player.GetComponent<CycleGuns> ().currentGun.GetComponent<GG_SinperZoom> () && !player.GetComponent<CycleGuns> ().currentGun.GetComponent<GG_SinperZoom> ().GetComponent<GG_SinperZoom> ().isZooming)) {
			if ((lockedAimMode || InputManager.GetButton ("Aim")) && GetComponent<Camera> ().fieldOfView > cameraFov - 5f) {
				GetComponent<Camera> ().fieldOfView = Mathf.Lerp (GetComponent<Camera> ().fieldOfView, cameraFov - 5f, Time.unscaledDeltaTime * 15f);
			} else if (!lockedAimMode && !InputManager.GetButton ("Aim") && GetComponent<Camera> ().fieldOfView < cameraFov) {
				GetComponent<Camera> ().fieldOfView = Mathf.Lerp (GetComponent<Camera> ().fieldOfView, cameraFov, Time.unscaledDeltaTime * 10f);
			}
		} else if (!player.GetComponent<CycleGuns> ().currentGun.GetComponent<GenericGun> ()) {
			
		}

		if (heroMovement.isMoving && heroMovement.speed == heroMovement.sprintSpeed && !heroMovement.isJumping) {
			if (InputManager.GetButton ("Sprint")) {
				_distance = Mathf.Lerp (_distance, distance * 0.5f, 10f * (Time.deltaTime / (Time.timeScale + 0.001f)) );
				_cameraHeight = Mathf.Lerp (_cameraHeight, 0.5f, 10f * (Time.deltaTime / (Time.timeScale + 0.001f)) );

			} else {
				_distance = Mathf.Lerp (_distance, distance, 20f * (Time.deltaTime / (Time.timeScale + 0.001f)) );
				_cameraHeight = Mathf.Lerp (_cameraHeight, cameraHeight, 10f * (Time.deltaTime / (Time.timeScale + 0.001f)) );
			}
		} else
			_distance = Mathf.Lerp (_distance, distance, 20f * (Time.deltaTime / (Time.timeScale + 0.001f)) );


		if (heroMovement.isCrouching) {
			_cameraHeight = cameraHeight / 2f;
			_distance = distance / 1.5f;
		} else
			_cameraHeight = cameraHeight;
	}


	private void CamDetectCollision(){
		//store raycast hit
		RaycastHit hit;
		LayerMask layerMask = LayerMask.GetMask ("Scenario");
		//en modo apuntar
		if (InputManager.GetButton ("Aim") || lockedAimMode) {
			if (Physics.SphereCast (player.position + Vector3.up, cameraRadius, (targetPositionRaw - playersHead.position).normalized, out hit, aimModeDistance + cameraRadius, layerMask, QueryTriggerInteraction.Ignore)) {
				zDistanceOffset = Mathf.Lerp (zDistanceOffset, (aimModeDistance - hit.distance) + cameraRadius * 2f, (Time.deltaTime / (Time.timeScale + 0.001f))  * 100f); 
				speedFactorOnColision = 100f;

			} else {
				zDistanceOffset = 0f;
				speedFactorOnColision = 1f;
			}
		}
		//Normal
		else if (!InputManager.GetButton ("Aim") && !lockedAimMode && !heroMovement.lockedComboMode) {
			if (Physics.SphereCast (player.position + Vector3.up, cameraRadius, (targetPositionRaw - playersHead.position).normalized, out hit, distance + cameraRadius, layerMask, QueryTriggerInteraction.Ignore)) {	
				zDistanceOffset = Mathf.Lerp (zDistanceOffset, (distance - hit.distance) + cameraRadius * 2f, (Time.deltaTime / (Time.timeScale + 0.001f))  * 100f); 
				//zDistanceOffset = Mathf.Clamp (zDistanceOffset, 0, distance);
				speedFactorOnColision = 100f;

				if (heroMovement.isMoving) {
					if (Vector3.Cross (transform.forward, hit.normal).y < 0f) {
						tempX += 1f;
						tempY += 0.25f;
					} else {
						tempX -= 1f;
						tempY += 0.25f;
					}
				}

				/*
				if (Vector3.Cross (transform.forward, hit.normal).x < 0f) {
					tempY += 1f;
				} else {
					tempY -= 1f;
				}
				*/

			} else {
				zDistanceOffset = 0f;
				speedFactorOnColision = 1f;
			}
		}
		//Modo Combo
		else if (!InputManager.GetButton ("Aim") && InputManager.GetButton ("Combo Mode") || heroMovement.lockedComboMode) {
			if (targetEnemy == null) {
				if (Physics.SphereCast (midPoint + Vector3.up, cameraRadius, (targetPositionRaw - midPoint).normalized, out hit, distance + cameraRadius, layerMask, QueryTriggerInteraction.Ignore)) {	
					zDistanceOffset = Mathf.Lerp (zDistanceOffset, (distance - hit.distance) + cameraRadius * 2f, (Time.deltaTime / (Time.timeScale + 0.001f))  * 100f); 
					speedFactorOnColision = 100f;

					if (heroMovement.isMoving) {
						if (Vector3.Cross (transform.forward, hit.normal).y < 0f) {
							tempX += 1f;
							tempY += 0.25f;
						} else {
							tempX -= 1f;
							tempY += 0.25f;
						}

						if (Vector3.Cross (transform.forward, hit.normal).x < 0f) {
							tempY += 0.25f;
						}
					}

				} else {
					zDistanceOffset = 0f;
					speedFactorOnColision = 1f;
				}
			} else {
				if (Physics.SphereCast (midPoint, cameraRadius, (targetPositionRaw - midPoint).normalized, out hit, distanceFromFighters + cameraRadius, layerMask, QueryTriggerInteraction.Ignore)) {
					zDistanceOffset = Mathf.Lerp (zDistanceOffset, (distanceFromFighters - hit.distance) + cameraRadius * 2f, (Time.deltaTime / (Time.timeScale + 0.001f)) * 100f); 
					speedFactorOnColision = 100f;

					if (heroMovement.isMoving) {
						if (Vector3.Cross (transform.forward, hit.normal).y < 0f) {
							tempX += 1.25f;
							tempY += 0.25f;
						} else {
							tempX -= 1.25f;
							tempY += 0.25f;
						}
					}

				} else {
					zDistanceOffset = 0f;
					speedFactorOnColision = 1f;
				}			
			}
		}
	}
}
			