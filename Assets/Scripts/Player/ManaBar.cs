using UnityEngine;
using System.Collections;

public class ManaBar : MonoBehaviour {

	public playerGUI playerGUI;
	public HealthBar health;
	public References references;

	public float MaxMana = 100f;
	public float CurMana = 100f;
	public float amountPerSecond = 2f;
	public float ManaBarLength;

	private bool isDead;

	// Use this for initialization
	void Start () {
		playerGUI = GetComponent<playerGUI> ();
		health = GetComponent<HealthBar> ();
		references = GetComponent<References> ();
		ManaBarLength = Screen.width / 3;	
		StartCoroutine(GrowOverTime());
	}

	// Update is called once per frame
	void Update () {	
		AddjustCurrentMana(0);

		isDead = health.isDead;

		playerGUI.MaxMana = MaxMana;
		playerGUI.CurMana = CurMana;
		playerGUI.ManaBarLength = ManaBarLength;
	}
/*
	void OnGUI () {
		GUI.Box (new Rect(10, 35, ManaBarLength, 20), CurMana.ToString("F0") + "/" + MaxMana );
	}
*/
	public void AddjustCurrentMana(float adj) {	
		CurMana += adj;
		if (CurMana < 0)
			CurMana = 0;

		if (CurMana > MaxMana)
			CurMana = MaxMana;
		ManaBarLength = (Screen.width / 3) * (CurMana / (float)MaxMana);
	} 

	IEnumerator GrowOverTime (){
		yield return new WaitForFixedUpdate ();
		if (!isDead && CurMana < MaxMana && !references.triggered) {
			CurMana = CurMana + amountPerSecond * Time.fixedDeltaTime;
		}
		StartCoroutine (GrowOverTime ());
	}
}

