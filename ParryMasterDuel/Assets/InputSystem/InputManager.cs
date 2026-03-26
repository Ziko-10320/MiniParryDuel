using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Characters")]
    public KnightMovement knight;
    public VikingMovement viking;

    private PlayerInput knightInput;
    private PlayerInput vikingInput;

    private InputAction knightMove, knightSpecial;
    private InputAction vikingMove, vikingSpecial;
    private InputAction knightAttack, vikingAttack;
    private InputAction knightBlock, vikingBlock;
    void Awake()
    {
        knightInput = knight.GetComponent<PlayerInput>();
        vikingInput = viking.GetComponent<PlayerInput>();

        // Knight reads from whichever action map is assigned
        string knightMap = knight.isPlayer1 ? "Player1" : "Player2";
        string vikingMap = viking.isPlayer1 ? "Player1" : "Player2";

        knightMove = knightInput.actions.FindActionMap(knightMap).FindAction("Move");
        knightSpecial = knightInput.actions.FindActionMap(knightMap).FindAction("Jump");
        vikingMove = vikingInput.actions.FindActionMap(vikingMap).FindAction("Move");
        vikingSpecial = vikingInput.actions.FindActionMap(vikingMap).FindAction("Jump");
        knightAttack = knightInput.actions.FindActionMap(knightMap).FindAction("Attack");
        vikingAttack = vikingInput.actions.FindActionMap(vikingMap).FindAction("Attack");
        knightBlock = knightInput.actions.FindActionMap(knightMap).FindAction("Block");
        vikingBlock = vikingInput.actions.FindActionMap(vikingMap).FindAction("Block");
    }

    void OnEnable()
    {
        knightMove.Enable(); knightSpecial.Enable();
        vikingMove.Enable(); vikingSpecial.Enable();
        knightAttack.Enable();
        vikingAttack.Enable();
        knightSpecial.performed += OnKnightJump;
        vikingSpecial.performed += OnVikingJump;
        knightAttack.performed += OnKnightAttack;
        vikingAttack.performed += OnVikingAttack;

        knightBlock.Enable();
        vikingBlock.Enable();
        knightBlock.started += ctx => knight.StartBlock();
        knightBlock.canceled += ctx => knight.EndBlock();
        vikingBlock.started += ctx => viking.StartBlock();
        vikingBlock.canceled += ctx => viking.EndBlock();
    }

    void OnDisable()
    {
        knightSpecial.performed -= OnKnightJump;
        vikingSpecial.performed -= OnVikingJump;
        knightAttack.performed -= OnKnightAttack;
        vikingAttack.performed -= OnVikingAttack;
        knightMove.Disable(); knightSpecial.Disable();
        vikingMove.Disable(); vikingSpecial.Disable();
        knightAttack.Disable();
        vikingAttack.Disable();

        knightBlock.started -= ctx => knight.StartBlock();
        knightBlock.canceled -= ctx => knight.EndBlock();
        vikingBlock.started -= ctx => viking.StartBlock();
        vikingBlock.canceled -= ctx => viking.EndBlock();
        knightBlock.Disable();
        vikingBlock.Disable();
    }

    void OnKnightJump(InputAction.CallbackContext ctx) => knight.OnSpecialPerformed();
    void OnVikingJump(InputAction.CallbackContext ctx) => viking.OnSpecialPerformed();

    void OnKnightAttack(InputAction.CallbackContext ctx) => knight.TriggerAttack();
    void OnVikingAttack(InputAction.CallbackContext ctx) => viking.TriggerAttack();
    void Update()
    {
        knight.SetMoveInput(knightMove.ReadValue<float>());
        viking.SetMoveInput(vikingMove.ReadValue<float>());
        float dir = viking.transform.position.x - knight.transform.position.x;

        knight.transform.localScale = new Vector3(dir > 0 ? 1f : -1f, 1f, 1f);
        viking.transform.localScale = new Vector3(dir > 0 ? -1f : 1f, 1f, 1f);
    }
}