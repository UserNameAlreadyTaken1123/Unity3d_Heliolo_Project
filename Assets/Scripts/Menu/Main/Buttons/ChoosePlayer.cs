using UnityEngine;
using System.Collections;

public class ChoosePlayer : MonoBehaviour {
	private float glow;
	private GameObject lights;

	// Use this for initialization
	void Start () {
		lights = GameObject.Find ("lights");
		glow = 0f;
		StopCoroutine ("Lighten");
		StopCoroutine ("Darken");
		lights.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {	
	}

	void OnMouseOver () {
		if (glow < 0.5f) {
			StartCoroutine ("Lighten");
			StopCoroutine ("Darken");
		}
	}

	void OnMouseExit (){
		StartCoroutine("Darken");
		StopCoroutine ("Lighten");
	}

	IEnumerator Lighten (){
		glow = glow + 0.1f;
		GameObject.Find ("PlayerGlow").GetComponent<Renderer> ().material.SetFloat("_MKGlowPower", glow);
		yield return new WaitForSeconds (0.05f);
	}

	IEnumerator Darken (){
		while (glow > 0) {
			glow = glow - 0.1f;
			GameObject.Find ("PlayerGlow").GetComponent<Renderer> ().material.SetFloat ("_MKGlowPower", glow);
			yield return new WaitForSeconds (0.05f);
		}
	}

	void OnMouseDown (){
		lights.SetActive (true);
		StartCoroutine("RotateCamera");
	}

	public IEnumerator RotateCamera (){
		Camera.main.transform.eulerAngles = new Vector3 (0f, -90f, 0f);
		yield break;
	}
}
