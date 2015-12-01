/* InstantVR Animator foot
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.2
 * date: April 10, 2015
 * 
 * - Name changed to IVR_AnimatorFoot
 */

using UnityEngine;
using System.Collections;

public class IVR_AnimatorFoot : IVR_Controller {

	[HideInInspector] private bool isLeftFoot;

	[HideInInspector] IVR_AnimatorFoot leftFoot, rightFoot;

	[HideInInspector] protected IVR_Animate leftFootAnimation, rightFootAnimation;
	[HideInInspector] protected Vector3 startHipFootL, startHipFootR;
	[HideInInspector] protected Vector3 startFootLposition, startFootRposition;

	[HideInInspector] Vector3 stepStart = Vector3.zero;

	void Start() {}

	public override void StartController(InstantVR ivr) {
		base.StartController(ivr);

		if (this.transform == ivr.leftFootTarget) {
			isLeftFoot = true;
			leftFoot = this;
			rightFoot = ivr.rightFootTarget.GetComponent<IVR_AnimatorFoot>();
			leftFootAnimation = this.gameObject.AddComponent<IVR_Animate>();
		} else {
			rightFoot = this;
			leftFoot = ivr.leftFootTarget.GetComponent<IVR_AnimatorFoot>();
			rightFootAnimation = this.gameObject.AddComponent<IVR_Animate>();
		}

		startHipFootL = ivr.hipTarget.InverseTransformPoint(ivr.leftFootTarget.position);//ivr.leftFootTarget.position - ivr.hipTarget.position;
		startHipFootR = ivr.hipTarget.InverseTransformPoint(ivr.rightFootTarget.position);//ivr.rightFootTarget.position - ivr.hipTarget.position;

		startFootLposition = ivr.leftFootTarget.position - ivr.BasePosition;
		startFootRposition = ivr.rightFootTarget.position - ivr.BasePosition;
	}

	public override void UpdateController () {
		if (this.enabled) {
			tracking = true;

			if (leftFootAnimation == null) 
				leftFootAnimation = ivr.leftFootTarget.GetComponent<IVR_Animate>();
			if (rightFootAnimation == null)
				rightFootAnimation = ivr.rightFootTarget.GetComponent<IVR_Animate>();

			if (selected)
				FeetAnimation();
		} else
			tracking = false;
	}

	[HideInInspector] private Vector3 curSpeed;
	[HideInInspector] private Vector3 lastHipPosition = Vector3.zero;
	[HideInInspector] private Vector3 basePointStart, basePointDelta;
	[HideInInspector] private bool lastStepLeft = false, lastStepRight = false;

