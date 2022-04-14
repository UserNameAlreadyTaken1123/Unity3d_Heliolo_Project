using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using StateStuff;

public class AI_MoveAlongPath : State<AI> {

	private AI __owner;
	private bool active = false;

	//=======================================================
	private NavMeshPath _path;
	private bool _hasPath;
	private NavMeshAgent _agent;
	private Queue<Vector3> _cornerQueue; //Lista de "waypoints", llamados "corner".
	private Vector3 _currentDestination;
	private Vector3 _direction;
	private float _currentDistance;

	public Vector3 _targetPoint;
	//=======================================================

	private float updatePath = 0f;

	public void Awake(){
		
	}

	public override bool EnterState(AI _owner){
		__owner = _owner;
		_owner.currentState = this.ToString();
		active = true;

		InitVars();
		CalculateNavMesh();
		SetupPath(_path);  

		return true;
	}

	public override void ExitState(AI _owner){
		_owner.references.animationScript.ResetValues ();
		active = false;
	}

	public override void ForceInterruption(AI _owner){
		if (currentCoroutine != null)
			StopCoroutine (currentCoroutine);
		active = false;
	}

	public override void UpdateState(AI _owner){
		if (updatePath <= 0) {
			updatePath = 1f;
			InitVars();
			CalculateNavMesh();
			SetupPath(_path); 
		} else
			updatePath -= Time.deltaTime;
	}

	private void InitVars(){
		print ("A");
		_targetPoint = __owner.target.transform.position; // Set target point here
		_agent = __owner.navMeshAgent;
		_path = new NavMeshPath();
	}

	private void CalculateNavMesh(){
		print ("B");
		_agent.enabled = true;
		_agent.CalculatePath(_targetPoint, _path);
		_agent.enabled = false;
	}

	void SetupPath(NavMeshPath path){  //Hace una lista de waypoints
		print ("C");
		_cornerQueue = new Queue<Vector3>();
		foreach (Vector3 corner in path.corners){
			_cornerQueue.Enqueue(corner);
		}

		GetNextCorner();
		_hasPath = true;
	}

	private void GetNextCorner(){
		print ("D");
		if (_cornerQueue.Count > 0){
			_currentDestination = _cornerQueue.Dequeue();
			_direction = (_currentDestination - transform.position).normalized;
			_hasPath = true;
		}
		else{
			_hasPath = false;
		}
	}

	void FixedUpdate(){
		MoveAlongPath ();
	}

	private void MoveAlongPath(){
		if (_hasPath) {
			print ("E");
			_currentDistance = Vector3.SqrMagnitude (transform.position - _currentDestination);

			if (_currentDistance > 1f) {
				print ("F");
				transform.position += _direction * 5f * Time.deltaTime;
			}else
				GetNextCorner ();
		}
	}
}
