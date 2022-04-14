using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ButtonStartScene : ButtonClass {

	public string sceneName;

	public void StartScene(){
		SceneManager.LoadScene (sceneName);
	}
}
