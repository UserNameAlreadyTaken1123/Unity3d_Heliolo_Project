using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSpellCPU : MonoBehaviour {

	//public MagicSpellClass spellCasted;
	//private MagicSpellClass spellCastedPrevFrame;
	private bool waitingForSpell;

	public Coroutine runningCoroutine;
	public GameObject Player;

	public References references;
	private Hero_Movement movementScript;
	private HealthBar healthScript;

	// Use this for initialization
	void Awake () {
		if (Player == null)
			Player = transform.parent.transform.Find ("Player").gameObject;
	}

	void Start(){
		references = Player.GetComponent<References>();
		movementScript = Player.GetComponent<Hero_Movement> ();
		healthScript = Player.GetComponent<HealthBar> ();
	}

	public void StartSpell(MagicSpellClass spellCasted){
		if (!waitingForSpell && !movementScript.cantStab && !movementScript.cantShoot && !healthScript.isDead) {
			waitingForSpell = true;
			runningCoroutine = StartCoroutine (spellCasted.DoSomeMagic ());
		}		
	}

	public void ForcedReset(){
		foreach (MagicSpellClass spellScript in transform) {
			print ("there are spells found");
			spellScript.ForcedReset ();
		}
	}

	public void SpellTerminated(){
		ForcedReset ();
		waitingForSpell = false;
		runningCoroutine = null;
	}
}
