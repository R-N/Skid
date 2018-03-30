using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class CancelArea : MonoBehaviour {
	public static CancelArea singleton = null;
	GraphicRaycaster raycaster = null;
	GameObject go = null;
	public float radius = 1;
	public Vector2 pos = Vector2.zero;
	// Use this for initialization
	void Awake () {
		pos = transform.position.xy ();
		raycaster = GetComponentInParent<GraphicRaycaster> ();
		go = gameObject;
		radius = GetComponent<RectTransform> ().sizeDelta.x * 0.5f;
		singleton = this;
		go.SetActive (false);
	}
	
	public bool CheckScreenPoint(PointerEventData data){
		if (Vector2.Distance (data.position, pos) > radius)
			return false;
		List<RaycastResult> rs = new List<RaycastResult> ();
		raycaster.Raycast (data, rs);
		return rs.Count (r => r.isValid && r.gameObject == go) > 0;
	}
}
