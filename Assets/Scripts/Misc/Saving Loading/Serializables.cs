using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerStatistics{

	//Helio Data

	public int Helio_SceneID;
	public float Helio_PositionX, Helio_PositionY, Helio_PositionZ;

	public float Helio_Maxhealth;
	public float Helio_CurHealth;
	public float Helio_MaxMana;
	public float Helio_CurMana;

	public float Helio_runSpeed;
	public float Helio_sprintSpeed;
	public float Helio_walkSpeed;

	public float Helio_jumpBaseForce;
	public int	Helio_jumpsAmount;
	public float Helio_airSpeed;

	//Duncan Data

	public int Duncan_SceneID;
	public float Duncan_PositionX, Duncan_PositionY, Duncan_PositionZ;

	public float Duncan_Maxhealth;
	public float Duncan_CurHealth;
	public float Duncan_MaxMana;
	public float Duncan_CurMana;

	public float Duncan_runSpeed;
	public float Duncan_sprintSpeed;
	public float Duncan_walkSpeed;

	public float Duncan_jumpBaseForce;
	public int	Duncan_jumpsAmount;
	public float Duncan_airSpeed;

}
