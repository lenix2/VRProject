using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ReactToWind : NetworkBehaviour {

	internal Rigidbody RBody;
	public Wind wind;

	// Use this for initialization
	void Start () {
		this.RBody = this.gameObject.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		RBody.AddForce (
			wind.getWindForce() * 
			Time.deltaTime, 
			ForceMode.Force);
	}
}
