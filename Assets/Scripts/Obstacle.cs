using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {
	public static Dictionary<Collider, Obstacle> obstaclesByCollider = new Dictionary<Collider, Obstacle>();

	public float sightMul = 0;
	public float hearMul = 0.5f;
	public float sightMulCrouch = 0;
	public float hearMulCrouch = 0;

	void Start(){
		Collider col = GetComponent<Collider> ();
		if (col == null)
			col = GetComponentInChildren<Collider> ();
		if (col == null)
			col = GetComponentInParent<Collider> ();
		if (col == null)
			Debug.LogError ("No collider in obstacle");
		else if (!obstaclesByCollider.ContainsKey (col))
			obstaclesByCollider.Add (col, this);

	}
}
