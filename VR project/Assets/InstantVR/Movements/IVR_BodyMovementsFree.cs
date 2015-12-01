/* Instant Body Movements Free
 * author: Pascal Serrarnes
 * email: support@passervr.com
 * version: 3.0.0
 * date: March 29, 2015

Changes: 
 * - Editor preview support
 */

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class IVR_BodyMovementsFree : BodyMovementsBasics {

	private Transform neck;
	private Transform spine;
	private Transform hips;

	private Quaternion fromNormNeck;
	private Quaternion fromNormTorso;
	private Quaternion fromNormHips;

    private Vector3 hipStartPosition;

	void Start() {
		StartMovements();
	}
	
	public override void StartMovements() {
		ivr = this.GetComponent<InstantVR>();
		
		characterTransform = ivr.transform;
		characterRigidbody = this.GetComponent<Rigidbody>();

		headTarget = ivr.headTarget;
		leftHandTarget = ivr.leftHandTarget;
		rightHandTarget = ivr.rightHandTarget;
		hipTarget = ivr.hipTarget;
		leftFootTarget = ivr.leftFootTarget;
		rightFootTarget = ivr.rightFootTarget;

		animator = characterTransform.GetComponentInChildren<Animator>();
		if (animator != null) {
			neck = animator.GetBoneTransform(HumanBodyBones.Neck);
			spine = animator.GetBoneTransform(HumanBodyBones.Spine);
			hips = animator.GetBoneTransform(HumanBodyBones.Hips);

            hipStartPosition = hips.position;

			fromNormNeck = Quaternion.Inverse(Quaternion.LookRotation(characterTransform.forward)) * neck.rotation;
			fromNormTorso = Quaternion.Inverse(Quaternion.LookRotation(neck.position - spine.position, hipTarget.forward)) * spine.rotation;
			fromNormHips = Quaternion.Inverse(Quaternion.LookRotation(hipTarget.forward)) * hips.rotation;

			leftArm = new ArmMovements_Free(ivr, ArmBasics.BodySide.Left, this);
			rightArm = new ArmMovements_Free(ivr, ArmBasics.BodySide.Right, this);

			leftLeg = new Leg(ArmBasics.BodySide.Left, animator, characterTransform);
			rightLeg = new Leg(ArmBasics.BodySide.Right, animator, characterTransform);
		}
	}
	
	protected virtual void LateUpdate () {
		if (leftArm == null)
			StartMovements();
		UpdateBodyMovements();
	}

	private void UpdateBodyMovements() {
		CalculateHorizontal(headTarget);
        CalculateVertical(headTarget);

		leftArm.Calculate(leftHandTarget);
		rightArm.Calculate(rightHandTarget);

		leftLeg.Calculate(leftFootTarget);
		rightLeg.Calculate(rightFootTarget);

		if (characterRigidbody) {
			characterRigidbody.MovePosition(hipTarget.position);
			characterRigidbody.MoveRotation(hipTarget.rotation);
		}
		
		CalculateHeadOrientation(neck, headTarget);
	}

	void CalculateHeadOrientation(Transform neck, Transform neckTarget) {
		neck.rotation = neckTarget.rotation * fromNormNeck;
	}

	public void CalculateHorizontal(Transform neckTarget) {
		if (hipTarget.gameObject.activeSelf)
			hips.position = new Vector3(hipTarget.position.x, hips.position.y, hipTarget.position.z);
		hips.rotation = Quaternion.LookRotation(hipTarget.forward, Vector3.up) * fromNormHips;
		spine.LookAt(neckTarget.transform, hipTarget.forward);
		spine.rotation *= fromNormTorso;			
	}

    public void CalculateVertical(Transform neckTarget)
    {
        float dY = hipTarget.position.y - hipStartPosition.y;
        hips.position = new Vector3(hips.position.x, hipStartPosition.y + dY, hips.position.z);
    }

	[System.Serializable]
	public class ArmMovements_Free : ArmBasics {
		public ArmMovements_Free(InstantVR ivr, ArmBasics.BodySide bodySide_in, BodyMovementsBasics bodyMovements) : base(ivr, bodySide_in, bodyMovements) {
		}
		
		public override void Calculate(Transform handTarget) {
			upperArm.LookAt(handTarget.position, handTarget.up);

			if (bodySide == BodySide.Left) {
				upperArm.rotation *= bodyMovements.fromNormLeftUpperArm;
			} else {
				upperArm.rotation *= bodyMovements.fromNormRightUpperArm;
			}
			//upperArm.rotation *= fromNormUpperArm;

			forearm.LookAt(handTarget.position, handTarget.up);

			if (bodySide == BodySide.Left) {
				forearm.rotation *= bodyMovements.fromNormLeftForearm;
				hand.rotation = handTarget.rotation * bodyMovements.fromNormLeftHand;
			} else {
				forearm.rotation *= bodyMovements.fromNormRightForearm;
				hand.rotation = handTarget.rotation * bodyMovements.fromNormRightHand;
			}
		}
	}
	
	[System.Serializable]
	public class Leg {
		private Transform characterTransform;
		public Transform upperLeg;
		public Transform lowerLeg;
		public Transform foot;
		
		private Quaternion fromNormUpperLeg;
		private Quaternion fromNormLowerLeg;
		private Quaternion fromNormFoot;
		
		private float upperLegLength, lowerLegLength;
		private float upperLegLength2, lowerLegLength2;
		
		public Leg(ArmBasics.BodySide bodySide_in, Animator animator, Transform characterTransform_in) {
			characterTransform = characterTransform_in;
		
			if (bodySide_in == ArmBasics.BodySide.Left) {
				upperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
				lowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
				foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
			} else {
				upperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
				lowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
				foot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
			}

			fromNormUpperLeg = Quaternion.Inverse(Quaternion.LookRotation (foot.position - upperLeg.position, characterTransform.forward)) * upperLeg.rotation;
			fromNormLowerLeg = Quaternion.Inverse(Quaternion.LookRotation(foot.position - lowerLeg.position, characterTransform.forward)) * lowerLeg.rotation;
			fromNormFoot = Quaternion.Inverse(Quaternion.LookRotation (characterTransform.forward)) * foot.rotation;
			
			upperLegLength = Vector3.Distance(upperLeg.position, lowerLeg.position);
			lowerLegLength = Vector3.Distance(lowerLeg.position, foot.position);
			
			upperLegLength2 = upperLegLength * upperLegLength;
			lowerLegLength2 = lowerLegLength * lowerLegLength;
		}
	
	public void Calculate(Transform footTarget) {
		float dHipTarget = Vector3.Distance (upperLeg.position, footTarget.position);
		float hipAngle = Mathf.Acos((dHipTarget * dHipTarget + upperLegLength2 - lowerLegLength2)/ (2 * upperLegLength * dHipTarget)) * Mathf.Rad2Deg;
		if (float.IsNaN(hipAngle)) hipAngle = 0;
		
		upperLeg.LookAt (footTarget.position, footTarget.forward);
		upperLeg.rotation = Quaternion.AngleAxis(-hipAngle, upperLeg.right) * upperLeg.rotation;
		upperLeg.rotation *= fromNormUpperLeg;
		
		lowerLeg.LookAt (footTarget.position, footTarget.forward);
		lowerLeg.rotation *= fromNormLowerLeg;
		
		foot.rotation = footTarget.rotation * fromNormFoot;
	}
}
}

