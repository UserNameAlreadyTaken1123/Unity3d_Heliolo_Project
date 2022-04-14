using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Luminosity.IO;

public class CustomMethods : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	public static void CheckCurrentAnimatorState (Animator animator){
		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Grounded Melee Attacks")) {
			float normalizedTime = animator.GetCurrentAnimatorStateInfo (0).normalizedTime;
			animator.Play ("Grounded Melee Attacks", -1, 0f);
			animator.Play ("Grounded Melee Attacks 0", -1, normalizedTime);
		} else if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Air Melee Attacks")) {
			float normalizedTime = animator.GetCurrentAnimatorStateInfo (0).normalizedTime;
			animator.Play ("Air Melee Attacks", -1, 0f);
			animator.Play ("Air Melee Attacks 0", -1, normalizedTime);
		}
	}

	public static IEnumerator CameraShake(Vector3 origin, float strenght, float duration){
		float normalizedTimer = 1f;
		while (normalizedTimer > 0) {
			Camera.main.transform.position = Camera.main.transform.position + (new Vector3 (Random.Range (-strenght, strenght), Random.Range (-strenght, strenght), Random.Range (-strenght, strenght)) / Vector3.Distance (origin, Camera.main.transform.position)) / 2 * normalizedTimer;
			Camera.main.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler (new Vector3 (Random.Range (-strenght, strenght), Random.Range (-strenght, strenght), Random.Range (-strenght, strenght)) / Vector3.Distance (origin, Camera.main.transform.position) * normalizedTimer);
			yield return null;
			normalizedTimer -= Time.deltaTime / duration;
		}
	}

	public static bool CheckDisplacementInput(){
		if (InputManager.GetAxis ("Vertical") > -0.01f && InputManager.GetAxis ("Vertical") < 0.01f && InputManager.GetAxis ("Horizontal") > -0.01f && InputManager.GetAxis ("Horizontal") < 0.01f)
			return false;
		else
			return true;
	}

	public static void CustomDebugLog(MonoBehaviour script, string message){
		Debug.Log (message + " at " + script + " from " + script.gameObject.name, script.gameObject);
	}

	public static IEnumerator FadeAndDestroyAudiosource (AudioSource ASource, float fadeTime){
		float startValue = ASource.volume;
		float normalizedTimeA = 0.0f;
		while (normalizedTimeA < 1.0f) {
			ASource.volume = Mathf.Lerp (startValue, 0f, normalizedTimeA);
			normalizedTimeA += Time.deltaTime / fadeTime;
			yield return null;
		}
		Destroy (ASource, 0.1f);
	}

	public static AudioSource PlayClipAt(AudioClip clip, Vector3 pos){
		GameObject tempGO = new GameObject("TempAudio"); // create the temp object
		tempGO.transform.position = pos; // set its position
		AudioSource aSource = tempGO.AddComponent<AudioSource>(); // add an audio source
		aSource.clip = clip; // define the clip
		aSource.spatialBlend = 1f;
		aSource.maxDistance = 30f;
		aSource.pitch = Random.Range (0.95f, 1.05f);
		aSource.rolloffMode = AudioRolloffMode.Linear;
		// set other aSource properties here, if desired
		aSource.Play(); // start the sound
		Destroy(tempGO, clip.length); // destroy object after clip duration
		return aSource; // return the AudioSource reference
	}

	public static AudioSource PlayLoopClipAt(AudioClip clip, Vector3 pos){
		GameObject tempGO = new GameObject("TempAudio"); // create the temp object
		tempGO.transform.position = pos; // set its position
		AudioSource aSource = tempGO.AddComponent<AudioSource>(); // add an audio source
		aSource.clip = clip; // define the clip
		aSource.spatialBlend = 1f;
		aSource.maxDistance = 30f;
		aSource.rolloffMode = AudioRolloffMode.Linear;
		aSource.loop = true;
		// set other aSource properties here, if desired
		aSource.Play(); // start the sound
		return aSource; // return the AudioSource reference
	}

	public static IEnumerator JumpToPositionParabola (GameObject who, Vector3 destination, float height, float duration) {
		Vector3 startPos = who.transform.position;
		Vector3 tempPos = startPos;
		Vector3 prevPos = startPos;
		Rigidbody rigidbody = who.GetComponent<Rigidbody> ();
		float normalizedTimeA = 0.0f;
		while (normalizedTimeA < 1.0f) {
			float yOffset = height * 4.0f*(normalizedTimeA - normalizedTimeA*normalizedTimeA);
			tempPos = Vector3.Lerp (startPos, destination, normalizedTimeA) + yOffset * Vector3.up;

			if (Physics.Raycast (prevPos, tempPos - prevPos, Vector3.Distance (tempPos, prevPos) + 0.2f, LayerMask.GetMask ("Scenario", "Default"), QueryTriggerInteraction.Collide)) {
				rigidbody.MovePosition (prevPos);
				rigidbody.velocity = Vector3.zero;
				yield break;
			}

			prevPos = tempPos;
			rigidbody.MovePosition (tempPos);
			normalizedTimeA += Time.deltaTime / duration;
			yield return new WaitForFixedUpdate();
		}
		yield break;
	}


	public static IEnumerator SuperJumpExecution(Rigidbody Target, Vector3 destination, float height) {
		yield return new WaitForFixedUpdate();
		HealthBar health = Target.GetComponent<HealthBar>();
		Target.velocity = Vector3.zero;

		Vector3 startPos = Target.transform.position;
		Vector3 tempPos = startPos;
		Vector3 prevPos = startPos;

		CapsuleCollider capsuleCollider = Target.gameObject.GetComponent<CapsuleCollider>();

		float duration = 8f;
		float normalizedTimeA = 0.0f;
		bool superJumpRunning = true;

		while (superJumpRunning) {
			if (normalizedTimeA > 0.1f) {
				if (health.justGotHurt/* || health.isDead*/) {
					superJumpRunning = false;
					break;
				}

				if (Target.gameObject.GetComponent<AI>() && Target.gameObject.GetComponent<AI>().isGrounded) {
					superJumpRunning = false;
					break;
				}
			}

			prevPos = tempPos;
			float yOffset = height * 4.0f * (normalizedTimeA - normalizedTimeA * normalizedTimeA); // formula para la altura
			tempPos = CustomMethods.Vector3LerpUnclamped(startPos, destination, normalizedTimeA) + yOffset * Vector3.up;

			RaycastHit hitPoint;
			Debug.DrawRay(prevPos + (destination - startPos).normalized * capsuleCollider.radius / 2f,
			   (tempPos - prevPos).normalized * 2f, Color.red, 0.01f, false);
			if (Physics.Raycast(prevPos + (destination - startPos).normalized * capsuleCollider.radius / 2f, tempPos - prevPos, out hitPoint, capsuleCollider.radius * 2, LayerMask.GetMask("Scenario", "Default"), QueryTriggerInteraction.Ignore)) { //detectar colision y mover al punto de impacto
				Target.MovePosition(hitPoint.point + ((prevPos - hitPoint.point).normalized * capsuleCollider.radius) + Target.transform.forward * (-0.5f) + Vector3.down * 0.25f);
				superJumpRunning = false;
				yield return new WaitForFixedUpdate();
				break;
			} else {
				Target.MovePosition(Vector3.Lerp(Target.transform.position, tempPos, 0.25f));
				yield return new WaitForFixedUpdate();
			}
			normalizedTimeA += Time.deltaTime + (Time.deltaTime - Time.deltaTime * Time.deltaTime) / duration;
		}

		if (Target.gameObject.GetComponent<AI>() && Target.gameObject.GetComponent<AI>().isGrounded) {
			Target.velocity = Vector3.zero;
		}
		yield return null;
	}

	public static IEnumerator MoveRigidbodyTogether (GameObject who, GameObject stickToWhat, Vector3 offsetPosition, float duration){
		Rigidbody whoRigid = who.GetComponent<Rigidbody> ();
		Transform stickToTransform = stickToWhat.transform;
		Vector3 temp;
		Quaternion offsetRotation;

		while (duration > 0f) {
			duration -= Time.deltaTime;
			temp = (stickToTransform.position - who.transform.position).normalized;
			temp.y = 0f;
			offsetRotation = Quaternion.LookRotation (temp);
			whoRigid.MovePosition (Vector3.Lerp (who.transform.position, stickToTransform.position + offsetRotation * offsetPosition, 50f * Time.deltaTime));
			yield return new WaitForFixedUpdate();
		}
		whoRigid.velocity = stickToWhat.GetComponent<Rigidbody>().velocity;
	}

	public static IEnumerator MoveRigidbodyTogetherLauncher (NGS_LauncherA callerClass , GameObject who, GameObject stickToWhat, Vector3 offsetPosition, float duration){
		yield return null;
		Rigidbody whoRigid = who.GetComponent<Rigidbody> ();
		Transform stickToTransform = stickToWhat.transform;
		Vector3 temp;
		Quaternion offsetRotation;
		float durationThreshold = 0f;

		temp = (stickToTransform.position - who.transform.position).normalized;
		temp.y = 0f;
		offsetRotation = Quaternion.LookRotation(temp);

		while (!callerClass.breakConditionsMet) {

			duration -= Time.deltaTime;
			durationThreshold += Time.deltaTime;

			temp = (stickToTransform.position - who.transform.position).normalized;
			temp.y = 0f;
			offsetRotation = Quaternion.LookRotation (temp);

			whoRigid.velocity = Vector3.zero;
			whoRigid.MovePosition (Vector3.Lerp (who.transform.position, stickToTransform.position + offsetRotation * offsetPosition, 40f * Time.deltaTime));

			yield return new WaitForFixedUpdate();
		}
		/*
		if (!callerClass.isObsolete) {
			whoRigid.velocity = Vector3.zero;
			whoRigid.MovePosition(stickToTransform.position + offsetRotation * offsetPosition + Vector3.down * 0.25f);
		}
		*/
	}

	public static GameObject SearchForTargetInDirection(GameObject caller){/*
		Vector3 forwardDirection = caller.GetComponent<References> ().Camera.transform.forward * InputManager.GetAxis ("Vertical");
		Vector3 rightDirection = caller.GetComponent<References> ().Camera.transform.right * InputManager.GetAxis ("Horizontal");
		Vector3 inputDirection = (forwardDirection + rightDirection).normalized;
		inputDirection.y = 0f;
		*/

		int callerLayer = caller.layer;
		caller.layer = 1;
		Vector3 forwardDirection = Camera.main.transform.forward * InputManager.GetAxis ("Vertical");
		Vector3 rightDirection = Camera.main.transform.right * InputManager.GetAxis ("Horizontal");
		Vector3 inputDirection = (forwardDirection + rightDirection).normalized;
		inputDirection.y = 0f;

		if (inputDirection.magnitude < 0.1f) {
			inputDirection = caller.transform.forward;
			inputDirection.y = 0f;
		}

		//Comparo si los controles "apuntan" hacia algún enemigo
		Collider[] enemiesInRange = Physics.OverlapSphere (caller.transform.position, 4f, LayerMask.GetMask ("Enemy", "Player"));
		List<GameObject> enemiesInDirection = new List<GameObject>();

		foreach (Collider enemyInRange in enemiesInRange) {
			if (Vector3.Angle (inputDirection.normalized, (enemyInRange.transform.position - caller.transform.position).normalized) < 10f)
				enemiesInDirection.Add (enemyInRange.gameObject);
		}

		if (enemiesInDirection.Count == 0) {
			foreach (Collider enemyInRange in enemiesInRange) {
				if (Vector3.Angle (inputDirection.normalized, (enemyInRange.transform.position - caller.transform.position).normalized) < 30f)
					enemiesInDirection.Add (enemyInRange.gameObject);
			}
		}

		if (enemiesInDirection.Count == 0) {
			foreach (Collider enemyInRange in enemiesInRange) {
				if (Vector3.Angle (inputDirection.normalized, (enemyInRange.transform.position - caller.transform.position).normalized) < 90f)
					enemiesInDirection.Add (enemyInRange.gameObject);
			}
		}

		if (enemiesInDirection.Count == 0) {
			foreach (Collider enemyInRange in enemiesInRange) {
				if (Vector3.Angle (inputDirection.normalized, (enemyInRange.transform.position - caller.transform.position).normalized) < 120f)
					enemiesInDirection.Add (enemyInRange.gameObject);
			}
		}

		caller.layer = callerLayer;
		//comparo qué enemigos fueron apuntados para ver cual es el más cercano, sólo si alguno fue apuntado.
		//Si "enemiesInDirection" == 0, entonces sólo girar al enemigo más cercano.
		GameObject closestEnemy;
		if (enemiesInDirection.Count != 0) {
			closestEnemy = enemiesInDirection [0];
			foreach (GameObject enemyInDir in enemiesInDirection) {
				if (Vector3.Distance (caller.transform.position, enemyInDir.transform.position) <
					Vector3.Distance (caller.transform.position, closestEnemy.transform.position)) {
					closestEnemy = enemyInDir;
				}
			}
			return closestEnemy;
		} 
		return null;
	}

	public static IEnumerator SmoothMoveTowards (GameObject who, Vector3 targetPosition, float duration){
		Vector3 originalPosition = who.transform.position;
		float timeElapsed = 0f;
		float progress;
		if (who.GetComponent<Rigidbody> ()) {
			Rigidbody rigidbody = who.GetComponent<Rigidbody> ();
			while (timeElapsed < duration) {
				timeElapsed += Time.deltaTime;
				progress = timeElapsed / duration;
				rigidbody.MovePosition (Vector3.Lerp (originalPosition, targetPosition, progress));
				yield return null;
			}
		} else {
			while (timeElapsed < duration) {
				timeElapsed += Time.deltaTime;
				progress = timeElapsed / duration;
				who.transform.position = Vector3.Lerp (originalPosition, targetPosition, progress);
				yield return null;
			}
		}
	}

	public static IEnumerator SmoothRotateTowards (GameObject who, Transform towardsWhat, float duration){
		Quaternion originalRotation = who.transform.rotation;
		Vector3 direction = (towardsWhat.position - who.transform.position).normalized;
		direction.y = 0f;

		if (duration > 0f) {
			float timeElapsed = 0f;
			float progress;
			while (timeElapsed < duration) {
				timeElapsed += Time.deltaTime;
				progress = timeElapsed / duration;
				who.transform.rotation = Quaternion.Slerp (originalRotation, Quaternion.LookRotation (direction), progress);
				yield return null;
			}
		} else {
			who.transform.rotation = Quaternion.LookRotation (direction);
		}
	}

	public static IEnumerator SmoothKeepRotatingTowards (GameObject who, Transform towardsWhat, float duration, float speed){
		Quaternion originalRotation;
		Vector3 direction;
		speed = Mathf.Clamp (speed, 0f, 1f);
		direction.y = 0f;
		float timeElapsed = 0f;
		while (timeElapsed < duration) {
			originalRotation = who.transform.rotation;
			direction = (towardsWhat.position - who.transform.position).normalized;
			direction.y = 0f;
			timeElapsed += Time.deltaTime;
			who.transform.rotation = Quaternion.Slerp (originalRotation, Quaternion.LookRotation (direction), speed);
			yield return null;
		}
	}

	public static Vector3 Vector3LerpUnclamped( Vector3 a, Vector3 b, float t ){
		return t*b + (1-t)*a;
	}
}
