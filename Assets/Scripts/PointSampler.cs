using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using UnityEngine.AI;

public class PointSampler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
	Action<Vector3> v3positionHandler = null;
	Action<Vector3> v3upHandler = null;
	Action cancelHandler = null;
	public CancelArea cancelArea = null;
	public Image img = null;

	public static PointSampler singleton = null;

	public LayerMask mask = Physics.AllLayers;

	void Awake(){
		singleton = this;
	}
	// Use this for initialization
	void Start () {
		img = GetComponent<Image> ();
		//img.raycastTarget = false;
		img.enabled = false;
		Done ();
	}

	/*public void OnPointerClick(PointerEventData data){
		OnPointerDown (data);
		OnPointerUp (data);
	}*/

	public void OnDrag(PointerEventData data){
		RaycastHit hit;
		if (CameraView.RaycastScreen (data.position, out hit, mask)) {
			//NavMeshHit nmHit;
			//if (NavMesh.SamplePosition (hit.point, out nmHit, 0.1f, NavMesh.AllAreas))
			v3positionHandler (hit.point);
			//else
			//	v3positionHandler (MyExtensions.v3Inf);
		} 
	}
	/*public void OnDrop(PointerEventData data){
		OnPointerUp (data);
	}*/


	public void OnPointerDown(PointerEventData data){
		RaycastHit hit;

		if (CameraView.RaycastScreen (data.position, out hit, mask)) {
			//NavMeshHit nmHit;
			//if (NavMesh.SamplePosition (hit.point, out nmHit, 0.1f, NavMesh.AllAreas))
			v3positionHandler (hit.point);
			//else
			//	v3positionHandler (MyExtensions.v3Inf);
		}
		//img.raycastTarget = false;
		img.enabled = false;
	}

	public void OnPointerUp(PointerEventData data){
		RaycastHit hit;
		if (cancelArea != null && cancelArea.CheckScreenPoint (data)) {
			cancelHandler ();
		} else {
			if (CameraView.RaycastScreen (data.position, out hit, mask)) {
				//NavMeshHit nmHit;
				//if (NavMesh.SamplePosition (hit.point, out nmHit, 0.1f, NavMesh.AllAreas))
				v3upHandler (hit.point);
				//else
				//	v3upHandler (MyExtensions.v3Inf);
			}
		}
		if (cancelArea != null)
			cancelArea.gameObject.SetActive (false);
		Done ();

	}
	public bool GetPoint(Action<Vector3> positionHandler, Action<Vector3> upHandler, Action cancelHandler, LayerMask mask){
		Debug.Log ("mask " + (int)mask);

		if (positionHandler == null || upHandler == null || cancelHandler == null)
			return false;
		if (cancelArea != null)
			cancelArea.gameObject.SetActive (true);
		
		this.mask = mask;
		gameObject.SetActive (true);
		//img.raycastTarget = true;
		img.enabled = true;
		v3positionHandler = positionHandler;
		v3upHandler = upHandler;
		this.cancelHandler = cancelHandler;
		GameManager.timeScale = 0.1f;
		return true;
	}

	public void Done(){
		GameManager.timeScale = 1.0f;
		gameObject.SetActive (false);
	}
}
