using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FoodSlot : MonoBehaviour, IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [SerializeField] private float m_padding = 20f;
    //[SerializeField] private float m_slotScale = 0.65f;
    private DropDragCtrl m_dropDragCtrl;

    private Color m_nomalColor = new Color(1f, 1f, 1f, 1f);
    private Color m_fadeColor = new Color(1f,1f, 1f, 0.5f);
    private Image m_imgFood;
    

    private GrillStation m_grillCtrl;
    public Image ImgFood => m_imgFood;

    private void Awake()
    {
       
        m_dropDragCtrl = FindAnyObjectByType<DropDragCtrl>();
        m_imgFood = this.transform.GetChild(0).GetComponent<Image>();
        m_imgFood.gameObject.SetActive(false);
        m_grillCtrl = GetComponentInParent<GrillStation>();


    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HasFood()) return;

        m_dropDragCtrl.OnBtnDown(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        m_dropDragCtrl.OnHasDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        m_dropDragCtrl.OnBtnUp(eventData);
    }
    private void ApplyLayout()
    {
        RectTransform rt = m_imgFood.rectTransform;

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(m_padding, m_padding);
        rt.offsetMax = new Vector2(-m_padding, -m_padding);

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        m_imgFood.preserveAspect = true;
    }

    public void ShowPreview(Sprite sprite)
    {
        if (sprite == null) return;

        m_imgFood.sprite = sprite;
        m_imgFood.color = m_fadeColor;
        m_imgFood.gameObject.SetActive(true);
        ApplyLayout();
    }
    public void OnSetSlot(Sprite foodSprite)
    {
        if (foodSprite == null) return;

        m_imgFood.sprite = foodSprite;
        m_imgFood.color = m_nomalColor;
        m_imgFood.gameObject.SetActive(true);

        RectTransform rt = m_imgFood.rectTransform;
        rt.pivot = new Vector2(0.5f, 0.5f);

        //  Reset transform 
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.anchoredPosition = Vector2.zero;
        ApplyLayout();
    }

    public void OnFadeFood()
    {
        OnActiveFood(true);
        m_imgFood.color = m_fadeColor;
    }
     
    public void OnHideFood()
    {
        OnActiveFood(false);
        m_imgFood.color = m_nomalColor;
    }

    public void OnActiveFood(bool active)
    {
        m_imgFood.gameObject.SetActive(active);
        m_imgFood.color = m_nomalColor;
    }

    public bool HasFood()
    {
        return m_imgFood.gameObject.activeSelf && m_imgFood.color == m_nomalColor;
    }
    public Sprite GetSpriteFood()
    {
        return m_imgFood.sprite;
    } 

    public FoodSlot GetSlotNull => m_grillCtrl.GetSlotNull();

    public void OnCheckMerge()
    {
        m_grillCtrl?.OnCheckMerge();
    }

    public void OnPrepareItem(Image img)
    {
        OnSetSlot(img.sprite);
        m_imgFood.color = m_nomalColor;
        m_imgFood.transform.position = img.transform.position;

        var rt = m_imgFood.rectTransform;

        // 1. set sprite
        m_imgFood.sprite = img.sprite;
        m_imgFood.color = m_nomalColor;
        m_imgFood.gameObject.SetActive(true);

        // 2. set về chuẩn slot
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.anchoredPosition = Vector2.zero;

        // 3. giữ aspect
        m_imgFood.preserveAspect = true;

        // 4. set vị trí bắt đầu (world)
        rt.position = img.transform.position;

        // 5. animate về slot
        rt.DOKill();
        rt.DOMove(transform.position, 0.3f).SetEase(Ease.OutBack);
        m_imgFood.transform.DOScale(1, 0.3f);
        

    }
    
    public void OnCheckPrepareTray()
    {
        m_grillCtrl?.OnCheckPrepareTray();
    }

    public void DoShake()
    {
        m_imgFood.transform.DOShakePosition(0.5f, 10f, 20, 180f);
    }
    public void ClearByMagnet()
    {
        OnHideFood();            
        m_imgFood.sprite = null;
        OnCheckPrepareTray();
    }



}
