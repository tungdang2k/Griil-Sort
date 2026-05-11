using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropDragCtrl : MonoBehaviour
{

    [SerializeField] private Image m_imgFoodDrag;
    [SerializeField] private float m_dragSize = 160f;

    private enum DragState { Idle, Dragging, Animating }
    private DragState m_state = DragState.Idle;

    private FoodSlot m_sourceSlot;   // slot đang bị kéo
    private FoodSlot m_previewSlot;  // slot đang hiện preview mờ
    private int m_pointerID = -1;

    private void Start()
    {
             
        m_imgFoodDrag.gameObject.SetActive(false);
        m_imgFoodDrag.raycastTarget = false;

    }

    private void Update()
    {
        if (m_state != DragState.Dragging) return;

        
    }

    // ═════════════════════════════════════════
    //  PUBLIC API — gọi từ FoodSlot
    // ═════════════════════════════════════════

    public void OnBtnDown(PointerEventData evt)
    {
        if (m_state != DragState.Idle) return;

        FoodSlot slot = GetSlot(evt);
        if (slot == null || !slot.HasFood()) return;

        // ── Lock drag ──
        m_state = DragState.Dragging;
        m_pointerID = evt.pointerId;

        m_sourceSlot = slot;

        AudioManager.Instance?.PlaySFX(SFXType.Click);

        // Ẩn food gốc, bật dummy
        m_sourceSlot.OnActiveFood(false);
        DummyShow(m_sourceSlot.GetSpriteFood(), evt.position);
        m_imgFoodDrag.transform.DOScale(Vector3.one * 1.1f, 0.15f);
    }

    public void OnHasDrag(PointerEventData evt)
    {
        if (m_state != DragState.Dragging) return;
        if (evt.pointerId != m_pointerID) return;


        DummyMoveTo(evt.position);
        UpdatePreview(ResolveTarget(evt));
    }

    public void OnBtnUp(PointerEventData evt)
    {
        if (m_state != DragState.Dragging) return;
        if (evt.pointerId != m_pointerID) return;

        m_state = DragState.Animating;
        AudioManager.Instance?.PlaySFX(SFXType.Smoke);

      

        if (m_previewSlot != null)
            DropToSlot(m_previewSlot);
        else
            ReturnToSource();
    }

    // ═════════════════════════════════════════
    //  DROP LOGIC
    // ═════════════════════════════════════════

    private void DropToSlot(FoodSlot dest)
    {
        FoodSlot src = m_sourceSlot;   // capture local — tránh race condition

        dest.OnHideFood();             // xóa preview trước khi animate

        m_imgFoodDrag.transform.DOKill();
        m_imgFoodDrag.transform.DOScale(Vector3.one, 0.2f);
        m_imgFoodDrag.transform.DOMove(dest.transform.position, 0.2f)
            .OnComplete(() =>
            {
                DummyHide();
                dest.OnSetSlot(src.GetSpriteFood());
                dest.OnActiveFood(true);
                dest.OnCheckMerge();
                src.OnCheckPrepareTray();
                ResetState();
            });
    }

    private void ReturnToSource()
    {
        FoodSlot src = m_sourceSlot;   // capture local
        ClearPreview();

        m_imgFoodDrag.transform.DOKill();
        m_imgFoodDrag.transform.DOScale(Vector3.one, 0.3f);
        m_imgFoodDrag.transform.DOMove(src.transform.position, 0.3f)
            .OnComplete(() =>
            {
                DummyHide();
                src.OnActiveFood(true);
                ResetState();
            });
    }

    // ═════════════════════════════════════════
    //  PREVIEW
    // ═════════════════════════════════════════

    private void UpdatePreview(FoodSlot target)
    {
        if (target == m_previewSlot) return;  // không đổi → skip

        ClearPreview();

        if (target != null)
        {
            m_previewSlot = target;
            m_previewSlot.ShowPreview(m_sourceSlot.GetSpriteFood());
        }
    }

    private void ClearPreview()
    {
        if (m_previewSlot == null) return;
        m_previewSlot.OnHideFood();
        m_previewSlot = null;
    }

    // ═════════════════════════════════════════
    //  TARGET RESOLUTION
    // ═════════════════════════════════════════

    /// <summary>
    /// Trả về slot hợp lệ để thả vào:
    ///  - Slot trống   → dùng luôn
    ///  - Slot có food → lấy slot null từ GrillStation (cho merge/move)
    ///  - Slot gốc / null → return null (trả về source)
    /// </summary>
    private FoodSlot ResolveTarget(PointerEventData evt)
    {
        FoodSlot hover = GetSlot(evt);
        if (hover == null || hover == m_sourceSlot) return null;

        if (!hover.HasFood()) return hover;

        return hover.GetSlotNull;  // null nếu GrillStation đầy
    }

    // ═════════════════════════════════════════
    //  DUMMY IMAGE
    // ═════════════════════════════════════════

    private void DummyShow(Sprite sprite, Vector2 screenPos)
    {
        m_imgFoodDrag.sprite = sprite;
        m_imgFoodDrag.preserveAspect = true;

        var rt = m_imgFoodDrag.rectTransform;
        rt.sizeDelta = new Vector2(m_dragSize, m_dragSize);
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        m_imgFoodDrag.gameObject.SetActive(true);
        DummyMoveTo(screenPos);
    }

    private void DummyMoveTo(Vector2 screenPos)
    {
        if (m_imgFoodDrag.canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_imgFoodDrag.canvas.transform as RectTransform,
            screenPos, null, out Vector2 local);

        m_imgFoodDrag.rectTransform.localPosition = local;
    }

    private void DummyHide()
    {
        m_imgFoodDrag.transform.DOKill();
        m_imgFoodDrag.gameObject.SetActive(false);
    }

    // ═════════════════════════════════════════
    //  UTILITY
    // ═════════════════════════════════════════

    private void ResetState()
    {
        m_sourceSlot = null;
        m_previewSlot = null;
        m_pointerID = -1;
        m_state = DragState.Idle;
    }

    private static FoodSlot GetSlot(PointerEventData evt)
    {
        var obj = evt.pointerCurrentRaycast.gameObject;
        return obj != null ? obj.GetComponent<FoodSlot>() : null;
    }
}