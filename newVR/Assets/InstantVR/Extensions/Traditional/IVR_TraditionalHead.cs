/* InstantVR Traditional head controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.8
 * date: June 26, 2015
 * 
 * - changes to support Free edition
 */

using UnityEngine;
using System.Collections;

public class IVR_TraditionalHead : IVR_Controller {

	[HideInInspector] private IVR_Input rightInput;

	public override void StartController(InstantVR ivr) {
		base.StartController(ivr);
		present = true;

		rightInput = ivr.rightHandTarget.GetComponent<IVR_Input>();
	}
	
	public override void UpdateController() {
		tracking = true;
		if (selected && this.enabled) {
			if (rightInput != null) {
				this.position = Vector3.zero;
				this.rotation = Quaternion.Euler(rightInput.xAngle, rightInput.yAngle, 0);
				base.UpdateController();
			}
		}
	}

    public override void OnTargetReset() {
    }
}
