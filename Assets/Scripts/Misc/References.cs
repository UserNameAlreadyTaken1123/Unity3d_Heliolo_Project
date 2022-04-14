using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Luminosity.IO;

public class References : MonoBehaviour {

	public bool isAI;

	private RaycastHit hit;
	public GameObject Camera;
	public Player_Animation animationScript;
	public EnemiesDetector enemiesDetector;

	public GameObject Player;
	public GameObject currentAutoaimTarget;
	public GameObject currentAutoaimTargetLastLong;
	public GameObject Inventory;
	public GameObject AutoAim;
	public GameObject OverShoulderCameraPos;
	public GameObject DistortedAura;
	[HideInInspector] public ParticleSystem _DistortedAuraPS;
	[HideInInspector] public Renderer _renderer;

	[HideInInspector] public float _Mass;

	public GameObject Head;
	public GameObject RightHand;
	public GameObject LeftHand;
	public GameObject Hips;
	public GameObject Spine;
	public GameObject Chest;
	public GameObject LeftShoulder;
	public GameObject RightShoulder;
	public GameObject RightFoot;
	public GameObject LeftFoot;

	public GameObject RightHandWeapon;
	public GameObject LeftHandWeapon;

	public GameObject AttackFlash;
	private GameObject _AttackFlash;

	public bool triggered = false;
	public bool triggering = false;
	public bool frameCancel = false;
	private float frameCancelTimer;

	public GameObject landingParticlesFX;
	private GameObject landingParticlesFXGO;
	public GameObject DashParticlesFX;

	private float prevVelocity;
	private bool strongLanding;

	public Renderer renderer;
	public Rigidbody rigidbody;
	public Collider collider;
	public CapsuleCollider capsuleCollider;
	[HideInInspector] public float capsuleColliderHeight;
	[HideInInspector] public float capsuleColliderRadius;
	[HideInInspector] public Vector3 capsuleColliderCenter;

	public AudioClip[] stepSoundsConcrete;
	public AudioClip landingSound;
	public GameObject audioSourceGO;
	private AudioSource audioSource;

	public float damageMultiplier;
	[Header("Teams max. value: 4")]
	public int team = 2;

	//private bool delayedStart = false;

	// Use this for initialization
	void Start () {	

		if (!GetComponent<Renderer> ()) {
			gameObject.AddComponent <MeshRenderer> ();
			Material dummyMat = new Material (Shader.Find ("Legacy Shaders/Diffuse"));
			dummyMat.color = new Color (0, 0, 0, 1f);
			gameObject.GetComponent<MeshRenderer> ().material = dummyMat;
		}
		renderer = GetComponent<Renderer> ();

		if (GetComponent<Hero_Movement>())
			isAI = false;
		else
			isAI = true;

		if (!isAI && Inventory && Inventory.GetComponent<InventoryDisplay> ())
			Inventory.GetComponent<InventoryDisplay> ().owner = gameObject;

		if (AttackFlash != null)
			_AttackFlash = Instantiate (AttackFlash, transform);

		Player = gameObject;
		rigidbody = Player.GetComponent<Rigidbody> ();
		collider = Player.GetComponent<Collider> ();
		capsuleCollider = Player.GetComponent<CapsuleCollider> ();
		capsuleColliderCenter = capsuleCollider.center;
		capsuleColliderHeight = capsuleCollider.height;
		capsuleColliderRadius = capsuleCollider.radius;

		_renderer = GetComponent<Renderer>();
		_Mass = rigidbody.mass;

		if (Hips != null) {
			Spine = Hips.transform.Find ("spine").gameObject;
			Chest = Spine.transform.Find ("chest").gameObject;
			LeftShoulder = Chest.transform.Find ("shoulder.L").gameObject;
			RightShoulder = Chest.transform.Find ("shoulder.R").gameObject;
		}

		landingParticlesFXGO = Instantiate (landingParticlesFX, transform.position + Vector3.down * 100f, landingParticlesFX.transform.rotation);

		audioSourceGO.transform.parent = transform;
		audioSourceGO.name = "Step Sounds Manager";
		audioSource = audioSourceGO.GetComponent<AudioSource> ();
		audioSource.rolloffMode = AudioRolloffMode.Linear;

		animationScript = Player.GetComponent<Player_Animation> ();

		if (team > 5)
			team = 4;

		frameCancelTimer = Time.fixedDeltaTime * 1f;
	}

/*	
	// Update is called once per frame
	void Update () {	
	}
*/

