using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class SimpleCharacterControl : NetworkBehaviour {

    private enum ControlMode
    {
        Tank,
        Direct
    }

    [SerializeField] private float m_moveSpeed = 2;
    [SerializeField] private float m_turnSpeed = 200;
    [SerializeField] private float m_jumpForce = 4;
    [SerializeField] private Animator m_animator;
    [SerializeField] private Rigidbody m_rigidBody;
	[SerializeField] private Transform m_Head;

    [SerializeField] private ControlMode m_controlMode = ControlMode.Direct;

    private float m_currentV = 0;
    private float m_currentH = 0;

    private readonly float m_interpolation = 10;
    private readonly float m_walkScale = 0.33f;
    private readonly float m_backwardsWalkScale = 0.16f;
    private readonly float m_backwardRunScale = 0.66f;

    private bool m_wasGrounded;
    private Vector3 m_currentDirection = Vector3.zero;

    private float m_jumpTimeStamp = 0;
    private float m_minJumpInterval = 0.25f;

    private bool m_isGrounded;
    private List<Collider> m_collisions = new List<Collider>();

	private Transform mainCam;

	public float m_lookSpeedH = 2.0f;
	public float m_lookSpeedV = 2.0f;

	private float yaw = 0.0f;
	private float pitch = 0.0f;

	private bool canClimb = false;

	void Start() {
		if (!isLocalPlayer) {
			Destroy (this);
			return;
		}

		Cursor.visible = false;
		mainCam = Camera.main.transform;

		m_Head.localScale = new Vector3 (0f, 0f, 0f);
	}

	void OnTriggerEnter(Collider collision) {
		if (collision.gameObject.layer == 8) {
			canClimb = true;
			m_rigidBody.useGravity = false;
		}
	}

	void OnTriggerExit(Collider collision) {
		if (collision.gameObject.layer == 8) {
			canClimb = false;
			m_rigidBody.useGravity = true;
		}
	}

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        for(int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                if (!m_collisions.Contains(collision.collider)) {
                    m_collisions.Add(collision.collider);
                }
                m_isGrounded = true;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        bool validSurfaceNormal = false;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                validSurfaceNormal = true; break;
            }
        }

        if(validSurfaceNormal)
        {
            m_isGrounded = true;
            if (!m_collisions.Contains(collision.collider))
            {
                m_collisions.Add(collision.collider);
            }
        } else
        {
            if (m_collisions.Contains(collision.collider))
            {
                m_collisions.Remove(collision.collider);
            }
            if (m_collisions.Count == 0) { m_isGrounded = false; }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(m_collisions.Contains(collision.collider))
        {
            m_collisions.Remove(collision.collider);
        }
        if (m_collisions.Count == 0) { m_isGrounded = false; }
    }

	void Update () {
        m_animator.SetBool("Grounded", m_isGrounded);

        switch(m_controlMode)
        {
            case ControlMode.Direct:
                DirectUpdate();
                break;

            case ControlMode.Tank:
                TankUpdate();
                break;

            default:
                Debug.LogError("Unsupported state");
                break;
        }

        m_wasGrounded = m_isGrounded;

		yaw += m_lookSpeedH * Input.GetAxis("Mouse X") * Time.deltaTime * 100;
		pitch -= m_lookSpeedV * Input.GetAxis("Mouse Y") * Time.deltaTime * 100;

		if (pitch < -60) {
			pitch = -60;
		}

		if (pitch > 60) {
			pitch = 60;
		}

		mainCam.eulerAngles = new Vector3(pitch, yaw, 0.0f);
		Vector3 pos = this.transform.position;

		pos.y = pos.y + 0.8f;
		mainCam.position = pos;
    }

    private void TankUpdate()
    {
		float v = Input.GetAxis ("Vertical");
		float h = Input.GetAxis ("Horizontal");

		bool walk = Input.GetKey (KeyCode.LeftShift);

		if (v < 0) {
			if (walk) {
				v *= m_backwardsWalkScale;
				h *= m_backwardsWalkScale;
			} else {
				v *= m_backwardRunScale;
				h *= m_backwardRunScale;
			}
		} else if (walk) {
			v *= m_walkScale;
			h *= m_backwardsWalkScale;
		}
			
		m_currentV = Mathf.Lerp (m_currentV, v, Time.deltaTime * m_interpolation);
		m_currentH = Mathf.Lerp (m_currentH, h, Time.deltaTime * m_interpolation);

		transform.position += transform.forward * m_currentV * m_moveSpeed * Time.deltaTime;
		transform.position += transform.right * m_currentH * m_moveSpeed * Time.deltaTime;
		transform.eulerAngles = new Vector3(0.0f , yaw, 0.0f);

		if (canClimb) {
			transform.position += transform.up * m_currentV * m_moveSpeed * Time.deltaTime;
		}

		if (m_currentV < 0.01f && m_currentV > -0.01f) {
			float tmpH = m_currentH;

			if (tmpH < 0) {
				tmpH *= -1;
			}

			m_animator.SetFloat ("MoveSpeed", tmpH);
		} else {
			m_animator.SetFloat ("MoveSpeed", m_currentV);
		}

		JumpingAndLanding ();


		if (Input.GetAxis("Wave") > 0.5f) {
			m_animator.SetTrigger("Wave");
		}
    }

    private void DirectUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        Transform camera = Camera.main.transform;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            v *= m_walkScale;
            h *= m_walkScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        Vector3 direction = camera.forward * m_currentV + camera.right * m_currentH;

        float directionLength = direction.magnitude;
        direction.y = 0;
        direction = direction.normalized * directionLength;

        if(direction != Vector3.zero)
        {
            m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);

            transform.rotation = Quaternion.LookRotation(m_currentDirection);
            transform.position += m_currentDirection * m_moveSpeed * Time.deltaTime;

            m_animator.SetFloat("MoveSpeed", direction.magnitude);
        }

        JumpingAndLanding();
    }

    private void JumpingAndLanding()
    {
        bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

        if (jumpCooldownOver && m_isGrounded && Input.GetKey(KeyCode.Space))
        {
            m_jumpTimeStamp = Time.time;
            m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
        }

        if (!m_wasGrounded && m_isGrounded)
        {
            m_animator.SetTrigger("Land");
        }

        if (!m_isGrounded && m_wasGrounded)
        {
            m_animator.SetTrigger("Jump");
        }
    }
}
