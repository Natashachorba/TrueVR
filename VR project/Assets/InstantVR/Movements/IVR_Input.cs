/* InstantVR Input
 * author: Pascal Serrarnes
 * email: support@passervr.com
 * version: 3.0.8
 * date: June 26, 2015
 * 
 */

using UnityEngine;
using System.Collections;

public class IVR_Input : MonoBehaviour {
	public float stickHorizontal;
	public float stickVertical;
	public float xAngle;
	public float yAngle;
	public bool stickButton;
	public bool buttonOne;
	public bool buttonTwo;
	public bool buttonThree;
	public bool buttonFour;
	public float bumper;
	public float trigger;
	public bool option;

	public virtual void StartInput(InstantVR ivr) {}
	public virtual void UpdateInput() {}

	public void ResetInput() {
		stickHorizontal = 0;
		stickVertical = 0;
		xAngle = 0;
		yAngle = 0;
		stickButton = false;
		buttonOne = false;
		buttonTwo = false;
		buttonThree = false;
		buttonFour = false;
		bumper = 0;
		trigger = 0;
		option = false;
	}
}