	void LateUpdate(){
		//Landing Particles and, in a near future, landing sounds;
		if (strongLanding)
			Physics.Raycast (transform.position, Vector3.down, out hit, collider.bounds.extents.y + 1.0f, LayerMask.GetMask ("Default", "Scenario"), QueryTriggerInteraction.Ignore);

		if (rigidbody) {
			if (rigidbody.velocity.y < -10f) {
				strongLanding = true;
			} else if (strongLanding && rigidbody.velocity.y > -1f && hit.collider) {
				strongLanding = false;
				Landing ();
			} else
				strongLanding = false;
		}

		if (!isAI && !InputManager.GetButton ("Combo Mode")) {
			currentAutoaimTarget = null;
			currentAutoaimTargetLastLong = null;
		}

		if (frameCancel && frameCancelTimer <= 0f) {
			frameCancel = false;
			frameCancelTimer = Time.fixedDeltaTime * 1f;
		} else if (frameCancel && frameCancelTimer > 0f) {
			frameCancelTimer -= Time.deltaTime;
		}
	}
		
	public void ToggleSwordDamage(int activated){
		if (RightHandWeapon != null && RightHandWeapon.GetComponent<NGS_NewCPU> ()) {
			RightHandWeapon.GetComponent<NGS_NewCPU> ().ToggleSwordDamage (activated);
		}
	}

	public void ParentTargetToSword(int activated){
		if (RightHandWeapon != null && RightHandWeapon.GetComponent<NGS_NewCPU> () && !RightHandWeapon.GetComponent<NGS_NewCPU> ().ownerIsAI) {
			RightHandWeapon.GetComponent<NGS_NewCPU> ().currentAttack.ParentTargetToSword (activated);
		}
	}

	/*
	public void ColliderAirLauncherAttackA(){
		if (RightHandWeapon != null && RightHandWeapon.GetComponent<NGS_NewCPU> () && !RightHandWeapon.GetComponent<NGS_NewCPU> ().ownerIsAI) {
			//RightHandWeapon.GetComponent<NGS_NewCPU> ().ColliderAirLauncherAttackA();
		}
	}
	*/

	public void SwordAttackCancelation (){
		if (RightHandWeapon != null && RightHandWeapon.GetComponent<NGS_NewCPU> () && !RightHandWeapon.GetComponent<NGS_NewCPU> ().ownerIsAI && RightHandWeapon.GetComponent<NGS_NewCPU> ().canCancel) {
			RightHandWeapon.GetComponent<NGS_NewCPU> ().AttackCancel ();
			if (_AttackFlash != null)
				_AttackFlash.GetComponent<ParticleSystem> ().Play();
		}
	}

	public void CallMethodOnCurrentSwordAttack (string methodName){
		if (RightHandWeapon != null && RightHandWeapon.GetComponent<NGS_NewCPU> () && !RightHandWeapon.GetComponent<NGS_NewCPU> ().ownerIsAI) {
			RightHandWeapon.GetComponent<NGS_NewCPU> ().ExecuteMethodAttack (methodName);
		}
	}

	public void Landing(){
		if (!strongLanding)			
			Physics.Raycast (transform.position, Vector3.down, out hit, collider.bounds.extents.y + 1.0f, LayerMask.GetMask ("Default", "Scenario"), QueryTriggerInteraction.Ignore);
		if (landingParticlesFX && hit.collider != null) {
			if (landingParticlesFXGO == null) {
				landingParticlesFXGO = Instantiate (landingParticlesFX, hit.point, landingParticlesFX.transform.rotation);
			} else {
				landingParticlesFXGO.transform.position = hit.point;
				landingParticlesFXGO.GetComponent<ParticleSystem> ().Play ();
			}
		}
	}

	public void StepSound(){
		if (/*!audioSource.isPlaying &&*/ animationScript.displacementSpeed >= 1.5f && rigidbody.velocity.magnitude > 0.1f) {
			if (Physics.Raycast (transform.position, Vector3.down, collider.bounds.extents.y + 0.1f, LayerMask.GetMask ("Default", "Scenario"))) {
				audioSource.transform.position = transform.position;
				if (animationScript.displacementSpeed > 2.5f) {
					audioSource.volume = 0.15f;
					audioSource.minDistance = 1.5f;
					audioSource.maxDistance = 40f;
				} else {
					audioSource.volume = 0.1f;
					audioSource.minDistance = 1f;
					audioSource.maxDistance = 35f;
				}
				audioSource.pitch = 1f;
				audioSource.transform.parent = transform;
				audioSource.clip = stepSoundsConcrete [Random.Range (0, stepSoundsConcrete.Length - 1)];
				if (GetComponent<Hero_Movement>() && GetComponent<Hero_Movement>().speed == GetComponent<Hero_Movement>().sprintSpeed) 
					CameraShake.Shake (0.1f, 3.5f / ((Camera.transform.position - transform.position).sqrMagnitude * 3f));				
				audioSource.Play ();
			}
		}
	}

	public void LandingSound(){
		if (Physics.Raycast (transform.position, Vector3.down, collider.bounds.extents.y + 0.5f, LayerMask.GetMask ("Default", "Scenario"))) {
			audioSource.transform.position = transform.position;
			audioSource.volume = 0.125f;
			audioSource.pitch = 0.8f;
			audioSource.minDistance = 2f;
			audioSource.maxDistance = 40f;

			audioSource.clip = landingSound;
			audioSource.Play ();
		}
	}
}
