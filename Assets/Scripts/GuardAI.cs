using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GuardAI : MonoBehaviour {
	public Transform myTrans = null;

	//public Dictionary<MyController, float> sightings = new Dictionary<MyController, float>();
	//public Dictionary<MyController, float> hearings = new Dictionary<MyController, float>();
	public Dictionary<MyController, float> targets = new Dictionary<MyController, float>();

	public Dictionary<TauntingRock, float> rocks = new Dictionary<TauntingRock, float>();

	public MyController target = null;
	public float targetTime = 0;

	public MyController ctrl = null;

	public Vector3 myFwd = Vector3.forward;
	//public Vector2 myV2Fwd = Vector2.up;
	public Vector2 myV2Pos = Vector2.zero;
	public Vector3 myPos = Vector3.zero;

	public float sightRange = 30;
	public float nearHearRange = 3;
	public float farHearRange = 5;

	public float nonAlertPresenceDetectionRange = 1;
	public float alertPresenceDetectionRange = 2;

	public bool onAlert = false;

	public RaycastHit[] hits = new RaycastHit[25];

	public LayerMask mask;

	public List<Vector3> patrolPoints = new List<Vector3>();

	public int maxPatrolPointsIndex = 0;
	public int currentPatrolPoint = -1;
	public int patrolType = 1;

	public  int searching = 0;

	public Vector3 lastTargetSpeed = Vector3.zero;

	public Vector3 prevTargetPos = Vector3.zero;

	public Vector3 localLookDir = Vector3.forward;
	public Vector3 worldLookDir = Vector3.forward;
	public Vector2 worldV2LookDir = Vector2.up;

	public int rotatingLookDir = 0;
	public Vector3 targetLookDir = Vector3.forward;

	public float lookAngularSpeedRad = 2 * Mathf.PI;

	public Quaternion myRot = Quaternion.identity;

	public float timer = 1;

	Vector3 nextLookTarget = Vector3.zero;


	// Use this for initialization
	void Start () {
		myTrans = transform;
		myTrans.hasChanged = false;
		if (ctrl == null)
			ctrl = GetComponent<MyController> ();
		if (ctrl == null)
			ctrl = GetComponentInParent<MyController> ();
		if (patrolPoints.Count == 0)
			patrolPoints.Add (transform.position);
		maxPatrolPointsIndex = patrolPoints.Count - 1;
	}



	void Update(){
		myPos = myTrans.position;
		myV2Pos = myPos.xz ();

		myRot = myTrans.rotation;
		myFwd = myRot * Vector3.forward;

		if (rotatingLookDir > 0) {
			if (rotatingLookDir == 1) {
				localLookDir = Vector3.RotateTowards (localLookDir, targetLookDir, lookAngularSpeedRad * Time.deltaTime, 0);
				if (localLookDir == targetLookDir)
					rotatingLookDir = 0;
				worldLookDir = myRot * localLookDir;
			} else if (rotatingLookDir == 2) {
				worldLookDir = Vector3.RotateTowards (worldLookDir, targetLookDir, lookAngularSpeedRad * Time.deltaTime, 0);
				worldLookDir = Vector3.RotateTowards (myFwd, worldLookDir, MyExtensions.deg90inRad, 0);
				if (worldLookDir == targetLookDir)
					rotatingLookDir = 0;
				localLookDir = myTrans.InverseTransformDirection(worldLookDir);
			}
		}else {
			
			worldLookDir = myRot * localLookDir;
			if (searching > 4) {
				if (!(ctrl.HasBuff (BuffIndex.blind) || ctrl.HasBuff (BuffIndex.stun))) {
					if (searching == 5) {
						if (timer > 0)
							timer -= Time.deltaTime;
						else
							searching++;
					} else if (searching == 6) {
						//TauntPos (myPos - 2 * myFwd);
						LookTowards (Vector3.forward);
						searching = 8;
					} else if (searching == 7) {
						if (timer > 0)
							timer -= Time.deltaTime;
						else
							searching++;
					} else if (searching == 8) {
						LookTowards (nextLookTarget);
						searching = 9;
					} else if (searching == 9) {
						if (timer > 0)
							timer -= Time.deltaTime;
						else
							searching++;
					} else if (searching == 10) {
						LookTowards (Vector3.forward);
						searching = 0;
					}
				}
			}
		}


		worldV2LookDir = worldLookDir.xz ();

		Debug.DrawLine (myPos, myPos + 5 * worldLookDir, Color.red);


		if (target == null) {
			if (searching > 0) {
				if (searching < 5 && !ctrl.hasPath) {
					LookAround ();
				}
			}else{
				if (patrolType > 0) {
					if (!ctrl.hasPath || Vector3.Distance (patrolPoints [currentPatrolPoint], myPos) < 0.1f) {
						if (currentPatrolPoint < maxPatrolPointsIndex)
							currentPatrolPoint++;
						else {
							if (patrolType == 1)
								patrolType = -1;
							else if (patrolType == 2)
								currentPatrolPoint = 0;
							else if (patrolType == 3) {
								currentPatrolPoint--;
								patrolType = 4;
							}
						}
					}
					ctrl.MoveTo (patrolPoints [currentPatrolPoint]);
				} else if (patrolType == 4) {
					if (!ctrl.hasPath || Vector3.Distance (patrolPoints [currentPatrolPoint], myPos) < 0.1f) {
						if (currentPatrolPoint > 0)
							currentPatrolPoint--;
						else {
							currentPatrolPoint++;
							patrolType = 3;
						}
					}
					ctrl.MoveTo (patrolPoints [currentPatrolPoint]);
				} else if (patrolType == -1) {
					if (!ctrl.hasPath && Vector3.Distance (patrolPoints [currentPatrolPoint], myPos) > 0.1f) {
						ctrl.MoveTo (patrolPoints [maxPatrolPointsIndex]);
					}
				}

			}

			foreach (MyController c in CharManager.registeredChars) {
				if ((searching < 3 || searching > 4) && Vector3.Distance (c.transform.position, myPos) <= presenceDetectionRange) {
					TauntPos (c.myTrans.position, false, 3);
				}
				float suspiciousness = 0;
				if (!ctrl.HasBuff (BuffIndex.blind))
					suspiciousness += SightCheck (c, Time.deltaTime);
				if (!ctrl.HasBuff (BuffIndex.loudNoises))
					suspiciousness += 0.5f * HearCheck (c, Time.deltaTime);

				if (suspiciousness > 0) {
					if (targets.ContainsKey (c))
						targets [c] += suspiciousness;
					else
						targets.Add (c, suspiciousness);
				} else if (targets.ContainsKey (c)) {
					if (onAlert)
						targets [c] -= Time.deltaTime * 0.5f;
					else
						targets [c] -= Time.deltaTime;
				}
			}

			foreach (TauntingRock c in TauntingRock.rocks) {
				float suspiciousness = 0;
				if (!ctrl.HasBuff (BuffIndex.blind))
					suspiciousness += RockSight (c, Time.deltaTime);
				Debug.Log ("suspiciousness " + suspiciousness);

				if ((searching < 3 || searching > 4) && Vector3.Distance (c.transform.position, myPos) <= presenceDetectionRange) {
					if (suspiciousness > 0)
						TauntPos (c.nodes [0], true, 3);
					else
						TauntPos (c.myTrans.position, false, 3);
				}
				if (suspiciousness > 0) {
					if (rocks.ContainsKey (c))
						rocks [c] += suspiciousness;
					else
						rocks.Add (c, suspiciousness);
				} else if (rocks.ContainsKey (c)) {
					if (onAlert)
						rocks [c] -= Time.deltaTime * 0.5f;
					else
						rocks [c] -= Time.deltaTime;
				}
			}
			if (!ctrl.HasBuff (BuffIndex.stun)) {
				if (targets.Count > 0) {
					IEnumerable<KeyValuePair<MyController, float>> targetsKnown = targets.Where (c => (c.Key.gameObject.activeInHierarchy && c.Value >= 1));
					int count = targetsKnown.Count ();
					if (count > 0) {
						KeyValuePair<MyController, float> sighted;
						if (count > 1) {
							sighted = targetsKnown.OrderByDescending (c => c.Value).First ();
						} else {
							sighted = targetsKnown.First ();
						}
						SetTarget (sighted.Key);
					} else if (searching < 2 || searching > 4) {
						IEnumerable<KeyValuePair<MyController, float>> targetsCheck = targets.Where (c => (c.Key.gameObject.activeInHierarchy && c.Value >= 0.5f));
						count = targetsCheck.Count ();
						if (count > 0) {
							KeyValuePair<MyController, float> sighted;
							if (count > 1) {
								sighted = targetsCheck.OrderByDescending (c => c.Value).First ();
							} else {
								sighted = targetsCheck.First ();
							}
							TauntPos (sighted.Key.myTrans.position, false, 2);
						}
					}
				}
				if ((searching < 2 || searching > 4) && rocks.Count > 0) {
					IEnumerable<KeyValuePair<TauntingRock, float>> rocksCheck = rocks.Where (c => (c.Key.gameObject.activeInHierarchy && c.Value >= 0.5f));
					int count = rocksCheck.Count ();
					if (count > 0) {
						KeyValuePair<TauntingRock, float> sighted;
						if (count > 1) {
							sighted = rocksCheck.OrderByDescending (c => c.Value).First ();
						} else {
							sighted = rocksCheck.First ();
						}
						TauntPos (sighted.Key.nodes [0], true, 2);
					}
				}
			}
		} else {
			float vis = 0;
			float dist = Vector3.Distance (target.myTrans.position, myPos);
			if (!ctrl.HasBuff (BuffIndex.stun)) {
				if (dist <= alertPresenceDetectionRange) {
					vis = 1;
				}
				if (vis < 0.2f)
					vis = SightCheck (target, Time.deltaTime) + 0.5f * HearCheck (target, Time.deltaTime);
			}
			if (vis < 0.2f) {
				if (ctrl.target != null)
					ctrl.target = null;
				if (!(ctrl.HasBuff (BuffIndex.blind) || ctrl.HasBuff (BuffIndex.stun))) {
					if (vis > 0 && dist <= sightRange) {
						ctrl.MoveTo (target.myTrans.position);
						lastTargetSpeed = ctrl.lastSpeed;
					} else {
						ctrl.MoveTo (ctrl.targetPos + lastTargetSpeed * Time.deltaTime);
					}
					if (searching < 5 && vis == 0) {
						if (ctrl.nma.path.corners.Length < 3)
							LookAround ();
					} else {
						LookTowards (myTrans.InverseTransformDirection (ctrl.targetPos - myPos));
						if (Vector3.Dot (targetLookDir, Vector3.forward) < 0) {
							if (targetLookDir.x > 0)
								LookTowards (Vector3.right);
							else
								LookTowards (Vector3.left);
						}
					}
				}
				targetTime -= Time.deltaTime * (1 + (0.2f - vis) * 5);
				if (targetTime <= 0) {
					target = null;
					if (ctrl.hasPath)
						searching = 2;
				}
			} else {
				searching = 4;
				LookTowards (ctrl.targetPos - myPos, false);
				lastTargetSpeed = target.lastSpeed;
				targetTime = Mathf.Clamp (targetTime + vis, 0, 5);
				if (ctrl.target == null)
					ctrl.MoveTo (target.myTrans);
			}
		}


	}

	public void LookAround(){
		//TauntPos (myPos - myTrans.right);
		if (localLookDir.x > 0)
			LookTowards (Vector3.right);
		else
			LookTowards (Vector3.left);
		nextLookTarget = -targetLookDir;
		searching = 5;
	}

	public void LookTowards(Vector3 dir, bool local = true){
		timer = 0.75f;
		if (local) {
			targetLookDir = Vector3.RotateTowards (Vector3.forward, dir, MyExtensions.deg90inRad, 0);
			rotatingLookDir = 1;
		} else {
			targetLookDir = dir;
			rotatingLookDir = 2;
		}
	}



	public void SetTarget(MyController c){
		onAlert = true;
		ctrl.RemoveBuff (BuffIndex.hazy);
		target = c;
		targetTime = 5;
		ctrl.MoveTo (target.myTrans);
		searching = 4;
		ctrl.rotating = false;
		targets.Clear ();
	}

	public void TauntPos(Vector3 pos, bool clear = true, int search = 1){
		if (ctrl.HasBuff (BuffIndex.stun))
			return;
		if (search == 2 && ctrl.HasBuff (BuffIndex.blind))
			return;
		target = null;
		ctrl.target = null;
		ctrl.MoveTo (pos);
		searching = search;

		if (clear) {
			targets.Clear ();
		}
	}
	public float HearCheck(MyController c, float dt){
		if (ctrl.HasBuff (BuffIndex.loudNoises) || ctrl.HasBuff(BuffIndex.stun))
			return 0;
		if (c.state == StateIndex.hidden)
			return 0;


		Vector3 tPos = c.myTrans.position;
		Vector3 delta = tPos - myPos;
		Vector3 v2Delta = delta.xz ();
		float v2Dist = v2Delta.magnitude;
		if (v2Dist > farHearRange)
			return 0;
		Vector2 v2Dir = v2Delta / v2Dist;
		float mul = 0;
		bool hear = false;

		if (tPos != c.prevPos) {
			if (c.HasBuff(BuffIndex.water))
				hear = true;
			else if (c.state != 1 || (v2Dist <= nearHearRange && !ctrl.HasBuff(BuffIndex.hazy)))
				hear = true;
		}
		if (hear)
			mul = 1 - hearRolloff (v2Dist);
		else
			return 0;


		int hitCount = Physics.SphereCastNonAlloc (myPos, 1, delta.normalized, hits, delta.magnitude, mask, QueryTriggerInteraction.Collide);
		if (hitCount > 0) {
			for (int i = 0; i < hitCount; i++) {
				if (Obstacle.obstaclesByCollider.ContainsKey (hits [i].collider)) {
					Obstacle o = Obstacle.obstaclesByCollider [hits [i].collider];
					if (c.state == 1)
						mul *= o.hearMulCrouch;
					else
						mul *= o.hearMul;
				}
			}
		} 
		if (c != target) {
			mul *= dt;
			if (onAlert)
				mul *= 5;
		}
		if (ctrl.HasBuff (BuffIndex.hazy))
			mul *= 0.5f;
		return mul;
	}

	public float presenceDetectionRange {
		get{
			if (!onAlert || ctrl.HasBuff (BuffIndex.hazy))
				return nonAlertPresenceDetectionRange;
			else
				return alertPresenceDetectionRange;
		}
	}

	public float SightCheck(MyController c, float dt){
		
		if (ctrl.HasBuff (BuffIndex.blind) || ctrl.HasBuff(BuffIndex.stun))
			return 0;
		if (ctrl.state == StateIndex.hidden)
			return 0;
		Vector3 tPos = c.myTrans.position;
		Vector3 delta = tPos - myPos;
		Vector3 v2Delta = delta.xz ();
		float v2Dist = v2Delta.magnitude;
		Vector2 v2Dir = v2Delta / v2Dist;
		bool hear = false;
		if (Vector2.Angle (v2Dir, worldV2LookDir) > 60)
			return 0;


		float mul = 1 / Mathf.Max (1, v2Dist - sightRange);

		int hitCount = Physics.RaycastNonAlloc (myPos, delta.normalized, hits, delta.magnitude, mask, QueryTriggerInteraction.Collide);
		if (hitCount > 0) {
			for (int i = 0; i < hitCount; i++) {
				if (Obstacle.obstaclesByCollider.ContainsKey (hits [i].collider)) {
					Obstacle o = Obstacle.obstaclesByCollider [hits [i].collider];
					if (c.state == 1)
						mul *= o.sightMulCrouch;
					else
						mul *= o.sightMul;
				}
			}
		} else {
			if (v2Dist > presenceDetectionRange) {
				float relSpd = Vector3.ProjectOnPlane (tPos - c.prevPos, worldLookDir).magnitude / dt / Mathf.Max (1, v2Dist);
				if (relSpd < 1)
					return mul;
				else
					return mul * (2 - relSpd);
			} else {
				return mul;
			}
		}
		if (!onAlert)
			mul *= dt;
		if (ctrl.HasBuff (BuffIndex.hazy))
			mul *= 0.5f;
		return mul;
	}

	public float RockSight(TauntingRock c, float dt){
		if (ctrl.HasBuff (BuffIndex.blind) || ctrl.HasBuff(BuffIndex.stun))
			return 0;
		if (c.done)
			return 0;
		Vector3 tPos = c.myTrans.position;
		Vector3 delta = tPos - myPos;
		float dist = delta.magnitude;
		Vector3 dir;
		if (dist == 0)
			dir = Vector3.zero;
		else
			dir = delta / dist;
		bool hear = false;
		if (Vector3.Angle (delta.normalized, worldLookDir) > 60)
			return 0;


		float mul = 1 / Mathf.Max (1, dist - sightRange);

		int hitCount = Physics.RaycastNonAlloc (myPos, dir, hits, dist, mask, QueryTriggerInteraction.Collide);
		if (hitCount > 0) {
			for (int i = 0; i < hitCount; i++) {
				if (Obstacle.obstaclesByCollider.ContainsKey (hits [i].collider)) {
					Obstacle o = Obstacle.obstaclesByCollider [hits [i].collider];
					mul *= o.sightMul;
				}
			}
		} 
		//mul *= dt;
		if (ctrl.HasBuff (BuffIndex.hazy))
			mul *= 0.5f;
		return mul;
	}

	public float hearRolloff(float dist){
		float a = Mathf.Clamp ((dist - nearHearRange) / (farHearRange - nearHearRange), 0, 1);
		return a * a;
	}
}
