using UnityEngine;
using System.Collections;
using Luminosity.IO;

public class Hero_Movement : MonoBehaviour {

	public bool doNotMove;
	private HealthBar healthBar;
	private Player_Animation playerAnimation;
	private References references;

	public RaycastHit hitInfo;
	private RaycastHit hitInfo2;
	private RaycastHit hitInfo3;
	private Transform transform;
	[HideInInspector] public Rigidbody rigidbody;
	private Collider collider;
	private CapsuleCollider capsuleCollider;

	public float runSpeed = 7f;
	public float sprintSpeed = 14f;
	public float walkSpeed = 2.5f;
	public float runRotateSpeed = 16f;
	public float walkRotateSpeed = 12f;
	public float stepHeight = 0.5f;
	[HideInInspector] public float speed;
	[HideInInspector] private float rotateSpeed;
	// is character grounded
	public bool isGrounded;
	private bool wasGrounded;
    public bool isOnSlope;
    public bool wasOnSlope;
    public bool isMoving;
	public bool isGroundedAndStable;
    public float distToGround;
	private bool applyExtraGrav;

	public bool isCrouching;
	private bool wasCrouching;

	public bool isJumping;
	public bool isBouncing;
	public bool isDashing;
	public bool startedBouncingOnSlope;
	public bool canBounce = true;
	private bool coroBounceReco = false;
	private bool coroNormalLanding = false;
	private bool coroJumping = false;
	private Coroutine coroutineJump;
	private Coroutine checkForBreakConditions;
	private Vector3 tempPos;
	private Vector3 prevPos;
	private bool breakConditionsMet;
	private bool paraJumpRunning;
	public bool isDoubleJumping;
	private bool CoroDoubleJumping;

	public bool lockedAimMode;
	public bool lockedComboMode;

	public bool wasInAir;
	public bool isFalling;
	public float jumpBaseForce = 5f;
	public int	jumpsAmount = 1;
	public float airSpeed = 2f;
	public float bouncingRecoveryTime = 2f;
	[HideInInspector] public float inAirSpeed;
	public int jumpsLeft;

	private float DownTimer;
	private float UpTimer;
	private float jumpPower;
	private float doubleJumpTimer;
	private bool spaceUpDone;
	private bool spaceDownDone;
	private float extraGravity = 0.15f;

	[HideInInspector] public bool jumpInertia;
	Vector3 inertiaForwardDirection;
	Vector3 inertiaRightDirection;
	Vector3 inertiaTotalDirection;
	private float inertiaSpeed;

	//more stuff
	[HideInInspector] public Vector3 totalDirection;
	Vector3 forwardDirection;
	Vector3 rightDirection;

	float cameraRotationY;
	float inputDirection;
	float floatTargetRotation;
	Quaternion targetRotation;

//	private float currentheight;
	private float previousheight;
	[HideInInspector] public float fallingSpeed;

	private bool startedFalling;
	[HideInInspector] public bool wasFalling;

	public LayerMask isGroundedIgnoreLayers;
	[HideInInspector] public Vector3 finalDirection;
	[HideInInspector] public float magnitude;
	[HideInInspector] public float velocityLimitXZ;

	public bool cantShoot;
	public bool cantStab;
	public bool isBlocking;

	private EnemiesDetector autoAim;


	[HideInInspector] public float sprintTimer;
	public AudioClip dashSound;

	//Displacement(). Cacheing everything for posible performance optimization
	Vector3 diagonalSpeedLimit;
	Vector3 jumpForwardDirection;
	Vector3 jumpRightDirection;
	Vector3 jumpTotalDirection;
	Vector3 jumpDiagonalSpeedLimit;
	Vector3 inertiaDiagonalSpeedLimit;

	//FixedUpdate()
	Vector3 maximizeCamFwd;

	//ComboMode()
	Quaternion currentRotation;

	void Awake(){
		isGroundedIgnoreLayers = ~isGroundedIgnoreLayers;
	}


	// Use this for initialization
	void Start () {	
		transform = GetComponent<Transform> ();
		rigidbody = GetComponent<Rigidbody> ();
		collider = GetComponent<Collider> ();
		capsuleCollider = GetComponent<CapsuleCollider> ();
		healthBar = GetComponent<HealthBar> ();
		playerAnimation = GetComponent<Player_Animation> ();
		references = GetComponent<References> ();

		autoAim = transform.parent.GetChild (1).GetChild (1).GetComponent<EnemiesDetector> ();
		extraGravity = PlayerPrefs.GetFloat ("Extra Gravity", 0.15f);

		jumpsLeft = jumpsAmount;
	}

	public void ResetStates(){
		coroJumping = false;
		paraJumpRunning = false;
		breakConditionsMet = true;
		isCrouching = false;
	}

	void Update(){

		//print (InputManager.GetAxis("Vertical"));

		if (!canBounce && !coroBounceReco && isGrounded && !isJumping && !isBouncing)
			StartCoroutine (BounceRecovery ());
		/*
		if (InputManager.GetButton ("Aim") && isGrounded) {
			if (InputManager.GetKey (KeyCode.LeftShift))
				speed = walkSpeed * 0.5f; //original 0.6
			else
				speed = runSpeed * 0.5f; // original 0.7
		}
		*/

		if (!doNotMove) {
			wasCrouching = playerAnimation.isCrouching;
			if (!isCrouching && (InputManager.GetAxis ("Horizontal") != 0 || InputManager.GetAxis ("Vertical") != 0)) {
				//playerAnimation.isCrouching = false;
				//isCrouching = false;
				isMoving = true;
				sprintTimer += Time.deltaTime;
			} else {
				isMoving = false;
				sprintTimer = 0f;
				if (InputManager.GetButton ("Sprint")) {
					isCrouching = true;
				} else {
					isCrouching = false;
				}
			}
		}

		if (!doNotMove && !isBlocking && !isCrouching) {
			//Displacement ();
			Jump ();
			if (!isDashing && (InputManager.GetButtonDown ("Jump") && InputManager.GetButton ("Magic")) && finalDirection.magnitude > 0.1f /*||
				(InputManager.GetButton ("Jump") && InputManager.GetButtonDown ("Magic") && InputManager.GetButton ("Combo Mode"))*/) {
				StartCoroutine (Dash ());
			}
		}

		InAirCheck ();
	}

