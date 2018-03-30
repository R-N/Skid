using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public static Dictionary<int, int> playerIdByConnectionId = new Dictionary<int, int>();
	public class PlayerInfo{
		public int points = 0;
		public List<int> ctrlIds = new List<int>();
	}
	public static Dictionary<int, PlayerInfo> playerInfoByPlayerId = new Dictionary<int, PlayerInfo>();
	public static Dictionary<int, Transform> fortByPlayerId = new Dictionary<int, Transform>();
	public static Dictionary<int, MyController> ctrlByCtrlId = new Dictionary<int, MyController>();
	public static int myPlayerId = 0;
	public static LayerMask worldCanvasMask;
	public static LayerMask playerMask;
	public static LayerMask enemyMask;
	public static LayerMask unitMask;
	public static LayerMask unitAndGroundMask;
	public static LayerMask unitGroundObstacleMask;
	public static LayerMask enemyAndObstacleMask;
	public static LayerMask enemyObstacleGroundMask;
	public static LayerMask obstacleMask;
	public static LayerMask obstacleGroundMask;
	public static GameManager singleton = null;
	public static float timeScale = 1.0f;
	public GameObject rock = null;
	public static PlayerInfo myPlayerInfo{
		get{
			return playerInfoByPlayerId [myPlayerId];
		}
	}

	void Start(){
		singleton = this;
	}

	public GameObject player = null;

	public void SpawnChar(){
		GameObject ply = (GameObject)Instantiate (player, new Vector3 (Random.Range (-10, 10), 1, Random.Range (-10, 10)), Quaternion.identity);
	}

}
