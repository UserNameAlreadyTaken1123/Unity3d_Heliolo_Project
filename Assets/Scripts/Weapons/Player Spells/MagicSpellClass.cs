using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSpellClass : MonoBehaviour {

	public GameObject Player;

	// Use this for initialization
	void Start () {
		
	}

	void ResetComboStateTimer(bool resetTimer){
	}

	public virtual void ForcedReset(){
	}

	public virtual IEnumerator DoSomeMagic(){
		yield return null;	
	}
}
