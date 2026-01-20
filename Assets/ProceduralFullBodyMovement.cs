using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ProceduralFullBodyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Character Controller")]
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Upper Body Bones")]
    public Transform leftArm;
    public Transform rightArm;
    public Transform spine;
    public Transform chest;
    public Transform head;
    public Transform leftShoulder;
    public Transform rightShoulder;

    [Header("Upper Body Motion Settings")]
    public float armAmplitude = 30f;
    public float armSpeed = 2.5f;
    public float spineAmplitude = 5f;
    public float spineSpeed = 1.5f;
    public float headAmplitude = 3f;
    public float headSpeed = 1f;
    public float shoulderAmplitude = 5f;
    public float shoulderSpeed = 2f;

    [Header("Leg Bones")]
    public Transform leftThigh;
    public Transform rightThigh;
    public Transform leftShin;
    public Transform rightShin;

    [Header("Leg Motion Settings")]
    public float legSwingAngle = 30f;
    public float legSwingSpeed = 4f;

    // --- Initial rotations ---
    private Quaternion leftArmStart, rightArmStart;
    private Quaternion spineStart, chestStart, headStart;
    private Quaternion leftShoulderStart, rightShoulderStart;
    private Quaternion leftThighStart, rightThighStart;
    private Quaternion leftShinStart, rightShinStart;

    // --- Input tracking ---
    private Vector2 movementInput;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Store initial rotations
        if (leftArm != null) leftArmStart = leftArm.localRotation;
        if (rightArm != null) rightArmStart = rightArm.localRotation;
        if (spine != null) spineStart = spine.localRotation;
        if (chest != null) chestStart = chest.localRotation;
        if (head != null) headStart = head.localRotation;
        if (leftShoulder != null) leftShoulderStart = leftShoulder.localRotation;
        if (rightShoulder != null) rightShoulderStart = rightShoulder.localRotation;

        if (leftThigh != null) leftThighStart = leftThigh.localRotation;
        if (rightThigh != null) rightThighStart = rightThigh.localRotation;
        if (leftShin != null) leftShinStart = leftShin.localRotation;
        if (rightShin != null) rightShinStart = rightShin.localRotation;
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
        AnimateUpperBody();
        AnimateLegs();
    }

    // -------------------------
    // Get player input
    // -------------------------
    void HandleInput()
    {
        movementInput = Vector2.zero;
        if (Keyboard.current != null)
        {
            movementInput.x = (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0);
            movementInput.y = (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0);
        }
        movementInput = Vector2.ClampMagnitude(movementInput, 1f);
    }

    // -------------------------
    // Movement + Jump
    // -------------------------
    void HandleMovement()
    {
        // Ground check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = 0f;

        // World movement
        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);

        // Rotate character to face movement
        if (move.magnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        // Move character
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = jumpForce;
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // -------------------------
    // Upper Body Animation
    // -------------------------
    void AnimateUpperBody()
    {
        // Arms
        if (leftArm != null)
        {
            float angle = Mathf.Sin(Time.time * armSpeed) * armAmplitude;
            leftArm.localRotation = leftArmStart * Quaternion.Euler(0, 0, angle);
        }
        if (rightArm != null)
        {
            float angle = Mathf.Sin(Time.time * armSpeed + Mathf.PI) * armAmplitude;
            rightArm.localRotation = rightArmStart * Quaternion.Euler(0, 0, angle);
        }

        // Spine + Chest
        float spineAngle = Mathf.Sin(Time.time * spineSpeed) * spineAmplitude;
        if (spine != null) spine.localRotation = spineStart * Quaternion.Euler(0, 0, spineAngle);
        if (chest != null) chest.localRotation = chestStart * Quaternion.Euler(0, 0, spineAngle * 0.5f);

        // Head
        if (head != null)
        {
            float angle = Mathf.Sin(Time.time * headSpeed) * headAmplitude;
            head.localRotation = headStart * Quaternion.Euler(0, 0, angle);
        }

        // Shoulders
        if (leftShoulder != null)
        {
            float angle = Mathf.Sin(Time.time * shoulderSpeed) * shoulderAmplitude;
            leftShoulder.localRotation = leftShoulderStart * Quaternion.Euler(0, 0, angle);
        }
        if (rightShoulder != null)
        {
            float angle = Mathf.Sin(Time.time * shoulderSpeed + Mathf.PI) * shoulderAmplitude;
            rightShoulder.localRotation = rightShoulderStart * Quaternion.Euler(0, 0, angle);
        }
    }

    // -------------------------
    // Leg Animation
    // -------------------------
    void AnimateLegs()
    {
        // Use input magnitude to drive leg swing
        float speedFactor = Mathf.Clamp01(movementInput.magnitude);

        // Scale swing speed by movement
        float thighAngle = Mathf.Sin(Time.time * legSwingSpeed * Mathf.PI * 2 * speedFactor) * legSwingAngle * speedFactor;
        float shinAngle = Mathf.Max(0, -thighAngle * 0.5f);

        if (leftThigh != null) leftThigh.localRotation = leftThighStart * Quaternion.Euler(thighAngle, 0, 0);
        if (rightThigh != null) rightThigh.localRotation = rightThighStart * Quaternion.Euler(-thighAngle, 0, 0);

        if (leftShin != null) leftShin.localRotation = leftShinStart * Quaternion.Euler(shinAngle, 0, 0);
        if (rightShin != null) rightShin.localRotation = rightShinStart * Quaternion.Euler(shinAngle, 0, 0);
    }
}