	protected void FeetAnimation() {
		curSpeed = ivr.hipTarget.position - lastHipPosition;
		curSpeed = ivr.hipTarget.InverseTransformDirection(curSpeed);
		curSpeed = new Vector3(curSpeed.x, 0, curSpeed.z);

		lastHipPosition = ivr.hipTarget.position;
		
		basePointDelta = ivr.BasePosition - basePointStart; // how much did the basepoint move?
		basePointDelta = new Vector3(basePointDelta.x, 0, basePointDelta.z);

		if (leftFootAnimation.f == 0 && rightFootAnimation.f == 0)
		if (isLeftFoot) {
			float delta = ivr.hipTarget.eulerAngles.y - ivr.leftFootTarget.eulerAngles.y;
			delta = AngleDifference(ivr.leftFootTarget.eulerAngles.y, ivr.hipTarget.eulerAngles.y);
			if (delta < -45)
				FootStepLeft(leftFootAnimation, ivr.hipTarget.position, ivr.leftFootTarget.position, false);
		} else {
			float delta = ivr.hipTarget.eulerAngles.y - ivr.rightFootTarget.eulerAngles.y;
			delta = AngleDifference(ivr.rightFootTarget.eulerAngles.y, ivr.hipTarget.eulerAngles.y);
			if (delta > 45)
				FootStepRight(rightFootAnimation, ivr.hipTarget.position, ivr.rightFootTarget.position, false);
		}

		if (leftFootAnimation.startStep == true || (rightFoot.lastStepRight == true && rightFootAnimation.startStep == false)) {
			if (isLeftFoot)
				FootStepLeft(leftFootAnimation, ivr.hipTarget.position, ivr.leftFootTarget.position, true);

		} else if (rightFootAnimation.startStep == true || (leftFoot.lastStepLeft == true && leftFootAnimation.startStep == false)) {
			if (!isLeftFoot)
				FootStepRight(rightFootAnimation, ivr.hipTarget.position, ivr.rightFootTarget.position, true);

		} else if (curSpeed.x > 0 || curSpeed.z != 0) {
			if (!isLeftFoot)
				FootStepRight(rightFootAnimation, ivr.hipTarget.position, ivr.rightFootTarget.position, false);
			
		} else if (curSpeed.x < 0)
			if (isLeftFoot)
				FootStepLeft(leftFootAnimation, ivr.hipTarget.position, ivr.leftFootTarget.position, false);
		
		else {
			leftFoot.lastStepLeft = false;
			rightFoot.lastStepRight = false;
		}


		if (isLeftFoot) {
			if (leftFootAnimation.f >= 1) {
				leftFootAnimation.startStep = false;
				leftFootAnimation.f = 0;
				if (lastStepLeft)
					FootStepRight(rightFootAnimation, ivr.hipTarget.position, ivr.rightFootTarget.position, true);
			} else {
				FootStepping(leftFootAnimation, startHipFootL);
			}
		} else {
			if (rightFootAnimation.f >= 1) {
				rightFootAnimation.startStep = false;
				rightFootAnimation.f = 0;
				if (lastStepRight)
					FootStepLeft(leftFootAnimation, ivr.hipTarget.position, ivr.leftFootTarget.position, true);
			} else {
				FootStepping(rightFootAnimation, startHipFootR);
			}
		}
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

	protected void FootStepLeft(IVR_Animate footAnimation, Vector3 hipTargetPosition, Vector3 footTargetPosition, bool follow) {
		if (footAnimation.startStep == false) {
			basePointStart = ivr.BasePosition;
			stepStart = new Vector3(footTargetPosition.x, ivr.BasePosition.y, footTargetPosition.z);
			footAnimation.startStep = true;

			if (follow == false)
				leftFoot.lastStepLeft = true;
			rightFoot.lastStepRight = false;
		}
	}
	
	protected void FootStepRight(IVR_Animate footAnimation, Vector3 hipTargetPosition, Vector3 footTargetPosition, bool follow) {
		if (footAnimation.startStep == false) {
			basePointStart = ivr.BasePosition;
			stepStart = new Vector3(footTargetPosition.x, ivr.BasePosition.y, footTargetPosition.z);
			footAnimation.startStep = true;

			if (!follow)
				rightFoot.lastStepRight = true;
			leftFoot.lastStepLeft = false;
		}
	}

	private void FootStepping(IVR_Animate footAnimation, Vector3 startHipFoot) {
		if (footAnimation.f >= 1) {
			footAnimation.startStep = false;
			footAnimation.f = 0;
		} else if (footAnimation.f > 0) {
			Vector3 stepTarget = ivr.hipTarget.TransformPoint(startHipFoot) - stepStart;
			stepTarget = new Vector3(stepTarget.x, startPosition.y, stepTarget.z);

			if (curSpeed.x != 0 || curSpeed.z != 0) {
				Vector3 avgSpeed2 = basePointDelta / footAnimation.f;
				stepTarget += avgSpeed2 / 2;
			}

			Vector3 newPosition = footAnimation.AnimationLerp(Vector3.zero, stepTarget, ref footAnimation.f);
			transform.position = stepStart + newPosition;
			transform.rotation = ivr.hipTarget.rotation; // should be eased towards this rotation in the end
		}
	}
}
