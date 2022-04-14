using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OuroborosEyeRotation : MonoBehaviour {

	public Vector3 axisRotation;
	private Vector3 axisRotationTemp;
	public List<float> amounts;
	private int amountsIndex;
	public bool canReverse;
	public float speed;
	public float coolDown;
	private int temp;

	public Quaternion targetAngle;
	public Quaternion startingAngle;
	public float coolDownTimer;

	private bool coroRunning;

	void Start(){
		coolDownTimer = coolDown;
		amountsIndex = Random.Range (0, amounts.Count);
		axisRotationTemp = axisRotation;
		if (canReverse) {
			temp = Random.Range (0, 1);
			if (temp == 1)
				axisRotationTemp = axisRotation * (-1f);
			else
				axisRotationTemp = axisRotation;
		}

		targetAngle = transform.rotation * Quaternion.AngleAxis(amounts[amountsIndex], axisRotationTemp);
		startingAngle = transform.rotation;
	}

	void Update(){
		if (coolDownTimer <= 0) {
			transform.rotation = Quaternion.RotateTowards (transform.rotation, targetAngle, Time.deltaTime * Time.timeScale * speed * 100f);
			if (Quaternion.Angle (transform.rotation, targetAngle) < 0.1f) {
				Start ();
			}
		} else {
			coolDownTimer -= Time.deltaTime * Time.timeScale;
		}
	}
}
