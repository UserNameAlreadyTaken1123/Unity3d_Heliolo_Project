using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class playerGUI : MonoBehaviour {

	private bool startup = true;

	public GUISkin defaultGUISkin;
	private GameObject Player;
	private Hero_Movement movementScript;
	private References references;


	public Texture2D crosshairTexture;
	public Texture2D crosshairTextureHit;
	public float crosshairScale = 1;
	public float timerHitBase = 0.5f;
	private float timerHit = 0.0f;


	private EnemiesDetector autoaimScript;
	private Camera playerCamera;
	private GameObject target;
	private Vector3 targetScreenPos;
	private Rect rect;
	private Vector2 pivot;

	public Texture2D lockedTargetTextureA;
	public Texture2D lockedTargetTextureB;
	private float lockedTargetRotationA;
	private float lockedTargetRotationB;

	public bool isDead;

	public HealthBar Health;
	public float MaxHealth;
	public float CurHealth;
	public float HealthBarLength;

	public HealthBar EnemyHealth;
	public float MaxEnemyHealth;
	public float CurEnemyHealth;
	public float EnemyHealthBarLength;


	public ManaBar Mana;
	public float MaxMana;
	public float CurMana;
	public float ManaBarLength;

	public CycleGuns guns;
	public GameObject gunsSlot2;
	public GameObject gunsSlot3;
	public GameObject gunsSlot4;

	public CycleSwords swords;
	public GameObject swordsSlot2;
	public GameObject swordsSlot3;
	public GameObject swordsSlot4;

	public bool isAiming;

	public bool wavesMode;
	public bool showTimer;
	public float timer;
	public int waveNumber;
	public int enemiesLeft;

	public bool youWin = false;
	//public float refreshRate = 0.1f;
	private float _refreshRate;


	// Use this for initialization
	void Start () {
		Player = this.gameObject;
		Health = Player.GetComponent<HealthBar> ();
		movementScript = Player.GetComponent<Hero_Movement> ();
		references = Player.GetComponent<References> ();
		playerCamera = GetComponent<References> ().Camera.GetComponent<Camera> ();
		autoaimScript = transform.GetChild (1).GetComponent<EnemiesDetector> ();
		Cursor.visible = false;
		//_refreshRate = refreshRate;
	}

	void OnGUI () {
		if (_refreshRate > 0f) {
			_refreshRate = 0f;
			if (PlayerPrefs.GetInt ("DisplayHUD", 1) != 0) {
				if (startup)
					GUI.skin.box.normal.background = defaultGUISkin.GetStyle ("textfield").normal.background;

				//Siempre que el juego no esté en pausa!!!
				if (Time.timeScale != 0) {

					//si se juega el modo de "Oleadas" de enemigos
					//si se juega el modo de "Oleadas" de enemigos
					//si se juega el modo de "Oleadas" de enemigos

					if (wavesMode && !youWin) {
						GUI.Box (new Rect (Screen.width / 2 - 40, 40, 80, 20), "Wave Nº: " + waveNumber);
						if (showTimer) {
							GUI.Box (new Rect (Screen.width / 2 - 80, 65, 160, 20), "Next Wave in " + timer.ToString ("F1") + "...");
						} else {
							GUI.Box (new Rect (Screen.width / 2 - 80, 65, 160, 20), "Enemies Left: " + enemiesLeft);
						}
					}

					//Player's Health and Mana bars
					//Player's Health and Mana bars
					//Player's Health and Mana bars

					GUI.Box (new Rect (10, 10, HealthBarLength, 20), "H: " + CurHealth.ToString ("F0") + "/" + MaxHealth);
					GUI.Box (new Rect (10, 35, ManaBarLength, 20), "M: " + CurMana.ToString ("F0") + "/" + MaxMana);

					//Focus Bar, blocking stamina
					if (Health.curFocus < Health.maxFocus) {
						float width = (Screen.width / 4) * (Health.curFocus / Health.maxFocus);
						if (width < 10f)
							width = 10f;
						GUI.Box (new Rect (10, 60, width, 20), "F: " + Health.curFocus.ToString ("F1") + "/" + Health.maxFocus);

					}

					//armas equipadas, pero la actualmente en uso (El arma activa hace su propio gui en su propio código
					//armas equipadas, pero la actualmente en uso (El arma activa hace su propio gui en su propio código
					//armas equipadas, pero la actualmente en uso (El arma activa hace su propio gui en su propio código

					if (gunsSlot2 != null)
						GUI.Label (new Rect (10, Screen.height * 3 / 4 + 15, 100, 20), gunsSlot2.ToString ());
					if (gunsSlot3 != null)
						GUI.Label (new Rect (10, Screen.height * 3 / 4 + 30, 100, 20), gunsSlot3.ToString ());
					if (gunsSlot4 != null)
						GUI.Label (new Rect (10, Screen.height * 3 / 4 + 45, 100, 20), gunsSlot4.ToString ());


					// Arma de Melee actualmente en uso en MANO IZQUIERDA!!!
					if (references.LeftHandWeapon && references.LeftHandWeapon.GetComponent<NGS_NewCPU> ()) {
						NGS_NewCPU swordScript = references.LeftHandWeapon.GetComponent<NGS_NewCPU> ();
						GUI.Box (new Rect (10, Screen.height - 30, 100, 20), swordScript.weaponName);
					}
					// Arma de fuego actualmente en uso en MANO IZQUIERDA!!!
					if (references.LeftHandWeapon && references.LeftHandWeapon.GetComponent<GenericGun> ()) {
						GenericGun gunScript = references.LeftHandWeapon.GetComponent<GenericGun> ();
						GUI.Box (new Rect (10, Screen.height - 30, 100, 20), gunScript.weaponName);
						if (!gunScript.isReloading)
							GUI.Box (new Rect (115, Screen.height - 30, 50, 20), "A: " + gunScript.curCapacity + "/" + gunScript.maxCapacity);
						else
							GUI.Box (new Rect (115, Screen.height - 30, 50, 20), "RELOADING!!!");
					}

					if (swordsSlot2 != null)
						GUI.Label (new Rect (Screen.width - 110, Screen.height * 3 / 4 + 15, 100, 20), swordsSlot2.ToString ());
					if (swordsSlot3 != null)
						GUI.Label (new Rect (Screen.width - 110, Screen.height * 3 / 4 + 30, 100, 20), swordsSlot3.ToString ());
					if (swordsSlot4 != null)
						GUI.Label (new Rect (Screen.width - 110, Screen.height * 3 / 4 + 45, 100, 20), swordsSlot4.ToString ());

					// Arma de Melee actualmente en uso en MANO DERECHA!!!
					if (references.RightHandWeapon && references.RightHandWeapon.GetComponent<NGS_NewCPU> ()) {
						NGS_NewCPU swordScript = references.RightHandWeapon.GetComponent<NGS_NewCPU> ();
						GUI.Box (new Rect (Screen.width - 110, Screen.height - 30, 100, 20), swordScript.weaponName);
					}
					// Arma de fuego actualmente en uso en MANO DERECHA!!!
					if (references.RightHand && references.RightHand.GetComponent<GenericGun> ()) {
						GenericGun gunScript = references.RightHand.GetComponent<GenericGun> ();
						GUI.Box (new Rect (Screen.width - 110, Screen.height - 30, 100, 20), gunScript.weaponName);
						if (!gunScript.isReloading)
							GUI.Box (new Rect (Screen.width - 165, Screen.height - 30, 50, 20), "A: " + gunScript.curCapacity + "/" + gunScript.maxCapacity);
						else
							GUI.Box (new Rect (Screen.width - 165, Screen.height - 30, 50, 20), "RELOADING!!!");
					}

					//Crosshair Crosshair Crosshair Crosshair
					//Crosshair Crosshair Crosshair Crosshair
					//Crosshair Crosshair Crosshair Crosshair
	
					if (InputManager.GetButton ("Aim") || movementScript.lockedAimMode) {
						if (crosshairTexture != null) {
							GUI.color = new Color (1, 1, 1, 0.5f);
							GUI.DrawTexture (new Rect ((Screen.width - crosshairTexture.width * crosshairScale) / 2, (Screen.height - crosshairTexture.height * crosshairScale) / 2, crosshairTexture.width * crosshairScale, crosshairTexture.height * crosshairScale), crosshairTexture);
						}
						if (crosshairTextureHit != null) {
							GUI.color = new Color (1, 1, 1, timerHit * 1.5f);
							GUI.DrawTexture (new Rect ((Screen.width - crosshairTextureHit.width * crosshairScale) / 2, (Screen.height - crosshairTextureHit.height * crosshairScale) / 2, crosshairTextureHit.width * crosshairScale, crosshairTextureHit.height * crosshairScale), crosshairTextureHit);
						}
					} else if (InputManager.GetButton ("Combo Mode") && isAiming && !autoaimScript.lookingAtAnEnemy) {
						if (crosshairTexture != null) {
							GUI.color = new Color (1, 1, 1, 0.5f);
							GUI.DrawTexture (new Rect ((Screen.width - crosshairTexture.width * crosshairScale) / 2, (Screen.height - crosshairTexture.height * crosshairScale) / 2, crosshairTexture.width * crosshairScale, crosshairTexture.height * crosshairScale), crosshairTexture);
						}
				
					} else if (autoaimScript.lookingAtAnEnemy) {
						//A partir de Acá, todo lo quetenga que ver con el Target
						//A partir de Acá, todo lo quetenga que ver con el Target
						//A partir de Acá, todo lo quetenga que ver con el Target
						target = autoaimScript.target;

						CurEnemyHealth = target.GetComponent<HealthBar> ().CurHealth;
						MaxEnemyHealth = target.GetComponent<HealthBar> ().Maxhealth;
						;
						EnemyHealthBarLength = 280 * (CurEnemyHealth / MaxEnemyHealth);

						//Display info del enemy
						GUI.Box (new Rect (Screen.width / 2 - 140, Screen.height - 35, 280, 25),
							target.name + "'s health: " + target.GetComponent<HealthBar> ().CurHealth.ToString ("F0") + "/" + target.GetComponent<HealthBar> ().Maxhealth);


						Texture2D texture = new Texture2D (1, 1);
						texture.SetPixel (0, 0, Color.black);
						texture.Apply ();
						GUI.skin.box.normal.background = texture;
						GUI.Box (new Rect (Screen.width / 2 - EnemyHealthBarLength / 2 - 2f, Screen.height - 45 - 2, EnemyHealthBarLength + 4, 5 + 4), "");

						GUI.skin.box.normal.background = defaultGUISkin.GetStyle ("textfield").normal.background;

						//Barra de vida
						texture = new Texture2D (1, 1);
						texture.SetPixel (0, 0, Color.green);
						texture.Apply ();
						GUI.skin.box.normal.background = texture;
						GUI.Box (new Rect (Screen.width / 2 - EnemyHealthBarLength / 2, Screen.height - 45, EnemyHealthBarLength, 5), "");

						GUI.skin.box.normal.background = defaultGUISkin.GetStyle ("textfield").normal.background;


						if (lockedTargetTextureA != null) {
							
							targetScreenPos = playerCamera.WorldToScreenPoint (target.transform.position + target.transform.up * target.GetComponent<CapsuleCollider> ().height / 4f);
							Vector2 pivot = new Vector2 (targetScreenPos.x, Screen.height - targetScreenPos.y);

							GUIUtility.RotateAroundPivot (lockedTargetRotationA, pivot);
							GUI.color = new Color (1, 1, 1, 0.175f);
							GUI.DrawTexture (new Rect (targetScreenPos.x - lockedTargetTextureA.width / 2 * 0.2f, Screen.height - targetScreenPos.y - lockedTargetTextureA.height / 2 * 0.2f, lockedTargetTextureA.height * 0.2f, lockedTargetTextureA.width * 0.2f), lockedTargetTextureA);

							lockedTargetRotationA = lockedTargetRotationA - 4f;
							if (lockedTargetRotationA <= -360)
								lockedTargetRotationA = 0;

							GUIUtility.RotateAroundPivot (lockedTargetRotationB, pivot);
							GUI.color = new Color (1, 1, 1, 0.2f);
							GUI.DrawTexture (new Rect (targetScreenPos.x - lockedTargetTextureB.width / 2 * 0.2f, Screen.height - targetScreenPos.y - lockedTargetTextureB.height / 2 * 0.2f, lockedTargetTextureB.height * 0.2f, lockedTargetTextureB.width * 0.2f), lockedTargetTextureB);
							lockedTargetRotationB = lockedTargetRotationB + 6.5f;
							if (lockedTargetRotationB >= 360)
								lockedTargetRotationB = 0;
				
						}			
					}
				}	

				if (youWin)
					GUI.Box (new Rect (Screen.width / 2 - 80, 65, 160, 20), "YOU WIN!!!");
			}
		} else {
			_refreshRate += Time.deltaTime;
		}
	}
		
	
	/*
	void UpdateSettings() {

		target = references.currentAutoaimTarget;
		targetScreenPos = playerCamera.WorldToScreenPoint (target.transform.position);

		rect = new Rect (targetScreenPos.x - lockedTargetTextureA.width/2 * crosshairScale * 2f, Screen.height - targetScreenPos.y - lockedTargetTextureA.height/2 * crosshairScale * 2f, lockedTargetTextureA.height * crosshairScale * 2f, lockedTargetTextureA.width * crosshairScale * 2f);
		pivot = new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);
	}
	*/

	// Update is called once per frame
	void Update () {
		if (timerHit > 0f) {
			timerHit -= Time.deltaTime * 1.5f;
		}
	}

	public void TargetWasHit(){
		timerHit = timerHitBase;
	}
}
