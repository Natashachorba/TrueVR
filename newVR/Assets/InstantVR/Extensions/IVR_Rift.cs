/* InstantVR Oculus Rift extension
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.8
 * date: June 26, 2015
 * 
 * - Check for Unity 5.1 added. Use IVR_UnityVR instead
 */

using UnityEngine;
using System.Collections;

public class IVR_Rift : IVR_Extension {
#if (UNITY_4_5 || UNITY_4_6 || UNITY_5_0)
    void OnDestroy()
    {
		InstantVR ivr = this.GetComponent<InstantVR>();

		if (ivr != null) {
			IVR_RiftHead riftHead = ivr.headTarget.GetComponent<IVR_RiftHead>();
			if (riftHead != null)
				DestroyImmediate(riftHead);
		}
	}
#endif
}
