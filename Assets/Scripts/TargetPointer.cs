using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPointer : MonoBehaviour {
	public Transform myTrans = null;
	public float angularSpeed = 90;
	// Use this for initialization
	void Start () {
		myTrans = transform;
		myTrans.parent = null;
		myTrans.LookAt (myTrans.up);
	}
	
	// Update is called once per frame
	void Update () {
		myTrans.Rotate (Vector3.forward, angularSpeed * Time.deltaTime * GameManager.timeScale, Space.Self);
	}
}
