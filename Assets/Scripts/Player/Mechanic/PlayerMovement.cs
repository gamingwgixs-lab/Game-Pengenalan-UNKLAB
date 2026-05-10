using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] bool _moveState = false;
    [SerializeField] bool _isWalking = false;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip footstepSFX;
    [SerializeField] private float stepInterval = 0.4f;
    private float nextStepTime;

    private Vector3 velocity;
    private Vector3 moveDirection;
    private const float groundedGravity = -2f;

    public bool MoveState { get => _moveState; private set => _moveState = value; }
    public bool isWalking { get => _isWalking; private set => _isWalking = value; }
    public Vector2 input { get; private set; }

    public void SetMoveState(bool state)
    {
        MoveState = state;
        if (!MoveState) SetIsWalking(false);
    }
    private void SetIsWalking(bool state) { isWalking = state; }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void EnablerMoveState()
    {
        MoveState = !MoveState;
        if (!MoveState) SetIsWalking(false);
    }

    private void Update()
    {
        ApplyGravity();
        if (!MoveState) return;
        ProcessMovement();
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = groundedGravity;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }

    private void ProcessMovement()
    {
        moveDirection.x = input.x;
        moveDirection.y = 0;
        moveDirection.z = input.y;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        characterController.Move(moveDirection * speed * Time.deltaTime);

        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
        bool beneranGerak = horizontalVelocity.magnitude > 0.1f;

        if (isWalking != beneranGerak)
        {
            SetIsWalking(beneranGerak);
        }

        ProcessSoundTracking(beneranGerak);
    }

    private void ProcessSoundTracking(bool beneranGerak)
    {
        if (beneranGerak)
        {
            if (Time.time >= nextStepTime)
            {
                if (footstepSFX != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayFootstep(footstepSFX, Random.Range(0.4f, 0.6f));
                }

                nextStepTime = Time.time + stepInterval;
            }
        }
        else
        {     
        }
    }

}