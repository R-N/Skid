using UnityEngine;
using System.Collections;

public class EchoSphere : MonoBehaviour {
	public Material mat = null;
	Transform myTrans = null;
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
		hasStarted = true;
		myTrans = transform;
		prevPos = myTrans.position;
		Vector3 curPos = myTrans.position;
		mat.SetVector ("_ParticlePos", new Vector4 (curPos.x, curPos.y, curPos.z, 1));
		gameObject.SetActive (false);
	}

	public void SetMaxSize(float size){
		maxSize = size;
		minSize = maxSize/startSizeDivider;

	}

	public void ShowTap(Vector3 pos, Color newCol){
		elapsedTime = 0;
		progress = 0;
		slider = -0.5f;
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
		if (progress < 1) {
			Vector3 curPos = myTrans.position;
			elapsedTime = Mathf.Clamp (elapsedTime + Time.deltaTime * GameManager.timeScale, 0, animDuration);
			progress = elapsedTime / animDuration;
			if (elapsedTime < fadeInDuration) {
				mat.SetColor ("_TintColor", new Color (col.r, col.g, col.b, Mathf.Lerp (startAlpha, col.a, elapsedTime / fadeInDuration)));
			} else {
				if (doneFadeIn)
					mat.SetColor ("_TintColor", new Color (col.r, col.g, col.b, Mathf.Lerp (col.a, 0, (fadeOutDuration - animDuration + elapsedTime) / fadeOutDuration)));
				else {
					doneFadeIn = true;
					mat.SetColor ("_TintColor", col);
				}

			}
			//slider = -progress * 3.3f;
			slider = -progress * 1.3f;
			/*while (slider > 1) {
				slider -= 2;
			}*/
			while (slider < -1) {
				slider += 2;
			}
			mat.SetFloat ("_Slider", slider);
			myTrans.localScale = Vector3.one * Mathf.Lerp (minSize, maxSize, Mathf.Sqrt(progress));

			if (myTrans.hasChanged) {
				if (curPos != prevPos) {
					mat.SetVector ("_ParticlePos", new Vector4 (curPos.x, curPos.y, curPos.z, 1));
					prevPos = curPos;
				}
			}
		} else {
			Destroy (gameObject);
		}

	}
}
