using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Rope : NetworkBehaviour {

	public GameObject ropePart;
	public GameObject ropeHook;
	public int ropeCount = 10;
	public float ropeLength = 1f;
	public float ropeWidth = 0.03f;

	public Rigidbody RBody;

	internal void Start()
	{

		if (!isServer) {
			Debug.Log ("DESTROY_ROPE");
			Destroy (this);
		}
		Vector3 savePos = this.gameObject.transform.position;

		float ropePartLength = ropeLength / ropeCount;
		int childCount = this.transform.childCount;
		int zCount = 0;

		ropePart.transform.localScale = new Vector3 (ropeWidth, ropePartLength, ropeWidth);

		for (int i = 1; i <= ropeCount; i++) {
			GameObject tmpRope = GameObject.Instantiate (ropePart, new Vector3 (ropePart.transform.position.x, ropePart.transform.position.y + (-2f * i * ropePartLength), ropePart.transform.position.z), ropePart.transform.localRotation, this.gameObject.transform) as GameObject;
			if (i == ropeCount) {
				GameObject tmpHook = GameObject.Instantiate (ropeHook, new Vector3 (ropePart.transform.position.x, ropePart.transform.position.y + (-2f * i * ropePartLength), ropePart.transform.position.z), ropePart.transform.localRotation, this.gameObject.transform) as GameObject;
			}
		}
			
		for (int i = 1; i <= ropeCount+2; i++) {

			Transform t = this.transform.GetChild (i);

			t.gameObject.AddComponent<HingeJoint> ();
			HingeJoint hinge = t.gameObject.GetComponent<HingeJoint> ();
			hinge.connectedBody = i == 0 ? this.RBody : this.transform.GetChild (i - 1).GetComponent<Rigidbody> ();

			hinge.useSpring = true;
			hinge.enableCollision = true;
		}
	}﻿
}
