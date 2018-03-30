using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class BuffIndex{
	public const int invincibility = 0;
	public const int blind = 1;
	public const int hazy = 2;
	public const int water = 3;
	public const int loudNoises = 4;
	public const int sand = 5;
	public const int stun = 6;
	public const int catching = 7;
}

public static class StateIndex{
	public const int run = 0;
	public const int crouch = 1;
	public const int patrol = 2;
	public const int sleeping = 3;
	public const int fall = 5;
	public const int hidden = 6;
	public const int stunned = 7;
	public const int caught = 8;
}

public class MyController : MonoBehaviour {

	public UnityEngine.AI.NavMeshAgent nma = null;
	public CharPanel panel = null;

	public int charId = 0;
	public int rocks = 0;
	public int team = 0;

	public Transform target = null;
	public Transform myTrans = null;
	public Vector3 targetPos = Vector3.zero;

	int pathStatus = 0;

	bool _selected = false;

	Vector3 lastAimPos = Vector3.zero;

	public static MyController selectedPlayer = null;


	public bool selected {
		get {
			return _selected;
		}
		set {
			if (panel != null) {
				panel.selected = value;
				selectedPlayer = this;
			}
			_selected = value;
		}
	}

	bool _isPrisoner = false;

	public bool canMove = true;
	public bool canDoSkill = true;

	public bool isPrisoner{
		get{
			return _isPrisoner;
		}
		set{
			if (value) {
				canMove = false;
				canDoSkill = false;
			}
			_isPrisoner = value;
		}
	}

	public float baseMvSpd = 5;
	public float mvSpd = 5;

	public class ConstantSpeed{
		public Vector3 speed;
		public float timeLeft;
		public ConstantSpeed(){
		}
	}

	public List<ConstantSpeed> speeds = new List<ConstantSpeed> ();

	Vector3 moveDir = Vector3.forward;

	Coroutine destCor = null;

	public Vector3 prevPos = Vector3.zero;

	bool pathEnabled = false;

	public class Buff{
		public int type;
		public float time;
	}

	public Dictionary<int, Buff> buffsByType = new Dictionary<int, Buff>();
	public List<Buff> buffs = new List<Buff>();

	public Vector3 deltaPos = Vector3.zero;
	public Vector3 lastSpeed = Vector3.zero;

	public int state = 0; 

	public bool rotating = false;
	public Quaternion targetRotation = Quaternion.identity;

	public float skillCd = 0;

	public bool aiming = false;

	void Awake (){
		if (nma == null)
			nma = GetComponent<UnityEngine.AI.NavMeshAgent> ();
	}

	public bool HasBuff(int id){
		return buffsByType.ContainsKey (id);
	}

	void Start(){
		myTrans = transform;
		prevPos = transform.position;
		nma.enabled = true;
		nma.speed = mvSpd;
		if (team == 0)
		CharManager.singleton.SpawnChar (this);
	}

	void DoBuffs(){
		canMove = true;
		for (int i = buffs.Count - 1; i >= 0; i--) {
			Buff b = buffs [i];
			switch (b.type) {
			case BuffIndex.water:
				{
					mvSpd = 0.5f * baseMvSpd;
					break;
				}
			case BuffIndex.blind : {
					canMove = false;

					break;
				}
			case BuffIndex.stun : {
					canMove = false;

					break;
				}
			}
		}
		nma.speed = mvSpd * GameManager.timeScale;
	}

	void BuffTimer(float dt){
		for (int i = buffs.Count - 1; i >= 0; i--) {
			Buff b = buffs [i];
			b.time -= dt;
			if (b.time <= 0) {
				RemoveBuff (b.type);
			}
		}
	}

	public void AddBuff(int type, float time){
		Debug.Log ("addbuff " + type);
		if (buffsByType.ContainsKey (type))
			buffsByType [type].time = time;
		else {
			Buff b = new Buff () {
				type = type,
				time = time
			};
			buffsByType.Add (type, b);
			buffs.Add (b);
			DoBuffs ();
		}
		switch (type) {
		case BuffIndex.blind : {
				canMove = false;
				nma.Stop ();
				if (target != null) {
					Transform t = target;
					target = null;
					MoveTo (t);
				}

				break;
			}
		case BuffIndex.stun : {
				canMove = false;
				nma.Stop ();
				if (target != null) {
					Transform t = target;
					target = null;
					MoveTo (t);
				}

				break;
			}
		case BuffIndex.catching:
			{
				//catching anim
				break;
			}
		}
	}

