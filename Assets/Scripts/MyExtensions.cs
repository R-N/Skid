using System;
using UnityEngine;

public static class MyExtensions{
	public static Vector3 v3Inf = new Vector3 (float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
	public static Vector3 v3NegInf = new Vector3 (float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
	public static float deg90inRad = 0.5f * Mathf.PI;
	public static float sqrt2 = Mathf.Sqrt(2.0f);

	public static float Cos2Sin(float cos){
		return Mathf.Sqrt (1 - cos * cos);
	}

	public static float AbsSqrt(float x){
		return Mathf.Sqrt (Mathf.Abs (x));
	}

	public static float CheapPow(float x, int power){
		float ret = 1;
		for (int i = 0; i < power; i++) {
			ret *= x;
		}
		return ret;
	}

	public static Vector2 xy(this Vector3 v){
		return new Vector2(v.x, v.y);
	}

	public static Vector2 xz (this Vector3 v){
		return new Vector2 (v.x, v.z);
	}


	public static Vector3 RemoveY(this Vector3 v){
		return new Vector3 (v.x, 0, v.z);
	}

	public static Vector3 ToV3AddZ(this Vector2 v, float z = 0){
		return new Vector3 (v.x, v.y, z);
	}
	public static Vector3 ToV3AddY(this Vector2 v, float y = 0){
		return new Vector3 (v.x, y, v.y);
	}

	public static int RotaryClamp(int x, int min, int max){
		int delta = max - min + 1;
		while (x < min)
			x += delta;
		while (x > max)
			x -= delta;
		return x;
	}
}
