using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TauntingRock : MonoBehaviour {
	public Rigidbody rb = null;
	public Transform myTrans = null;
	Vector3 prevPos = Vector3.zero;

	public bool done = false;

	public LayerMask enemyMask;

	public float tauntRadiusMul = 1;

	public static HashSet<TauntingRock> rocks = new HashSet<TauntingRock> ();

	public List<Vector3> nodes = new List<Vector3>();

	public GameObject echoSphere = null;

	float timer = 0;

	// Use this for initialization
	void Start () {
		if (rb == null)
			rb = GetComponent<Rigidbody> ();
		if (rb == null)
			rb = GetComponentInParent<Rigidbody> ();
		myTrans = transform;
		prevPos = myTrans.position;
	}

	void FixedUpdate () {

		if (done) {
			if (timer > 0)
				timer -= Time.fixedDeltaTime;
			else
				Destroy (gameObject);
		} else {
			Vector3 temp = myTrans.position;
			if (prevPos == temp) {
				done = true;
				timer = 5;
				rocks.Remove (this);
			} else
				prevPos = temp;
		}
	}

	public void Throw(Vector3 startingPoint, Vector3 speed){
		myTrans.position = startingPoint;
		prevPos = startingPoint;
		//rb.velocity = speed; //physics based
		rb.velocity = Vector3.zero;
		rb.AddForce(speed * rb.mass, ForceMode.Impulse);
		done = false;
		rocks.Add (this);
		nodes.Clear ();
		nodes.Add (startingPoint);

		Vector3 pos = startingPoint;

		Vector3 v = rb.velocity;
		Debug.DrawLine (pos, pos + v, Color.red, 5);
		Vector3 xz = Vector3.ProjectOnPlane (v, Vector3.up);
		Debug.DrawLine (pos + xz, pos + v, Color.red, 5);
		Debug.DrawLine (pos, pos + xz, Color.red, 5);
	}

	void OnCollisionEnter(Collision col){
		Vector3 pos = col.contacts [0].point;
		GameObject es = (GameObject)GameObject.Instantiate (echoSphere, pos, Quaternion.identity);

		//float tauntRadius = tauntRadiusMul * Mathf.Abs (col.impulse.magnitude / rb.mass);
		//float tauntRadius = tauntRadiusMul * col.relativeVelocity.magnitude;

		float tauntRadius = tauntRadiusMul * Mathf.Max(Mathf.Abs(Vector3.Dot(col.contacts[0].normal, col.relativeVelocity)), Mathf.Abs(Vector3.Dot(col.impulse.normalized, col.relativeVelocity)));
		es.transform.localScale = Vector3.one * tauntRadius;
		EchoSphere es1 = es.GetComponent<EchoSphere> ();
		es1.SetMaxSize (tauntRadius);
		es1.Start ();
		es1.ShowTap (pos, Color.red);
		Collider[] enemies = Physics.OverlapSphere (pos, tauntRadius, enemyMask, QueryTriggerInteraction.Ignore);

		foreach (Collider c in enemies) {
			GuardAI ai = FindAI (c);
			 if (ai != null && ai.searching < 2)
				ai.TauntPos (pos, true, 1);
		}
		GuardAI ai2 = FindAI (col.collider);
		if (ai2 != null) {
			if (col.impulse.magnitude > 10 && pos.y - ai2.myTrans.position.y > 0.5f && Vector3.Distance (pos, ai2.myTrans.position + Vector3.up) <= 1.2f )
				ai2.ctrl.AddBuff (BuffIndex.stun, 1);
			else
				ai2.TauntPos (pos, true, 3);
		}
		if (Vector3.Distance (pos, nodes [nodes.Count - 1]) > 0.1f) {
			nodes.Add (pos);
			while (nodes.Count > 2)
				nodes.RemoveAt (0);
		}

		CurveCaster.singleton.StopAim (gameObject);
	}

	GuardAI FindAI(Collider c){
		GuardAI ai = c.GetComponent<GuardAI> ();
		if (ai == null)
			ai = c.GetComponentInParent<GuardAI> ();
		if (ai == null)
			ai = c.GetComponentInChildren<GuardAI> ();
		return ai;
	}


}
