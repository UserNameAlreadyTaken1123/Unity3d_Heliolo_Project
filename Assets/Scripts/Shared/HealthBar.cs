using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour {

	[HideInInspector]
	public new Rigidbody rigidbody;
	[HideInInspector]
	public new Transform transform;
	[HideInInspector]
	public new References references;
	[HideInInspector]
	public Player_Animation animationScript;
	[HideInInspector]
	public new Collider collider;

	public float Maxhealth = 100;
	public float CurHealth = 100;
	public int painChance = 100;
	public float painTimer = 0.5f;
	public float maxFocus = 5;
	public float curFocus = 5;
	public float healAmountPerSecond = 1;
	public float standUpDelay = 0.5f;
	public bool cheatGodMode;
	//[HideInInspector]
	public bool godMode, cantBeHitMode;
	public bool isDead;
	public bool inPain;
    private bool _inpain;

	public bool justGotHurt = false;
	private int countToRestartJustGotHurt = 1;


	[HideInInspector]
	public GameObject BloodSplatObject;
	public GameObject BloodSplatObjectBullet;
	public GameObject BloodSplatReference;
	public Color SwordSparkColor;
	public GameObject SwordSpark;

	private Coroutine coroCantBeHitMode;

	public virtual bool AddjustCurrentHealth(Transform attacker, float adj, float stunTime, bool unstoppable, bool overthrows) {
		return false;
	} 

	public virtual bool BulletDamage(Vector3 hitPoint, Transform shooter, Transform bullet, float adj) {
		return false;
	}

	public virtual IEnumerator SwordCrash(Transform other){
		yield return null;
	}

	public virtual void ResetValues(){
	}

    private void FixedUpdate() {
		/*
		countToRestartJustGotHurt += 1;
		if (countToRestartJustGotHurt > 1) {
			countToRestartJustGotHurt = 0;
			justGotHurt = false;
		}
		*/
	}

	private void LateUpdate() {
		inPain = false;
		justGotHurt = false;
	}

	public void CantBeHitMode(float duration){
		if (coroCantBeHitMode == null)
			coroCantBeHitMode = StartCoroutine (CoroCantBeHitMode (duration));
		else{
			cantBeHitMode = false;
			StopCoroutine (coroCantBeHitMode);
			coroCantBeHitMode = StartCoroutine (CoroCantBeHitMode (duration));
		}
	}

	public virtual IEnumerator CoroCantBeHitMode(float duration){
		cantBeHitMode = true;
		while (duration > 0) {
			cantBeHitMode = true;
			duration -= Time.deltaTime;
			yield return new WaitForEndOfFrame ();
		}
		cantBeHitMode = false;
	}
}

