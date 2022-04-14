using UnityEngine;
using System.Collections;
using Luminosity.IO;

public class GenericGun : MonoBehaviour {

	public bool ownerIsAI = false;

	private bool awoke = false;
	[HideInInspector] public Vector3 _defaultPosition;
	[HideInInspector] public Quaternion _defaultRotation;
	[HideInInspector] public Vector3 _defaultScale;

	public bool isInventory = false;
	public int weaponType = 2; 	/*	1 = fist
									2 = gun
									3 = dual guns
									4 = rifle
								*/
	public GameObject Player;
	private GameObject PlayerCamera;
	private Player_Animation playerAnimation;
	public GameObject muzzleFlash;
	private Hero_Movement movementScript;
	public string weaponName = "Generic Gun";
	public int maxCapacity = 15;
	public int curCapacity = 15;
	public bool isReloading;
	private Coroutine coroReloading;
	public int pellets = 1;
	public float bulletSpeed = 15f;
	public float baseDamage = 10;
	public float reloadingTime = 2.25f;
	public float recoilBasePower = 1f;
	public float recoilTime = 0.05f;
	private float recoil;
	private Coroutine coroRecoil;
	public float maxSpread = 2.5f;
	public float minSpread = 1.0f;
	private float spread;
	public float spreadGrowthPerShot = 0.75f;
	public float spreadRestPerSec = 0.25f;
	public Transform GunTransform;
	public GameObject BulletSpawn;
	public AudioClip gunshot;
	public AudioClip reloadSound;
	public float coolDown;
	private float timer = 0.6f;
	public bool isMachineGun = false;
	public bool ripper = false;
	private float coolTimer;
	public bool riseGun;
	public bool fireGun;
//	public bool doubleHanded;
	private float handRisedTimer;
	public GameObject target;
	private GameObject bulletInstance;
	public Material tracerMaterial;

//	public bool unableToFire;
	public bool initialized = false;
	private BulletBehavior bulletBehavior;

	public Vector3 overShoulderRelativePos;
	public LayerMask bulletIgnoreLayersA;

	private bool coroRunning;
	private float fireTimer;

	public void Awake(){
		if (!ownerIsAI && !awoke) {
			_defaultPosition = transform.position;
			_defaultRotation = transform.rotation;
			_defaultScale = transform.localScale;
		}
	}

	public void Reinitialize(){
		Start ();
	}

