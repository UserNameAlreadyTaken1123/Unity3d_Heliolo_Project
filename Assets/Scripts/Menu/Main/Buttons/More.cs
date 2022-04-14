using UnityEngine;
using System.Collections;

public class More : MonoBehaviour {
	private float glow;

	// Use this for initialization
	void Start () {
		glow = 0f;
		StopCoroutine ("Lighten");
		StopCoroutine ("Darken");
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
		GameObject.Find ("MoreGlow").GetComponent<Renderer> ().material.SetFloat("_MKGlowPower", glow);
		yield return new WaitForSeconds (0.05f);
	}

	IEnumerator Darken (){
		while (glow > 0) {
			glow = glow - 0.1f;
			GameObject.Find ("MoreGlow").GetComponent<Renderer> ().material.SetFloat ("_MKGlowPower", glow);
			yield return new WaitForSeconds (0.05f);
		}
	}

	void OnMouseDown (){

		StartCoroutine("RotateCamera");
	}

	public IEnumerator RotateCamera (){
		Camera.main.transform.eulerAngles = new Vector3 (0f, 90f, 0f);
		yield break;
	}
}
