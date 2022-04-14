using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentLighting : MonoBehaviour {

	public Color color = new Color (0.5f, 0.5f, 0.5f, 1f);

	// Use this for initialization
	void Start () {
		if (QualitySettings.GetQualityLevel () < 2) {
			RenderSettings.ambientLight = color;
		}
	}
}