	// Use this for initialization
	void Start () {
		initialized = false;
		if (!ownerIsAI) {
			if (transform.parent && transform.parent.name != "Inventory") {
				isInventory = false;
				GetComponent<Renderer> ().enabled = true;

				gameObject.layer = 9;
				transform.localPosition = _defaultPosition;
				transform.localRotation = _defaultRotation;
				transform.localScale = _defaultScale;

				Player = transform.parent.parent.parent.parent.parent.parent.parent.parent.parent.parent.gameObject as GameObject;
				PlayerCamera = Player.GetComponent<References> ().Camera;
				movementScript = Player.GetComponent<Hero_Movement> ();
				playerAnimation = Player.GetComponent<Player_Animation> ();	
				GunTransform = transform.Find ("Tip");
				riseGun = Player.GetComponent<Player_Animation> ().rise;
				recoil = -recoilBasePower;
				isReloading = false;
				coroRunning = false;

				bulletIgnoreLayersA = LayerMask.GetMask ("Default", "Rig", "Scenario", "Enemy", "Player");

				if (Player.GetComponent<References> ().LeftHandWeapon == this.gameObject) {
					playerAnimation.gunWeaponType = weaponType;
					Player.GetComponent<References> ().OverShoulderCameraPos.GetComponent<OverShoulderCamera> ().SetRelativePosition (overShoulderRelativePos, 2f);
				}

				if (muzzleFlash == null && transform.Find ("MuzzleFlash").gameObject) {
					muzzleFlash = transform.Find ("MuzzleFlash").gameObject;
				}

			} else {
				isInventory = true;
				GetComponent<Renderer> ().enabled = false;
			}
			initialized = true;
		} else {
			isInventory = false;
			//gameObject.layer = 1;

			/*
			transform.localPosition = _defaultPosition;
			transform.localRotation = _defaultRotation;
			transform.localScale = _defaultScale;
			*/

			//Player = transform.parent.parent.parent.parent.parent.parent.parent.parent.parent.parent.gameObject as GameObject;
			//PlayerCamera = Player.GetComponent<References> ().Camera;
			//movementScript = Player.GetComponent<Hero_Movement> ();
			playerAnimation = Player.GetComponent<Player_Animation> ();
			GunTransform = transform.Find ("Tip");
			riseGun = Player.GetComponent<Player_Animation> ().rise;
			recoil = -recoilBasePower;
			isReloading = false;
			coroRunning = false;

			if (Player.GetComponent<References> ().LeftHandWeapon == this.gameObject && Player.GetComponent<References> ().OverShoulderCameraPos != null) {
				playerAnimation.gunWeaponType = weaponType;
				Player.GetComponent<References> ().OverShoulderCameraPos.GetComponent<OverShoulderCamera> ().SetRelativePosition (overShoulderRelativePos, 2f);
			}

			if (muzzleFlash == null && transform.Find ("MuzzleFlash").gameObject) {
				muzzleFlash = transform.Find ("MuzzleFlash").gameObject;
			}
			initialized = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!ownerIsAI && !isInventory) {
			if (spread > minSpread) {
				if (spread > maxSpread)
					spread = maxSpread;
				if ((!InputManager.GetButton ("Fire") && InputManager.GetAxis ("Fire") <= 0f) || movementScript.cantShoot)
					spread = spread -(Mathf.Pow(spreadRestPerSec * Time.fixedDeltaTime, 2) * 100);
			} else {
				spread = minSpread;
			}

			if (curCapacity > maxCapacity)
				curCapacity = maxCapacity;

			if (handRisedTimer > 0f) {
				handRisedTimer = handRisedTimer - Time.deltaTime;
				Player.GetComponent<Player_Animation> ().rise = true;
				Player.GetComponent<playerGUI> ().isAiming = true;
				riseGun = true;
			} else {
				Player.GetComponent<Player_Animation> ().rise = false;
				Player.GetComponent<playerGUI> ().isAiming = false;
				riseGun = false;
			}

			if (!isInventory && !Player.GetComponent<HealthBar> ().inPain && !Player.GetComponent<HealthBar> ().isDead) {
			
				if (movementScript.cantShoot)
					riseGun = false;

				if (Player.GetComponent<References>().AutoAim.GetComponent<EnemiesDetector> ().target != null) {
					target = Player.transform.Find ("closestEnemy").gameObject.GetComponent<EnemiesDetector> ().target;
				} else {
					target = null;
				}

				Player.GetComponent<Player_Animation> ().lHandTrigger = true;

				if (coolTimer < coolDown)
					coolTimer += Time.deltaTime;

				if (!movementScript.cantShoot && !movementScript.isBlocking && !isReloading) {
					InputReload ();
					Shoot ();
					Aim ();
				}
			}
		}
	}

	void FixedUpdate(){
		if (!ownerIsAI && !isInventory) {
			if (fireGun && weaponType == 2 && fireTimer > 0.025f) {
				Player.GetComponent<Player_Animation> ().anim.SetLayerWeight (1, 1.0f);
				Player.GetComponent<Player_Animation> ().anim.Play ("Fire", 1, 0f);
				Player.GetComponent<Player_Animation> ().fire = true;
				fireGun = false;
			} else if (!fireGun && weaponType == 2) {
				fireTimer = 0f;
				if (Player.GetComponent<Player_Animation> ().anim.GetCurrentAnimatorStateInfo (1).normalizedTime > 0.9f) {
					Player.GetComponent<Player_Animation> ().anim.SetLayerWeight (1, 0f);
					Player.GetComponent<Player_Animation> ().fire = false;
				}
			} else if (fireGun && fireTimer < 0.05f)
				fireTimer += Time.deltaTime;
		}
	}

/*
	void OnGUI () {
		if (!ownerIsAI && isActiveAndEnabled && !isInventory) {
			GUI.Box (new Rect (10, Screen.height - 30, 100, 20), weaponName);
			if (!isReloading)
				GUI.Box (new Rect (115, Screen.height - 30, 50, 20), curCapacity + "/" + maxCapacity);
			else
				GUI.Box (new Rect (115, Screen.height - 30, 50, 20), "RELOADING!!!");
		}
	}
*/

