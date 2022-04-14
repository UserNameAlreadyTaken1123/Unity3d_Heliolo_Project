using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;

public class AI_ComboBreaker : State<AI> {
	
	private bool coroPlaying;
	private AI_HealthBar healthBar;

	void Start(){
		healthBar = GetComponent<AI_HealthBar> ();
	}

	public override bool EnterState(AI _owner){
		_owner.currentState = this.ToString();
		return true;
	}

	public override void ExitState(AI _owner){
	}

	public override void UpdateState(AI _owner){
		if (!coroPlaying)
			_owner.stateMachine.ChangeState(_owner.GetComponent<AI_IdleState>());
	}


	public IEnumerator ComboBreaker (AI _owner){
		coroPlaying = true;
		_owner.stateMachine.ChangeState(GetComponent<AI_ComboBreaker>());
		healthBar.cantBeHitMode = true;
		_owner.references.animationScript.ResetValues ();
		_owner.references.animationScript.anim.Play ("ComboBreaker");
		_owner.references.rigidbody.velocity = Vector3.zero;
		_owner.references.rigidbody.AddForce (-transform.forward * 4f, ForceMode.VelocityChange);
		yield return new WaitForSeconds (2f);
		_owner.references.rigidbody.velocity = Vector3.zero;
		yield return new WaitForSeconds (0.65f);
		healthBar.cantBeHitMode = false;
		coroPlaying = false;
	}

	public override void ForceInterruption(AI _owner){
		if (currentCoroutine != null)
			StopCoroutine (currentCoroutine);
		_owner.coroRunning = false;
		coroPlaying = false;
		healthBar.cantBeHitMode = false;
	}
}