public abstract class BodyMovementsBasics : MonoBehaviour {
	[HideInInspector] protected InstantVR ivr;

	protected Transform headTarget;
	protected Transform rightHandTarget, leftHandTarget;
	protected Transform hipTarget;
	protected Transform rightFootTarget, leftFootTarget;

	[HideInInspector] protected Animator animator = null;

	[HideInInspector] protected Transform characterTransform = null;
	[HideInInspector] protected Rigidbody characterRigidbody;

	[HideInInspector] public ArmBasics leftArm;
	[HideInInspector] public ArmBasics rightArm;

	[HideInInspector] protected IVR_BodyMovementsFree.Leg rightLeg;
	[HideInInspector] protected IVR_BodyMovementsFree.Leg leftLeg;
	
	[HideInInspector] public Quaternion fromNormLeftUpperArm = Quaternion.identity;
	[HideInInspector] public Quaternion fromNormLeftForearm = Quaternion.identity;
	[HideInInspector] public Quaternion fromNormLeftHand = Quaternion.identity;
	[HideInInspector] public Quaternion fromNormRightUpperArm = Quaternion.identity;
	[HideInInspector] public Quaternion fromNormRightForearm = Quaternion.identity;
	[HideInInspector] public Quaternion fromNormRightHand = Quaternion.identity;

