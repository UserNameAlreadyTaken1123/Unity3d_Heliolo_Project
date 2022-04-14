using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshMerger : MonoBehaviour {

	public MeshFilter[] meshFilters;
	public CombineInstance[] combine;

	public bool mergeColliders;

	void Start() {
		meshFilters = GetComponentsInChildren<MeshFilter>();
		combine = new CombineInstance[meshFilters.Length];
		int i = 0;
		while (i < meshFilters.Length) {
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			meshFilters[i].gameObject.active = false;
			i++;
		}
		transform.GetComponent<MeshFilter>().mesh = new Mesh();
		transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
		transform.gameObject.active = true;

	}
}