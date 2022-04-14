using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class NGS_AttackClass : MonoBehaviour {

	[HideInInspector] public NGS_NewCPU swordCPU;
	[HideInInspector] public Hero_Movement movementScript;
	[HideInInspector] public Player_Animation playerAnimation;
	[HideInInspector] public Player_HealthBar playerHealthBarScript;
	[HideInInspector] public ManaBar manaBarScript;

	[HideInInspector] public Animator animator;
	[HideInInspector] public Rigidbody rigidbody;
	[HideInInspector] public BoxCollider boxCollider;

	[HideInInspector] public MeleeWeaponTrail meleeWeaponTrail;
	[HideInInspector] public MeleeWeaponTrailDistortion meleeWeaponTrailDistortion;

	[HideInInspector] public int priority;
	[HideInInspector] public float animationTimer, SpeedMultiplier;
	[HideInInspector] public GameObject particlesRunning;

	[HideInInspector] public GameObject Player;

	public float cooldown = 0f;
	public float stunTime = 0.3f;
	public bool unstoppable = false;
	public bool overthrows = false;


	public AudioClip startSound;
	public AudioClip hitSound;	

	public bool resetOnLanding = true;
	private bool initialized = false;

	// Use this for initialization
	public virtual void Start () {
		Player = GetComponent<NGS_NewCPU> ().Player;
		swordCPU = GetComponent<NGS_NewCPU> ();
		movementScript = Player.GetComponent<Hero_Movement> ();
		playerAnimation = Player.GetComponent<Player_Animation> ();
		playerHealthBarScript = Player.GetComponent<Player_HealthBar> ();
		manaBarScript = Player.GetComponent<ManaBar> ();

		animator = Player.GetComponent<Animator> ();
		rigidbody = Player.GetComponent<Rigidbody> ();
		meleeWeaponTrail = transform.parent.GetComponent<MeleeWeaponTrail> ();
		meleeWeaponTrailDistortion = transform.parent.GetComponent<MeleeWeaponTrailDistortion> ();
		boxCollider = GetComponent<BoxCollider> ();
		initialized = true;
	}
	
	// Update is called once per frame
	void Update () {
	}

	public virtual void ForcedReset(){
	}

	public virtual void OnEnable(){
		if (initialized)
			ForcedReset ();
	}

	public virtual IEnumerator ExecuteAttack (){
		swordCPU.AttackDone ();
		yield return null;
	}

	public void CheckCurrentAnimatorState (){
		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Grounded Melee Attacks")) {
			float normalizedTime = animator.GetCurrentAnimatorStateInfo (0).normalizedTime;
			animator.Play ("Grounded Melee Attacks", -1, 0f);
			animator.Play ("Grounded Melee Attacks 0", -1, normalizedTime);
		} else if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Air Melee Attacks")) {
			float normalizedTime = animator.GetCurrentAnimatorStateInfo (0).normalizedTime;
			animator.Play ("Air Melee Attacks", -1, 0f);
			animator.Play ("Air Melee Attacks 0", -1, normalizedTime);
		}
	}
		
	public bool TerminateAnimation (){
		if (movementScript.isFalling || swordCPU.attacksQueue.Count > 1 ||
		     InputManager.GetButton ("Melee") || InputManager.GetButton ("Fire") || InputManager.GetButton ("R2") || InputManager.GetButton ("Aim") ||
		     InputManager.GetButton ("Launcher") || InputManager.GetButton ("Jump") ||
		     InputManager.GetAxis ("Horizontal") < -0.5f || InputManager.GetAxis ("Horizontal") > 0.5f || InputManager.GetAxis ("Vertical") < -0.5f || InputManager.GetAxis ("Vertical") > 0.5f)
			return false;
		else
			return true;
	}

	public virtual void ParentTargetToSword(int activated){
	}

	public virtual bool SpecialOntriggerenter(Collider collider){
		return false;
	}
}