	public abstract void StartMovements();

	public void SetLeftHandTarget(Transform newTarget) {
		leftHandTarget = newTarget;
	}
	public void SetRightHandTarget(Transform newTarget) {
		rightHandTarget = newTarget;
	}
}

[System.Serializable]
public abstract class ArmBasics {
	protected InstantVR ivr;
	protected Animator animator;


	public enum BodySide {
		Left,
		Right
	};
	protected BodySide bodySide;
	
	public Transform upperArm;
	public Transform forearm;
	public Transform hand;

	protected BodyMovementsBasics bodyMovements;

	public float length;
	public Vector3 upperArmStartPosition;

	[HideInInspector]
	public Quaternion fromNormUpperArm = Quaternion.identity;
	[HideInInspector]
	public Quaternion fromNormForearm = Quaternion.identity;
	[HideInInspector]
	public Quaternion fromNormHand = Quaternion.identity;

	public ArmBasics(InstantVR ivr, BodySide bodySide_in, BodyMovementsBasics bodyMovements_in) {
		this.ivr = ivr;
		animator = ivr.GetComponentInChildren<Animator>();

		this.bodyMovements = bodyMovements_in;
		this.bodySide = bodySide_in;

		if (bodySide_in == BodySide.Left) {
			upperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			forearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
			hand = animator.GetBoneTransform(HumanBodyBones.LeftHand);

			if (bodyMovements.fromNormLeftHand == Quaternion.identity) {
				bodyMovements.fromNormLeftUpperArm = Quaternion.Inverse(Quaternion.LookRotation(forearm.position - upperArm.position)) * upperArm.rotation;
				bodyMovements.fromNormLeftForearm = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * forearm.rotation;
				bodyMovements.fromNormLeftHand = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * hand.rotation;
			}
			if (fromNormHand == Quaternion.identity) {
				fromNormUpperArm = Quaternion.Inverse(Quaternion.LookRotation(forearm.position - upperArm.position)) * upperArm.rotation;
				fromNormForearm = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * forearm.rotation;
				fromNormHand = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * hand.rotation;
			}
		} else {
			upperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			forearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
			hand = animator.GetBoneTransform(HumanBodyBones.RightHand);

			if (bodyMovements.fromNormRightHand == Quaternion.identity) {
				bodyMovements.fromNormRightUpperArm = Quaternion.Inverse(Quaternion.LookRotation(forearm.position - upperArm.position)) * upperArm.rotation;
				bodyMovements.fromNormRightForearm = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * forearm.rotation;
				bodyMovements.fromNormRightHand = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * hand.rotation;
			}
			if (fromNormHand == Quaternion.identity) {
				fromNormUpperArm = Quaternion.Inverse(Quaternion.LookRotation(forearm.position - upperArm.position)) * upperArm.rotation;
				fromNormForearm = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * forearm.rotation;
				fromNormHand = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * hand.rotation;
			}
		}

		float upperArmLength = Vector3.Distance(upperArm.position, forearm.position);
		float forearmLength = Vector3.Distance(forearm.position, hand.position);
		length = upperArmLength + forearmLength;

		upperArmStartPosition = upperArm.position;

	}
	
	public abstract void Calculate(Transform handTarget);
}