	public void RemoveBuff(int type){
		Debug.Log ("RemoveBuff " + type);
		if (buffsByType.ContainsKey(type)){
			switch (type) {

			case BuffIndex.blind : {
					if (nma.hasPath)
						nma.Resume ();
					else if (target != null)
						MoveTo (target);
					else
						MoveTo (targetPos);
					AddBuff (BuffIndex.hazy, 2);
					break;
				}
			case BuffIndex.stun : {
					if (nma.hasPath)
						nma.Resume ();
					else if (target != null)
						MoveTo (target);
					else
						MoveTo (targetPos);
					AddBuff (BuffIndex.hazy, 2);
					break;
				}
			case BuffIndex.catching : {
					Catch ();
					break;
				}
			}
			buffs.Remove(buffsByType[type]);
			buffsByType.Remove(type);
			DoBuffs ();
		}
	}

	public void Dash(){
		if (moveDir == Vector3.zero)
			Dash (transform.forward);
		else
			Dash (moveDir.normalized);
	}


	public void Dash(Vector3 dir){
		AddConstSpd (dir * mvSpd * 4 , 0.5f);
	}

	public void DirectedDash(Vector3 dashTarget){
		Vector3 dir = Vector3.ClampMagnitude(dashTarget - transform.position, mvSpd * 2); //2 as in 4 * 0.5f
		AddConstSpd (dir * 2, 0.5f); //2 as in 1/0.5f
	}

	public void AddConstSpd(Vector3 spd, float time){
		if (time <= 0)
			return;
		speeds.Add (new ConstantSpeed (){ speed = spd, timeLeft = time });
	}

	public void ConstSpdUpdate(float dt){
		int count = speeds.Count;
		if (count > 0) {
			for (int i = count - 1; i >= 0; i--) {
				ConstantSpeed spd = speeds [i];
				float rdt = Mathf.Min (spd.timeLeft, dt);
				nma.Move (spd.speed * rdt);
				spd.timeLeft -= rdt;
				if (spd.timeLeft <= 0) {
					speeds.RemoveAt (i);
				}
			}
		}
	}

	void Update(){
		DoBuffs ();

		deltaPos = transform.position - prevPos;
		lastSpeed = deltaPos / Time.deltaTime;

		if (skillCd > 0)
			skillCd -= Time.deltaTime;

		if (rotating) {
			Quaternion res = Quaternion.RotateTowards (myTrans.rotation, targetRotation, nma.angularSpeed * Time.deltaTime);
			myTrans.rotation = res;
			if (res == targetRotation)
				rotating = false;
		}

		ConstSpdUpdate (Time.deltaTime * GameManager.timeScale);

		if (nma.hasPath) {
			moveDir = nma.path.corners[1] - nma.path.corners[0];

			if (nma.path.corners.Length == 2){
				Vector3 prevTargetDir = targetPos - prevPos;
				Vector3 proj = Vector3.Project (prevTargetDir, deltaPos);

				if (Vector3.Dot(proj, deltaPos) > 0 && proj.sqrMagnitude <= deltaPos.sqrMagnitude) {
					float sqrMag = (prevTargetDir - proj).sqrMagnitude;
					if (sqrMag <= 4) {
						Stop ();
					}else
						Debug.Log ("sqrMag " + sqrMag);
				}
			}

		}
		if (target != null) {
			if ((targetPos - target.position).sqrMagnitude < 9) {
				SetDestination (target.position);
			} else {
				MoveTo (targetPos);
				SetTarget (targetPos);
			}
		} else if (pathStatus == 1) {
			if (!nma.pathPending) {
				pathStatus = 2;
			}
			SetTarget (nma.pathEndPosition);
		} else if (pathStatus == 2 && !nma.hasPath) {
			Stop ();
		} 


		prevPos = transform.position;
		BuffTimer (Time.deltaTime);
	}

	public void RotateTowards(Quaternion t){
		targetRotation = t;
		rotating =true;
	}



	void LateUpdate(){
		if (selected){
			if (nma.hasPath) {
				CameraView.RefreshPath (nma.path.corners, nma.remainingDistance);
				pathEnabled = true;
			} else if (pathEnabled){
				CameraView.DisablePath ();
				pathEnabled = false;
			}
		}
		Vector3 myPos = myTrans.position;
		Vector3 fwd = myTrans.forward;
		Debug.DrawLine (myPos, myPos + 5 * fwd, Color.green);
		if (aiming)
			AimShoot (lastAimPos);
	}

	public bool hasPath {
		get {
			return nma.hasPath;
		}
	}

