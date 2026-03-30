using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Characters")]
    public KnightMovement knight;
    public VikingMovement viking;
    public NinjaMovement  ninja;
    public SoldierMovement soldier;
    public CaveManMovement caveMan;
   
    private PlayerInput knightInput;
    private PlayerInput vikingInput;
    private PlayerInput ninjaInput;

    private InputAction knightMove, knightSpecial, ninjaMove;
    private InputAction vikingMove, vikingSpecial, ninjaSpecial;
    private InputAction knightAttack, vikingAttack, ninjaAttack;
    private InputAction knightBlock, vikingBlock, ninjaBlock;

  
    private PlayerInput soldierInput;
    private InputAction soldierMove, soldierSpecial, soldierAttack, soldierBlock;

    private PlayerInput caveManInput;
    private InputAction caveManMove, caveManSpecial, caveManAttack, caveManBlock;
    void Awake()
    {
        knightInput = knight.GetComponent<PlayerInput>();
        vikingInput = viking.GetComponent<PlayerInput>();
        ninjaInput = ninja.GetComponent<PlayerInput>();
        soldierInput = soldier.GetComponent<PlayerInput>();
        caveManInput = caveMan.GetComponent<PlayerInput>();
       
   
        // Knight reads from whichever action map is assigned
        string knightMap = knight.isPlayer1 ? "Player1" : "Player2";
        string vikingMap = viking.isPlayer1 ? "Player1" : "Player2";
        string ninjaMap = ninja.isPlayer1 ? "Player1" : "Player2";
        string soldierMap = soldier.isPlayer1 ? "Player1" : "Player2";
        string caveManMap = caveMan.isPlayer1 ? "Player1" : "Player2";

        knightMove = knightInput.actions.FindActionMap(knightMap).FindAction("Move");
        ninjaSpecial = ninjaInput.actions.FindActionMap(ninjaMap).FindAction("Jump");
        ninjaMove = ninjaInput.actions.FindActionMap(ninjaMap).FindAction("Move");
        knightSpecial = knightInput.actions.FindActionMap(knightMap).FindAction("Jump");
        vikingMove = vikingInput.actions.FindActionMap(vikingMap).FindAction("Move");
        vikingSpecial = vikingInput.actions.FindActionMap(vikingMap).FindAction("Jump");
        knightAttack = knightInput.actions.FindActionMap(knightMap).FindAction("Attack");
        vikingAttack = vikingInput.actions.FindActionMap(vikingMap).FindAction("Attack");
        ninjaAttack = ninjaInput.actions.FindActionMap(ninjaMap).FindAction("Attack");
        knightBlock = knightInput.actions.FindActionMap(knightMap).FindAction("Block");
        vikingBlock = vikingInput.actions.FindActionMap(vikingMap).FindAction("Block");
        ninjaBlock = ninjaInput.actions.FindActionMap(ninjaMap).FindAction("Block");

        soldierMove = soldierInput.actions.FindActionMap(soldierMap).FindAction("Move");
        soldierSpecial = soldierInput.actions.FindActionMap(soldierMap).FindAction("Jump");
        soldierAttack = soldierInput.actions.FindActionMap(soldierMap).FindAction("Attack");
        soldierBlock = soldierInput.actions.FindActionMap(soldierMap).FindAction("Block");

        caveManMove = caveManInput.actions.FindActionMap(caveManMap).FindAction("Move");
        caveManSpecial = caveManInput.actions.FindActionMap(caveManMap).FindAction("Jump");
        caveManAttack = caveManInput.actions.FindActionMap(caveManMap).FindAction("Attack");
        caveManBlock = caveManInput.actions.FindActionMap(caveManMap).FindAction("Block");
    }

    void OnEnable()
    {
        knightMove.Enable(); knightSpecial.Enable();
        vikingMove.Enable(); vikingSpecial.Enable();
        knightAttack.Enable();
        vikingAttack.Enable();
        ninjaMove.Enable(); ninjaSpecial.Enable();
        ninjaAttack.Enable();
        knightSpecial.performed += OnKnightJump;
        vikingSpecial.performed += OnVikingJump;
        knightAttack.performed += OnKnightAttack;
        vikingAttack.performed += OnVikingAttack;
        ninjaSpecial.performed += OnNinjaJump;
        ninjaAttack.performed += OnNinjaAttack;

        knightBlock.Enable();
        vikingBlock.Enable();
        ninjaBlock.Enable();
        knightBlock.started += OnKnightBlockStart;
        knightBlock.canceled += OnKnightBlockEnd;
        vikingBlock.started += OnVikingBlockStart;
        vikingBlock.canceled += OnVikingBlockEnd;
        ninjaBlock.started += OnNinjaBlockStart;
        ninjaBlock.canceled += OnNinjaBlockEnd;

        soldierMove.Enable(); soldierSpecial.Enable();
        soldierAttack.Enable(); soldierBlock.Enable();
        soldierSpecial.performed += OnSoldierJump;
        soldierAttack.performed += OnSoldierAttack;
        soldierBlock.started += OnSoldierBlockStart;
        soldierBlock.canceled += OnSoldierBlockEnd;

        caveManMove.Enable(); caveManSpecial.Enable();
        caveManAttack.Enable(); caveManBlock.Enable();
        caveManSpecial.performed += OnCaveManJump;
        caveManAttack.performed += OnCaveManAttack;
        caveManBlock.started += OnCaveManBlockStart;
        caveManBlock.canceled += OnCaveManBlockEnd;
    }

    void OnDisable()
    {
        knightSpecial.performed -= OnKnightJump;
        vikingSpecial.performed -= OnVikingJump;
        knightAttack.performed -= OnKnightAttack;
        vikingAttack.performed -= OnVikingAttack;
        ninjaSpecial.performed -= OnNinjaJump;
        ninjaAttack.performed  -= OnNinjaAttack;
        knightMove.Disable(); knightSpecial.Disable();
        vikingMove.Disable(); vikingSpecial.Disable();
        knightAttack.Disable();
        vikingAttack.Disable();
        ninjaMove.Disable(); ninjaSpecial.Disable();
        ninjaAttack.Disable();

        knightBlock.started -= OnKnightBlockStart;
        knightBlock.canceled -= OnKnightBlockEnd;
        vikingBlock.started -= OnVikingBlockStart;
        vikingBlock.canceled -= OnVikingBlockEnd;
        ninjaBlock.started  -= OnNinjaBlockStart;
        ninjaBlock.canceled -= OnNinjaBlockEnd;
        knightBlock.Disable();
        vikingBlock.Disable();
        ninjaBlock.Disable();

        soldierSpecial.performed -= OnSoldierJump;
        soldierAttack.performed -= OnSoldierAttack;
        soldierBlock.started -= OnSoldierBlockStart;
        soldierBlock.canceled -= OnSoldierBlockEnd;
        soldierMove.Disable(); soldierSpecial.Disable();
        soldierAttack.Disable(); soldierBlock.Disable();

        caveManSpecial.performed -= OnCaveManJump;
        caveManAttack.performed -= OnCaveManAttack;
        caveManBlock.started -= OnCaveManBlockStart;
        caveManBlock.canceled -= OnCaveManBlockEnd;
        caveManMove.Disable(); caveManSpecial.Disable();
        caveManAttack.Disable(); caveManBlock.Disable();
    }

    void OnKnightJump(InputAction.CallbackContext ctx) { if (knight != null && knight.gameObject.activeInHierarchy) knight.OnSpecialPerformed(); }
    void OnVikingJump(InputAction.CallbackContext ctx) { if (viking != null && viking.gameObject.activeInHierarchy) viking.OnSpecialPerformed(); }
    void OnNinjaJump(InputAction.CallbackContext ctx) { if (ninja != null && ninja.gameObject.activeInHierarchy) ninja.OnSpecialPerformed(); }

    void OnKnightAttack(InputAction.CallbackContext ctx) { if (knight != null && knight.gameObject.activeInHierarchy) knight.TriggerAttack(); }
    void OnVikingAttack(InputAction.CallbackContext ctx) { if (viking != null && viking.gameObject.activeInHierarchy) viking.TriggerAttack(); }
    void OnNinjaAttack(InputAction.CallbackContext ctx) { if (ninja != null && ninja.gameObject.activeInHierarchy) ninja.TriggerAttack(); }

    void OnKnightBlockStart(InputAction.CallbackContext ctx) { if (knight != null && knight.gameObject.activeInHierarchy) knight.StartBlock(); }
    void OnKnightBlockEnd(InputAction.CallbackContext ctx) { if (knight != null && knight.gameObject.activeInHierarchy) knight.EndBlock(); }
    void OnVikingBlockStart(InputAction.CallbackContext ctx) { if (viking != null && viking.gameObject.activeInHierarchy) viking.StartBlock(); }
    void OnVikingBlockEnd(InputAction.CallbackContext ctx) { if (viking != null && viking.gameObject.activeInHierarchy) viking.EndBlock(); }
    void OnNinjaBlockStart(InputAction.CallbackContext ctx) { if (ninja != null && ninja.gameObject.activeInHierarchy) ninja.StartBlock(); }
    void OnNinjaBlockEnd(InputAction.CallbackContext ctx) { if (ninja != null && ninja.gameObject.activeInHierarchy) ninja.EndBlock(); }

    void OnSoldierJump(InputAction.CallbackContext ctx) { if (soldier != null && soldier.gameObject.activeInHierarchy) soldier.OnSpecialPerformed(); }
    void OnSoldierAttack(InputAction.CallbackContext ctx) { if (soldier != null && soldier.gameObject.activeInHierarchy) soldier.TriggerAttack(); }
    void OnSoldierBlockStart(InputAction.CallbackContext ctx) { if (soldier != null && soldier.gameObject.activeInHierarchy) soldier.StartBlock(); }
    void OnSoldierBlockEnd(InputAction.CallbackContext ctx) { if (soldier != null && soldier.gameObject.activeInHierarchy) soldier.EndBlock(); }
    void OnCaveManJump(InputAction.CallbackContext ctx) { if (caveMan != null && caveMan.gameObject.activeInHierarchy) caveMan.OnSpecialPerformed(); }
    void OnCaveManAttack(InputAction.CallbackContext ctx) { if (caveMan != null && caveMan.gameObject.activeInHierarchy) caveMan.TriggerAttack(); }
    void OnCaveManBlockStart(InputAction.CallbackContext ctx) { if (caveMan != null && caveMan.gameObject.activeInHierarchy) caveMan.StartBlock(); }
    void OnCaveManBlockEnd(InputAction.CallbackContext ctx) { if (caveMan != null && caveMan.gameObject.activeInHierarchy) caveMan.EndBlock(); }
    void Update()
    {
        if (knight != null && knight.gameObject.activeInHierarchy)
            knight.SetMoveInput(knightMove.ReadValue<float>());
        if (viking != null && viking.gameObject.activeInHierarchy)
            viking.SetMoveInput(vikingMove.ReadValue<float>());
        if (ninja != null && ninja.gameObject.activeInHierarchy)
            ninja.SetMoveInput(ninjaMove.ReadValue<float>());
        if (soldier != null && soldier.gameObject.activeInHierarchy)
            soldier.SetMoveInput(soldierMove.ReadValue<float>());
        if (caveMan != null && caveMan.gameObject.activeInHierarchy)
            caveMan.SetMoveInput(caveManMove.ReadValue<float>());
        UpdateFacing();
    }

    void UpdateFacing()
    {
        // collect active characters
        Transform p1 = null, p2 = null;

        if (knight != null && knight.gameObject.activeInHierarchy)
            AssignPlayers(knight.transform, ref p1, ref p2);
        if (viking != null && viking.gameObject.activeInHierarchy)
            AssignPlayers(viking.transform, ref p1, ref p2);
        if (ninja != null && ninja.gameObject.activeInHierarchy)
            AssignPlayers(ninja.transform, ref p1, ref p2);
        if (soldier != null && soldier.gameObject.activeInHierarchy)
            AssignPlayers(soldier.transform, ref p1, ref p2);
        if (caveMan != null && caveMan.gameObject.activeInHierarchy)
            AssignPlayers(caveMan.transform, ref p1, ref p2);

        if (p1 == null || p2 == null) return;

        // whoever is on the left faces right, whoever is on the right faces left
        Transform leftPlayer = p1.position.x < p2.position.x ? p1 : p2;
        Transform rightPlayer = p1.position.x < p2.position.x ? p2 : p1;

        leftPlayer.localScale = new Vector3(1f, 1f, 1f);
        rightPlayer.localScale = new Vector3(-1f, 1f, 1f);
    }

    void AssignPlayers(Transform t, ref Transform p1, ref Transform p2)
    {
        if (p1 == null) p1 = t;
        else if (p2 == null) p2 = t;
    }
}