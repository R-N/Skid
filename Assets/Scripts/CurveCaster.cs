using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CurveCaster : MonoBehaviour {
	public ParticleSystem ps = null;
	public ParticleSystem cps = null;
	public Transform myTrans = null;
	public float speed = 5;

	public float gravity = 9.81f;

	public Vector3 prevCameraPos = Vector3.zero;

	public Action<Vector3> collisionHandler = null;

	public ParticleSystem.Particle[] particles = null;

	public Transform targetPointer = null;

	public float prevStartSizeMul = 1;
	public static CurveCaster singleton = null;

	public Vector3 dir = Vector3.zero;



	public GameObject stopper = null;

	public float timer = 0;

	public bool done = false;


	public float tauntRadiusMul = 1;

	// Use this for initialization
	void Start () {
		singleton = this;
		myTrans = transform;
		if (ps == null)
			ps = GetComponent<ParticleSystem> ();
		if (cps == null)
			cps = GetComponentInChildren<ParticleSystem> ();
		myTrans.parent = null;
		gravity = Physics.gravity.magnitude;
		SetSpeed (speed);

		particles = new ParticleSystem.Particle[cps.maxParticles];
		prevStartSizeMul = Vector3.Distance (myTrans.position, CameraView.singleton.myTrans.position);
		cps.startSize = cps.startSize * prevStartSizeMul;
		StopAim ();
	}
	
	// Update is called once per frame
	void Update () {
		if (done) {
			if (timer > 0)
				timer -= Time.deltaTime;
			else
				StopAim ();
		}
	}

	void LateUpdate(){
		Vector3 camPos = CameraView.singleton.camTrans.position;
		int count = cps.GetParticles (particles);
		float startSizeMul;
		if (count > 0) {
			 startSizeMul = Vector3.Distance (particles [count - 1].position, camPos);
		} else {
			startSizeMul = Vector3.Distance (myTrans.position, camPos);
		}
		cps.startSize = cps.startSize * startSizeMul / prevStartSizeMul;
		prevStartSizeMul = startSizeMul;
		prevCameraPos = camPos;
	}

	public void SetSpeed (float spd){
		speed = spd;
		ps.startSpeed = spd;

	}

	public void SetVelocity(Vector3 dir, float speed, bool setSpeed = false){
		if (setSpeed)
			SetSpeed (speed);
		myTrans.rotation = Quaternion.LookRotation (dir);
		this.dir = dir;
	}
	public void SetPosition(Vector3 pos){
		myTrans.position = pos;
	}
	public void StartAim(){
		targetPointer.gameObject.SetActive (true);
		gameObject.SetActive (true);
		done = false;
		ps.Play ();
	}
	public void StopAim(){
		ps.Stop ();
		targetPointer.gameObject.SetActive (false);
		gameObject.SetActive (false);
		EchoTarget.singleton.gameObject.SetActive (false);
	}


	public void StopAim(GameObject go){
		if (go == stopper || stopper == null) {
			stopper = null;
			StopAim ();
		}
	}

	public float CalculateMax(float height){

		float asd = (2 * gravity * height + speed * speed) / (2 * gravity * height + 2 * speed * speed);
		float cos = Mathf.Sqrt (asd);
		float sin = Mathf.Sqrt (1 - asd);
		//float sin = speed * Mathf.Sqrt(1 / (2 * gravity * height + 2 * speed * speed));


		float optimalXZSpd = speed * cos;
		float optimalYSpd = speed * sin;
		float optimalTime = (optimalYSpd + Mathf.Sqrt (optimalYSpd * optimalYSpd - 2 * gravity * height)) / gravity;

		Debug.Log ("max " + (optimalXZSpd * optimalTime));
		return optimalXZSpd * optimalTime;
	}

	public void SetTarget(Vector3 pos){
		Vector3 delta = pos - myTrans.position;
		float xzDist = delta.xz ().magnitude;
		if (xzDist == 0)
			return;
		float height = -delta.y;
		float maxXZDist = CalculateMax (height);

		float speed = this.speed;


		float height2 = height * height;
		float height3 = height2 * height;
		float gravity2 = gravity * gravity;
		float xzDist2;
		float xzDist4;

		if (xzDist > maxXZDist) {
			delta = Vector2.ClampMagnitude (delta.xz (), maxXZDist).ToV3AddY (delta.y);
			xzDist = maxXZDist;
			xzDist2 = xzDist * xzDist;
			xzDist4 = xzDist2 * xzDist2;
		} else {
			xzDist2 = xzDist * xzDist;
			xzDist4 = xzDist2 * xzDist2;
			if (xzDist < maxXZDist) {
				//speed = MyExtensions.AbsSqrt (((gravity * height) / (height2 - xzDist2)) * (-height2 + xzDist2 - height * MyExtensions.AbsSqrt (height2 - xzDist2)));
				//speed = Mathf.Sqrt(gravity * (Mathf.Sqrt(height2 + xzDist2) - height));
					Debug.Log ("speed = " + speed);
			}
		}

		float speed2 = speed * speed;
		float speed4 = speed2 * speed2;

		float angleRad = Mathf.Acos (
			              Mathf.Sqrt (
				              ((gravity * height * speed2 * xzDist2)
				              + (speed4 * xzDist2)
				              - Mathf.Sqrt (-speed4 * xzDist4 * (gravity2 * xzDist2 - 2 * gravity * height * speed2 - speed4)))
				              / (speed4 * (height2 + xzDist2)))
			              / MyExtensions.sqrt2);
		Vector3 deltaXZ = new Vector3(delta.x, 0, delta.z);
		Vector3 rotationAxis = Vector3.Cross (deltaXZ, Vector3.up).normalized;
		Quaternion rotation = Quaternion.AngleAxis (angleRad * Mathf.Rad2Deg, rotationAxis);
		 dir = (rotation * deltaXZ).normalized;

		SetVelocity (dir, speed);
	}



	public float time{
		get{
			return 2 * speed * Mathf.Abs (dir.y) / gravity;
		}
	}

	void OnParticleCollision(GameObject other)
	{
		List<ParticleCollisionEvent> events = new List<ParticleCollisionEvent> ();
		int cols = ps.GetCollisionEvents (other, events);
		ParticleCollisionEvent e = events [cols - 1];
		Vector3 pos = e.intersection;
		if (collisionHandler != null)
			collisionHandler (pos);
		//cps.Emit (pos, Vector3.zero, cps.startSize * Vector3.Distance (pos, prevCameraPos), cps.startLifetime, cps.startColor);
		targetPointer.position = pos;
		targetPointer.rotation = Quaternion.LookRotation (e.normal, targetPointer.up);
		EchoTarget.singleton.SetMaxSize (tauntRadiusMul * Mathf.Abs(Vector3.Dot(e.normal, e.velocity)));
		if (EchoTarget.singleton.isActiveAndEnabled)
			EchoTarget.singleton.myTrans.position = pos;
		else
			EchoTarget.singleton.ShowTap (pos, Color.red);
	}
}
