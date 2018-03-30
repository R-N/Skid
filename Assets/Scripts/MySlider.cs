using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MySlider : MonoBehaviour {
	public static MySlider singleton = null;
	public Slider slid = null;
	public Action<float> slideHandler = null;

	// Use this for initialization
	void Awake(){
		singleton = this;
		slid = GetComponent<Slider> ();
		slid.minValue = Mathf.Pow(10, -26);
		slid.maxValue = 1;
	}

	void Start () {
		StopSlider ();
	}

	public void Handler(){
		if (enabled) {
			slideHandler (slid.value);
		}
	}

	public void StartSlider(Action<float> handler, float initialValue = 1){
		slid.value = initialValue;
		slideHandler = handler;
		enabled = true;
		gameObject.SetActive (true);
	}

	public void StopSlider(){
		slideHandler = null;
		enabled = false;
		gameObject.SetActive (false);
	}
}
