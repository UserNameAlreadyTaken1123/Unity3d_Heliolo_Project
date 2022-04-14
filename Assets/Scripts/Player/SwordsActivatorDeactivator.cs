using UnityEngine;
using System.Collections;

public class SwordsActivatorDeactivator : MonoBehaviour {

	public bool doingStuff;

	private GameObject Player;
	public GameObject currentSword;
	public GameObject previousSword;

	private Hero_Movement movementScript;

	private Animator anim;

	private string animationForSaving;
	private string animationForDrawing;

	private bool IKnowWhatToSave;
	private bool IKnowWhatToDraw;
	private bool alreadySaved;

	private bool alreadyExecuted1;
	private bool alreadyExecuted2;
	private bool alreadyExecuted3;
	private bool alreadyExecuted4;

//	public bool unableToAttack;

	// Use this for initialization
	void Start () {
	
		Player = this.gameObject;
		movementScript = Player.GetComponent<Hero_Movement> ();
		anim = Player.GetComponent<Animator> ();
		currentSword = Player.GetComponent<CycleSwords> ().currentSword;
		previousSword = Player.GetComponent<CycleSwords> ().previousSword;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

//**************************************************************
//	FOR SWORDS - FOR SWORDS - FOR SWORDS - FOR SWORDS - FOR SWORDS - 
//**************************************************************

	public void InstaDoSwords (){
		if (currentSword.GetComponent<NGS_NewCPU> ())
			GetComponent<Player_Animation> ().meleeWeaponType = currentSword.GetComponent<NGS_NewCPU> ().weaponType;
		else
			GetComponent<Player_Animation> ().meleeWeaponType = 0f;

		currentSword = Player.GetComponent<CycleSwords> ().currentSword;
		previousSword = Player.GetComponent<CycleSwords> ().previousSword;
		previousSword.SetActive (false);
		currentSword.SetActive (true);
	}

	public void DoSwords (){
		currentSword = Player.GetComponent<CycleSwords> ().currentSword;
		previousSword = Player.GetComponent<CycleSwords> ().previousSword;

		StartAskingQuestionsSave ();
		StartAskingQuestionsDraw ();

		if (IKnowWhatToSave && !alreadySaved && !doingStuff) {
			anim.Play (animationForSaving, 3, normalizedTime: 0.0f);
			anim.Play (animationForSaving, 5, normalizedTime: 0.0f);

			StartCoroutine (Saving());
		}

	}

	void StartAskingQuestionsSave(){
		if (previousSword.tag == "EquipedWeaponSword") {
			animationForSaving = "Sword Saving";
		} else if (previousSword.tag == "Fists") {
			animationForSaving = "Fists Saving";
		}
		else {
			animationForSaving = null;
		}
		IKnowWhatToSave = true;
	}

	void StartAskingQuestionsDraw(){
		if (currentSword.tag == "EquipedWeaponSword") {
			animationForDrawing = "Sword Drawing";
		} else if (currentSword.tag == "Fists") {
			animationForDrawing = "Fists Drawing";
		}
		else {
			animationForDrawing = null;
		}
		IKnowWhatToDraw = true;
	}

	IEnumerator Saving(){
		yield return null;
		if (!alreadyExecuted1) {
			anim.SetLayerWeight (3, 1.0f);
			anim.SetLayerWeight (5, 1.0f);
			doingStuff = true;
		}

		//si tengo que guardar "puños", desenfundar arma y luego de 0.3 segundos habilitar todo.
		if (animationForSaving == "Fists Saving") {
			if (!alreadyExecuted1 && animationForDrawing != "Fists Drawing") {
				anim.Play (animationForDrawing, 3, normalizedTime: 0.0f);
				anim.Play (animationForDrawing, 5, normalizedTime: 0.0f);
			} else if (!alreadyExecuted1) {
				anim.Play ("Idle", -1, normalizedTime: 0.0f);
			}

			//aplicar lo que se deba aplicar una sola vez!
			if (!alreadyExecuted1) {
				yield return new WaitForSeconds (0.2f);
				alreadyExecuted1 = true;
				currentSword.SetActive (true);

				alreadySaved = false;
				IKnowWhatToSave = false;
				IKnowWhatToDraw = false;
				movementScript.cantStab = false;
			}
			//Esta animación terminó? si Sí, terminar, sino, reiniciar.
			if (anim.GetCurrentAnimatorStateInfo (3).normalizedTime > 1.0f /*&& !anim.IsInTransition (3)*/) {
				alreadyExecuted1 = false;
				alreadyExecuted2 = false;
				alreadyExecuted3 = false;
				alreadyExecuted4 = false;
				while (anim.GetLayerWeight (3) > 0.0f) {
					anim.SetLayerWeight (3, anim.GetLayerWeight (3) - 0.1f);
					anim.SetLayerWeight (5, anim.GetLayerWeight (5) - 0.1f);
					yield return null;
				}
				doingStuff = false;
			} else {
				StartCoroutine (Saving ());
			}
			yield break;
		}

		//Si tengo un arma en la mano y debo guardarla primero, entonces guardarla y despues sacar la nueva.
		if (anim.GetCurrentAnimatorStateInfo (3).normalizedTime > 0.5f /*&& !anim.IsInTransition (3)*/) {
			//desactivar una sola vez el arma vieja.
			if (!alreadyExecuted2) {
				alreadyExecuted2 = true;

				if (currentSword.GetComponent<NGS_NewCPU> ())
					GetComponent<Player_Animation> ().meleeWeaponType = currentSword.GetComponent<NGS_NewCPU> ().weaponType;
				else
					GetComponent<Player_Animation> ().meleeWeaponType = 0f;

				previousSword.SetActive (false);
				movementScript.cantStab = true;	
			}

			//desenfundar la nueva.
			if (animationForDrawing != "Fists Drawing" && !alreadyExecuted4) {
				alreadyExecuted4 = true;
				anim.Play (animationForDrawing, 3, normalizedTime: 0.0f);
				anim.Play (animationForDrawing, 5, normalizedTime: 0.0f);
			} else {
				alreadyExecuted3 = true;
				currentSword.SetActive (true);
				alreadySaved = false;
				IKnowWhatToSave = false;
				IKnowWhatToDraw = false;
				movementScript.cantStab = false;
			}

			//activar la nueva una sola vez.
			yield return null;
			if (!alreadyExecuted3 && anim.GetCurrentAnimatorStateInfo (3).normalizedTime > 0.5f /*&& !anim.IsInTransition (3)*/) {
				alreadyExecuted3 = true;
				currentSword.SetActive (true);

				alreadySaved = false;
				IKnowWhatToSave = false;
				IKnowWhatToDraw = false;
				movementScript.cantStab = false;
			}
			//Esta animación terminó? si Sí, terminar, sino, reiniciar.
			if (anim.GetCurrentAnimatorStateInfo (3).normalizedTime > 1.0f /*&& !anim.IsInTransition (3)*/) {
				alreadyExecuted1 = false;
				alreadyExecuted2 = false;
				alreadyExecuted3 = false;
				alreadyExecuted4 = false;
				anim.SetLayerWeight (3, 0.0f);
				anim.SetLayerWeight (5, 0.0f);
				doingStuff = false;
			} else {
				StartCoroutine (Saving ());
			}
			yield break;

		} else {
			StartCoroutine (Saving ());
			yield break;
		}

//		}
	/* else {
			anim.Play (animationForSaving, -1, normalizedTime: 0.0f);
			yield return new WaitForEndOfFrame ();
			StartCoroutine (Saving ());
			yield break;
		}
		*/

	}

}