	void OnDisable() {
		if (!ownerIsAI && Player != null && initialized && !isInventory) {
			riseGun = false;
			fireGun = false;
			Player.GetComponent<Player_Animation> ().rise = false;
			Player.GetComponent<Player_Animation> ().fire = false;
			isReloading = false;
			curCapacity = maxCapacity;
			if (Player && Player.GetComponent<References> ().OverShoulderCameraPos != null && !GetComponent<GG_SinperZoom>() || !GetComponent<GG_SinperZoom>().isZooming)
				Player.GetComponent<References> ().OverShoulderCameraPos.GetComponent<OverShoulderCamera> ().ResetPosition(4f);
			if (coroReloading != null)
				StopCoroutine (coroReloading);
		}
	}

	void OnEnable(){
		Start ();
		/*
		if (initialized &&
		    !isInventory &&
		    !ownerIsAI &&
		    (!GetComponent<GG_SinperZoom> () ||
				!GetComponent<GG_SinperZoom> ().isZooming)) {
			Player.GetComponent<References> ().OverShoulderCameraPos.GetComponent<OverShoulderCamera> ().SetRelativePosition (overShoulderRelativePos, 2f);
		}
		*/
	}

	void InputReload(){
		if (!isReloading && InputManager.GetAxisRaw ("Direction Vertical") > 0.0f) {
			StartCoroutine (Reload ());
		}
	}

