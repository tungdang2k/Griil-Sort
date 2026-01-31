using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class DropDragCtrl : MonoBehaviour
{
    [SerializeField] private Image m_imgFoodDrag;
    [SerializeField] private float m_timeCheckSuggest;

    private FoodSlot m_currentfoodSlot, m_cacheFood;
    private bool m_hasDrag;
    private Vector3 m_offset;
    private float m_countTime;

    private float _timeAtClick;
    void Start()
    {
        m_imgFoodDrag.gameObject.SetActive(false);
    }
   
    private void Update()
    {

       
        m_countTime += Time.deltaTime;

        if (m_countTime >= m_timeCheckSuggest)
        {
            m_countTime = 0f;   
                GameManager.Instance?.OnCheckAndShake();
        }



        if (Input.GetMouseButtonDown(0)) // check khi click chuot
        {
            FoodSlot tapSlot = Utils.GetRayCastUI<FoodSlot>(Input.mousePosition); // check o vi tri click chuot xem co Ui gan class FoodSlot
            if (tapSlot != null)
            {
                if (tapSlot.HasFood())
                {
                    m_hasDrag = true;
                    m_currentfoodSlot?.OnActiveFood(true);
                    m_cacheFood = m_currentfoodSlot = tapSlot;
                    // Gan sprite food cho dummy image
                    m_imgFoodDrag.gameObject.SetActive(true);
                    m_imgFoodDrag.sprite = m_currentfoodSlot.GetSpriteFood();
                    m_imgFoodDrag.SetNativeSize();
                    m_imgFoodDrag.transform.position = m_currentfoodSlot.transform.position; // gan vi tri               

                    
                    m_imgFoodDrag.transform.position = Input.mousePosition;
                    m_offset = Vector2.zero;
                    m_currentfoodSlot.OnActiveFood(false);
                    m_imgFoodDrag.transform.DOScale(Vector3.one * 1.2f, 0.2f);
                }
                else
                {
                    if (m_currentfoodSlot != null) // di chuyen item vua slect toi vi tri nay
                    {
                        m_imgFoodDrag.transform.DOKill();
                        m_imgFoodDrag.transform.DOMove(tapSlot.transform.position, 0.4f).OnComplete(() => {
                            tapSlot.OnSetSlot(m_currentfoodSlot.GetSpriteFood());
                            tapSlot.OnActiveFood(true);
                            tapSlot.OnCheckMerge();
                            m_currentfoodSlot?.OnCheckPrepareTray();
                            m_currentfoodSlot = null;
                            m_imgFoodDrag.gameObject.SetActive(false);
                        });
                        m_imgFoodDrag.transform.DOScale(Vector3.one, 0.4f);
                    }
                }
            }
            else
            {
                m_currentfoodSlot?.OnActiveFood(true);
                m_currentfoodSlot = null;
                m_imgFoodDrag.gameObject.SetActive(false);
            }

            
        }
        if (m_hasDrag)
        {
            if (!m_imgFoodDrag || !m_imgFoodDrag.canvas) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
             m_imgFoodDrag.canvas.transform as RectTransform,
                Input.mousePosition,
                null,
                out Vector2 localPos
            );

            m_imgFoodDrag.rectTransform.localPosition = localPos + (Vector2)m_offset;

            m_countTime = 0f;

            FoodSlot slot = Utils.GetRayCastUI<FoodSlot>(Input.mousePosition);
            if (slot != null)
            {
                if (!slot.HasFood()) // vi tri item chua co food
                {
                    if (m_cacheFood == null || m_cacheFood.GetInstanceID() != slot.GetInstanceID())
                    {
                        m_cacheFood?.OnHideFood();
                        m_cacheFood = slot;
                        m_cacheFood.OnFadeFood();
                        m_cacheFood.OnSetSlot(m_currentfoodSlot.GetSpriteFood());
                    }
                }
                else // vi tri tro chuot da co item
                {
                    FoodSlot slotAvalable = slot.GetSlotNull;
                    if (slotAvalable != null)
                    {
                        m_cacheFood?.OnHideFood();
                        m_cacheFood = slotAvalable;
                        m_cacheFood.OnFadeFood();
                        m_cacheFood.OnSetSlot(m_currentfoodSlot.GetSpriteFood());
                    }
                    else
                    {
                        this.OnClearCacheFood();
                    }
                }
            }
            else
            {
                if (m_cacheFood != null)
                {
                    m_cacheFood.OnHideFood();
                    m_cacheFood = null;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && m_hasDrag)
        {
            if (Time.time - _timeAtClick < 0.15f) // Controll by click
            {

            }
            else
            {
                if (m_cacheFood != null) // xu ly fill item
                {
                    m_imgFoodDrag.transform.DOMove(m_cacheFood.transform.position, 0.2f).OnComplete(() =>
                    {
                        m_imgFoodDrag.gameObject.SetActive(false);
                        m_cacheFood.OnSetSlot(m_currentfoodSlot.GetSpriteFood());
                        m_cacheFood.OnActiveFood(true);
                        m_cacheFood.OnCheckMerge();
                        m_currentfoodSlot?.OnCheckPrepareTray();
                        m_cacheFood = null;
                        m_currentfoodSlot = null;
                    });
                    m_imgFoodDrag.transform.DOScale(Vector3.one, 0.22f);
                }
                else // xu ly tro ve vi tri ban dau
                {
                    m_imgFoodDrag.transform.DOMove(m_currentfoodSlot.transform.position, 0.3f).OnComplete(() =>
                    {
                        m_imgFoodDrag.gameObject.SetActive(false);
                        m_currentfoodSlot.OnActiveFood(true);
                    });
                    m_imgFoodDrag.transform.DOScale(Vector3.one, 0.3f);
                }
            }
            m_hasDrag = false;

        }
        

    }
   
    private void OnClearCacheFood()
    {
        if (m_cacheFood != null && m_cacheFood.GetInstanceID() != m_currentfoodSlot.GetInstanceID())
        {
            m_cacheFood.OnHideFood();
            m_cacheFood = null;
        }
    }


}
