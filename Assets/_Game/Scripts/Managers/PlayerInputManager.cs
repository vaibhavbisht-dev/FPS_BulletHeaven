using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool IsJumping { get; private set; }

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    private void Awake()
    {
        // Define Move Action (WASD + Stick)
        moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
        moveAction.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // Define Look Action (Mouse Delta + Stick)
        lookAction = new InputAction("Look", binding: "<Gamepad>/rightStick");
        lookAction.AddBinding("<Mouse>/delta");

        // Define Jump Action (Space + Gamepad South)
        jumpAction = new InputAction("Jump", binding: "<Gamepad>/buttonSouth");
        jumpAction.AddBinding("<Keyboard>/space");

    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
    }
    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {
        MoveInput = moveAction.ReadValue<Vector2>();

        LookInput = lookAction.ReadValue<Vector2>();

        JumpTriggered = jumpAction.WasPerformedThisFrame();
        IsJumping = jumpAction.IsPressed();
    }

}
