/* InstantVR Oculus Rift head controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.9
 * date: June 28, 2015
 * 
 * - Unity 5.1: Fixed issue when avatar is not at 0,0,0
 */

using UnityEngine;
using System.Collections;
#if !(UNITY_4_5 || UNITY_4_6 || UNITY_5_0)
using UnityEngine.VR;
#endif

public class IVR_RiftHead : IVR_Controller
{
#if (UNITY_4_5 || UNITY_4_6 || UNITY_5_0)

    [HideInInspector] private OVRCameraRig ovrCR;
	[HideInInspector] private Transform headcam;

	[HideInInspector] private Vector3 ovrCRstartPoint;
	[HideInInspector] private Vector3 baseStartPoint;
	[HideInInspector] private Quaternion ovrCRstartRotation;
	[HideInInspector] private float baseStartAngle;
	[HideInInspector] private Vector3 neckOffset;

	void Start() {
	}

	public override void StartController(InstantVR ivr) {

		headcam = this.transform.FindChild("Headcam").GetComponentInChildren<Camera>().transform;

		OVRCameraRig ovrCRprefab = Resources.Load <OVRCameraRig>("OVRCameraRig");
		ovrCR = (OVRCameraRig) Instantiate(ovrCRprefab, headcam.position, headcam.rotation);
		if (ovrCR != null) {
			ovrCR.transform.parent = ivr.transform;

			if (ovrCR.centerEyeAnchor == null)
				Debug.LogWarning("centerEyeAnchor not found in OVR Camera Rift. Head animation will not work.");
		} else 
			Debug.LogError("Could not instantiate OVRCameraRig. Prefab is missin?");


		ovrCRstartPoint = ovrCR.transform.position;
		baseStartPoint = ivr.BasePosition;
		ovrCRstartRotation = ovrCR.transform.rotation;
		baseStartAngle = ivr.BaseRotation.eulerAngles.y;
		neckOffset = -headcam.localPosition;

		present = CheckRiftPresent();
		if (!present) {
			ovrCR.gameObject.SetActive(false);
		}

		base.StartController(ivr);
		startRotation = Quaternion.Inverse(ivr.BaseRotation);
	}
	
	private bool CheckRiftPresent() {
		if (OVRManager.display != null)
			return (OVRManager.display.isPresent);
		else
			return false;
	}

	public override void UpdateController() {
		if (present && this.enabled)
			UpdateRift();
		else
			tracking = false;
	}

	private void UpdateRift() {
		if (ovrCR != null) {
			Vector3 baseDelta = ivr.BasePosition - baseStartPoint;
			ovrCR.transform.position = ovrCRstartPoint + baseDelta;

			float baseAngleDelta = ivr.BaseRotation.eulerAngles.y - baseStartAngle;
			ovrCR.transform.rotation = ovrCRstartRotation * Quaternion.AngleAxis(baseAngleDelta, Vector3.up);

			if (ovrCR.centerEyeAnchor != null) {
				Vector3 thisNeckOffset = ovrCR.centerEyeAnchor.rotation * -neckOffset;
				
				this.position = ovrCR.centerEyeAnchor.position - thisNeckOffset - baseDelta;
				this.rotation = ovrCR.centerEyeAnchor.rotation * Quaternion.AngleAxis(-baseAngleDelta, Vector3.up);;

				if (this.position.z > 1000) {
					//Debug.LogWarning("Head position out of range");
					tracking = false;
					this.position = Vector3.zero;
					this.rotation = Quaternion.identity;
				} else if (!tracking) {
					Calibrate(false);
					tracking = true;
				}


				base.UpdateController();
			}
		}
	}

	public override void OnTargetReset() {
		if (selected) {
			Vector3 referencePosBefore = this.referencePosition;
			Calibrate(false);
			ovrCRstartPoint += this.referencePosition - referencePosBefore;
			this.referencePosition = referencePosBefore;
		}
	}
#else
    [HideInInspector] private GameObject vrCamera;
	[HideInInspector] private Transform headcam;

	[HideInInspector] private Vector3 ovrCRstartPoint;
	[HideInInspector] private Vector3 baseStartPoint;
	[HideInInspector] private Quaternion ovrCRstartRotation;
	[HideInInspector] private float baseStartAngle;
	[HideInInspector] private Vector3 neckOffset;

	// This dummy code is here to ensure the checkbox is present in editor
	void Start() {}

	public override void StartController(InstantVR ivr) {

		headcam = this.transform.FindChild("Headcam").GetComponentInChildren<Camera>().transform;

		vrCamera = new GameObject("VRCameraRoot");
		vrCamera.transform.parent = ivr.transform;
		vrCamera.transform.position = this.transform.position;
		vrCamera.transform.rotation = this.transform.rotation;

		GameObject vrCameraObj = new GameObject("VRCamera");
		Camera hmdCam = vrCameraObj.AddComponent<Camera>();
		hmdCam.nearClipPlane = 0.01f;
		hmdCam.farClipPlane = 1000;
		vrCameraObj.transform.position = headcam.position;
		vrCameraObj.transform.rotation = headcam.rotation;
		vrCameraObj.transform.parent = vrCamera.transform;

		ovrCRstartPoint = vrCamera.transform.position;
		baseStartPoint = ivr.BasePosition;

		ovrCRstartRotation = vrCamera.transform.rotation;
		baseStartAngle = ivr.BaseRotation.eulerAngles.y;

		neckOffset = -headcam.localPosition;

		present = VRDevice.isPresent;
		if (!present)
			vrCamera.SetActive(false);
		else
			headcam.gameObject.SetActive(false);

   		headcam = vrCameraObj.transform;

		base.StartController(ivr);
		referencePosition = Vector3.zero;
		startRotation = Quaternion.Inverse(ivr.BaseRotation);
		startRotation = Quaternion.identity;
		InputTracking.Recenter();
	}
	
	public override void UpdateController() {
		if (present && this.enabled)
			UpdateRift();
		else
			tracking = false;
	}

	private void UpdateRift() {
		Vector3 baseDelta = ivr.BasePosition - baseStartPoint;
		vrCamera.transform.position = ovrCRstartPoint + baseDelta;
		
		float baseAngleDelta = ivr.BaseRotation.eulerAngles.y - baseStartAngle;
		vrCamera.transform.rotation = ovrCRstartRotation * Quaternion.AngleAxis(baseAngleDelta, Vector3.up);

		Vector3 centerPos = InputTracking.GetLocalPosition(VRNode.CenterEye);
		Quaternion centerRot = InputTracking.GetLocalRotation(VRNode.CenterEye);

		if (centerPos.magnitude < 10 && centerPos.magnitude > 0) {
			Vector3 thisNeckOffset = centerRot * -neckOffset;
			
			this.position = vrCamera.transform.position + centerPos - thisNeckOffset - baseDelta;
			this.rotation = vrCamera.transform.rotation * centerRot * Quaternion.AngleAxis(-baseAngleDelta, Vector3.up);
			
			if (this.position.z > 1000) {
				Debug.LogWarning("Head position out of range");
				
				tracking = false;
				this.position = Vector3.zero;
				this.rotation = Quaternion.identity;
			} else if (!tracking) {
				Calibrate(false);
				tracking = true;
			}
			if (referencePosition.magnitude > 1000)
				Debug.Log ("Head reference position is out of range");
			
			base.UpdateController();
		}
	}

	public override void OnTargetReset() {
		if (selected) {
			Vector3 referencePosBefore = this.referencePosition;
			Calibrate(false);
			ovrCRstartPoint += this.referencePosition - referencePosBefore;
			this.referencePosition = referencePosBefore;
		}
	}
#endif
}
