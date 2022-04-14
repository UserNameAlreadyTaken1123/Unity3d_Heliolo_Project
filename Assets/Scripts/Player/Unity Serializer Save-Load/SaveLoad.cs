using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveLoad : MonoBehaviour {
	public string saveFile;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("f5")) {
			if (!Directory.Exists(Application.dataPath + "/Savegames/"))
				Directory.CreateDirectory(Application.dataPath + "/Savegames/");
				
			saveFile = LevelSerializer.SerializeLevel ();

			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Create (Application.dataPath + "/Savegames/QuickSave.dat");
			StoredData data = new StoredData ();
			data.saveString = saveFile;
			bf.Serialize (file, data);
			file.Close ();
			print ("saved!");

		}
		if (Input.GetKeyDown ("f6")) {
			if (File.Exists (Application.dataPath + "/Savegames/QuickSave.dat")) {
				BinaryFormatter bf = new BinaryFormatter ();
				FileStream file = File.Open(Application.dataPath + "/Savegames/QuickSave.dat", FileMode.Open);
				StoredData data = (StoredData) bf.Deserialize(file);
				file.Close();

				LevelSerializer.LoadSavedLevel (data.saveString);
				print ("loaded! at " + Application.dataPath + "/Savegames/QuickSave.dat");
				print (file);
			}
		}
	}
}

[Serializable]
class StoredData {
	public string saveString;
}
