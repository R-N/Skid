using UnityEngine;
using System.Collections;

public class EchoTarget : MonoBehaviour {
	public static EchoTarget singleton = null;
	public Material mat = null;
	public Transform myTrans = null;
	Vector3 prevPos = Vector3.zero;

	float elapsedTime = 0;
	float progress = 1;
	float slider = 0;
	float maxSize = 5;
	public float startSizeDivider = 3f;
	float minSize = 5f / 3f;
	public float animDuration = 1;
	public float startAlphaMul = 0;
	float startAlpha = 0;
	public float fadeInDuration = 0.1f;
	public float fadeOutDuration = 0.5f;
	Color col = Color.white;
	bool doneFadeIn = true;
	// Use this for initialization
	bool hasStarted = false;
	public void Start () {
		if (hasStarted)
			return;
		singleton = this;
		hasStarted = true;
		myTrans = transform;
		prevPos = myTrans.position;
		Vector3 curPos = myTrans.position;
		mat.SetVector ("_ParticlePos", new Vector4 (curPos.x, curPos.y, curPos.z, 1));
		gameObject.SetActive (false);
		SetMaxSize (maxSize);
	}

	public void SetMaxSize(float size){
		maxSize = size;
		minSize = maxSize/startSizeDivider;
		myTrans.localScale = Vector3.one * minSize;
	}

	public void ShowTap(Vector3 pos, Color newCol){
		while (elapsedTime > 0)
			elapsedTime -= animDuration;
		progress = elapsedTime / animDuration;
		slider = -progress * 1.3f;
		myTrans.position = pos;
		col = newCol;
		myTrans.localScale = Vector3.one * minSize;
		gameObject.SetActive (true);
		if (fadeInDuration > 0 && startAlphaMul < 1 && startAlphaMul >= 0) {
			startAlpha = col.a * startAlphaMul;
			doneFadeIn = false;
		} else {
			startAlpha = col.a;
		}
		mat.SetColor ("_TintColor", new Color(col.r, col.g, col.b, startAlpha));
	}

	// Update is called once per frame
	void LateUpdate () {
		
			Vector3 curPos = myTrans.position;
		elapsedTime += Time.deltaTime * GameManager.timeScale;
		while (elapsedTime > animDuration)
			elapsedTime -= animDuration;
			progress = elapsedTime / animDuration;
			mat.SetColor ("_TintColor", col);

			//slider = -progress * 3.3f;
		slider = -progress * 2;
			/*while (slider > 1) {
				slider -= 2;
			}*/
			while (slider < -1) {
				slider += 2;
			}
			mat.SetFloat ("_Slider", slider);

			if (myTrans.hasChanged) {
				if (curPos != prevPos) {
					mat.SetVector ("_ParticlePos", new Vector4 (curPos.x, curPos.y, curPos.z, 1));
					prevPos = curPos;
				}
			}

	}
}
