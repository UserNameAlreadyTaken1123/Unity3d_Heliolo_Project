using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiplomacyManager : MonoBehaviour {

	public static bool [,] diplomacyGrid = new bool[5,5];

	public bool[] alliesForA = new bool[5];
	public bool[] alliesForB = new bool[5]; 
	public bool[] alliesForC = new bool[5]; 
	public bool[] alliesForD = new bool[5]; 
	public bool[] alliesForE = new bool[5]; 

	private int index = 0;
	private int faction = 0;

	void Start(){
		faction = 0;
		index = 0;
		foreach (bool otherFaction in alliesForA) {
			if (otherFaction)
				SetAsAllies (faction,index);
			else
				SetAsEnemies (faction, index);
			//print (faction + " - " + index + ": " + AreEnemies (faction, index));
			index++;
		}

		faction++;
		index = 0;
		foreach (bool otherFaction in alliesForB) {
			if (otherFaction)
				SetAsAllies (faction, index);
			else
				SetAsEnemies (faction, index);
			//print (faction + " - " + index + ": " + AreEnemies (faction, index));
			index++;
		}

		faction++;
		index = 0;
		foreach (bool otherFaction in alliesForC) {
			if (otherFaction)
				SetAsAllies (faction, index);
			else
				SetAsEnemies (faction, index);
			//print (faction + " - " + index + ": " + AreEnemies (faction, index));
			index++;
		}

		faction++;
		index = 0;
		foreach (bool otherFaction in alliesForD) {
			if (otherFaction)
				SetAsAllies (faction, index);
			else
				SetAsEnemies (faction, index);
			//print (faction + " - " + index + ": " + AreEnemies (faction, index));
			index++;
		}

		faction++;
		index = 0;
		foreach (bool otherFaction in alliesForE) {
			if (otherFaction)
				SetAsAllies (faction, index);
			else
				SetAsEnemies (faction, index);
			//print (faction + " - " + index + ": " + AreEnemies (faction, index));
			index++;
		}
	}

	public static bool AreEnemies(int Faction1, int Faction2){
		return diplomacyGrid[Faction1, Faction2] ;
	}

	public static void SetAsEnemies(int Faction1,int Faction2){
		diplomacyGrid[Faction1, Faction2] = true;
		diplomacyGrid[Faction2, Faction1] = true;  //should faction2 ALSO now consider faction 1 an enemy?  I'll assume so.
	}

	public static void SetAsAllies(int Faction1,int Faction2){
		diplomacyGrid[Faction1, Faction2] = false;
		diplomacyGrid[Faction2, Faction1] = false;
	}
}
