using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class ActionBarSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] Image icon;
    [SerializeField] GameObject emptyBackground;   // NEW: placeholder shown when slot has no ability
    [SerializeField] TMP_Text keyBindLabel;

    public int SlotIndex { get; private set; }

    ActionBar bar;
    AbilityData ability;

    public void Init(ActionBar bar, int index)
    {
        this.bar = bar;
        SlotIndex = index;
    }

    public void SetAbility(AbilityData ability, Key key)
    {
        this.ability = ability;
        bool hasAbility = ability != null;

        if (icon != null)
        {
            icon.sprite = hasAbility ? ability.icon : null;
            icon.enabled = hasAbility;
        }

        if (emptyBackground != null)
            emptyBackground.SetActive(!hasAbility);   // show placeholder only when empty

        if (keyBindLabel != null)
            keyBindLabel.text = KeyLabel(key);
    }

    // Key.Digit1 -> "1", Key.Numpad1 -> "1", leaves Q/W/E/R/T untouched.
    static string KeyLabel(Key key)
    {
        if (key == Key.None) return "";
        string s = key.ToString();
        if (s.StartsWith("Digit")) return s.Substring(5);
        if (s.StartsWith("Numpad")) return s.Substring(6);
        return s;
    }

    // ---- Drag & drop, gated on Left Shift ----
    static bool ShiftHeld =>
        Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

    public void OnBeginDrag(PointerEventData e)
    {
        if (!ShiftHeld || ability == null) return;
        bar.BeginDrag(this, e.position);   // pass cursor pos so the ghost starts under the mouse
    }

    public void OnDrag(PointerEventData e) => bar.DragTo(e.position);
    public void OnEndDrag(PointerEventData e) => bar.EndDrag();
    public void OnDrop(PointerEventData e) => bar.DropOnto(this);

}
