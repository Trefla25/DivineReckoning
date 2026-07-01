using System;
using UnityEngine;

public class PlayerInputUtils : MonoBehaviour
{
    public event Action<bool> MoveClick;        // RMB  (bool = wasHeld)
    public event Action<bool> AttackMoveClick;  // LMB  (bool = wasHeld)

    public bool IsMoveHeld => input.Main.MoveClick.IsPressed();
    public bool IsAttackHeld => input.Main.SelectClick.IsPressed();

    CustomInputs input;

    void Awake()
    {
        input = new CustomInputs();
        input.Main.MoveClick.performed += _ => MoveClick?.Invoke(false);
        input.Main.SelectClick.performed += _ => AttackMoveClick?.Invoke(false);
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();
}
