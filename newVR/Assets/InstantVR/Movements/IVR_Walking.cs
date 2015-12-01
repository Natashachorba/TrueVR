/* InstantVR walking
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.8
 * date: June 26, 2014
 * 
 * - changes to support Free edition
 */

using UnityEngine;
using System.Collections;

public class IVR_Walking : MonoBehaviour {
	[HideInInspector] private InstantVR ivr;
	
	public bool walking = true;
	public bool sidestepping = true;
	public bool rotating = false;
	public float rotationSpeedRate = 60;

	public bool proximitySpeed = true;
	public float proximitySpeedRate = 0.8f;

	[HideInInspector] private IVR_Input leftInput;
	[HideInInspector] private CapsuleCollider bodyCapsule;
	
	void Start() {
		ivr = this.GetComponent<InstantVR>();

		leftInput = ivr.leftHandTarget.GetComponent<IVR_Input>();
		bodyCapsule = AddHipCollider(ivr.hipTarget.gameObject);
	}
	
	private CapsuleCollider AddHipCollider(GameObject hipGameObject) {
		Rigidbody rb = hipGameObject.AddComponent<Rigidbody>();
		if (rb != null) {
			rb.mass = 1;
			rb.useGravity = false;
			rb.isKinematic = true;
		}
		
		CapsuleCollider collider = hipGameObject.AddComponent<CapsuleCollider>();
		if (collider != null) {
			collider.isTrigger = true;
			collider.radius = 1f;
			collider.height = 0.90f;
			collider.center = new Vector3(-hipGameObject.transform.localPosition.x, 0, -hipGameObject.transform.localPosition.z);
		}

		return collider;
	}

	public void Update() {
		if (leftInput != null) {
			Vector3 movement = CheckMovement ();
			ivr.MoveMe (movement);
			float angle = CheckRotation ();
			ivr.RotateMe (angle);
		}
	}

	private float CheckRotation() {
		if (rotating && leftInput != null) {
			float yRotation = leftInput.yAngle;
			
			if (yRotation != 0) {
				float dOrientation = (yRotation / 90) * (rotationSpeedRate * Time.deltaTime);
				return dOrientation;
			}
		}
		return 0;
	}

	private float curProximitySpeed = 1;
	private Vector3 directionVector = Vector3.zero;
	
	private Vector3 CheckMovement() {
		float maxAcceleration = 0;
		float sidewardSpeed = 0;
		
		float horizontal = 0;
		float vertical = leftInput.stickVertical;
		float forwardSpeed = Mathf.Min(1.0f, Mathf.Abs(vertical));
		
		if (proximitySpeed)
			curProximitySpeed = CalculateProximitySpeed(bodyCapsule, curProximitySpeed);
		
		if (walking) {
			if (forwardSpeed != 0 || directionVector.z != 0) {
				forwardSpeed = forwardSpeed * forwardSpeed;
				forwardSpeed *= Mathf.Sign(vertical);
				if (vertical < 0)
					forwardSpeed *= 0.6f;
				
				if (proximitySpeed)
					forwardSpeed *= curProximitySpeed;
				
				float acceleration = forwardSpeed - directionVector.z;
				maxAcceleration = 1f * Time.deltaTime;
				acceleration = Mathf.Clamp(acceleration, -maxAcceleration, maxAcceleration);
				forwardSpeed = directionVector.z + acceleration;
			}
		}
		
		if (sidestepping) {
			horizontal = leftInput.stickHorizontal;
			sidewardSpeed = Mathf.Min(1.0f, Mathf.Abs(horizontal));
			
			if (sidewardSpeed != 0 || directionVector.x != 0) {
				sidewardSpeed = sidewardSpeed * sidewardSpeed;
				sidewardSpeed *= Mathf.Sign(horizontal) * 0.5f;
				
				if (proximitySpeed)
					sidewardSpeed *= curProximitySpeed;
				
				float acceleration = sidewardSpeed - directionVector.x;
				maxAcceleration = 1f * Time.deltaTime;
				acceleration = Mathf.Clamp(acceleration, -maxAcceleration, maxAcceleration);
				sidewardSpeed = directionVector.x + acceleration;
			}
		}
		directionVector = new Vector3(sidewardSpeed, 0, forwardSpeed);
		Vector3 worldDirectionVector = ivr.hipTarget.TransformDirection(directionVector);
		ivr.inputDirection = worldDirectionVector;
		
		if (curProximitySpeed <= 0.15f) {
			float angle = Vector3.Angle(worldDirectionVector, ivr.hitNormal);
			if (angle > 90.1) {
				directionVector = Vector3.zero;
				worldDirectionVector = Vector3.zero;
			}
		}
		
		return worldDirectionVector;
	}
	
	protected float CalculateProximitySpeed(CapsuleCollider cc, float curProximitySpeed) {
		if (ivr.hit) {
			if (cc.radius > 0.25f) {
				if (Physics.CheckCapsule(ivr.hipTarget.position + (cc.radius - 1) * Vector3.up, ivr.hipTarget.position - (cc.radius - 1) * Vector3.up, cc.radius - 0.05f, ~LayerMask.GetMask("MyBody"))) {
					cc.radius -= 0.05f / proximitySpeedRate;
					cc.height += 0.05f / proximitySpeedRate;
					curProximitySpeed = EaseIn(1, (-0.90f), 1 - cc.radius, 0.75f);
				}
			}
		} else if (curProximitySpeed < 1) {
			if (!Physics.CheckCapsule(ivr.hipTarget.position + (cc.radius - 0.95f) * Vector3.up, ivr.hipTarget.position - (cc.radius - 0.95f) * Vector3.up, cc.radius, ~LayerMask.GetMask("MyBody"))) {
				cc.radius += 0.05f / proximitySpeedRate;
				cc.height -= 0.05f / proximitySpeedRate;
				curProximitySpeed = EaseIn(1, (-0.90f) , 1 - cc.radius, 0.75f);
			}
		} 

		return curProximitySpeed;
	}                            
	
	private static float EaseIn(float start, float distance, float elapsedTime, float duration) {
		// clamp elapsedTime so that it cannot be greater than duration
		elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
		return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
	}
}