using UnityEngine;
using UnityEngine.UI;

public class ActionBar : MonoBehaviour
{
    [SerializeField] AbilityCaster caster;
    [SerializeField] ActionBarSlot[] slots;    // Slot1..Slot4 in order
    [SerializeField] Image dragGhost;          // icon that follows the cursor(raycastTarget OFF, starts disabled)

        ActionBarSlot draggingSlot;

    void Awake()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].Init(this, i);
    }

    void OnEnable()
    {
        if (caster != null) caster.OnHotbarChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (caster != null) caster.OnHotbarChanged -= Refresh;
    }

    // Re-draw every slot from the caster (the source of truth).
    public void Refresh()
    {
        if (caster == null) return;
        for (int i = 0; i < slots.Length; i++)
            slots[i].SetAbility(caster.GetAbility(i), caster.GetKey(i));
    }

    // ---- drag lifecycle (called by ActionBarSlot) ----
    public void BeginDrag(ActionBarSlot from, Vector2 pointerPos)
    {
        draggingSlot = from;
        if (dragGhost != null)
        {
            var a = caster.GetAbility(from.SlotIndex);
            dragGhost.sprite = a != null ? a.icon : null;
            dragGhost.transform.SetAsLastSibling();       // render on top of every slot
            dragGhost.rectTransform.position = pointerPos; // appear under the cursor immediately
            dragGhost.gameObject.SetActive(true);          // robust: activates the whole object
        }
    }

    public void DragTo(Vector2 screenPos)
    {
        if (draggingSlot == null || dragGhost == null) return;
        dragGhost.rectTransform.position = screenPos;
    }

    public void DropOnto(ActionBarSlot target)
    {
        if (draggingSlot == null) return;          // not a valid (shift) drag
        caster.SwapSlots(draggingSlot.SlotIndex, target.SlotIndex);
    }

    public void EndDrag()
    {
        draggingSlot = null;
        if (dragGhost != null) dragGhost.gameObject.SetActive(false);
    }

}