	void LateUpdate(){
		if (!isFalling && !isJumping && !isMoving) {
			collider.material.dynamicFriction = 1f;
			collider.material.staticFriction = 1f;
		} else if (isFalling || isMoving || isBouncing) {
			collider.material.dynamicFriction = 0.0f;
			collider.material.staticFriction = 0.0f;
		}

		if (!isCrouching && wasCrouching) {
			references.OverShoulderCameraPos.GetComponent<OverShoulderCamera> ().heightModifier = 0f;
			references.OverShoulderCameraPos.GetComponent<OverShoulderCamera> ().SetRelativePosition (references.OverShoulderCameraPos.GetComponent<OverShoulderCamera> ().lastAsignedRelPosition, 0.2f);
		}else if (isCrouching && !wasCrouching) {
			references.OverShoulderCameraPos.GetComponent<OverShoulderCamera> ().heightModifier = -0.5f;
			references.OverShoulderCameraPos.GetComponent<OverShoulderCamera> ().SetRelativePosition (references.OverShoulderCameraPos.GetComponent<OverShoulderCamera> ().lastAsignedRelPosition, 0.2f);
		}

		if (!doNotMove && !isBlocking)
			AimModeRotation ();
	}

	IEnumerator BounceRecovery(){
		//StartCoroutine (BounceLanding ());
		coroBounceReco = true;
		canBounce = false;
		yield return new WaitForSeconds (bouncingRecoveryTime + 0.01f);
		canBounce = true;
		coroBounceReco = false;
	}

	IEnumerator BounceLanding(){

		float timer = 0.5f;
		while (timer > 0f && !isGrounded && !isGroundedAndStable) {
			timer -= Time.deltaTime;
			yield return null;
		}
		
		if (isGrounded) {
			collider.material.dynamicFriction = 0.0f;
			collider.material.staticFriction = 0.0f;
			//yield return new WaitForSeconds (0.125f);

			doNotMove = true;
			rigidbody.velocity = Vector3.zero; 
			rigidbody.AddForce (Vector3.down * extraGravity, ForceMode.VelocityChange);
			isBouncing = false;
			isJumping = false;
			GetComponent<References> ().Landing ();
			GetComponent<References> ().LandingSound ();
			//CameraShake.Shake (0.025f, 1f / Vector3.Distance (Camera.main.transform.position, transform.position) * 50f);

			float t = 0.125f;
			while (t > 0f) {
				doNotMove = true;
				rigidbody.velocity = Vector3.zero; 	
				yield return null;
				t -= Time.deltaTime;
			}
			doNotMove = false;
			collider.material.dynamicFriction = 0.0f;
			collider.material.staticFriction = 0.0f;
		} else {
			collider.material.dynamicFriction = 0.0f;
			collider.material.staticFriction = 0.0f;
			isBouncing = false;
			isJumping = false;
			doNotMove = false;
		}
	}

	public IEnumerator NormalLanding (){
		coroNormalLanding = true;
		playerAnimation.landingStrenght = 0f;
        doNotMove = true;
        if (fallingSpeed < -10f) {
			playerAnimation.landingStrenght = 3f;
			rigidbody.velocity = Vector3.zero;
			GetComponent<References> ().LandingSound ();
			CameraShake.Shake (0.05f, 1f / Vector3.Distance (Camera.main.transform.position, transform.position) * 2f);
			float t = 0.2f;
			while (t > 0f) {
				doNotMove = true;
				rigidbody.velocity = Vector3.zero; 	
				yield return null;
				t -= Time.deltaTime;
			}
		} else if (fallingSpeed >= -10f && fallingSpeed <= -10.0f) {
			playerAnimation.landingStrenght = 2f;
			rigidbody.velocity = Vector3.zero;
			GetComponent<References> ().LandingSound ();
			CameraShake.Shake (0.025f, 1f / Vector3.Distance (Camera.main.transform.position, transform.position) * 1f);
			float t = 0.1f;
			while (t > 0f) {
				rigidbody.velocity = Vector3.zero; 	
				yield return null;
				t -= Time.deltaTime;
			}
		} else {
			playerAnimation.landingStrenght = 0f;
			GetComponent<References> ().StepSound ();
			CameraShake.Shake (0.01f, 1f / Vector3.Distance (Camera.main.transform.position, transform.position) * 0.5f);
		}
        doNotMove = false;
        coroNormalLanding = false;
	}

