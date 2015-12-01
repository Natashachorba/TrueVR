/* InstantVR Extension
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.1
 * date: March 31, 2015
 * 
 * - fixed extrapolation
 */

using UnityEngine;
using System.Collections;

public class IVR_Extension : MonoBehaviour {
	[HideInInspector]
	public int priority = -1;

	public virtual void StartExtension() {}
	public virtual void UpdateExtension() {}
	public virtual void LateUpdateExtension() {}
}

public class IVR_Controller : MonoBehaviour {
	[HideInInspector] protected InstantVR ivr;
	[HideInInspector] 
	public IVR_Extension extension;

	protected Vector3 startPosition;
	protected Vector3 referencePosition = Vector3.zero;
	public Vector3 position = Vector3.zero;
	
	protected Quaternion startRotation;
	protected Quaternion referenceRotation = Quaternion.identity;
	public Quaternion rotation = Quaternion.identity;
	
	protected bool extrapolation = false;
	
	protected bool present = false;
	public bool isPresent() {
		return present;
	}
	protected bool tracking = false;
	public bool isTracking() {
		return tracking;
	}
	
	protected bool selected = false;
	public bool isSelected() {
		return selected;
	}
	public void SetSelection(bool selection) {
		selected = selection;
	}
	
	[HideInInspector] private float updateTime;
	
	[HideInInspector] private Vector3 lastPosition = Vector3.zero;
	[HideInInspector] private Quaternion lastRotation = Quaternion.identity;
	private Vector3 positionalVelocity = Vector3.zero;
	private float angularVelocity = 0;
	private Vector3 velocityAxis = Vector3.one;
	
	void Start() {
		updateTime = Time.time;
	}
	
	public virtual void StartController(InstantVR ivr) {
		this.ivr = ivr;
		startPosition = transform.position - ivr.BasePosition;
		startRotation = Quaternion.Inverse(ivr.BaseRotation) * transform.rotation;
		
		lastPosition = startPosition;
		lastRotation = startRotation;
	}
	
	public virtual void OnTargetReset() {
		if (selected)
			Calibrate(true);
	}
	
	public void Calibrate(bool calibrateOrientation) {
		referencePosition = -position;
		
		if (calibrateOrientation) {
			referenceRotation = Quaternion.Inverse(rotation);
		}
	}
	
	public void TransferCalibrarion(IVR_Controller fromController) {
		Vector3 delta = Vector3.zero;
		//delta = position - fromController.position;
		referencePosition -= delta;
	}

	
	[HideInInspector] private bool indirectUpdate = false;
	
	public virtual void UpdateController() {
		if (selected) { // this should be moved out of here in the future, because it removes the possibility to combine controllers
			if (extrapolation == false) {
				Vector3 controllerDelta = ivr.BaseRotation * referenceRotation * (referencePosition + position);
				this.transform.position = ivr.BasePosition + controllerDelta + startPosition; //(ivr.BaseRotation * startPosition);
				
				this.transform.rotation = ivr.BaseRotation * referenceRotation * rotation * startRotation;
			} else {
				float deltaTime = Time.time - updateTime;
				if (deltaTime > 0) {
					Vector3 controllerDelta = ivr.BaseRotation * referenceRotation * (referencePosition + position);
					Vector3 newPosition = ivr.BasePosition + controllerDelta +  startPosition; //(ivr.BaseRotation * startPosition);
					
					Quaternion newRotation = ivr.BaseRotation * referenceRotation * rotation * startRotation;

					float angle = 0;
					Quaternion rotationalChange = Quaternion.Inverse(lastRotation) * newRotation;
					rotationalChange.ToAngleAxis(out angle, out velocityAxis);
					if (angle == 0)
						velocityAxis = Vector3.one;
					else if (angle > 180) angle -= 360;
					
					positionalVelocity = (newPosition - lastPosition) / deltaTime;
					angularVelocity = angle / deltaTime;
					
					lastPosition = newPosition;
					lastRotation = newRotation;
					
					updateTime = Time.time;
					indirectUpdate = true;
				}
			}
		}
	}
	
	void Update() {
		if (indirectUpdate) {
			float dTime = Time.time - updateTime;
			if (dTime < 0.1f) { // do not extrapolate for more than 1/10th second
				this.transform.position = lastPosition + positionalVelocity * dTime;
				this.transform.rotation = lastRotation * Quaternion.AngleAxis(angularVelocity * dTime, velocityAxis);
			} else {
				indirectUpdate = false;
			}
		}
	}
	
}