	void Shoot (){
		if ((InputManager.GetButtonDown ("Fire") || InputManager.GetAxis ("Fire") > 0.5f) && curCapacity >= 0 && !movementScript.cantShoot && !InputManager.GetButton ("Aim")) {
			handRisedTimer = 3.0f;
		}

		if (!movementScript.cantShoot && (InputManager.GetButton ("Fire") || InputManager.GetAxis ("Fire") > 0.5f) && !InputManager.GetButton ("Aim") && !movementScript.lockedAimMode && coolTimer >= coolDown && riseGun) {
			if (curCapacity > 0) {
				int playerLayer = Player.layer;
				Player.layer = 1;

				riseGun = true;
				Player.GetComponent<Player_Animation> ().rise = true;
				if (isMachineGun || (InputManager.GetButtonDown ("Fire") || InputManager.GetAxis ("Fire") > 0.5f)) {
					fireGun = true;
					fireTimer = 0f;
					curCapacity = curCapacity - 1;
					Player.GetComponent<Player_Animation> ().fire = true;
//			Instantiate (BulletSpawn, GunTransform.position, Player.transform.rotation);
					if (target) {
						recoil = -recoilBasePower / 4;
						if (coroRecoil != null)
							StopCoroutine (coroRecoil);
						coroRecoil = StartCoroutine (RecoilGoAndBack (recoil, recoilTime / 2f));
						float distance = Mathf.Clamp01 (Vector3.Distance (transform.position, target.transform.position) / 5);
						Vector3 aimToEnemyBody = target.GetComponent<References> ().Head.transform.position + Vector3.down * distance * 1.5f;
						for (int i = pellets; i > 0; i--) {
							if (InputManager.GetButton ("Combo Mode"))
								bulletInstance = (GameObject)Instantiate (BulletSpawn, GunTransform.position, Quaternion.LookRotation (aimToEnemyBody - Player.transform.position));
							else
								bulletInstance = (GameObject)Instantiate (BulletSpawn, GunTransform.position, Player.transform.rotation);
							bulletInstance.GetComponent<BulletBehavior> ().shooter = Player.transform;
							bulletInstance.GetComponent<BulletBehavior> ().baseDamage = baseDamage;

							if (!movementScript.isCrouching) {
								recoil = -recoilBasePower;
							} else {
								recoil = -recoilBasePower / 2f;
							}

							bulletInstance.GetComponent<BulletBehavior> ().spread = Mathf.Clamp (spread, minSpread, maxSpread);
							bulletInstance.GetComponent<BulletBehavior> ().speed = bulletSpeed;
							bulletInstance.GetComponent<BulletBehavior> ().isRipper = ripper;


							bulletInstance.transform.GetChild (0).GetComponent<TrailRenderer> ().material = tracerMaterial;
							bulletInstance.transform.GetChild (0).GetComponent<MeshRenderer> ().material = tracerMaterial;

							Physics.IgnoreCollision (bulletInstance.transform.GetComponent<Collider> (), Player.transform.GetComponent<Collider> ());
						}
					} else {
						recoil = -recoilBasePower / 4;
						if (coroRecoil != null)
							StopCoroutine (coroRecoil);
						coroRecoil = StartCoroutine (RecoilGoAndBack (recoil, recoilTime / 2f));
						RaycastHit hit;
						Physics.Raycast (Camera.main.transform.position + Camera.main.transform.forward * 1f, Camera.main.transform.forward, out hit, 100f, bulletIgnoreLayersA);
						for (int i = pellets; i > 0; i--) {						
							if (InputManager.GetButton ("Combo Mode")) {
								if (hit.collider && hit.collider.gameObject != Player) {
									//bulletInstance = (GameObject)Instantiate (BulletSpawn, GunTransform.position, Quaternion.LookRotation (hit.point - GunTransform.position));
									bulletInstance = (GameObject)Instantiate (BulletSpawn, GunTransform.position, Quaternion.LookRotation (hit.point - Player.transform.position));
								} else {
									//bulletInstance = (GameObject)Instantiate (BulletSpawn, GunTransform.position, Camera.main.transform.rotation);
									bulletInstance = (GameObject)Instantiate (BulletSpawn, GunTransform.position, Camera.main.transform.rotation);
								}
							} else {
								bulletInstance = (GameObject)Instantiate (BulletSpawn, GunTransform.position, Player.transform.rotation);
							}
							bulletInstance.GetComponent<BulletBehavior> ().shooter = Player.transform;
							bulletInstance.GetComponent<BulletBehavior> ().baseDamage = baseDamage;

							if (!movementScript.isCrouching) {
								recoil = -recoilBasePower;
							} else {
								recoil = -recoilBasePower / 2f;
							}

							bulletInstance.GetComponent<BulletBehavior> ().spread = Mathf.Clamp (spread, minSpread, maxSpread);
							bulletInstance.GetComponent<BulletBehavior> ().speed = bulletSpeed;
							bulletInstance.GetComponent<BulletBehavior> ().isRipper = ripper;


							bulletInstance.transform.GetChild (0).GetComponent<TrailRenderer> ().material = tracerMaterial;
							bulletInstance.transform.GetChild (0).GetComponent<MeshRenderer> ().material = tracerMaterial;

							Physics.IgnoreCollision (bulletInstance.transform.GetComponent<Collider> (), Player.transform.GetComponent<Collider> ());
						}
					}
					coolTimer = 0f;

					AudioSource audioSource = CustomMethods.PlayClipAt (gunshot, transform.position);
					audioSource.volume = 0.8f;
					audioSource.maxDistance = 50f;
					muzzleFlash.GetComponent<ParticleSystem> ().Play ();
				}

				if (!movementScript.isCrouching || pellets > 1) {
					spread = Mathf.Clamp (spread + spreadGrowthPerShot, minSpread, maxSpread);
				} else {
					spread = Mathf.Clamp (spread + spreadGrowthPerShot / 2f, minSpread, maxSpread);
				}
			Player.layer = playerLayer;
			}
			//Reload
			else if (curCapacity <= 0 && (InputManager.GetButtonDown ("Fire") || InputManager.GetAxis ("Fire") > 0.5f)) {
				coroReloading = StartCoroutine (Reload ());
			}
		}

		if (curCapacity <= 0 && (InputManager.GetButtonDown ("Fire") || InputManager.GetAxis ("Fire") > 0.5f))
			coroReloading = StartCoroutine (Reload ());

		//if (!(InputManager.GetButtonDown ("Fire") && !InputManager.GetButton ("Aim") && coolTimer >= coolDown && riseGun)) {
		//	Player.GetComponent<Player_Animation> ().fire = false;		
		//}
	}

	void Aim (){
		if (InputManager.GetButton ("Aim") || movementScript.lockedAimMode) {
			timer = timer + Time.deltaTime;
			
			if (!isMachineGun && (InputManager.GetButtonDown ("Fire") || InputManager.GetAxis ("Fire") > 0.5f) & timer > 0.125f & coolTimer >= coolDown) {
				AimShoot ();
			}
			else if (isMachineGun && (InputManager.GetButton ("Fire") || InputManager.GetAxis ("Fire") > 0.5f) & timer > 0.125f & coolTimer >= coolDown) {
				AimShoot ();
			}
		} else{
			timer = 0;
		}
	}

