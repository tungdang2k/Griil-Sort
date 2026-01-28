using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FoodSlot : MonoBehaviour
{
    private Color m_nomalColor = new Color(1f, 1f, 1f, 1f);
    private Color m_fadeColor = new Color(1f,1f, 1f, 0.5f);
    private Image m_imgFood;
    private GrillStation m_grillCtrl;

    public Image ImgFood => m_imgFood;
    private void Awake()
    {
        m_grillCtrl = transform.parent.parent.GetComponent<GrillStation>();
        m_imgFood = this.transform.GetChild(0).GetComponent<Image>();
        m_imgFood.gameObject.SetActive(false);
    }

    public void OnSetSlot(Sprite foodSprite)
    {
        m_imgFood.sprite = foodSprite;
        m_imgFood.gameObject.SetActive(true);
        m_imgFood.SetNativeSize();
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
        m_imgFood.transform.localScale = img.transform.localScale;
        m_imgFood.transform.localEulerAngles = img.transform.localEulerAngles;

        m_imgFood.transform.DOLocalMove(Vector3.zero, 0.3f);
        m_imgFood.transform.DOScale(Vector3.one, 0.3f);
        m_imgFood.transform.DOLocalRotate(Vector3.zero, 0.3f);
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
