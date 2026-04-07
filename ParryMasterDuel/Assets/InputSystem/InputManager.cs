using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // ?? Private ???????????????????????????????????????????????
    // Generic movement interface so we don't care which character it is
    private MonoBehaviour p1Movement;
    private MonoBehaviour p2Movement;
    public bool inputEnabled = false;
    private InputAction p1Move, p1Attack, p1Block, p1Jump;
    private InputAction p2Move, p2Attack, p2Block, p2Jump;

    private bool isSetup = false;

    void Awake()
    {
        Instance = this;
    }

    // ?? Called by GameManager after spawning both players ??????
    public void SetPlayers(GameObject p1, GameObject p2)
    {
        // Grab whichever movement script is on each spawned character
        p1Movement = GetMovement(p1);
        p2Movement = GetMovement(p2);

        if (p1Movement == null) { Debug.LogError("No movement script found on Player 1!"); return; }
        if (p2Movement == null) { Debug.LogError("No movement script found on Player 2!"); return; }

        // Grab PlayerInput components
        PlayerInput p1Input = p1.GetComponent<PlayerInput>();
        PlayerInput p2Input = p2.GetComponent<PlayerInput>();

        if (p1Input == null) { Debug.LogError("No PlayerInput on Player 1!"); return; }
        if (p2Input == null) { Debug.LogError("No PlayerInput on Player 2!"); return; }

        // Always use Player1 map for p1 and Player2 map for p2
        var p1Map = p1Input.actions.FindActionMap("Player1");
        var p2Map = p2Input.actions.FindActionMap("Player2");

        p1Move = p1Map.FindAction("Move");
        p1Attack = p1Map.FindAction("Attack");
        p1Block = p1Map.FindAction("Block");
        p1Jump = p1Map.FindAction("Jump");

        p2Move = p2Map.FindAction("Move");
        p2Attack = p2Map.FindAction("Attack");
        p2Block = p2Map.FindAction("Block");
        p2Jump = p2Map.FindAction("Jump");

        // Enable all actions
        p1Move.Enable(); p1Attack.Enable(); p1Block.Enable(); p1Jump.Enable();
        p2Move.Enable(); p2Attack.Enable(); p2Block.Enable(); p2Jump.Enable();

        // Hook up events
        p1Attack.performed += OnP1Attack;
        p1Jump.performed += OnP1Jump;
        p1Block.started += OnP1BlockStart;
        p1Block.canceled += OnP1BlockEnd;

        p2Attack.performed += OnP2Attack;
        p2Jump.performed += OnP2Jump;
        p2Block.started += OnP2BlockStart;
        p2Block.canceled += OnP2BlockEnd;

        isSetup = true;
    }

    void Update()
    {
        if (!isSetup || !inputEnabled) return;

        // Feed movement every frame
        if (p1Movement != null && p1Movement.gameObject.activeInHierarchy)
            CallSetMoveInput(p1Movement, p1Move.ReadValue<float>());

        if (p2Movement != null && p2Movement.gameObject.activeInHierarchy)
            CallSetMoveInput(p2Movement, p2Move.ReadValue<float>());

        UpdateFacing();
    }

    // ?? Input callbacks ????????????????????????????????????????
    void OnP1Attack(InputAction.CallbackContext ctx)
    {
        if (!inputEnabled) return;
        if (IsActive(p1Movement)) CallTriggerAttack(p1Movement);
    }
    void OnP2Attack(InputAction.CallbackContext ctx)
    {
        if (!inputEnabled) return;
        if (IsActive(p2Movement)) CallTriggerAttack(p2Movement);
    }

    void OnP1Jump(InputAction.CallbackContext ctx)
    {
        if (!inputEnabled) return;
        if (IsActive(p1Movement)) CallOnSpecial(p1Movement);
    }
    void OnP2Jump(InputAction.CallbackContext ctx)
    {
        if (!inputEnabled) return;
        if (IsActive(p2Movement)) CallOnSpecial(p2Movement);
    }

    void OnP1BlockStart(InputAction.CallbackContext ctx)
    {
        if (!inputEnabled) return;
        if (IsActive(p1Movement)) CallStartBlock(p1Movement);
    }
    void OnP1BlockEnd(InputAction.CallbackContext ctx)
    {
        if (!inputEnabled) return;
        if (IsActive(p1Movement)) CallEndBlock(p1Movement);
    }
    void OnP2BlockStart(InputAction.CallbackContext ctx)
    {
        if (!inputEnabled) return;
        if (IsActive(p2Movement)) CallStartBlock(p2Movement);
    }
    void OnP2BlockEnd(InputAction.CallbackContext ctx)
    {
        if (!inputEnabled) return;
        if (IsActive(p2Movement)) CallEndBlock(p2Movement);
    }

    // ?? Facing update ??????????????????????????????????????????
    void UpdateFacing()
    {
        if (p1Movement == null || p2Movement == null) return;
        if (!p1Movement.gameObject.activeInHierarchy) return;
        if (!p2Movement.gameObject.activeInHierarchy) return;

        Transform t1 = p1Movement.transform;
        Transform t2 = p2Movement.transform;

        Transform left = t1.position.x < t2.position.x ? t1 : t2;
        Transform right = t1.position.x < t2.position.x ? t2 : t1;

        left.localScale = new Vector3(Mathf.Abs(left.localScale.x), left.localScale.y, left.localScale.z);
        right.localScale = new Vector3(-Mathf.Abs(right.localScale.x), right.localScale.y, right.localScale.z);
    }

    // ?? Helpers ????????????????????????????????????????????????
    // Gets whichever movement script exists on the GameObject
    MonoBehaviour GetMovement(GameObject go)
    {
        MonoBehaviour m;
        m = go.GetComponent<KnightMovement>(); if (m != null) return m;
        m = go.GetComponent<VikingMovement>(); if (m != null) return m;
        m = go.GetComponent<NinjaMovement>(); if (m != null) return m;
        m = go.GetComponent<SoldierMovement>(); if (m != null) return m;
        m = go.GetComponent<CaveManMovement>(); if (m != null) return m;
        return null;
    }

    bool IsActive(MonoBehaviour m) => m != null && m.gameObject.activeInHierarchy;

    void CallSetMoveInput(MonoBehaviour m, float value)
    {
        if (m is KnightMovement k) k.SetMoveInput(value);
        else if (m is VikingMovement v) v.SetMoveInput(value);
        else if (m is NinjaMovement n) n.SetMoveInput(value);
        else if (m is SoldierMovement s) s.SetMoveInput(value);
        else if (m is CaveManMovement c) c.SetMoveInput(value);
    }

    void CallTriggerAttack(MonoBehaviour m)
    {
        if (m is KnightMovement k) k.TriggerAttack();
        else if (m is VikingMovement v) v.TriggerAttack();
        else if (m is NinjaMovement n) n.TriggerAttack();
        else if (m is SoldierMovement s) s.TriggerAttack();
        else if (m is CaveManMovement c) c.TriggerAttack();
    }

    void CallOnSpecial(MonoBehaviour m)
    {
        if (m is KnightMovement k) k.OnSpecialPerformed();
        else if (m is VikingMovement v) v.OnSpecialPerformed();
        else if (m is NinjaMovement n) n.OnSpecialPerformed();
        else if (m is SoldierMovement s) s.OnSpecialPerformed();
        else if (m is CaveManMovement c) c.OnSpecialPerformed();
    }

    void CallStartBlock(MonoBehaviour m)
    {
        if (m is KnightMovement k) k.StartBlock();
        else if (m is VikingMovement v) v.StartBlock();
        else if (m is NinjaMovement n) n.StartBlock();
        else if (m is SoldierMovement s) s.StartBlock();
        else if (m is CaveManMovement c) c.StartBlock();
    }

    void CallEndBlock(MonoBehaviour m)
    {
        if (m is KnightMovement k) k.EndBlock();
        else if (m is VikingMovement v) v.EndBlock();
        else if (m is NinjaMovement n) n.EndBlock();
        else if (m is SoldierMovement s) s.EndBlock();
        else if (m is CaveManMovement c) c.EndBlock();
    }

    void OnDisable()
    {
        if (!isSetup) return;

        p1Attack.performed -= OnP1Attack;
        p1Jump.performed -= OnP1Jump;
        p1Block.started -= OnP1BlockStart;
        p1Block.canceled -= OnP1BlockEnd;

        p2Attack.performed -= OnP2Attack;
        p2Jump.performed -= OnP2Jump;
        p2Block.started -= OnP2BlockStart;
        p2Block.canceled -= OnP2BlockEnd;

        p1Move.Disable(); p1Attack.Disable(); p1Block.Disable(); p1Jump.Disable();
        p2Move.Disable(); p2Attack.Disable(); p2Block.Disable(); p2Jump.Disable();
    }
}