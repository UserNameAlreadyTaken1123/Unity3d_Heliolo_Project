using UnityEngine;
using System.Collections;

public class DuncanButton : MonoBehaviour {
	private float glow;
	private GameObject lights;
	Vector3 v3Current;
	Quaternion mainAssembly;

	// Use this for initialization
	void Start () {
		glow = 0f;
		StopCoroutine ("Lighten");
		StopCoroutine ("Darken");
		lights = GameObject.Find ("lights");
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
		GameObject.Find ("DuncanGlow").GetComponent<Renderer> ().material.SetFloat("_MKGlowPower", glow);
		yield return new WaitForSeconds (0.05f);
	}

	IEnumerator Darken (){
		while (glow > 0) {
			glow = glow - 0.1f;
			GameObject.Find ("DuncanGlow").GetComponent<Renderer> ().material.SetFloat ("_MKGlowPower", glow);
			yield return new WaitForSeconds (0.05f);
		}
	}

	void OnMouseDown (){
		GameObject.Find ("Player").GetComponent<SelectedPlayer> ().currentPlayer = 2;
		StartCoroutine("RotateCamera");
		lights.SetActive (false);
	}

	public IEnumerator RotateCamera (){
			Camera.main.transform.rotation = Quaternion.identity;
			yield return new WaitForSeconds (0.05f);
			yield break;
	}
}
