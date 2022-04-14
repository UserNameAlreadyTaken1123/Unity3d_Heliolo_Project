using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonExitGame : ButtonClass {

	public void ExitGame(){
		System.GC.Collect();
		Application.Quit ();
	}
}
