/* InstantVR Traditional hand
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.7
 * date: May 29, 2015
 * 
 * - mouse input correction for hip-followHead & lookrotation
 */

using UnityEngine;
using System.Collections;

public class IVR_TraditionalHand : IVR_Controller {

	public bool mouseInput = true;

	[HideInInspector] private IVR_Input ivrInput;
	[HideInInspector] private bool joystick2present;

	[HideInInspector] private float hipStartRotationY;

	void Start() {
	}

	public override void StartController(InstantVR ivr) {
		base.StartController(ivr);
		present = true;

		joystick2present = CheckJoystick2Present();
		ivrInput = this.gameObject.GetComponent<IVR_Input>();

		hipStartRotationY = ivr.hipTarget.eulerAngles.y;
	}

	public override void UpdateController() {
		if (this.enabled) {
			UpdateInput();
			this.position = Vector3.zero;
			this.rotation = Quaternion.identity;
			base.UpdateController();
		}
	}

	private void UpdateInput() {
		if (ivrInput != null) {
			if (this.transform == ivr.leftHandTarget) {
				ivrInput.stickHorizontal += Input.GetAxis("Horizontal");
				ivrInput.stickVertical += Input.GetAxis("Vertical");
				ivrInput.yAngle += calculateStickYAngle();
				ivrInput.xAngle += calculateStickXAngle();
			} else {
				if (joystick2present) {
					ivrInput.stickHorizontal += Input.GetAxis("Horizontal R");
					ivrInput.stickVertical += Input.GetAxis("Vertical R");
				}
				ivrInput.yAngle += calculateStickYAngle();
				ivrInput.xAngle += calculateStickXAngle();
			}
			ivrInput.option |= Input.GetKey(KeyCode.Tab);
		}
	}

	public override void OnTargetReset() {
		if (selected) {
			Calibrate(true);
		}
	}

	private bool CheckJoystick2Present() {
		bool joy4available = IsAxisAvailable("Horizontal R");
		bool joy5available = IsAxisAvailable("Vertical R");
		return (joy4available && joy5available);
	}

	private bool IsAxisAvailable(string axisName)
	{
		try
		{
			Input.GetAxis(axisName);
			return true;
		}
		catch (System.Exception)
		{
			return false;
		}
	}

	private static float maxXangle = 60;
	private static float sensitivityX = 5;
	
	private float calculateStickXAngle() {
		float joy5 = 0;

		if (this.transform == ivr.leftHandTarget)
			joy5 -= Input.GetAxis("Vertical");
		else {
			if (joystick2present)
				joy5 -= Input.GetAxis("Vertical R");
		}

		if (joy5 != 0) {
			xAngle = joy5 * maxXangle;
			lastJoy5 = joy5;
		} else if (lastJoy5 != 0) {
			xAngle = 0;
			lastJoy5 = 0;
		}
		
		if (this.transform == ivr.rightHandTarget) {
			if (mouseInput)
			xAngle -= Input.GetAxis("Mouse Y") * sensitivityX;
		}

		xAngle = Mathf.Clamp (xAngle, -maxXangle, maxXangle);
		
		return xAngle;
	}
	
	[HideInInspector] private float xAngle = 0;
	[HideInInspector] private float yAngle = 0;
	[HideInInspector] private float lastJoy4, lastJoy5 = 0;

	private static float maxYangle = 70;
	private static float sensitivityY = 5;
	
	private float calculateStickYAngle() {
		float joy4 = 0;

		if (this.transform == ivr.leftHandTarget)
			joy4 = Input.GetAxis("Horizontal");
		else {
			if (joystick2present)
				joy4 = Input.GetAxis("Horizontal R");
		}

		if (joy4 != 0) {
			yAngle = joy4 * maxYangle;
			lastJoy4 = joy4;
		} else if (lastJoy4 != 0) {
			yAngle = 0;
			lastJoy4 = 0;
		}
		
		float correctedYAngle = NormalizeAngle180(yAngle);
		if (this.transform == ivr.rightHandTarget) {
			if (mouseInput)
				yAngle += Input.GetAxis("Mouse X") * sensitivityY;

			float deltaHipRot = NormalizeAngle180(ivr.hipTarget.eulerAngles.y - hipStartRotationY);
			while (deltaHipRot - correctedYAngle > 180)		deltaHipRot -= 360;
			while (deltaHipRot - correctedYAngle < -180)	deltaHipRot += 360;

			float maxHipYangle = maxYangle + deltaHipRot;
			float minHipYangle = -maxYangle + deltaHipRot;
			correctedYAngle = Mathf.Clamp(correctedYAngle, minHipYangle, maxHipYangle);

			correctedYAngle = NormalizeAngle180(correctedYAngle);
		}
		
		return correctedYAngle;
	}

	private float NormalizeAngle180(float angle) {
		while (angle > 180)		angle -= 360;
		while (angle < -180)	angle += 360;
		return angle;
	}
}
