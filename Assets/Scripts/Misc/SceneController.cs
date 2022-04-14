using UnityEngine;
using System.Collections;

public class SceneController : MonoBehaviour {

	private bool initialized = false;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		if (!initialized) {
			//Si es partida guardada, cargar las coordenadas guardadas.
			if (GlobalControl.Instance.Player.name == "Helio") {
				if (GlobalControl.Instance.IsSceneBeingLoaded) {
					PlayerState.Instance.localPlayerData = GlobalControl.Instance.LocalCopyOfData;

					GlobalControl.Instance.Player.transform.position = new Vector3 (
						GlobalControl.Instance.LocalCopyOfData.Helio_PositionX,
						GlobalControl.Instance.LocalCopyOfData.Helio_PositionY,
						GlobalControl.Instance.LocalCopyOfData.Helio_PositionZ + 0.1f);

					GlobalControl.Instance.IsSceneBeingLoaded = false;

				} else if (!GlobalControl.Instance.IsSceneBeingLoaded) {
					Transform startingCoordinates = GameObject.Find ("startingCoordinates").transform;
					GlobalControl.Instance.Player.transform.position = startingCoordinates.position;
					GlobalControl.Instance.Player.transform.rotation = startingCoordinates.rotation;
					GlobalControl.Instance.Player.transform.localScale = startingCoordinates.localScale;
				}
			}

			if (GlobalControl.Instance.Player.name == "Duncan") {
				if (GlobalControl.Instance.IsSceneBeingLoaded) {
					PlayerState.Instance.localPlayerData = GlobalControl.Instance.LocalCopyOfData;

					GlobalControl.Instance.Player.transform.position = new Vector3 (
						GlobalControl.Instance.LocalCopyOfData.Duncan_PositionX,
						GlobalControl.Instance.LocalCopyOfData.Duncan_PositionY,
						GlobalControl.Instance.LocalCopyOfData.Duncan_PositionZ + 0.1f);

					GlobalControl.Instance.IsSceneBeingLoaded = false;

				} else if (!GlobalControl.Instance.IsSceneBeingLoaded) {
					Transform startingCoordinates = GameObject.Find ("startingCoordinates").transform;
					GlobalControl.Instance.Player.transform.position = startingCoordinates.position;
					GlobalControl.Instance.Player.transform.rotation = startingCoordinates.rotation;
					GlobalControl.Instance.Player.transform.localScale = startingCoordinates.localScale;
				}
			}			
		}
	
		initialized = true;
	}
}
