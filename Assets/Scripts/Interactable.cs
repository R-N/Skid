using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
	public MyController selected = null;
	public float interactRadius = 1;
	public Transform myTrans = null;
	public GameObject canvas = null;
	Transform canvasTrans = null;
	void Start(){
		myTrans = transform;
		canvasTrans = canvas.transform;
	}
	void RefreshSelected(){
		Vector3 myPos = myTrans.position;
		Collider[] players = Physics.OverlapSphere (myPos, interactRadius, GameManager.playerMask, QueryTriggerInteraction.Ignore);
		int pLen = players.Length;
		if (pLen == 0)
			selected = null;
		else if (pLen == 1)
			selected = players [0].GetComponent<MyController>();
		else {
			selected = null;
			if (MyController.selectedPlayer != null) {
				for (int i = 0; i < pLen; ++i) {
					MyController ctrl = players [i].GetComponent<MyController> ();
					if (ctrl == MyController.selectedPlayer) {
						selected = ctrl;
						break;
					}
				}
			}
			if (selected == null) {
				float minDist = float.PositiveInfinity;
				Collider s = null;
				for (int i = 0; i < pLen; ++i) {
					float dist = Vector3.Distance (players [i].transform.position, myPos);
					if (dist < minDist) {
						minDist = dist;
						s = players [i];
					}
				}
				selected = s.GetComponent<MyController> ();
			}
		}
	}
	void LateUpdate () {
		RefreshSelected ();
		if (selected == null) {
			if (canvas.activeInHierarchy) {
				canvas.SetActive (false);
			}
		} else {
			if (!canvas.activeInHierarchy) {
				canvas.SetActive (true);
			}
			canvasTrans.LookAt (CameraView.singleton.camTrans.position);
			canvasTrans.rotation = CameraView.singleton.camTrans.rotation;
		}
	}
	public virtual void OnClick(int option){
	}
}
