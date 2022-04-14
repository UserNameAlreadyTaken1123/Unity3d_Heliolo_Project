using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Luminosity.IO;

public class BulletBehavior : MonoBehaviour {

//	private Transform BulletTransform;
	public Transform shooter;
	public bool AIShoter;
	private float timer = 3;
	public float spread = 2.5f;
	public float speed = 15;
	public float baseDamage = 10f;
	public bool isRipper = false;
	public int isRipperCount = 3;
	public float hitForce = 1f;
	public GameObject flash;

	public List<GameObject> targetsHit;

	private float damageMultiplier;
	private GameObject applyDamageTo;
	private Collider collider;
	private Vector3 prevPos;
	private RaycastHit Hit;
	//public LayerMask lyrMsk;
	//public LayerMask bulletIgnoreLayersB;

//	private Collider bulletcollider;
//	private GameObject playerid;

	public float _spread;
	public float _speed;

	private bool initialized = false;
    public bool isFirstShot = false;

	void Update(){
		if (!initialized) {
			initialized = true;
			collider = GetComponent<Collider> ();
			Destroy (gameObject, timer);
			_spread = spread / 100;
			//if (!AIShoter && !InputManager.GetButton ("Aim"))
			//	_spread = _spread * 2;

			_speed = speed * 10;
			float spreadX = Random.Range(_spread/2,-_spread/2);
			float spreadY = Random.Range(_spread/2,-_spread/2);
			float spreadZ = Random.Range(_spread/2,-_spread/2);
			Vector3 randomDir = new Vector3 (spreadX,spreadY,spreadZ);
			Quaternion direction = Quaternion.LookRotation(transform.forward + randomDir, Vector3.up);
			transform.rotation = direction;
			GetComponent<Rigidbody>().AddForce (transform.forward * _speed);
			prevPos = transform.position;
		}

		Debug.DrawRay (prevPos, transform.position - prevPos, Color.red, 0.01f);

		if (initialized) {
			Physics.SphereCast (prevPos, transform.lossyScale.x, (transform.position - prevPos).normalized, out Hit, Vector3.Distance (transform.position, prevPos) + 0.1f, LayerMask.GetMask ("Rig", "Scenario", "Default"), QueryTriggerInteraction.Collide);
			//Physics.Raycast (prevPos, transform.position - prevPos, out Hit, Vector3.Distance (transform.position, prevPos) + 0.1f, LayerMask.GetMask ("Rig", "Scenario", "Default"), QueryTriggerInteraction.Collide);<
			if (Hit.collider) {
				BulletImpacted (Hit.collider);
			} else {
				//Este paso es necesario hasta que TODOS los personajes tengan colliders en su esqueleto. Hasta entonces, por las dudas se utilizará el collider
				//general del personake, en el Layer ENEMY o PLAYER.
				//Physics.Raycast (prevPos, transform.position - prevPos, out Hit, Vector3.Distance (transform.position, prevPos) + 0.1f, LayerMask.GetMask ("Enemy", "Scenario", "Default"), QueryTriggerInteraction.Collide);
				//Physics.SphereCast (prevPos,  transform.localScale.x, (transform.position - prevPos).normalized, out Hit, Vector3.Distance (transform.position, prevPos) + 0.1f, LayerMask.GetMask ("Enemy", "Scenario", "Default"), QueryTriggerInteraction.Collide);
				//if (Hit.collider && Hit.collider.GetComponent<ColliderChild> ()) {
				//	BulletImpacted (Hit.collider);
				//}
			}
			prevPos = transform.position;
		}
	}

		
	private void BulletImpacted(Collider checker){
		if (checker.gameObject.layer == LayerMask.NameToLayer ("Default") || checker.gameObject.layer == LayerMask.NameToLayer ("Scenario")) {
			transform.position = Hit.point;
			Death ();
		} else if (checker.gameObject != shooter.gameObject && checker.GetComponent<ColliderChild> () && shooter.gameObject != checker.GetComponent<ColliderChild> ().Player && !targetsHit.Contains (checker.GetComponent<ColliderChild> ().Player)) {
			transform.position = Hit.point;
			targetsHit.Add (checker.GetComponent<ColliderChild> ().Player);		
			applyDamageTo = checker.GetComponent<ColliderChild> ().Player;
			damageMultiplier = checker.GetComponent<ColliderChild> ().damageMultiplier;

			if (!isFirstShot && applyDamageTo != shooter.gameObject && DiplomacyManager.AreEnemies (shooter.GetComponent<References> ().team, checker.GetComponent<ColliderChild> ().colliderManagerScript.gameObject.GetComponent<References> ().team)) {
				HealthBar health = applyDamageTo.GetComponent<HealthBar> ();
				health.BulletDamage (Hit.point, shooter, transform, -baseDamage * damageMultiplier * Random.Range (0.9f, 1.1f));
				if (!health.isDead)
					applyDamageTo.GetComponent<Rigidbody> ().AddForce (transform.forward * hitForce * Random.Range (0.9f, 1.1f), ForceMode.VelocityChange);
				else
					applyDamageTo.GetComponent<Rigidbody> ().AddForce (transform.forward * hitForce * Random.Range (0.9f, 1.1f) * 2f, ForceMode.VelocityChange);

				if (shooter.GetComponent<playerGUI> ())
					shooter.GetComponent<playerGUI> ().TargetWasHit ();
			}

			if (!isRipper || isRipperCount == 0) {
				Death ();
			} else {
				isRipperCount -= 1;
			}
		} else if (checker.GetComponent<ColliderChild> () && shooter.gameObject == checker.GetComponent<ColliderChild> ().Player) {
			Physics.IgnoreCollision (checker, collider);	
		}
	}

	private void Death(){
		flash.transform.SetParent (null, true);
		flash.GetComponent<ParticleSystem> ().Play ();
		Destroy (gameObject, 2f);
		Destroy (flash, 1f);
		gameObject.SetActive (false);
	}
}



