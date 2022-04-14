using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSpot : MonoBehaviour {

	public GameObject EnWvMngr;
	public GameObject ParticlesEffect;

	// Use this for initialization
	void Start () {
		if (EnWvMngr == null) {
			EnWvMngr = GameObject.Find ("Enemy Waves Manager");
		}
		EnWvMngr.GetComponent<EnemyWavesManager> ().SpawnSpots.Add (this.gameObject);		
		GetComponent<MeshRenderer> ().enabled = false;
	}
}
