namespace Controllers.Player
{
    using UnityEngine;

    public class PlayerController : MonoBehaviour
    {
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int Sprint = Animator.StringToHash("Sprint");
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int VerticalVelocity = Animator.StringToHash("VerticalVelocity");
        private CharacterController controller;
        
        private float horizontal;
        private float vertical;
        private bool sprinting;
        private bool jumpPressed;
        private Vector3 move;
        
        [SerializeField] private Animator animator;
        [SerializeField] private Transform cameraTransform;
        
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintSpeed = 7f;
        
        public float gravity = -9.81f;
        public float jumpForce = 9f;

        private float verticalVelocity;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
        }
        
        void Start()
        {
            LockCursor();
        }

        void Update()
        {
            HandleCursor();
            
            ReadInput();
            RotatePlayer();
            MoveCharacter();
            UpdateAnimations();
        }
        
        private void HandleCursor()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnlockCursor();
            }

            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }
        
        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        private void ReadInput()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            sprinting = Input.GetKey(KeyCode.LeftShift);
            jumpPressed = Input.GetButtonDown("Jump");
        }

        private void RotatePlayer()
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            move = forward * vertical + right * horizontal;
            move = Vector3.ClampMagnitude(move, 1f);

            if (move.sqrMagnitude < 0.01f)
                return;

            Quaternion target;

            if (Input.GetMouseButton(1))
                target = Quaternion.LookRotation(forward);
            else
                target = Quaternion.LookRotation(move);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                target,
                720f * Time.deltaTime
            );
        }
        
        private void MoveCharacter()
        {
            if (controller.isGrounded)
            {
                if (verticalVelocity < 0f)
                    verticalVelocity = -2f;

                if (jumpPressed)
                {
                    verticalVelocity = jumpForce;
                    animator.SetTrigger(Jump);
                }
            }

            verticalVelocity += gravity * Time.deltaTime;

            float currentSpeed = sprinting ? sprintSpeed : walkSpeed;

            Vector3 velocity = move * currentSpeed;
            velocity.y = verticalVelocity;

            controller.Move(velocity * Time.deltaTime);
        }
        
        private void UpdateAnimations()
        {
            Vector3 localMove = transform.InverseTransformDirection(move);

            animator.SetFloat(MoveX, localMove.x, 0.1f, Time.deltaTime);
            animator.SetFloat(MoveY, localMove.z, 0.1f, Time.deltaTime);
            
            animator.SetBool(Grounded, controller.isGrounded);
            animator.SetFloat(VerticalVelocity, Mathf.Clamp(verticalVelocity, -10f, 10f));

            animator.SetBool(Sprint, sprinting);
        }
    }
}