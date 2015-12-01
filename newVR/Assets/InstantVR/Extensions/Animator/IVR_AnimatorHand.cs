/* InstantVR Animator hand
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.7
 * date: June 5, 2015
 * 
 * - Fixed hand position with lookrotation
 * - Added arm swing switch
 */

using UnityEngine;
using System.Collections;

public class IVR_AnimatorHand : IVR_Controller {
	public bool followHip = true;
	public bool armSwing = true;

	[HideInInspector] private IVR_AnimatorHip animatorHip;

	[HideInInspector] private Vector3 lastHipPosition;
	[HideInInspector] private Vector3 animatorHipStartPosition;
	[HideInInspector] private float animatorHipStartYRotation;
	[HideInInspector] private Vector3 hip2hand, foot2hand;

	public override void StartController(InstantVR ivr) {
		base.StartController(ivr);
		present = true;
	
		animatorHip = ivr.hipTarget.GetComponent<IVR_AnimatorHip>();
		lastHipPosition = ivr.hipTarget.position;
		animatorHipStartPosition = ivr.hipTarget.position;
		animatorHipStartYRotation = ivr.hipTarget.eulerAngles.y;

		hip2hand = animatorHipStartPosition - this.startPosition;
		if (this.transform == ivr.leftHandTarget) {
			foot2hand = Quaternion.Inverse(ivr.BaseRotation) * (ivr.leftHandTarget.position - ivr.rightFootTarget.position);
		} else {
			foot2hand = Quaternion.Inverse(ivr.BaseRotation) * (ivr.rightHandTarget.position - ivr.leftFootTarget.position);
		}
	}

	public override void UpdateController() {
		if (this.enabled) {
			if (followHip) {
				FollowHip();
				if (animatorHip.isTracking() && tracking == false) {
					Calibrate(true);
					tracking = true;
				}
				if (armSwing)
					ArmSwingAnimation();
			} else {
				if (tracking == false) {
					Calibrate(true);
					tracking = true;
				}
			}

			base.UpdateController();
		} else
			tracking = false;
	}

	private void FollowHip() {
		if (animatorHip != null) {
			float deltaRot = ivr.hipTarget.eulerAngles.y - animatorHipStartYRotation;
			Vector3 newHip2hand = Quaternion.AngleAxis(deltaRot, Vector3.up) * hip2hand;
			this.position = hip2hand -  newHip2hand;

			this.rotation = Quaternion.Euler(0,	deltaRot, 0);
		}
	}

	protected void ArmSwingAnimation() {
		Vector3 curSpeed = ivr.hipTarget.InverseTransformDirection(ivr.hipTarget.position - lastHipPosition) / Time.deltaTime;
		float curSpeedZ = curSpeed.z;

		lastHipPosition = ivr.hipTarget.position;
		
		if (curSpeedZ < 0.01f || curSpeedZ > 0.01f) {
			float deltaRot = ivr.hipTarget.eulerAngles.y - animatorHipStartYRotation;
			Quaternion hipRotation = Quaternion.AngleAxis(deltaRot, Vector3.up);

			Vector3 projectFoot2Hand;
			float localFootZ;
			if (this.transform == ivr.leftHandTarget) {
				localFootZ = ivr.hipTarget.InverseTransformPoint(ivr.rightFootTarget.position).z;
				projectFoot2Hand = ivr.rightFootTarget.position + hipRotation * ivr.BaseRotation * foot2hand;

				this.rotation *= Quaternion.AngleAxis((localFootZ * 160 + 10), Quaternion.Inverse(hipRotation * ivr.BaseRotation) * this.transform.up);
			} else {
				localFootZ = ivr.hipTarget.InverseTransformPoint(ivr.leftFootTarget.position).z;
				projectFoot2Hand = ivr.leftFootTarget.position + hipRotation * ivr.BaseRotation * foot2hand;

				this.rotation *= Quaternion.AngleAxis((localFootZ * 160 + 10), Quaternion.Inverse(hipRotation * ivr.BaseRotation) * -this.transform.up);
			}

			Vector3 newPosition = Quaternion.Inverse(ivr.BaseRotation) * (projectFoot2Hand - startPosition - ivr.BasePosition);
			this.position = new Vector3(newPosition.x, localFootZ / 2 + 0.02f, newPosition.z) - referencePosition;
		}
	}

	public override void OnTargetReset() {
	}
	
}