	public void MoveTo(Transform trans){
		if (trans != null) {
			target = trans.transform;
			pathStatus = 0;
			//if (!(HasBuff(BuffIndex.blind) || HasBuff(BuffIndex.stun)))
				SetDestination (target.position);
			SetTarget (trans);
		} else {
			Debug.Log ("trans is null");
		}
		if (selected)
			CameraView.SetTarget (trans);
	}

	public void SetTarget (Transform trans){
		if (trans != null) {
			target = trans.transform;
			targetPos = target.position;
			if (selected) {
				CameraView.SetTarget (trans.transform);
			}
		} else {
			target = null;
			targetPos = transform.position;
			if (selected) {
				CameraView.SetTarget (null);
			}
		}
	}

	public void SetTarget (Vector3 pos){
		targetPos = pos;
		if (selected)
			CameraView.SetTarget (pos);
	}




	public void MoveTo(Vector3 dest){
		UnityEngine.AI.NavMeshHit hit;
		if (UnityEngine.AI.NavMesh.SamplePosition (dest, out hit, Mathf.Infinity, UnityEngine.AI.NavMesh.AllAreas)) {
			if (selected)
				CameraView.SetTarget (hit.position);
			
			target = null;
			targetPos = hit.position;

			//if (!(HasBuff(BuffIndex.blind) || HasBuff(BuffIndex.stun)))
				SetDestination (hit.position);
			pathStatus = 1;
		}

	}




	void SetDestination (Vector3 dest){
		if (destCor != null)
			StopCoroutine (destCor);
		destCor = StartCoroutine (SetDestCor (dest));
	}

	IEnumerator SetDestCor (Vector3 dest){
		while (!nma.enabled || !nma.isOnNavMesh || !canMove) {
			yield return new WaitForEndOfFrame ();
		}
		targetPos = dest;
		nma.SetDestination (dest);

		//nma.Resume ();
		nma.isStopped = false;
		destCor = null;
	}

	public void Stop(){
		if (nma.isActiveAndEnabled && nma.isOnNavMesh) {
			nma.Stop ();
			nma.ResetPath ();
		}
		pathStatus = 3;
		target = null;
		targetPos = transform.position;
		if (destCor != null) {
			StopCoroutine (destCor);
			destCor = null;
		}
		SetTarget (null);
		if (selected)
			CameraView.SetTarget (null);
	}

	public void DoSkill(){
		DoSkill (charId);
	}

	public void SwitchState(int state){
		this.state = state;
	}

	public void DoSkill(int id){
		switch (id) {
		case 0:
			{
				CameraView.singleton.kickPointer.SetParent (myTrans);
				CameraView.singleton.kickPointer.localRotation = Quaternion.LookRotation (Vector3.down, Vector3.forward);
				CameraView.singleton.kickPointer.localPosition = new Vector3 (0, -1, 0);
				CameraView.singleton.kickPointer.gameObject.SetActive (true);
				DirectionSampler.singleton.GetDirection ((v) => AimKick (v), (v) => Kick (v), () => CancelKick ());
				break;
			}
		case 1:
			{
				MySlider.singleton.StartSlider ((x) => AdjustShootPower (x));
				CurveCaster.singleton.StopAim ();
				PointSampler.singleton.GetPoint ((v) => AimShoot(v), (v) => Shoot(v), () => CancelShoot(), GameManager.enemyObstacleGroundMask);
				ParticleSystem.CollisionModule cm = CurveCaster.singleton.ps.collision;
				cm.collidesWith = GameManager.enemyObstacleGroundMask;
				CurveCaster.singleton.ps.Play ();

				break;
			}
		case 2:
			{
				//banana peel
				break;
			}
		case 3:
			{
				AddBuff (BuffIndex.catching, 1);
				break;
			}
		}
	}

	public void Catch(){
		Collider[] cols = Physics.OverlapSphere (myTrans.position, 1.5f, GameManager.playerMask, QueryTriggerInteraction.Ignore);
		bool caught = false;
		if (cols.Length > 0) {
			foreach (Collider col in cols) {
				MyController ctrl = col.GetComponent<MyController> ();
				if (ctrl.state != StateIndex.hidden && ctrl.state != StateIndex.caught) {
					ctrl.SwitchState (StateIndex.caught);
					caught = true;
				}
			}
		}
	}

	public void AimKick(Vector2 dir00){
		Vector3 dir0 = dir00.ToV3AddY ();
		CameraView.singleton.kickPointer.rotation = Quaternion.LookRotation (Vector3.down, dir0);
	}

