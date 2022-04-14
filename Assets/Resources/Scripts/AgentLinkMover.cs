using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public enum OffMeshLinkMoveMethod {
	Teleport,
	NormalSpeed,
	Parabola,
	Curve
}

[RequireComponent (typeof (NavMeshAgent))]
public class AgentLinkMover : MonoBehaviour {
	public OffMeshLinkMoveMethod method = OffMeshLinkMoveMethod.Parabola;
	public AnimationCurve curve = new AnimationCurve ();

	private float normalizedTimeA;
	private float normalizedTimeB;

	public IEnumerator StartJump () {
		NavMeshAgent agent = GetComponent<NavMeshAgent> ();
		agent.autoTraverseOffMeshLink = false;
		normalizedTimeA = 0f;
		if (agent.isOnOffMeshLink) {
			if (method == OffMeshLinkMoveMethod.NormalSpeed)
				yield return StartCoroutine (NormalSpeed (agent));
			else if (method == OffMeshLinkMoveMethod.Parabola)
				yield return StartCoroutine (JumpParabola (agent, 2.0f, 0.5f));
			else if (method == OffMeshLinkMoveMethod.Curve)
				yield return StartCoroutine (Curve (agent, 0.5f));
			agent.CompleteOffMeshLink ();
			yield return null;
		}
	}
	IEnumerator NormalSpeed (NavMeshAgent agent) {
		OffMeshLinkData data = agent.currentOffMeshLinkData;
		Vector3 endPos = data.endPos + Vector3.up*agent.baseOffset;
		while (agent.transform.position != endPos) {
			agent.transform.position = Vector3.MoveTowards (agent.transform.position, endPos, agent.speed*Time.deltaTime);
			yield return null;
		}
		agent.CompleteOffMeshLink ();
	}
	public static IEnumerator JumpParabola (NavMeshAgent agent, float height, float duration) {
		HealthBar agentHealth = agent.GetComponent<HealthBar> ();
		OffMeshLinkData data = agent.currentOffMeshLinkData;
		Vector3 startPos = agent.transform.position;
		Vector3 endPos = data.endPos + Vector3.up*agent.baseOffset;
		Quaternion startRot = agent.transform.rotation;
		agent.transform.position = startPos;
		duration = duration /*+ Vector3.Distance (data.startPos, data.endPos)/10f*/;

		float normalizedTimeA = 0.0f;
		float normalizedTimeB = 0.0f;
		while (normalizedTimeA < 1.0f && !agentHealth.inPain) {
			while (normalizedTimeB < 1f) {
				agent.transform.rotation = Quaternion.Slerp (startRot, Quaternion.LookRotation (agent.destination - agent.transform.position), normalizedTimeB + Time.deltaTime);
				agent.transform.position = Vector3.Lerp (startPos, startPos, 0.8f);
				if (normalizedTimeB > 0.2f && agent.gameObject.GetComponent<Player_Animation> ())
					agent.gameObject.GetComponent<Player_Animation> ().isJumping = false;
				normalizedTimeB += Time.deltaTime / 0.25f;
				yield return null;
			}
			float yOffset = height * 4.0f*(normalizedTimeA - normalizedTimeA*normalizedTimeA);
			agent.transform.position = Vector3.Lerp (startPos, endPos, normalizedTimeA) + yOffset * Vector3.up;
			normalizedTimeA += Time.deltaTime / duration;
			yield return null;
		}
		agent.gameObject.GetComponent<Player_Animation> ().anim.Play ("Landing 0", -1, 0f);
		yield return new WaitForSeconds (0.2f);
		agent.CompleteOffMeshLink ();
		yield break;
	}
	IEnumerator Curve (NavMeshAgent agent, float duration) {
		OffMeshLinkData data = agent.currentOffMeshLinkData;
		Vector3 startPos = agent.transform.position;
		Vector3 endPos = data.endPos + Vector3.up*agent.baseOffset;
		normalizedTimeA = 0.0f;
		while (normalizedTimeA < 1.0f) {
			float yOffset = curve.Evaluate (normalizedTimeA);
			agent.transform.position = Vector3.Lerp (startPos, endPos, normalizedTimeA) + yOffset * Vector3.up;
			normalizedTimeA += Time.deltaTime / duration;
			yield return null;
		}
		agent.CompleteOffMeshLink ();
	}
}