	void AimShoot (){
		if (curCapacity > 0) {
			fireGun = true;
			fireTimer = 0f;
			curCapacity = curCapacity - 1;
			int playerLayer = Player.layer;
			Player.layer = 1;
			for (int i = pellets; i > 0; i--) {
				RaycastHit hit;
				Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100f, bulletIgnoreLayersA);
				if (hit.collider && hit.collider.gameObject != Player)
					bulletInstance = (GameObject)Instantiate(BulletSpawn, GunTransform.position, Quaternion.LookRotation(hit.point - GunTransform.position));
				else
					bulletInstance = (GameObject)Instantiate(BulletSpawn, GunTransform.position, Camera.main.transform.rotation);
				bulletInstance.GetComponent<BulletBehavior>().shooter = Player.transform;
				bulletInstance.GetComponent<BulletBehavior>().baseDamage = baseDamage;

				if (!movementScript.isCrouching) {
					recoil = -recoilBasePower / 2;
				} else {
					recoil = -recoilBasePower / 3f;
				}

				bulletInstance.GetComponent<BulletBehavior>().spread = Mathf.Clamp(spread, minSpread, maxSpread);
				bulletInstance.GetComponent<BulletBehavior>().speed = bulletSpeed;
				bulletInstance.GetComponent<BulletBehavior>().isRipper = ripper;


				bulletInstance.transform.GetChild(0).GetComponent<TrailRenderer>().material = tracerMaterial;
				bulletInstance.transform.GetChild(0).GetComponent<MeshRenderer>().material = tracerMaterial;

				Physics.IgnoreCollision(bulletInstance.transform.GetComponent<Collider>(), Player.transform.GetComponent<Collider>());
			}
			Player.layer = playerLayer;

			if (coroRecoil != null)
				StopCoroutine(coroRecoil);
			coroRecoil = StartCoroutine(RecoilGoAndBack(recoil, recoilTime));

			muzzleFlash.GetComponent<ParticleSystem>().Play();


			if (pellets <= 1) {
				if (!movementScript.isCrouching) {
					spread = Mathf.Clamp(spread + spreadGrowthPerShot / 2f, minSpread, maxSpread);
				} else {
					spread = Mathf.Clamp(spread + spreadGrowthPerShot / 3f, minSpread, maxSpread);
				}
			} else
				spread = Mathf.Clamp(spread + spreadGrowthPerShot, minSpread, maxSpread);

			if (GetComponent<GG_SinperZoom> ())
				CameraShake.Shake (0.025f, 1f / Vector3.Distance (Camera.main.transform.position, transform.position));
			else
				CameraShake.Shake (0.025f, recoilBasePower / Vector3.Distance (Camera.main.transform.position, transform.position));

			AudioSource audioSource = CustomMethods.PlayClipAt (gunshot, transform.position);
			audioSource.volume = 0.8f;
			audioSource.maxDistance = 50f;
			coolTimer = 0f;
		} else
			StartCoroutine (Reload ());
			
	}

	IEnumerator Reload(){
		if (!isReloading) {
			isReloading = true;
			playerAnimation.isReloading = true;

			yield return new WaitForSeconds (0.225f);
			AudioSource audioSource = CustomMethods.PlayClipAt (reloadSound, transform.position);
			audioSource.transform.parent = Player.transform;
			audioSource.maxDistance = 20f;
			yield return new WaitForSeconds (reloadingTime);
			curCapacity = maxCapacity;
			isReloading = false;
			playerAnimation.isReloading = false;
		}
	}

	IEnumerator RecoilGoAndBack(float strength, float time){
		yield return null;
		float normalizedTimeA = 0.0f;
		strength = strength * 2.5f;

		Quaternion newRotation = Quaternion.Euler (strength * Random.Range (1.0f, 1.2f), Random.Range (-0.1f, 0.1f), 0f);
		yield return new WaitForFixedUpdate ();
		yield return new WaitForFixedUpdate ();

		while (normalizedTimeA < 1) {
			normalizedTimeA += Time.deltaTime / 0.015f;
			PlayerCamera.transform.rotation = Quaternion.Slerp (PlayerCamera.transform.rotation, PlayerCamera.transform.rotation * newRotation, normalizedTimeA);
			yield return null;
		}	
		normalizedTimeA = 0.0f;
		newRotation = Quaternion.Euler (-strength * Random.Range (0.3f, 0.5f), Random.Range (-0.1f, 0.1f), 0f);
		while (normalizedTimeA < 1) {
			normalizedTimeA += Time.deltaTime / (time * 1.5f);
			PlayerCamera.transform.rotation = Quaternion.Slerp (PlayerCamera.transform.rotation, PlayerCamera.transform.rotation * newRotation, normalizedTimeA);
			yield return null;
		}	
	}
}
