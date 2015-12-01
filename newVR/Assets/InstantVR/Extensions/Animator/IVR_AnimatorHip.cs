/* InstantVR Animator hip
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.8
 * date: June 26, 2015
 * 
 * - Fixed calibration issue
 */


using UnityEngine;
using System.Collections;

public class IVR_AnimatorHip : IVR_Controller {

	public bool followHead = true;
	public enum Rotations {
		NoRotation = 0,
		HandRotation = 1,
		LookRotation = 2
	};
	public Rotations rotationMethod = Rotations.HandRotation;

	[HideInInspector] private Vector3 headStartPosition;
	[HideInInspector] private Vector3 startHip2Head;

	void Start() {}

	public override void StartController(InstantVR ivr) {
		base.StartController(ivr);

		present = true;
		headStartPosition = ivr.headTarget.position - ivr.BasePosition;
		startHip2Head = ivr.hipTarget.position - ivr.headTarget.position;
	}
	
	public override void UpdateController() {
		if (this.enabled) {
			if (followHead)
				FollowHead();

			switch (rotationMethod) {
			case Rotations.HandRotation:
				HandRotation();
				break;
			case Rotations.LookRotation:
				LookRotation();
				break;
			}

			if (tracking == false) {
				Calibrate(true);
				tracking = true;
			}
			base.UpdateController();
		} else
			tracking = false;
	}

	private void FollowHead() {
		Vector3 headDelta = (ivr.headTarget.position - ivr.BasePosition) - headStartPosition;

		Vector3 hip2head = ivr.hipTarget.position - ivr.headTarget.position;
		Vector3 delta = hip2head - startHip2Head;

		if (delta.magnitude > 0.01f) {
			this.position = headDelta;
		}
	}

	private void HandRotation() {
		float dOrientation = 0;

		if (ivr.LeftHandController != null && ivr.RightHandController != null && ivr.LeftHandController.isTracking() && ivr.RightHandController.isTracking()) {
			float dOrientationL = AngleDifference(ivr.hipTarget.eulerAngles.y, ivr.leftHandTarget.eulerAngles.y);
			float dOrientationR = AngleDifference(ivr.hipTarget.eulerAngles.y, ivr.rightHandTarget.eulerAngles.y);

			if (Mathf.Sign(dOrientationL) == Mathf.Sign(dOrientationR)) {
				if (Mathf.Abs(dOrientationL) < Mathf.Abs(dOrientationR))
					dOrientation = dOrientationL;
				else
					dOrientation = dOrientationR;
			}
			if (dOrientation < 90 || dOrientation > 270) { // max turning speed = 90 degrees per update	
				float neckOrientation = NormalizeAngle(ivr.headTarget.eulerAngles.y - ivr.hipTarget.eulerAngles.y - dOrientation);
				
				if (neckOrientation < 90 && neckOrientation > -90) { // head cannot turn more than 90 degrees
					this.rotation *= Quaternion.AngleAxis(dOrientation, Vector3.up);
				}
			}
		} else {
			this.rotation = Quaternion.identity;
			this.position = Vector3.zero;
		}
	}

	private void LookRotation() {
		float deltaY = this.rotation.eulerAngles.y - ivr.headTarget.eulerAngles.y;
		this.rotation = Quaternion.Euler(
			this.rotation.eulerAngles.x,
			ivr.headTarget.eulerAngles.y,
			this.rotation.eulerAngles.z);
		if (ivr.HeadController != null)
			ivr.HeadController.rotation *= Quaternion.AngleAxis(deltaY, Vector3.down);
	}

	public override void OnTargetReset() {
	}


	private float AngleDifference(float a, float b) {
		float r = b - a;
		return NormalizeAngle(r);
	}

	private float NormalizeAngle(float a) {
		while (a < -180) a += 360;
		while (a > 180) a -= 360;
		return a;
	}

	void OnTriggerStay() {
		if (ivr != null && ivr.hit == false)
			OnTriggerEnter();
	}

	void OnTriggerEnter() {
		if (ivr != null) {
			ivr.hit = false;
			Vector3 dir = ivr.inputDirection;
			Vector3 origin = transform.position - (dir.normalized * 0.1f);
			CapsuleCollider sc = ivr.hipTarget.GetComponent<CapsuleCollider>();
			RaycastHit[] hits = Physics.SphereCastAll(origin, sc.radius, dir, 0.35f,  ~LayerMask.GetMask("MyBody"));
			for (int i = 0; i < hits.Length && ivr.hit == false; i++) {
				if (hits[i].rigidbody == null) {
					ivr.hit = true;
					ivr.hitNormal = hits[i].normal;
				}
			}
		}
	}
	
	void OnTriggerExit() {
		ivr.hit = false;
	}

}
