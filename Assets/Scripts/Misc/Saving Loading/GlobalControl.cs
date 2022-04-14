using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GlobalControl : MonoBehaviour 
{
	public static GlobalControl Instance;
	public GameObject Player;	//usado por "PlayerState" y sus variantes (PlayerStateDuncan);

	public PlayerStatistics savedPlayerData;// = new PlayerStatistics();

	void Awake (){
		if (Instance == null){
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
		else if (Instance != this){
			Destroy (gameObject);
		}
	}

	void Start(){
/*		//Si es partida guardada, cargar las coordenadas guardadas.
		if (GlobalControl.Instance.IsSceneBeingLoaded) {
			PlayerStateHelio.Instance.localPlayerData = GlobalControl.Instance.LocalCopyOfData;

			Player.transform.position = new Vector3 (
				GlobalControl.Instance.LocalCopyOfData.PositionX,
				GlobalControl.Instance.LocalCopyOfData.PositionY,
				GlobalControl.Instance.LocalCopyOfData.PositionZ + 0.1f);

			GlobalControl.Instance.IsSceneBeingLoaded = false;
		} else if (!GlobalControl.Instance.IsSceneBeingLoaded) {
			Transform startingCoordinates = GameObject.Find ("startingCoordinates").transform;
			Player.transform.position = startingCoordinates.position;
			Player.transform.rotation = startingCoordinates.rotation;
			Player.transform.localScale = startingCoordinates.localScale;
		}
*/
	}

	public PlayerStatistics LocalCopyOfData;
	public bool IsSceneBeingLoaded = false;

	public void SaveData(){
		if (!Directory.Exists (Application.dataPath + "/Saves")) {
			Directory.CreateDirectory (Application.dataPath + "/Saves");
		}

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Create(Application.dataPath + "/Saves/save.binary");

		LocalCopyOfData = PlayerState.Instance.localPlayerData;

		formatter.Serialize(saveFile, LocalCopyOfData);

		saveFile.Close();
	}

	public void LoadData(){
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Open(Application.dataPath + "/Saves/save.binary", FileMode.Open);

		LocalCopyOfData = (PlayerStatistics)formatter.Deserialize(saveFile);

		saveFile.Close();
	}
}