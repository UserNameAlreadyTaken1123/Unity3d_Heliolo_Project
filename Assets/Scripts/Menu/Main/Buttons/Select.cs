using UnityEngine;
using System.Collections;

public class Select : MonoBehaviour {
	private float glow;
	public int scene;

	// Use this for initialization
	void Start () {
		glow = 0f;
		scene = 1;
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
		GameObject.Find ("SelectGlow").GetComponent<Renderer> ().material.SetFloat("_MKGlowPower", glow);
		yield return new WaitForSeconds (0.05f);
	}

	IEnumerator Darken (){
		while (glow > 0) {
			glow = glow - 0.1f;
			GameObject.Find ("SelectGlow").GetComponent<Renderer> ().material.SetFloat ("_MKGlowPower", glow);
			yield return new WaitForSeconds (0.05f);
		}
	}

	void OnMouseDown (){
		scene = scene + 1;
		if (scene > 6)
			scene = 1;
	}

}
