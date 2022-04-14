using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine;

public class ButtonClass : MonoBehaviour{

	[Header("Activated Button Action")]
	public UnityEvent myUnityEventA;
	[Header("Highlighted Button Action")]
	public UnityEvent myUnityEventB;

	public bool canCycleOptions;
	public bool thisOneHighlighted;

	public List<GameObject> optionsToCycle = new List<GameObject>();
	public int optionIndex;

	public bool usesCustomColors = false;
	public Color customSelected = new Vector4 (1.0f, 1.0f, 1.0f, 1f);
	public Color customUnselected = new Vector4 (0.6f, 0.6f, 0.6f, 1f);

	// Use this for initialization
	public virtual void Start(){
		if (canCycleOptions) {
			optionsToCycle.Clear ();
			foreach (Transform child in transform) {
				optionsToCycle.Add (child.gameObject);
			}
			ScrollButtonOptions (0);
		}			
	}

	public virtual void ScrollButtonOptions(int scroll){
		optionIndex = optionIndex + scroll;
		if (optionIndex > optionsToCycle.Count - 1) {
			optionIndex = 0;
		}
		if (optionIndex < 0) {
			optionIndex = optionsToCycle.Count - 1;
		}
		foreach (GameObject option in optionsToCycle) {
			if (optionsToCycle.IndexOf (option) == optionIndex) {
				option.SetActive(true);
			} else {
				option.SetActive(false);
			}
		}
	}

	public virtual void OnEnable(){
		Start ();
	}


	void OnMouseEnter(){
		transform.parent.GetComponent<NewMenuButtonScroller> ().GameobjectWhereMouseEntered (this.gameObject);
	}

	/*
	void OnMouseUp() {
		transform.parent.GetComponent<NewMenuButtonScroller>().hasSelected = true;
	}
	*/

	public void PerformButtonAction (){
		myUnityEventA.Invoke();
	}

	public virtual void CallDeactivation(){
		gameObject.SetActive (false);
	}

	public virtual void PerformHighlightedAction(){
		myUnityEventB.Invoke();
	}
}