	// Update is called once per frame
	void FixedUpdate () {

		//preguntando por el fotograma anterior, si estuvo o no grounded.
		if (isGrounded)
			wasGrounded = true;
		else
			wasGrounded = false;

		//maximizeCamFwd es para que al mirar hacia abajo no disminuya la velocidad del pj
		maximizeCamFwd = Camera.main.transform.forward;
		maximizeCamFwd.y = 0.0f;
		maximizeCamFwd = Vector3.ClampMagnitude (maximizeCamFwd * 100, 1.0f);

		forwardDirection = maximizeCamFwd * InputManager.GetAxis ("Vertical");
		rightDirection = Camera.main.transform.right * InputManager.GetAxis ("Horizontal");
		totalDirection = forwardDirection + rightDirection;
		totalDirection.y = 0f;

		cameraRotationY = Camera.main.transform.eulerAngles.y;
		inputDirection = Mathf.Atan2 (InputManager.GetAxis ("Horizontal"), InputManager.GetAxis ("Vertical")) * Mathf.Rad2Deg;
		floatTargetRotation = cameraRotationY + inputDirection;
		targetRotation = Quaternion.Euler (new Vector3 (0, floatTargetRotation, 0));

		distToGround = collider.bounds.extents.y;

		//Debug.DrawLine (transform.position, transform.position + Vector3.down * (distToGround + 0.1f), Color.red, 0.1f);

		isGrounded = Physics.SphereCast (transform.TransformPoint(capsuleCollider.center), 0.1f, Vector3.down, out hitInfo, distToGround + 0.01f, isGroundedIgnoreLayers);
		isGroundedAndStable = Physics.Raycast (transform.TransformPoint(capsuleCollider.center), Vector3.down, distToGround + 0.01f /*0.25f*/, isGroundedIgnoreLayers);

        if (isOnSlope)
            wasOnSlope = true;
        else
            wasOnSlope = false;

		isOnSlope = false;
		if (Physics.Raycast (transform.position, Vector3.down, out hitInfo, distToGround + 0.2f /*0.25f*/, isGroundedIgnoreLayers, QueryTriggerInteraction.UseGlobal)){
			if (Vector3.Angle (transform.forward, hitInfo.normal) > 100f || Vector3.Angle (transform.forward, hitInfo.normal) < -100f )
				isOnSlope = true;	
		}


		if (!isGrounded)
			wasInAir = true;
		else if (!isJumping)
			jumpsLeft = jumpsAmount;

		if (!doNotMove && !isBlocking) {
			if (!isCrouching) {
				Displacement ();
			}
			Crouching ();
			ComboMode ();
		}

		if (isFalling == true) {
			wasFalling = true;
		} else
			wasFalling = false;

		//no duplicado del de Displacement. Es necesario en caso de que doNotMove sea true!
		if (isGrounded == true && wasFalling == true || isGrounded == true && wasInAir == true) {
			if (!coroNormalLanding)
				StartCoroutine (NormalLanding ());
			isFalling = false;
			isJumping = false;
			isDoubleJumping = false;
			isBouncing = false;
			wasInAir = false;
		}


		//Speed Limiter
		float velocityY;


		if (!doNotMove && isGrounded && !isBouncing && !isJumping && rigidbody.velocity.magnitude > speed && !lockedComboMode && !InputManager.GetButton ("Combo Mode") && !InputManager.GetButton ("Aim"))
			rigidbody.velocity = rigidbody.velocity.normalized * speed;
		else if (!doNotMove && isGrounded && !isBouncing && !isJumping && rigidbody.velocity.magnitude > speed * 0.7f && lockedComboMode | InputManager.GetButton ("Combo Mode") | InputManager.GetButton ("Aim")) {
			velocityY = rigidbody.velocity.y;
			rigidbody.velocity = rigidbody.velocity.normalized * speed * 0.65f;
			rigidbody.velocity = new Vector3 (rigidbody.velocity.x, velocityY, rigidbody.velocity.z);
		}

		velocityY = rigidbody.velocity.y;		
		rigidbody.velocity = new Vector3 (rigidbody.velocity.x, 0f, rigidbody.velocity.z);

        if (hitInfo.point != Vector3.zero && isMoving && wasOnSlope || isOnSlope)
        {
            rigidbody.MovePosition(hitInfo.point + Vector3.up * distToGround);
        }

		if (velocityLimitXZ <= 0f) {

			if (isBouncing && rigidbody.velocity.magnitude > runSpeed * 1.5f) {
				rigidbody.velocity = rigidbody.velocity.normalized * runSpeed * 1.5f;
			} else if (isBouncing && startedBouncingOnSlope && rigidbody.velocity.magnitude > walkSpeed * 1.5f) {
				rigidbody.velocity = rigidbody.velocity.normalized * walkSpeed * 1.5f;
			} else if (isJumping && jumpInertia && !isDoubleJumping && !isBouncing && rigidbody.velocity.magnitude > inertiaSpeed * 0.8f) {
				rigidbody.velocity = rigidbody.velocity.normalized * inertiaSpeed * 0.8f;
			} else if (isJumping && !jumpInertia && !isDoubleJumping && !isBouncing && rigidbody.velocity.magnitude > inAirSpeed) {
				rigidbody.velocity = rigidbody.velocity.normalized * inAirSpeed;
			} else if (wasInAir && !isJumping && !isBouncing && rigidbody.velocity.magnitude > inertiaSpeed * 0.8f) {
				rigidbody.velocity = rigidbody.velocity.normalized * inertiaSpeed * 0.8f;
			} else if (isDoubleJumping && rigidbody.velocity.magnitude > inAirSpeed * 1f) {
				rigidbody.velocity = rigidbody.velocity.normalized * inAirSpeed * 1f;
			}

			if (healthBar.inPain && rigidbody.velocity.magnitude > walkSpeed)
				rigidbody.velocity = rigidbody.velocity.normalized * walkSpeed;
		} else if (velocityLimitXZ > 0f && rigidbody.velocity.magnitude > velocityLimitXZ) {
			rigidbody.velocity = rigidbody.velocity.normalized * velocityLimitXZ;
		}

		rigidbody.velocity = new Vector3 (rigidbody.velocity.x, velocityY, rigidbody.velocity.z);
	}

	void AimModeRotation(){
		//no need to be grounded
		if (InputManager.GetButton ("Aim") || lockedAimMode) {
			//cameraRotationY = Physics.Raycast (Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, Mathf.Infinity, LayerMask.GetMask ("Scenario", "Default", "Rig"));
			targetRotation = Quaternion.Euler (new Vector3 (0, cameraRotationY /*+ 10f*/, 0));

			Vector3 playerAngle = transform.forward;
			playerAngle.x = 0f;
			playerAngle.z = 0f;
			Vector3 cameraAngle = Camera.main.transform.forward;
			cameraAngle.x = 0f;
			cameraAngle.z = 0f;
			float angle = Vector3.Angle (playerAngle, cameraAngle);
			Vector3 cross = Vector3.Cross (cameraAngle, playerAngle);
			if (cross.y < 0) angle = -angle;

			if (angle > 20f || angle < 0f)
				rigidbody.MoveRotation ( Quaternion.Lerp (transform.rotation, targetRotation, Time.deltaTime * rotateSpeed * 1f));
			else
				Quaternion.Lerp (transform.rotation, targetRotation, Time.deltaTime * rotateSpeed * 2f);
		}
	}

