using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BananaPeel : MonoBehaviour {


	void OnCollisionEnter(Collision col){
		MyController ctrl = col.collider.GetComponent<MyController> ();
		if (ctrl != null) {
			ctrl.AddBuff (BuffIndex.stun, 2);
			ctrl.SwitchState (StateIndex.fall);
		}
	}
}
