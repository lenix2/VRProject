using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TurnCrane : NetworkBehaviour {

	public int dir = 1;
	public Transform crane;
	public int turnspeed = 10;

	void OnTriggerStay(Collider collision) {
		if (collision.gameObject.layer == 9) {
			crane.Rotate (new Vector3 (0f, turnspeed * Time.deltaTime * dir, 0f));
		}
	}
}