	void Crouching(){
		if (isCrouching) {			
			playerAnimation.isCrouching = true;
			references.capsuleCollider.height = Mathf.Lerp (references.capsuleCollider.height, references.capsuleColliderHeight * 0.6f, 10f * Time.deltaTime);
			references.capsuleCollider.center = Vector3.Lerp (references.capsuleCollider.center, references.capsuleColliderCenter - Vector3.up * references.capsuleColliderHeight / 4.5f, 10f * Time.deltaTime);
		} else {
			playerAnimation.isCrouching = false;
			references.capsuleCollider.height = Mathf.Lerp (references.capsuleCollider.height, references.capsuleColliderHeight, 10f * Time.deltaTime);
			references.capsuleCollider.center = Vector3.Lerp (references.capsuleCollider.center, references.capsuleColliderCenter, 10f * Time.deltaTime);
		}
	}


	void Displacement(){
		diagonalSpeedLimit = Vector3.ClampMagnitude (totalDirection, 1.0f);
		diagonalSpeedLimit.y = 0f;
		finalDirection = diagonalSpeedLimit * 10;
		finalDirection = Vector3.ClampMagnitude (finalDirection, 1.0f);
        
        if (isGrounded)
        {
            finalDirection = Vector3.ProjectOnPlane(finalDirection, hitInfo.normal);
        }

        magnitude = Vector3.SqrMagnitude (diagonalSpeedLimit);

		if ((isGrounded == true && wasFalling == true) || (isGrounded == true && wasInAir == true)) {

			isFalling = false;
			isJumping = false;
			isBouncing = false;
			isDoubleJumping = false;
			startedFalling = false;

			speed = runSpeed;
			jumpsLeft = jumpsAmount;
			inAirSpeed = airSpeed;
		}

		//no need to be grounded
		if (InputManager.GetButton ("Aim") || lockedAimMode) {
			//cameraRotationY = Physics.Raycast (Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, Mathf.Infinity, LayerMask.GetMask ("Scenario", "Default", "Rig"));
			targetRotation = Quaternion.Euler (new Vector3 (0, cameraRotationY /*+ 10f*/, 0));

			Vector3 playerAngle = transform.forward;
			playerAngle.x = 0f;
			playerAngle.z = 0f;
			Vector3 cameraAngle = Camera.main.transform.forward;
			cameraAngle.x = 0f;
			cameraAngle.z = 0f;
			float angle = Vector3.Angle (playerAngle, cameraAngle);
			Vector3 cross = Vector3.Cross (cameraAngle, playerAngle);
			if (cross.y < 0) angle = -angle;

			if (angle > 20f || angle < 0f)
				rigidbody.MoveRotation ( Quaternion.Lerp (transform.rotation, targetRotation, Time.deltaTime * rotateSpeed * 1f));
			else
				transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, Time.deltaTime * rotateSpeed * 2f);
			rigidbody.AddForce (finalDirection * speed * 0.8f * Time.deltaTime * 1000);
		}

