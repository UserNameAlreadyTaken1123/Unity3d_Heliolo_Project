using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour {
	private float glow;
	private int loadScene;
	private int loadPlayer;

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
		GameObject.Find ("StartGlow").GetComponent<Renderer> ().material.SetFloat("_MKGlowPower", glow);
		yield return new WaitForSeconds (0.05f);
	}

	IEnumerator Darken (){
		while (glow > 0) {
			glow = glow - 0.1f;
			GameObject.Find ("StartGlow").GetComponent<Renderer> ().material.SetFloat ("_MKGlowPower", glow);
			yield return new WaitForSeconds (0.05f);
		}
	}

	void OnMouseDown (){
		loadScene = GameObject.Find ("Select").GetComponent<Select>().scene;
		loadPlayer = GameObject.Find ("Player").GetComponent<SelectedPlayer> ().currentPlayer;
		switch (loadScene) {

		case 1:			
			SceneManager.LoadScene ("Tutoro");
			break;

		case 2:			
			SceneManager.LoadScene ("CampoVerde");
			break;

		case 3:			
			SceneManager.LoadScene ("ProtoJungle");
			break;

		case 4:
			SceneManager.LoadScene ("TreasureChestTemple");
			break;
		case 5:
			SceneManager.LoadScene ("iceworld");
			break;
		case 6:
			SceneManager.LoadScene ("cs_assault");
			break;
		}
	}
}
