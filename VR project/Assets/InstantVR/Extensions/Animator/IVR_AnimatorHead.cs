/* InstantVR Animator
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.2
 * date: April 10, 2015
 * 
 * - Name changed to IVR_AnimatorHead
 */

using UnityEngine;

public class IVR_AnimatorHead : IVR_Controller {

	public override void StartController(InstantVR ivr)
	{
		present = true;
		tracking = true;

		base.StartController (ivr);
	}

	public override void OnTargetReset() {
		if (selected) {
			Calibrate(true);
		}
	}
}
