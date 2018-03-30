using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using UnityEngine.AI;

public class DirectionSampler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
	Action<Vector2> v2dirHandler = null;
	Action<Vector2> v2upHandler = null;
	Action cancelHandler = null;
	public CancelArea cancelArea = null;
	public Image img = null;

	public static DirectionSampler singleton = null;
	Vector2 startPoint = Vector2.zero;
	Vector2 dir = Vector2.zero;
	public Transform pointerHolder = null;
	public Transform pointer = null;

	public float cancelRatio = 0.2f;
	float cancelRadius = 1;

	void Awake(){
		singleton = this;
	}
	// Use this for initialization
	void Start () {
		img = GetComponent<Image> ();
		//img.raycastTarget = false;
		img.enabled = false;
		cancelRadius = cancelRatio * GetComponent<RectTransform> ().sizeDelta.x * 0.5f;
		Stop ();
	}

	/*public void OnPointerClick(PointerEventData data){
		OnPointerDown (data);
		OnPointerUp (data);
	}*/

	public void OnDrag(PointerEventData data){
		Vector2 delta = data.position - startPoint;
		float mag = delta.magnitude;
		if (mag <= cancelRadius){
			dir = Vector2.zero;
			pointer.gameObject.SetActive (false);
		} else {
			dir = delta / mag;
			pointer.rotation = Quaternion.Euler (0, 0, Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg);
			pointer.gameObject.SetActive (true);
		}
		v2dirHandler (dir);
	}
	/*public void OnDrop(PointerEventData data){
		OnPointerUp (data);
	}*/


	public void OnPointerDown(PointerEventData data){
		Start (data.position.ToV3AddZ());
	}

	public void OnPointerUp(PointerEventData data){
		if (cancelArea != null && cancelArea.CheckScreenPoint (data)) {
			cancelHandler ();
		} else {
			Vector2 delta = data.position - startPoint;
			float mag = delta.magnitude;
			if (mag <= cancelRadius)
				dir = Vector2.zero;
			else
				dir = delta/mag;
			v2upHandler (dir);
		}
		if (cancelArea != null)
			cancelArea.gameObject.SetActive (false);
		Stop ();

	}
	public bool GetDirection(Action<Vector2> dirHandler, Action<Vector2> upHandler, Action cancelHandler){
		if (dirHandler == null || upHandler == null)
			return false;
		if (cancelArea != null)
			cancelArea.gameObject.SetActive (true);
		gameObject.SetActive (true);
		//img.raycastTarget = true;
		this.cancelHandler = cancelHandler;
		img.enabled = true;
		v2dirHandler = dirHandler;
		v2upHandler = upHandler;
		dir = Vector2.zero;
		return true;
	}

	public void Start(Vector3 worldScreenPos){
		pointerHolder.position = worldScreenPos;
		pointerHolder.gameObject.SetActive (true);

		startPoint = worldScreenPos.xy();
		dir = Vector2.zero;

		//img.raycastTarget = false;
		img.enabled = false;
	}

	public void Stop(){
		dir = Vector2.zero;
		pointer.gameObject.SetActive (false);
		pointerHolder.gameObject.SetActive (false);
		gameObject.SetActive (false);
	}
}
