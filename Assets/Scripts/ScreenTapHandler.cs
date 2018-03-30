using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.AI;

public class ScreenTapHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
	public static Transform selected = null;
	public static int selectedType = 0;
	public static ScreenTapHandler singleton = null;
	float holdTime = 0;
	MyController hitCtrl = null;
	Collider hitCol = null;
	Vector3 hitPos = Vector3.zero;
	float hitDist = 0;
	float dragDelta = 0;
	Vector2 clickPos = Vector2.zero;
	float prevDist = 0;
	public float zoom = 0;
	static float maxZoomCoef = 6.7275f;
	static float minZoomCoef = 0.14864f;

	public TargetPointer targetPointer2 = null;


	public int touchCount = 0;
	public List<int> touchIds = new List<int>();

	public Button hitBut = null;



	float zoomCoef {
		get {
			return Mathf.Clamp(Mathf.Pow (1.1f, -zoom), minZoomCoef, maxZoomCoef); 
		}
	}
	// Use this for initialization
	void Awake () {
		singleton = this;
		minZoomCoef = Mathf.Pow (1.1f, -20);
		maxZoomCoef = Mathf.Pow (1.1f, 20);
	}

	// Update is called once per frame
	void Update () {
		if (holdTime >= 0)
			holdTime += Time.deltaTime;
		Zoom (Input.mouseScrollDelta.y * 1.5f); 
	}

	/*public void OnPointerClick(PointerEventData data){
		OnPointerDown (data);
		OnPointerUp (data);

	}*/

	public void OnPointerDown(PointerEventData data){
		touchCount++;
		touchIds.Add (data.pointerId);
		if (touchCount <= 1){
			prevDist = 0;
			hitCtrl = null;
			hitCol = null;
			hitBut = null;
			hitPos = Vector3.zero;
			holdTime = 0;
			dragDelta = 0;
			clickPos = data.position;
			RaycastHit hit;

			if (CameraView.RaycastWorldCanvas(data.position, out hit)){
				Button b = hit.collider.GetComponent<Button> ();
				b.Select ();
				//var pointer = new PointerEventData(EventSystem.current);

				//ExecuteEvents.Execute(b.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
				//ExecuteEvents.Execute(b.gameObject, pointer, ExecuteEvents.pointerDownHandler);
				b.OnPointerEnter(data);
				b.OnPointerDown (data);
				hitBut = b;
				hitCol = hit.collider;
			}else if (CameraView.RaycastScreen (data.position, out hit)) {
				hitCol = hit.collider;
				hitCtrl = hitCol.GetComponent<MyController> ();
				hitPos = hit.point;
				hitDist = hit.distance;
				if (hitCtrl == null) {
					hitCtrl = CameraView.GetClosestControllerToPoint (hitPos, 0.06f * hitDist);
				}
				if (hitCtrl == null) {
					CameraView.ShowTapNeutral (hitPos);
				} else {
					if (hitCtrl.team == 0) {
						CameraView.ShowTapAlly (hitCtrl.transform.position);
					} else {
						CameraView.ShowTapEnemy (hitCtrl.transform.position);
					}
				}
			} else if (CameraView.SphereCastScreen (data.position, out hit)) {
				hitCol = hit.collider;
				hitCtrl = hitCol.GetComponent<MyController> ();
				hitPos = hit.point;
				hitDist = hit.distance;
				if (hitCtrl == null) {
					hitCtrl = CameraView.GetClosestControllerToPoint (hitPos, 0.06f * hitDist);
				}
				if (hitCtrl == null) {
					CameraView.ShowTapNeutral (hitPos);
				} else {
					if (hitCtrl.team == 0) {
						CameraView.ShowTapAlly (hitCtrl.transform.position);
					} else {
						CameraView.ShowTapEnemy (hitCtrl.transform.position);
					}
				}
			}

		}else{
			RaycastHit hit;
			if (CameraView.RaycastScreen (data.position, out hit)) {
				CameraView.ShowTapNeutral (hit.point);
			} else if (CameraView.SphereCastScreen (data.position, out hit)) {
				CameraView.ShowTapNeutral (hit.point);
			}
			holdTime = -1;
			Vector2 a = Input.GetTouch (0).position;
			Vector2 b = Input.GetTouch (1).position;
			prevDist = new Vector2 ((a.x - b.x) * 12 / Screen.width, (a.y - b.y) * 12 / Screen.height).magnitude;
		}
	}

	public void OnPointerUp(PointerEventData data){
		touchCount--;
		touchIds.Remove (data.pointerId);
		if (touchCount < 1 && holdTime > 0){
			RaycastHit hit;
			if (holdTime > 0.3f) {
				if (dragDelta < 0.6f && dragDelta / holdTime < 1.2f) {
					if (hitCol != null && selectedType == 1) {
						if (hitCtrl != null) {
							if (hitCtrl.team == 0) {
								if (hitCtrl.transform == selected) {
									hitCtrl.Stop ();
									holdTime = -1;
									return;
								}
							}
							selected.GetComponent<MyController> ().MoveTo (hitCtrl.transform);
							holdTime = -1;
							return;
						}else if (hitBut != null){
							//var pointer = new PointerEventData(EventSystem.current);
							//ExecuteEvents.Execute(hitBut.gameObject, pointer, ExecuteEvents.pointerUpHandler);
							//ExecuteEvents.Execute(hitBut.gameObject, pointer, ExecuteEvents.pointerClickHandler);
							hitBut.OnPointerUp(data);
							hitBut.OnPointerClick (data);
							holdTime = -1;
							return;
						} else if (selectedType == 1) {
							selected.GetComponent<MyController> ().MoveTo (hitPos);
						}
					} 
				}
			} else {
				if (hitCol != null) {
					if (hitCtrl != null) {
						if (hitCtrl.team == 0) {
							SetSelection (hitCtrl);
							holdTime = -1;
							return;
						} else if (selectedType == 1) {
							//RaycastHit hit;
							if (CameraView.RaycastScreenWalk (clickPos, out hit)) {
								selected.GetComponent<MyController> ().MoveTo (hit.point);
							} else {
								selected.GetComponent<MyController> ().MoveTo (hitPos);
							}
						}
					}else if (hitBut != null){
						//var pointer = new PointerEventData(EventSystem.current);
						//ExecuteEvents.Execute(hitBut.gameObject, pointer, ExecuteEvents.pointerUpHandler);
						//ExecuteEvents.Execute(hitBut.gameObject, pointer, ExecuteEvents.pointerClickHandler);
						hitBut.OnPointerUp(data);
						hitBut.OnPointerClick (data);
						holdTime = -1;
						return;
					} else if (selectedType == 1) {
						selected.GetComponent<MyController> ().MoveTo (hitPos);
					}
				}
			}
			if (hitBut != null) {
				hitBut.OnPointerExit (data);
				hitBut.OnPointerUp (data);
			}

		}
		holdTime = -1;
	}
	public void SetSelection(MyController ctrl){
		if (selectedType == 1) {
			selected.GetComponent<MyController> ().selected = false;
		}

		ctrl.selected = true;
		selected = ctrl.transform;
		selectedType = 1;
		CameraView.SetSelf (selected);
		if (ctrl.target == null) {
			if (ctrl.nma.hasPath)
				CameraView.SetTarget (ctrl.nma.pathEndPosition);
			else
				CameraView.SetTarget (null);
		} else {
			CameraView.SetTarget (ctrl.target);
		}
	}

	/*public void OnDrop(PointerEventData data){
		OnPointerUp (data);
	}*/

	public void OnDrag(PointerEventData data){
		if (touchCount <= 1) {
			Vector2 drag = new Vector2 (data.delta.x * -20 / Screen.width, data.delta.y * -20 / Screen.height);
			/*if (zoom >= 0)
				CameraView.Move (drag / (1 + zoom));
			else
				CameraView.Move (drag * (1 - zoom));*/
			CameraView.Move(drag * zoomCoef);
			dragDelta += data.delta.magnitude / Screen.dpi;
		} else {
			Vector2 a = Input.GetTouch (touchIds[0]).position;
			Vector2 b = Input.GetTouch (touchIds[1]).position;
			float z = new Vector2 ((a.x - b.x) * 12 / Screen.width, (a.y - b.y) * 12 / Screen.height).magnitude;

			if (prevDist != 0) {
				Zoom((z - prevDist) * 2);
			}
			prevDist = z;
		}
	}

	void Zoom (float d){
		float prev = zoomCoef;
		zoom = zoom + d;
		CameraView.Move (d * prev);
	}


}
