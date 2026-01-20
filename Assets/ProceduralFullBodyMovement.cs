using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(CharacterController))]
public class ProceduralFullBodyMovement_WithAnimationRiggingIK : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Upper Body Bones")]
    public Transform leftArm, rightArm;
    public Transform spine, chest, head;
    public Transform leftShoulder, rightShoulder;

    [Header("Leg Bones")]
    public Transform leftFoot, rightFoot;

    [Header("Foot IK Targets")]
    public Transform leftFootTarget;
    public Transform rightFootTarget;

    [Header("IK Constraints")]
    public TwoBoneIKConstraint leftLegIK;
    public TwoBoneIKConstraint rightLegIK;

    [Header("IK Settings")]
    public LayerMask groundMask;
    public float footRaycastDistance = 1.5f;
    public float footHeightOffset = 0.05f;
    public float footMoveSpeed = 12f;

    [Header("Procedural Motion")]
    public float armSwing = 30f;
    public float armSpeed = 2.5f;
    public float spineSway = 5f;
    public float spineSpeed = 1.5f;

    // Internals
    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 input;
    private bool isGrounded;

    // Initial rotations
    private Quaternion lArmStart, rArmStart;
    private Quaternion spineStart, chestStart, headStart;
    private Quaternion lShoulderStart, rShoulderStart;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        lArmStart = leftArm.localRotation;
        rArmStart = rightArm.localRotation;
        spineStart = spine.localRotation;
        chestStart = chest.localRotation;
        headStart = head.localRotation;
        lShoulderStart = leftShoulder.localRotation;
        rShoulderStart = rightShoulder.localRotation;
    }

    void Update()
    {
        ReadInput();
        HandleMovement();
        AnimateUpperBody();
    }

    void LateUpdate()
    {
        UpdateFootIK();
    }

    // ---------------- INPUT ----------------
    void ReadInput()
    {
        input = Vector2.zero;
        if (Keyboard.current != null)
        {
            input.x = (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0);
            input.y = (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0);
        }
        input = Vector2.ClampMagnitude(input, 1f);
    }

    // ---------------- MOVEMENT ----------------
    void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = 0f;

        Vector3 move = new Vector3(input.x, 0, input.y);

        if (move.magnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            velocity.y = jumpForce;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ---------------- UPPER BODY ----------------
    void AnimateUpperBody()
    {
        float swing = Mathf.Sin(Time.time * armSpeed);

        leftArm.localRotation = lArmStart * Quaternion.Euler(0, 0, swing * armSwing);
        rightArm.localRotation = rArmStart * Quaternion.Euler(0, 0, -swing * armSwing);

        spine.localRotation = spineStart * Quaternion.Euler(0, 0, swing * spineSway);
        chest.localRotation = chestStart * Quaternion.Euler(0, 0, swing * spineSway * 0.5f);
        head.localRotation = headStart * Quaternion.Euler(0, 0, swing * spineSway * 0.3f);

        leftShoulder.localRotation = lShoulderStart * Quaternion.Euler(0, 0, swing * 5f);
        rightShoulder.localRotation = rShoulderStart * Quaternion.Euler(0, 0, -swing * 5f);
    }

    // ---------------- FOOT IK ----------------
    void UpdateFootIK()
    {
        float movementAmount = input.magnitude;

        UpdateFootTarget(leftFoot, leftFootTarget);
        UpdateFootTarget(rightFoot, rightFootTarget);

        // Blend IK strength with movement
        leftLegIK.weight = movementAmount;
        rightLegIK.weight = movementAmount;
    }

    void UpdateFootTarget(Transform foot, Transform target)
    {
        Vector3 rayStart = foot.position + Vector3.up * 0.5f;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, footRaycastDistance, groundMask))
        {
            Vector3 desired = hit.point;
            desired.y += footHeightOffset;

            target.position = Vector3.Lerp(
                target.position,
                desired,
                Time.deltaTime * footMoveSpeed
            );

            target.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;
        }
    }
}
