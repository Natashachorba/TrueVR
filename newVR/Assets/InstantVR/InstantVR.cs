/* InstantVR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.0.8
 * date: June 26, 2015
 * 
 * - fixed occasional vertical liftoff
 * - changes to support Free edition
 */

using UnityEngine;
using System.Collections;

public class InstantVR : MonoBehaviour {

	public Transform headTarget;
	public Transform leftHandTarget;
	public Transform rightHandTarget;
	public Transform hipTarget;
	public Transform leftFootTarget;
	public Transform rightFootTarget;

	private Vector3 basePosition = Vector3.zero;
	public Vector3 BasePosition { get { return basePosition; } set { basePosition = value; } }
	private Quaternion baseRotation = Quaternion.identity;
	public Quaternion BaseRotation { get { return baseRotation; } set { baseRotation = value; } }

	public enum BodySide {
		Unknown = 0,
		Left = 1,
		Right = 2,
	};

	[HideInInspector] private IVR_Extension[] extensions;

	[HideInInspector] private IVR_Controller[] headControllers;
	[HideInInspector] private IVR_Controller[] leftHandControllers, rightHandControllers;
	[HideInInspector] private IVR_Controller[] hipControllers;
	[HideInInspector] private IVR_Controller[] leftFootControllers, rightFootControllers;

	private IVR_Controller headController;
	public IVR_Controller HeadController { get { return headController; } set { headController = value; } }
	private IVR_Controller leftHandController, rightHandController;
	public IVR_Controller LeftHandController { get { return leftHandController; } set { leftHandController = value; } }
	public IVR_Controller RightHandController { get { return rightHandController; } set { rightHandController = value; } }
	private IVR_Controller hipController;
	public IVR_Controller HipController { get { return hipController; } set { hipController = value; } }
	private IVR_Controller leftFootController, rightFootController;
	public IVR_Controller LeftFootController { get { return leftFootController; } set { leftFootController = value; } }
	public IVR_Controller RightFootController { get { return rightFootController; } set { rightFootController = value; } }

	[HideInInspector] private IVR_Input leftInput, rightInput;
	[HideInInspector] public IVR_Movements leftMovements, rightMovements;

	[HideInInspector] public Transform characterTransform;

	[HideInInspector] public int playerID = 0;

	[HideInInspector] public bool hit = false;
	[HideInInspector] public Vector3 hitNormal = Vector3.zero;
	[HideInInspector] public Vector3 inputDirection;

	[HideInInspector] private static int myBodyLayer;

