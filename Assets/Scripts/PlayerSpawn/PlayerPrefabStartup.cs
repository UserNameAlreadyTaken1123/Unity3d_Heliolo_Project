using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerPrefabStartup : MonoBehaviour {

	public bool isPlayer = true;

	public GameObject Helio;
	public GameObject Duncan;

	public string selectedPlayer;
	private GameObject currentHero;

	void Awake () {
		if (isPlayer) {
			selectedPlayer = PlayerPrefs.GetString ("NewPlayer");
			if (selectedPlayer == "Helio") {			
				Helio.SetActive (true);	
				currentHero = Helio.transform.FindChildIncludingDeactivated ("Player").gameObject;
				foreach (Transform heroSpawner in transform) {
					if (heroSpawner.gameObject != Helio)
						Destroy (heroSpawner.gameObject);
				}
			} else if (selectedPlayer == "Duncan") {
				Duncan.SetActive (true);	
				currentHero = Duncan.transform.FindChildIncludingDeactivated ("Player").gameObject;
				foreach (Transform heroSpawner in transform) {
					if (heroSpawner.gameObject != Duncan)
						Destroy (heroSpawner.gameObject);
				}
			} else {
				selectedPlayer = "Helio";
				Helio.SetActive (true);	
				currentHero = Helio.transform.FindChildIncludingDeactivated ("Player").gameObject;
				foreach (Transform heroSpawner in transform) {
					if (heroSpawner.gameObject != Helio)
						Destroy (heroSpawner.gameObject);
				}
			}

			GetComponent<PlayerSpawn> ().Player = currentHero.transform.FindChildIncludingDeactivated ("Player").gameObject;

/*		//wich player let survive :P
		switch (selectedPlayer) {
		case 0:
			Helio.SetActive (true);	
			Destroy (Duncan);
			break;
		case 1:
			Helio.SetActive (true);	
			Destroy (Duncan);
			break;
		case 2:
			Duncan.SetActive (true);
			Destroy (Helio);
			break;
		}
*/
		} else {
			foreach (Transform heroSpawner in transform) {
				if (heroSpawner.gameObject != Helio)
					Destroy (heroSpawner.gameObject);
			}
		}
			
	}
}
