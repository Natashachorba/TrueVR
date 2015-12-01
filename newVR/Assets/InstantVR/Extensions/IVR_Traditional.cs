/* InstantVR default target controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.0
 * date: December 31, 2014
 */

using UnityEngine;
using System.Collections;

public class IVR_Traditional : IVR_Extension {
	void OnDestroy() {
		InstantVR ivr = this.GetComponent<InstantVR>();

		IVR_TraditionalHead traditionalHead = ivr.headTarget.GetComponent<IVR_TraditionalHead>();
		if (traditionalHead != null)
			DestroyImmediate(traditionalHead);
		
		IVR_TraditionalHand traditionalLeftHand = ivr.leftHandTarget.GetComponent<IVR_TraditionalHand>();
		if (traditionalLeftHand != null)
			DestroyImmediate(traditionalLeftHand);
		
		IVR_TraditionalHand traditionalRightHand = ivr.rightHandTarget.GetComponent<IVR_TraditionalHand>();
		if (traditionalRightHand != null)
			DestroyImmediate(traditionalRightHand);
	}
}