	protected virtual void Awake() {
        RaycastHit hit;
        Vector3 rayStart = hipTarget.position;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 2f, ~myBodyLayer))
            basePosition = new Vector3(hipTarget.position.x, hit.point.y, hipTarget.position.z);
        else
            basePosition = new Vector3(hipTarget.position.x, 0, hipTarget.position.z);
        baseRotation = this.transform.rotation;

		extensions = this.GetComponents<IVR_Extension>();
		foreach (IVR_Extension extension in extensions)
			extension.StartExtension();

		headControllers = headTarget.GetComponents<IVR_Controller>();
		leftHandControllers = leftHandTarget.GetComponents<IVR_Controller>();
		rightHandControllers = rightHandTarget.GetComponents<IVR_Controller>();
		hipControllers = hipTarget.GetComponents<IVR_Controller>();
		leftFootControllers = leftFootTarget.GetComponents<IVR_Controller>();
		rightFootControllers = rightFootTarget.GetComponents<IVR_Controller>();

		headController = FindActiveController(headControllers);
		leftHandController = FindActiveController(leftHandControllers);
		rightHandController = FindActiveController(rightHandControllers);
		hipController = FindActiveController(hipControllers);
		leftFootController = FindActiveController(leftFootControllers);
		rightFootController = FindActiveController(rightFootControllers);

		leftInput = leftHandTarget.GetComponent<IVR_Input>();
		if (leftInput)
			leftInput.StartInput(this);
		rightInput = rightHandTarget.GetComponent<IVR_Input>();
		if (rightInput)
			rightInput.StartInput(this);

		leftMovements = leftHandTarget.GetComponent<IVR_Movements>();
		rightMovements = rightHandTarget.GetComponent<IVR_Movements>();

		SetIVRLayer();

		Animator[] animators = GetComponentsInChildren<Animator>();
		for (int i = 0; i < animators.Length; i++) {
			Avatar avatar = animators[i].avatar;
			if (avatar.isValid && avatar.isHuman) {
				characterTransform = animators[i].transform;
				
				AddRigidbody(characterTransform.gameObject);
			}
		}
		
		foreach (IVR_Controller c in headControllers)
			c.StartController(this);
		foreach (IVR_Controller c in leftHandControllers)
			c.StartController(this);
		foreach (IVR_Controller c in rightHandControllers)
			c.StartController(this);
		foreach (IVR_Controller c in hipControllers)
			c.StartController(this);
		foreach (IVR_Controller c in leftFootControllers)
			c.StartController(this);
		foreach (IVR_Controller c in rightFootControllers)
			c.StartController(this);

		BodyMovementsBasics bm = GetComponent<BodyMovementsBasics>();
		if (bm != null)
			bm.StartMovements();

		if (leftMovements != null)
			leftMovements.StartMovements(this);
		if (rightMovements != null)
			rightMovements.StartMovements(this);
	}

	private IVR_Controller FindActiveController(IVR_Controller[] controllers) {
		for (int i = 0; i < controllers.Length; i++) {
			if (controllers[i].isTracking())
				return(controllers[i]);
		}
		return null;
	}


	void Update () {
		UpdateExtensions();
		ResetInputs();
		UpdateControllers();
		UpdateMovements();

		CheckCalibrating();

		CheckQuit();
	}

	void LateUpdate() {
		LateUpdateExtensions();
	}

	private void UpdateExtensions() {
		foreach (IVR_Extension extension in extensions)
			extension.UpdateExtension();
	}

	private void LateUpdateExtensions() {
		foreach (IVR_Extension extension in extensions)
			extension.LateUpdateExtension();
	}

	private void ResetInputs() {
		if (leftInput && leftHandControllers.Length > 0)
			leftInput.ResetInput();
		if (rightInput && rightHandControllers.Length > 0)
			rightInput.ResetInput();
	}
	
	private void UpdateControllers() {
		leftHandController = UpdateController(leftHandControllers, leftHandController);
		rightHandController = UpdateController(rightHandControllers, rightHandController);
		hipController = UpdateController(hipControllers, hipController);
		leftFootController = UpdateController(leftFootControllers, leftFootController);
		rightFootController = UpdateController(rightFootControllers, rightFootController);
		// Head needs to be after hands because of traditional controller.
		headController = UpdateController(headControllers, headController);
	}

	private IVR_Controller UpdateController(IVR_Controller[] controllers, IVR_Controller lastActiveController) {
		if (controllers != null) {
			int lastIndex = 0, newIndex = 0;

			IVR_Controller activeController = null;
			for (int i = 0; i < controllers.Length; i++) {
				if (controllers [i] != null) {
					controllers [i].UpdateController ();
					if (activeController == null && controllers [i].isTracking ()) {
						activeController = controllers [i];
						controllers [i].SetSelection (true);
						newIndex = i;
					} else
						controllers [i].SetSelection (false);

					if (controllers [i] == lastActiveController)
						lastIndex = i;
				}
			}

			if (lastIndex < newIndex && lastActiveController != null) { // we are degreding
				activeController.TransferCalibrarion (lastActiveController);
			}

			return activeController;
		} else
			return null;
	}

	private void UpdateMovements() {
		if (leftInput)
			leftInput.UpdateInput();
		if (rightInput)
			rightInput.UpdateInput();
		if (leftMovements)
			leftMovements.UpdateMovements();
		if (rightMovements)
			rightMovements.UpdateMovements();

		RaycastHit hit;
		Vector3 rayStart = basePosition + new Vector3(0, 0.1f, 0);
		if (Physics.Raycast(rayStart, Vector3.down, out hit, 0.15f, ~myBodyLayer)) {
			//if (hit.distance < 0.5f)
			//	Debug.Log ("ai");
			if (hit.distance > 0)
				basePosition = rayStart - Vector3.up * hit.distance;
		}
	}

	[HideInInspector] private bool calibrating = false;

	void CheckCalibrating() {
		if ((leftInput != null || rightInput != null)) {
			if (calibrating == false &&
			    (leftInput == null || leftInput.option) && 
			    (rightInput == null || rightInput.option)) {
				
				calibrating = true;
				Calibrate();
			} else
				if (calibrating == true &&
				    (leftInput == null || !leftInput.option) && 
				   (rightInput == null || !rightInput.option)) {
				calibrating = false;
			}
		}
	}

	public void Calibrate() {
		foreach (Transform t in headTarget.parent) {
			t.gameObject.SendMessage("OnTargetReset", SendMessageOptions.DontRequireReceiver);
		}
	}
	
	public void MoveMe(Vector3 translationVector) {
		if (translationVector.magnitude > 0) {
			Vector3 translation = translationVector * Time.deltaTime;
			basePosition = new Vector3(basePosition.x + translation.x, basePosition.y, basePosition.z + translation.z);

		}
	}

	public void RotateMe(float angle) {
		Vector3 baseFromHip = basePosition - hipTarget.position;
		Vector3 newBaseFromHip = Quaternion.AngleAxis(angle, Vector3.up) * baseFromHip;
		basePosition = hipTarget.position + newBaseFromHip;

		baseRotation *= Quaternion.AngleAxis(angle, Vector3.up);
	}

	protected void CheckQuit() {
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
	}
	
	protected void AddRigidbody(GameObject gameObject) {
		Rigidbody rb = gameObject.AddComponent<Rigidbody>();
		if (rb != null) {
			rb.mass = 75;
			rb.useGravity = false;
			rb.isKinematic = true;
		}
	}

	private void SetIVRLayer() {
		myBodyLayer = LayerMask.NameToLayer("MyBody");
		if (myBodyLayer < 0) {
			Debug.LogWarning("MyBody layer does not exist. Body movement will have issues.\nPlease add layer MyBody in Project Settings->Tags and Layers");
		} else {
			SetToLayerRecursively(this.gameObject, myBodyLayer);
		}
	}

	private void SetToLayerRecursively(GameObject go, int layer) {
		go.layer = layer;
		foreach (Transform childTransform in go.transform) {
			SetToLayerRecursively(childTransform.gameObject, layer);
		}
	}

}