		if (isGrounded == true) {
			if (/*InputManager.GetKey (KeyCode.LeftShift) ||*/ magnitude <= 0.99f) {
				speed = walkSpeed;
				rotateSpeed = walkRotateSpeed;
			} else {
				speed = runSpeed;
				rotateSpeed = runRotateSpeed;
			}

			if (sprintTimer > 0.3f && InputManager.GetButton ("Sprint") && magnitude > 0.99f && !InputManager.GetButton ("Aim") && !InputManager.GetButton ("Combo Mode") && !playerAnimation.isReloading) {
				if (!lockedAimMode) {
					speed = sprintSpeed;
					rotateSpeed = rotateSpeed / 2;
				} else if (lockedAimMode && InputManager.GetAxis ("Horizontal") == 0f && InputManager.GetAxis ("Vertical") > 0f) {
					speed = sprintSpeed;
					rotateSpeed = rotateSpeed / 2;
				} else {
					speed = runSpeed;
					rotateSpeed = runRotateSpeed;
				}
					
			} else if (/*!InputManager.GetKey (KeyCode.LeftShift) &&*/ magnitude > 0.99f) {
				speed = runSpeed;
				rotateSpeed = runRotateSpeed;
			}

			if (finalDirection != Vector3.zero 
				&& !InputManager.GetButton ("Aim") /*&& !lockedAimMode*/ && !InputManager.GetButton ("Combo Mode") && !lockedComboMode) {
				rigidbody.AddForce (finalDirection * speed * Time.deltaTime * 1000);
				rigidbody.MoveRotation (Quaternion.Slerp (transform.rotation, targetRotation, Time.deltaTime * rotateSpeed));
				jumpInertia = true;
				inertiaTotalDirection = totalDirection;
				inertiaTotalDirection.y = 0f;
				inertiaSpeed = speed;
			} else {
				jumpInertia = false;
			}


		} else if (isGrounded == false) {
			jumpForwardDirection = Camera.main.transform.forward * InputManager.GetAxis ("Vertical");
			jumpRightDirection = Camera.main.transform.right * InputManager.GetAxis ("Horizontal");
			jumpTotalDirection = jumpForwardDirection + jumpRightDirection;
			jumpTotalDirection.y = 0f;
			jumpDiagonalSpeedLimit = Vector3.ClampMagnitude (jumpTotalDirection, 1.0f);

			if (jumpInertia == true && !isDoubleJumping) {
				inertiaDiagonalSpeedLimit = Vector3.ClampMagnitude (inertiaTotalDirection, 1.0f);
//				rigidbody.MovePosition (transform.position + inertiaDiagonalSpeedLimit * (inertiaSpeed * 0.8f) * Time.deltaTime);
				rigidbody.AddForce (inertiaDiagonalSpeedLimit * (inertiaSpeed * 0.8f) * Time.deltaTime * 1000);
			}

			if (!lockedAimMode && InputManager.GetAxis ("Vertical") != 0 || InputManager.GetAxis ("Horizontal") != 0) {
				//float airRotateSpeed = rotateSpeed / 20;
				rigidbody.MoveRotation ( Quaternion.Slerp (transform.rotation, targetRotation, Time.deltaTime  * rotateSpeed / 2));
			}

			if (!isBouncing) {//always except when bouncing
				inAirSpeed = airSpeed;
				rigidbody.AddForce (jumpDiagonalSpeedLimit * inAirSpeed * Time.deltaTime * 1000);
			} else if (isFalling || !isBouncing && isDoubleJumping) {
				inAirSpeed = airSpeed * 2f;
				rigidbody.AddForce (jumpDiagonalSpeedLimit * inAirSpeed * Time.deltaTime * 1000);
			}
		}

	}

	void Jump (){
		if (!coroJumping && InputManager.GetButtonDown ("Jump") && isGrounded == true  && !InputManager.GetButton ("Launcher") && !InputManager.GetButton("Magic")) {
			if (!coroJumping)
				coroutineJump = StartCoroutine (CoroJump ());
		}
		if (!isGrounded) {
		}
	}

	IEnumerator CoroJump(){
		if (canBounce && InputManager.GetButton ("Combo Mode") && !InputManager.GetButton ("Launcher") && !InputManager.GetButton("Melee") && InputManager.GetAxis ("Horizontal") != 0 | InputManager.GetAxis ("Vertical") != 0) {
			coroJumping = true;
			if (InputManager.GetButton ("Jump") && !InputManager.GetButton ("Magic")) {
				//then bounce.
				rigidbody.velocity = Vector3.zero;
				doNotMove = true;
				canBounce = false;
				jumpsLeft = jumpsLeft - 1;
				isJumping = true;
				isBouncing = true;

				startedBouncingOnSlope = false;
				if (Physics.Raycast (transform.position, Vector3.down, out hitInfo, capsuleCollider.height / 2f + 0.2f /*0.25f*/, isGroundedIgnoreLayers, QueryTriggerInteraction.UseGlobal)) {
					if (Vector3.Angle (totalDirection.normalized, hitInfo.normal) > 120f)
						startedBouncingOnSlope = true;
					else
						startedBouncingOnSlope = false;
				}

				//yield return new WaitForSeconds (0.066f);
				inAirSpeed = airSpeed * 1.5f;
				//doNotMove = false;
				collider.material.dynamicFriction = 0.0f;
				collider.material.staticFriction = 0.0f;
				yield return new WaitForSeconds (0.05f);
				rigidbody.velocity = Vector3.zero;
				if (isGrounded == true) {
					GetComponent<References> ().Landing ();
					isGrounded = false;
					isJumping = true;
					healthBar.CantBeHitMode (0.35f);
					rigidbody.AddForce (Vector3.up * jumpBaseForce * 0.333f, ForceMode.VelocityChange); //original 0.475
					rigidbody.AddForce (diagonalSpeedLimit * (runSpeed * 10.0f), ForceMode.VelocityChange);
					yield return new WaitForSeconds (0.125f);
					//rigidbody.AddForce (Vector3.up * jumpBaseForce * 0.175f, ForceMode.VelocityChange);
					//rigidbody.AddForce (diagonalSpeedLimit * (runSpeed * 0.75f), ForceMode.VelocityChange);
					isGrounded = false;
					isJumping = true;
					isBouncing = true;
				}
				StartCoroutine (BounceRecovery ());
				StartCoroutine (BounceLanding ());
			}
		} else if ((canBounce && !InputManager.GetButton ("Launcher") && !InputManager.GetButton("Melee")) ||
			(!isBouncing && !InputManager.GetButton ("Combo Mode") && !InputManager.GetButton ("Launcher") && !InputManager.GetButton("Melee")) ||
			(InputManager.GetAxis ("Horizontal") == 0 && InputManager.GetAxis ("Vertical") == 0 && !InputManager.GetButton ("Launcher") && !InputManager.GetButton("Melee"))) {
			//NORMAL JUMP
			coroJumping = true;
			rigidbody.velocity = Vector3.zero;
			doNotMove = true;
			cantStab = true;
			cantShoot = true;
			isJumping = true;
			healthBar.CantBeHitMode (0.15f);

			float powerMod = 0f;
			float t = 0f;
			float f = 0.08f;
			while (t < f) {
				if (InputManager.GetButton ("Jump"))
					powerMod += Time.deltaTime * 2f;

				rigidbody.velocity = Vector3.zero;
				//isGrounded = false;
				isJumping = true;
				cantStab = false;
				cantShoot = false;
				doNotMove = true;
				t += Time.deltaTime;
				yield return null;
			}

			doNotMove = false;			

			if (isGrounded == true) {
				//NUEVA VERSION!!!	NUEVA VERSION!!!	NUEVA VERSION!!!	NUEVA VERSION!!!

				rigidbody.velocity = Vector3.zero;
				collider.material.dynamicFriction = 0.0f;
				collider.material.staticFriction = 0.0f;

				Vector3 startPos = transform.position;
				Vector3 tempPos = startPos;
				Vector3 prevPos = startPos;
				Vector3 destination = transform.position + finalDirection * (airSpeed + speed / 2f) * (powerMod / f) / 2f;

				powerMod = Mathf.Clamp (powerMod / f, 0.25f, 1f);
				float height = jumpBaseForce / 2 * powerMod;
				float duration = 2.5f * powerMod;
				float normalizedTimeA = 0.0f;

				paraJumpRunning = true;
				breakConditionsMet = false;

				if (checkForBreakConditions != null)
					StopCoroutine (checkForBreakConditions);
				
				checkForBreakConditions = StartCoroutine (CheckForBreakConditions ());
				jumpsLeft = jumpsLeft - 1;
				while (!breakConditionsMet && (normalizedTimeA < 1 || !isGrounded && normalizedTimeA > 1)) {
					if (breakConditionsMet)
						break;

					prevPos = tempPos;
					if (CustomMethods.CheckDisplacementInput ()) //Direccion en el aire, con cierta disminucion al pasar el tiempo y así evitar aceleración
						destination += totalDirection / 20f / (1 + normalizedTimeA);

					float yOffset = height * 4.0f * (normalizedTimeA - normalizedTimeA * normalizedTimeA); // formula para la altura
					tempPos = CustomMethods.Vector3LerpUnclamped (startPos, destination, normalizedTimeA) + yOffset * Vector3.up;

					rigidbody.MovePosition (Vector3.Lerp (transform.position, tempPos, 0.25f));
					normalizedTimeA += Time.deltaTime + (Time.deltaTime - Time.deltaTime * Time.deltaTime) / duration;

					yield return new WaitForFixedUpdate();

					RaycastHit hitPoint;
					if (Physics.Raycast(prevPos + (destination - startPos).normalized * capsuleCollider.radius / 2f, tempPos - prevPos, out hitPoint, capsuleCollider.radius, LayerMask.GetMask("Scenario", "Default"), QueryTriggerInteraction.Ignore)) { //detectar colision y mover al punto de impacto
						rigidbody.MovePosition(rigidbody.position + (prevPos - hitPoint.point).normalized * capsuleCollider.radius);
						break;
					}
				}

				paraJumpRunning = false;
				doNotMove = true;

				if (isGrounded) {
					references.rigidbody.velocity = Vector3.zero;
					yield return new WaitForSeconds (0.05f);
				}

				isGrounded = false;
				isJumping = false;
				cantStab = false;
				cantShoot = false;
				doNotMove = false;
				coroJumping = false;
			}
		}

		if (isJumping || isBouncing) {
			yield return new WaitForFixedUpdate ();
			if (isGrounded) {
				doNotMove = true;
				isJumping = false;
				isBouncing = false;
				rigidbody.velocity = Vector3.zero;
				yield return new WaitForSeconds (0.2f);
				
				doNotMove = false;
				isJumping = false;
				isBouncing = false;
				
			} else {
				yield return new WaitForSeconds (0.2f);
				
			}
			startedBouncingOnSlope = false;
			coroJumping = false;
		}
	}

	IEnumerator CheckForBreakConditions (){
		breakConditionsMet = false;
		while (paraJumpRunning) {
			if (!isDoubleJumping) {
				if (isDashing || CoroDoubleJumping || (InputManager.GetButtonDown ("Jump") && jumpsLeft > 0) || (references.RightHandWeapon.GetComponent<NGS_NewCPU>() && references.RightHandWeapon.GetComponent<NGS_NewCPU>().attacksQueue.Count > 0) ||
				   healthBar.inPain || healthBar.isDead || references.triggering || references.frameCancel) {
					rigidbody.velocity = Vector3.zero;
					breakConditionsMet = true;
					break;
				}
			} else {
				if (isDashing || (InputManager.GetButtonDown ("Jump") && jumpsLeft > 0) || (references.RightHandWeapon.GetComponent<NGS_NewCPU>() && references.RightHandWeapon.GetComponent<NGS_NewCPU>().attacksQueue.Count > 0) ||
				   healthBar.inPain || healthBar.isDead || references.triggering || references.frameCancel) {
					rigidbody.velocity = Vector3.zero;
					CoroDoubleJumping = false;
					breakConditionsMet = true;
					break;
				}
			}

            isJumping = false;
            CoroDoubleJumping = false;
            yield return null;
		}
	}

	void InAirCheck (){
		if (isGrounded == false && isJumping == false && !isDoubleJumping && !cantStab) { //cantstab es utilizado para evitar problemas con LauncherA a la hora de restar un valor a jumpsLeft
			if (startedFalling == false) {
				jumpsLeft = jumpsLeft - 1;
				startedFalling = true;
			}
		}

		if (!doNotMove && !isGrounded && jumpsLeft > 0 && InputManager.GetButtonDown ("Jump") && (!InputManager.GetButton ("Magic") || !InputManager.GetButton ("Combo Mode"))) {
			isJumping = true;
			isBouncing = false;
			//doubleJumpTimer = 0.1f;
			DoubleJump ();
		}

		if (rigidbody.velocity.y < -0.125f && !isGrounded) {
			rigidbody.AddForce (Vector3.down * extraGravity, ForceMode.VelocityChange);
		} 
		if (rigidbody.velocity.y < -10.0f && !isGrounded) {
			isFalling = true;
		} else
			isFalling = false;	
		fallingSpeed = rigidbody.velocity.y;
	}
		

	void DoubleJump (){
		isBouncing = false;
		jumpInertia = false;
		//doubleJumpTimer = 0;
		//doubleJumpTimer = Mathf.Abs (Time.time - DownTimer);
		if (coroutineJump != null) {
			StopCoroutine (coroutineJump);
			coroJumping = false;
			CoroDoubleJumping = false;
		}
		coroutineJump = StartCoroutine (CoroDoubleJump ());
	}

	IEnumerator CoroDoubleJump(){
		if (!CoroDoubleJumping) {
			CoroDoubleJumping = true;
			isDoubleJumping = true;
			jumpInertia = false;
			doNotMove = true;
			//inAirSpeed = airSpeed * 1.2f;
			jumpsLeft = jumpsLeft - 1;
			healthBar.CantBeHitMode (0.175f);

			playerAnimation.isDoubleJumping = true;
			rigidbody.velocity = Vector3.zero;
			yield return new WaitForFixedUpdate ();

			float powerMod = 0f;
			float t = 0f;
			float f = 0.035f;
			while (t < f) {
				if (InputManager.GetButton ("Jump"))
					powerMod += Time.deltaTime;

				rigidbody.velocity = Vector3.zero;
				isGrounded = false;
				isJumping = true;
				cantStab = false;
				cantShoot = false;
				doNotMove = true;
				t += Time.deltaTime;
				yield return null;
			}

			doNotMove = false;
			//yield return new WaitForFixedUpdate ();

			playerAnimation.isDoubleJumping = false;
			rigidbody.velocity = Vector3.zero;
			collider.material.dynamicFriction = 0.0f;
			collider.material.staticFriction = 0.0f;

			Vector3 startPos = transform.position;
			Vector3 tempPos = startPos;
			Vector3 prevPos = startPos;
			Vector3 destination = transform.position + finalDirection * (airSpeed + speed / 10f) * (powerMod / f) / 2f;

			powerMod = Mathf.Clamp (powerMod / f, 0.25f, 1f);
			float height = jumpBaseForce / 2.5f * powerMod;
			float duration = 2.5f * powerMod;
			float normalizedTimeA = 0.0f;
			bool applyForceDown = true;

			paraJumpRunning = true;
			breakConditionsMet = false;

			if (checkForBreakConditions != null)
				StopCoroutine (checkForBreakConditions);

			checkForBreakConditions = StartCoroutine (CheckForBreakConditions ());
			while (!breakConditionsMet && (normalizedTimeA < 1 || !isGrounded && normalizedTimeA > 1)) {
				if (breakConditionsMet)
					break;

				prevPos = tempPos;
				if (CustomMethods.CheckDisplacementInput ()) //Direccion en el aire, con cierta disminucion al pasar el tiempo y así evitar aceleración
					destination += totalDirection / 10f / (1 + normalizedTimeA);

				float yOffset = height * 4.0f * (normalizedTimeA - normalizedTimeA * normalizedTimeA); // formula para la altura
				tempPos = CustomMethods.Vector3LerpUnclamped (startPos, destination, normalizedTimeA) + yOffset * Vector3.up;

			
				RaycastHit hitPoint;
				if (Physics.Raycast (prevPos + (destination - startPos).normalized * capsuleCollider.radius / 2f, tempPos - prevPos, out hitPoint, capsuleCollider.radius, LayerMask.GetMask ("Scenario", "Default"), QueryTriggerInteraction.Ignore)) { //detectar colision y mover al punto de impacto
					/*rigidbody.velocity = Vector3.zero;
						destination = hitPoint.point + (startPos - hitPoint.point).normalized * capsuleCollider.radius;
						startPos = destination;
						breakConditionsMet = true;
						*/
					rigidbody.MovePosition (rigidbody.position + (prevPos - hitPoint.point).normalized * capsuleCollider.radius);
					break;
				}

				//rigidbody.MovePosition (tempPos); //posicionar rigidbody y actualizar loop
				rigidbody.MovePosition (Vector3.Lerp (transform.position, tempPos, 0.25f));
				normalizedTimeA += Time.deltaTime + (Time.deltaTime - Time.deltaTime * Time.deltaTime) / duration;
				yield return new WaitForFixedUpdate();
			}

			if (applyForceDown) {
				//rigidbody.AddForce (Vector3.down * 2.5f, ForceMode.VelocityChange);
			}

			paraJumpRunning = false;
			isDoubleJumping = false;
			doNotMove = false;
			CoroDoubleJumping = false;

			/* CODIGO ORIGINAL CON ADDFORCE
			 * 
			 * 
			CoroDoubleJumping = true;
			jumpInertia = false;
			doNotMove = true;
			healthBar.CantBeHitMode (0.175f);
			rigidbody.velocity = Vector3.zero;
			float timer = 0.125f;
			while (timer > 0f) {
				rigidbody.velocity = Vector3.zero;
				yield return null;
				timer -= Time.deltaTime;
			}	
			isDoubleJumping = true;
			inAirSpeed = airSpeed;
			rigidbody.velocity = Vector3.zero;
			rigidbody.AddForce (Vector3.up * jumpBaseForce * 1.4f, ForceMode.VelocityChange);
			rigidbody.AddForce (finalDirection.normalized, ForceMode.VelocityChange);
			jumpsLeft = jumpsLeft - 1;
			playerAnimation.isDoubleJumping = false;
			doNotMove = false;
			CoroDoubleJumping = false;
			*/
		}
	}

	IEnumerator Dash(){
		if ((isGrounded && GetComponent<ManaBar> ().CurMana >= 17.5f) ||
			(!isGrounded && GetComponent<ManaBar> ().CurMana >= 22f)) {
			//then dash.
			isDashing = true;

			if (isGrounded)
				GetComponent<ManaBar> ().CurMana -= 17.5f;
			else
				GetComponent<ManaBar> ().CurMana -= 22f;
			
			rigidbody.velocity = Vector3.zero;
			doNotMove = true;
			canBounce = false;
			inAirSpeed = airSpeed * 1.2f;
			collider.material.dynamicFriction = 0.0f;
			collider.material.staticFriction = 0.0f;
			GetComponent<References> ().Landing ();
			isGrounded = false;
			healthBar.CantBeHitMode (0.2f);
			CameraShake.Shake (0.025f, 1f / Vector3.Distance (Camera.main.transform.position, transform.position) * 20f);
			Vector3 startPos = rigidbody.position;
			AudioSource _dashSound = CustomMethods.PlayClipAt (dashSound, transform.position);
			_dashSound.transform.parent = transform;
			_dashSound.dopplerLevel = 0.125f;

			//Revisar con un raycast si hay colision o no...
			RaycastHit rayHit;	

			if (autoAim.target) {
				Vector3 targetNewPos = autoAim.target.transform.position;
				targetNewPos.y = 0f;
				if (Vector3.Angle (totalDirection, targetNewPos - transform.position) < 60f) {
					if (Vector3.Distance (autoAim.target.transform.position, transform.position) <= 7f) {
						if (!Physics.Raycast (transform.position, (autoAim.target.transform.position - transform.position).normalized, out rayHit, Vector3.Distance (transform.position, autoAim.target.transform.position), isGroundedIgnoreLayers)) {
							//rigidbody.MovePosition (autoAim.target.transform.position + (transform.forward * autoAim.target.GetComponent<CapsuleCollider> ().radius * -1.05f) + (transform.forward * GetComponent<CapsuleCollider> ().radius * -1.05f) + Vector3.up * 0.5f);
							rigidbody.MovePosition (autoAim.target.transform.position + (transform.forward * autoAim.target.GetComponent<CapsuleCollider> ().radius * -1.2f) + (transform.forward * GetComponent<CapsuleCollider> ().radius * -2.5f) + Vector3.down * 0.1f);
							if (!Physics.Raycast (autoAim.target.transform.position, Vector3.down, autoAim.target.GetComponent<Collider> ().bounds.extents.y + 0.3f, isGroundedIgnoreLayers)) {
								print ("Balabolka");
								playerAnimation.anim.Play ("AirComboMode");
								inertiaTotalDirection = Vector3.zero;
								inertiaSpeed = 0f;
								references.rigidbody.velocity = Vector3.zero;
								references.rigidbody.AddForce (Vector3.up * 2f, ForceMode.VelocityChange);
								autoAim.target.GetComponent<Rigidbody> ().velocity = Vector3.zero;
								autoAim.target.GetComponent<Rigidbody> ().AddForce (Vector3.up * 3f, ForceMode.VelocityChange);
								yield return new WaitForFixedUpdate ();
								references.rigidbody.velocity = Vector3.zero;
								references.rigidbody.AddForce (Vector3.up * 1.5f, ForceMode.VelocityChange);
							}
						} else {
							//rigidbody.MovePosition (rayHit.point + (transform.forward * capsuleCollider.radius * -1f) + (transform.forward * -0.125f) + Vector3.up * 0.5f);
							print ("A");
							rigidbody.MovePosition (rayHit.point + ((rayHit.point - transform.position).normalized * capsuleCollider.radius * -1f) + (transform.forward * -0.125f));
						}
					} else {
						if (!Physics.Raycast (transform.position, (autoAim.target.transform.position - transform.position).normalized, out rayHit, 5f, isGroundedIgnoreLayers)) {
							//rigidbody.MovePosition (transform.position + (autoAim.target.transform.position - transform.position).normalized * 5f + Vector3.up * 0.35f);
							print ("b");
							rigidbody.MovePosition (transform.position + (autoAim.target.transform.position - transform.position).normalized * 5f);
						} else {
							//rigidbody.MovePosition (rayHit.point + (transform.forward * capsuleCollider.radius * -1f) + (transform.forward * -0.125f) + Vector3.up * 0.5f);
							print ("c");
							rigidbody.MovePosition (rayHit.point + ((rayHit.point - transform.position).normalized * capsuleCollider.radius * -1f) + (transform.forward * -0.125f));
						}
					}
				} else {
					if (!Physics.Raycast (transform.position, diagonalSpeedLimit.normalized, out rayHit, 5f + capsuleCollider.radius * 2f, isGroundedIgnoreLayers)) {
						//rigidbody.MovePosition (transform.position + diagonalSpeedLimit.normalized * 5f + Vector3.up * 0.35f);
						print ("d");
						rigidbody.MovePosition (transform.position + diagonalSpeedLimit.normalized * 5f);
					} else {
						print ("e");
						rigidbody.MovePosition (rayHit.point + ((rayHit.point - transform.position).normalized * capsuleCollider.radius * -2f) + (transform.forward * -0.125f) + Vector3.up * 0.35f);
						rigidbody.MovePosition (rayHit.point + ((rayHit.point - transform.position).normalized * capsuleCollider.radius * -2f) + (transform.forward * -0.125f));
					}
				}
			} else {
				if (!Physics.Raycast (transform.position, diagonalSpeedLimit.normalized, out rayHit, 5f + capsuleCollider.radius * 2f, isGroundedIgnoreLayers)) {
					//rigidbody.MovePosition (transform.position + diagonalSpeedLimit.normalized * 5f + Vector3.up * 0.35f);
					print ("f");
					rigidbody.MovePosition (transform.position + diagonalSpeedLimit.normalized * 5f);
				} else {
					print ("g");
					rigidbody.MovePosition (rayHit.point + (transform.forward * capsuleCollider.radius * -4f) + (transform.forward * -0.125f) + Vector3.up * 0.35f);
					rigidbody.MovePosition (rayHit.point + (transform.forward * capsuleCollider.radius * -4f) + (transform.forward * -0.125f));
				}
			}
				
			GameObject DashParticles = Instantiate (references.DashParticlesFX, startPos, Quaternion.LookRotation (rigidbody.position - startPos), transform);
			DashParticles.transform.parent = null;
			DashParticles.transform.localScale = new Vector3 (capsuleCollider.radius * 4f, capsuleCollider.height * 2f, Vector3.Distance (rigidbody.position, startPos) + 1f);
			isGrounded = false;
			doNotMove = true;
			coroJumping = false;
			rigidbody.velocity = Vector3.zero;
			GetComponent<References> ().Landing ();
			yield return new WaitForFixedUpdate ();
			canBounce = true;
			doNotMove = false;
			
			isDashing = false;
			yield break;
		}
	}


	void ComboMode(){
		//player movement
		if(!doNotMove && !InputManager.GetButton("Aim") && !lockedAimMode && InputManager.GetButton("Combo Mode") || lockedComboMode){
			//rigidbody.MoveRotation ( Quaternion.Euler (transform.rotation.x, Camera.main.transform.rotation.eulerAngles.y, transform.rotation.z));

			//diagonalSpeedLimit = Vector3.ClampMagnitude (totalDirection, 1.0f);
			diagonalSpeedLimit.x = diagonalSpeedLimit.x * 10;
			diagonalSpeedLimit.y = 0f;
			diagonalSpeedLimit.z = diagonalSpeedLimit.z * 10;
			diagonalSpeedLimit = Vector3.ClampMagnitude (diagonalSpeedLimit, 1.0f);

			//targetRotation = Quaternion.Euler (new Vector3 (0, transform.rotation.y, 0)); //Rotar hacia donde mire la camara
			targetRotation = Quaternion.Euler (new Vector3 (0, floatTargetRotation, 0));

			if (autoAim.lookingAtAnEnemy) {
				targetRotation = Quaternion.LookRotation ((autoAim.target.transform.position - transform.position).normalized);
				if (!isGrounded && playerAnimation.isAiming && !playerAnimation.isAttackingMelee) {
				} else {
					targetRotation = Quaternion.Euler (new Vector3 (0, targetRotation.eulerAngles.y, 0));
				}
				rigidbody.MoveRotation ( Quaternion.Slerp (transform.rotation, targetRotation, Time.deltaTime  * rotateSpeed * 4f));
			} else if (!InputManager.GetButton ("Fire") && InputManager.GetAxis ("Vertical") != 0 || InputManager.GetAxis ("Horizontal") != 0) {
				rigidbody.MoveRotation ( Quaternion.Slerp (transform.rotation, targetRotation, Time.deltaTime  * rotateSpeed));
			} else{
				targetRotation = Quaternion.Euler (new Vector3 (0, Camera.main.transform.rotation.eulerAngles.y, 0)); //Rotar hacia donde mire la camara
				rigidbody.MoveRotation ( Quaternion.Slerp (transform.rotation, targetRotation, Time.deltaTime * rotateSpeed));
			}

			if (!isCrouching)
				rigidbody.AddForce (diagonalSpeedLimit * (runSpeed * 0.7f) * Time.deltaTime * 1000);
			
		}

	}

}
