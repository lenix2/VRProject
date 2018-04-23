using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public class Wind : NetworkBehaviour {

	private float windforce = 10f;

	// Use this for initialization
	void Start () {
		if (!isServer) {
			windforce = 1f;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public Vector3 getWindForce () {
		return this.gameObject.transform.forward * windforce * Random.Range(0.8f, 1.2f);
	}
}
