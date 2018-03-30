using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerMaskSetter : MonoBehaviour {
	public  LayerMask worldCanvasMask;
	public  LayerMask playerMask;
	public  LayerMask enemyMask;
	public  LayerMask unitMask;
	public  LayerMask unitAndGroundMask;
	public  LayerMask unitGroundObstacleMask;
	public  LayerMask enemyAndObstacleMask;
	public  LayerMask enemyObstacleGroundMask;
	public  LayerMask obstacleMask;
	public  LayerMask obstacleGroundMask;
	// Use this for initialization
	void Awake () {
		GameManager.worldCanvasMask = worldCanvasMask;
		GameManager.playerMask = playerMask;
		GameManager.enemyMask = enemyMask;
		GameManager.unitMask = unitMask;
		GameManager.unitAndGroundMask = unitAndGroundMask;
		GameManager.enemyAndObstacleMask = enemyAndObstacleMask;
		GameManager.unitGroundObstacleMask = unitGroundObstacleMask;
		GameManager.enemyObstacleGroundMask = enemyObstacleGroundMask;
		GameManager.obstacleMask = obstacleMask;
		GameManager.obstacleGroundMask = obstacleGroundMask;
	}

}
