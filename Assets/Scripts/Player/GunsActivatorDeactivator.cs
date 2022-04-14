using UnityEngine;
using System.Collections;

public class GunsActivatorDeactivator : MonoBehaviour {

	public bool doingStuff;

	private GameObject Player;
	public GameObject currentGun;
	public GameObject previousGun;

	private Hero_Movement movementScript;
	private Player_Animation animationScript;
	private CycleGuns cycleGunsScript;

	private Animator anim;

	private string animationForSaving;
	private string animationForDrawing;

	private bool IKnowWhatToSave;
	private bool IKnowWhatToDraw;
	private bool alreadySaved;

	public bool saving;
	public bool drawing;

	private bool alreadyExecuted1;
	private bool alreadyExecuted2;
	private bool alreadyExecuted3;

//	public bool unableToFire;

	// Use this for initialization
	void Start () {
	
		Player = this.gameObject;
		movementScript = Player.GetComponent<Hero_Movement> ();
		animationScript = Player.GetComponent<Player_Animation> ();
		anim = Player.GetComponent<Animator> ();
		cycleGunsScript = Player.GetComponent<CycleGuns> (); 
		currentGun = Player.GetComponent<CycleGuns> ().currentGun;
		previousGun = Player.GetComponent<CycleGuns> ().previousGun;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

//**************************************************************
//	FOR GUNS - FOR GUNS - FOR GUNS - FOR GUNS - FOR GUNS - 
//**************************************************************

	public void InstaDoGuns (){
		if (currentGun.GetComponent<GenericGun> ())
			GetComponent<Player_Animation> ().gunWeaponType = currentGun.GetComponent<GenericGun> ().weaponType;
		else
			GetComponent<Player_Animation> ().gunWeaponType = 0f;

		currentGun = Player.GetComponent<CycleGuns> ().currentGun;
		previousGun = Player.GetComponent<CycleGuns> ().previousGun;
		previousGun.SetActive (false);
		previousGun.SetActive (true);
	}

	public void DoGuns (){
		currentGun = Player.GetComponent<CycleGuns> ().currentGun;
		previousGun = Player.GetComponent<CycleGuns> ().previousGun;

		StartAskingQuestionsSave ();
		StartAskingQuestionsDraw ();

		if (IKnowWhatToSave && !alreadySaved && !doingStuff) {
			anim.Play (animationForSaving, 1, normalizedTime: 0.0f);
			anim.Play (animationForSaving, 5, normalizedTime: 0.0f);
			animationScript.gunSlot = cycleGunsScript.gunsCarried.IndexOf (previousGun);
			if (previousGun.GetComponent<GenericGun> () && previousGun.GetComponent<GenericGun> ().weaponType > 2)
				anim.Play (animationForSaving, 3, normalizedTime: 0.0f);
			StartCoroutine (Saving());
			animationScript.isSaving = true;
			saving = true;
		}
	}

	void StartAskingQuestionsSave(){
		if (previousGun.tag == "EquipedWeaponGun") {
			animationForSaving = "Firearm Saving";
		} else if (previousGun.tag == "Fists") {
			animationForSaving = "Fists Saving";
		}
		else {
			animationForSaving = null;
		}
		IKnowWhatToSave = true;
	}

	void StartAskingQuestionsDraw(){
		if (currentGun.tag == "EquipedWeaponGun") {
			animationForDrawing = "Firearm Drawing";
		} else if (currentGun.tag == "Fists") {
			animationForDrawing = "Fists Drawing";
		}
		else {
			animationForDrawing = null;
		}
		IKnowWhatToDraw = true;
	}

	IEnumerator Saving(){
//		Player.GetComponent<Player_Animation> ().rise = false;
		yield return null;
		if (!alreadyExecuted1) {
			anim.SetLayerWeight (1, 1.0f);
			anim.SetLayerWeight (5, 1.0f);
			if (previousGun.GetComponent<GenericGun> () && previousGun.GetComponent<GenericGun> ().weaponType > 2)
				anim.SetLayerWeight (3, 1.0f);		
			doingStuff = true;
		}

		if (anim.GetCurrentAnimatorStateInfo (1).normalizedTime > 0.35 && !anim.IsInTransition (1)) {
			if (!alreadyExecuted1) {
				alreadyExecuted1 = true;
				yield return new WaitForSeconds (0.2f);
				previousGun.SetActive (false);
				movementScript.cantShoot = true;
			}
		
			if (anim.GetCurrentAnimatorStateInfo (1).normalizedTime > 0.35f && !anim.IsInTransition (1)) {
				if (!alreadyExecuted2) {
					
					if (currentGun.GetComponent<GenericGun> ())
						GetComponent<Player_Animation> ().gunWeaponType = currentGun.GetComponent<GenericGun> ().weaponType;
					else
						GetComponent<Player_Animation> ().gunWeaponType = 0f;
					
					alreadyExecuted2 = true;
					anim.Play (animationForDrawing, 1, normalizedTime: 0.0f);
					anim.Play (animationForDrawing, 5, normalizedTime: 0.0f);
					animationScript.gunSlot = cycleGunsScript.gunsCarried.IndexOf (currentGun);
					if (currentGun.GetComponent<GenericGun> () && currentGun.GetComponent<GenericGun> ().weaponType > 2) {
						anim.Play (animationForDrawing, 3, normalizedTime: 0.0f);
						anim.SetLayerWeight (1, 1.0f);
						anim.SetLayerWeight (3, 1.0f);
						anim.SetLayerWeight (5, 1.0f);
					}
					saving = false;
					drawing = true;
					yield return new WaitForSeconds (0.1f);
					if (currentGun) //Porque de vez en cuando tira error
						currentGun.SetActive (true);
					alreadySaved = false;
					IKnowWhatToSave = false;
					IKnowWhatToDraw = false;

					yield return new WaitForSeconds (0.1f);
					movementScript.cantShoot = false;
				}
				while (alreadyExecuted2 && anim.GetLayerWeight (1) > 0.0f) {
					yield return null;
					anim.SetLayerWeight (1, anim.GetLayerWeight (1) - 0.125f);
					anim.SetLayerWeight (3, anim.GetLayerWeight (3) - 0.125f);
					anim.SetLayerWeight (5, anim.GetLayerWeight (5) - 0.125f);
					//if (previousGun.GetComponent<GenericGun> () && previousGun.GetComponent<GenericGun> ().weaponType > 2)
					//	anim.SetLayerWeight (3, anim.GetLayerWeight (5) - 0.125f);
				}
				animationScript.isSaving = false;
				drawing = false;
				doingStuff = false;
				alreadyExecuted1 = false;
				alreadyExecuted2 = false;
				alreadyExecuted3 = false;
				yield break;
			}
			else {
				StartCoroutine (Saving ());
				yield break;
			}

		} else {
			StartCoroutine (Saving ());
			yield break;
		}

	}

}