	public void CancelKick(){
		CameraView.singleton.kickPointer.SetParent (null);
		CameraView.singleton.kickPointer.gameObject.SetActive (false);
	}

	public void Kick(Vector2 dir00){
		Vector3 dir0 = dir00.ToV3AddY ();
		myTrans.rotation = Quaternion.LookRotation (dir0, myTrans.up);
		Collider[] cols = Physics.OverlapSphere (myTrans.position, 3, GameManager.unitMask, QueryTriggerInteraction.Ignore);
		RaycastHit[] hits = new RaycastHit[10];
		Vector3 myPos = myTrans.position;
		Debug.Log ("KICK");
		foreach (Collider c in cols) {
			MyController mc = c.GetComponent<MyController> ();
			if (mc == null || mc == this)
				continue;

			Debug.Log ("Passed 1");
			Vector3 dir = (mc.myTrans.position - myPos).RemoveY().normalized;
			if (Vector3.Dot (dir, dir0) < 0.5f)
				continue;
			Debug.Log ("Passed 2");
			RaycastHit hit;
			if (c.Raycast (new Ray (myPos, dir), out hit, 1.5f))
				continue;

			Debug.Log ("Passed 3");
			int count = Physics.RaycastNonAlloc (myPos, dir, hits, 1.5f, GameManager.obstacleMask, QueryTriggerInteraction.Ignore);
			float multiplier = 1;
			for (int i = 0; i < count; i++) {
				multiplier *= hits [i].collider.GetComponent<Obstacle> ().sightMul;
			}
			if (multiplier > 0.1f) {
				Debug.Log ("Passed 4");
				mc.AddBuff (BuffIndex.stun, 2);
				mc.AddBuff (BuffIndex.blind, Mathf.Clamp(multiplier * UnityEngine.Random.Range (4, 6), 3, 6));
			}
		}
		CancelKick ();
	}

	public void AimShoot(Vector3 point){
		if (!CurveCaster.singleton.gameObject.activeInHierarchy) {
			CurveCaster.singleton.StartAim ();
			CurveCaster.singleton.SetSpeed (15);
			aiming = true;
		}
		CurveCaster.singleton.SetPosition (myTrans.position);
		lastAimPos = point;
		CurveCaster.singleton.SetTarget (point);
		RotateTowards (Quaternion.LookRotation ((point - myTrans.position).RemoveY(), myTrans.up));
	}

	public void AdjustShootPower(float x){
		CurveCaster.singleton.SetSpeed (Mathf.Clamp(x * 15, 1, 15));
		AimShoot (lastAimPos);
	}

	public void CancelShoot(){
		if (aiming) {
			MySlider.singleton.StopSlider ();
			rotating = false;
			aiming = false;
			CurveCaster.singleton.done = true;
			CurveCaster.singleton.StopAim ();
		}
	}

	public void Shoot(Vector3 point){
		if (!aiming)
			return;
		//myTrans.rotation = Quaternion.LookRotation (point - myTrans.position, myTrans.up);

		rotating = false;
		aiming = false;
		CurveCaster.singleton.done = true;
		myTrans.rotation = targetRotation;
		//CurveCaster.singleton.SetTarget (point);

		Vector3 pos = CurveCaster.singleton.myTrans.position;
		Vector3 spd = CurveCaster.singleton.myTrans.rotation * Vector3.forward * CurveCaster.singleton.speed;
		TauntingRock rock = ((GameObject)GameObject.Instantiate (GameManager.singleton.rock, pos, CurveCaster.singleton.myTrans.rotation)).GetComponent<TauntingRock> ();
		rock.tauntRadiusMul = CurveCaster.singleton.tauntRadiusMul;
		//rock.Throw (CurveCaster.singleton.myTrans.position, CurveCaster.singleton.dir * CurveCaster.singleton.speed);
		rock.Throw (pos, spd );
		//CurveCaster.singleton.StopAim ();
		CurveCaster.singleton.timer = CurveCaster.singleton.time;
		CurveCaster.singleton.stopper = rock.gameObject;

		Vector3 v = spd;
		Debug.DrawLine (pos, pos + v, Color.red, 5);
		Vector3 xz = Vector3.ProjectOnPlane (v, Vector3.up);
		Debug.DrawLine (pos + xz, pos + v, Color.red, 5);
		Debug.DrawLine (pos, pos + xz, Color.red, 5);

		MySlider.singleton.StopSlider ();
	}



	public void SetPosition(Vector3 pos){
		transform.position = pos;
		nma.Warp (pos);
		Stop ();
	}
}
