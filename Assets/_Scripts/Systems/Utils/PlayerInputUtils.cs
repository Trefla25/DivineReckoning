using System;
using UnityEngine;

public class PlayerInputUtils : MonoBehaviour
{
    public event Action<bool> MoveClick;   // bool = wasHeld
    public event Action SelectClick;
    public bool IsHeld => input.Main.MoveClick.IsPressed();

    CustomInputs input;

    void Awake()
    {
        input = new CustomInputs();
        input.Main.MoveClick.performed += _ => MoveClick?.Invoke(false);
        input.Main.SelectClick.performed += _ => SelectClick?.Invoke();
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();
}
