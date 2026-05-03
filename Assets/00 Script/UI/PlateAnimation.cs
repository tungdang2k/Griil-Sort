using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlateAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform m_plateRoot;
    [SerializeField] private RectTransform m_platePrefab;
    [SerializeField] private List<Image> m_imgDummyList;

    [SerializeField] private float m_plateEnterTime = 0.4f;
    [SerializeField] private float m_plateExitTime = 0.4f;
    [SerializeField] private float m_itemFlyTime = 0.3f;
    private GameObject m_inputBlocker;
    public void PlayPlateAnimation(List<Image> foods, System.Action onComplete)
    {
       
        RectTransform canvasRect = m_plateRoot.GetComponentInParent<Canvas>()
                                              .GetComponent<RectTransform>();
        float canvasW = canvasRect.rect.width;
        float canvasH = canvasRect.rect.height;

        Vector2 targetLocal = new Vector2(canvasW / 2f - 200f, canvasH / 2f - 200f);
        Vector2 offScreenLocal = new Vector2(canvasW / 2f + 300f, canvasH / 2f - 200f);

        // 1. Spawn Plate
        RectTransform plate = Instantiate(m_platePrefab, m_plateRoot);
        plate.anchoredPosition = offScreenLocal;
        plate.localScale = Vector3.one * 0.8f;

        Sequence seq = DOTween.Sequence();

        // 2. Plate bay vào
        seq.Append(plate.DOAnchorPos(targetLocal, m_plateEnterTime).SetEase(Ease.OutBack));
        seq.Join(plate.DOScale(1f, m_plateEnterTime));
        seq.AppendInterval(0.2f);

        // 3. Items bay vào đĩa
        Vector2[] offsets = new Vector2[]
        {
        new Vector2(  0,  20),
        new Vector2(-40, -20),
        new Vector2( 40, -20)
        };

        List<RectTransform> activeDummies = new List<RectTransform>();
        List<FoodSlot> lockedSlots = new List<FoodSlot>();
        foreach (var food in foods)
        {
            FoodSlot slot = food.GetComponentInParent<FoodSlot>();
            if (slot != null)
            {
                slot.Lock();
                lockedSlots.Add(slot);
            }
        }

        for (int i = 0; i < foods.Count; i++)
        {
            Image imgFood = foods[i];
            Image dummy = GetFreeDummy();
            if (dummy == null) continue;

            dummy.sprite = imgFood.sprite;
            dummy.transform.position = imgFood.transform.position;

            dummy.gameObject.SetActive(true);
            imgFood.gameObject.SetActive(false);



            RectTransform foodRect = imgFood.rectTransform;
            RectTransform dummyRect = dummy.rectTransform;
            // ✅ Lấy kích thước từ sprite thực tế
            Vector3[] corners = new Vector3[4];
            foodRect.GetWorldCorners(corners);

            float worldW = corners[2].x - corners[0].x;
            float worldH = corners[1].y - corners[0].y;

            // convert về local size của dummy
            Vector3 scale = dummyRect.lossyScale;

            dummyRect.localScale = Vector3.one;

            activeDummies.Add(dummyRect);

            float delay = i * 0.08f;

            seq.Insert(delay + m_plateEnterTime,
                DOTween.To(
                    () => dummyRect.anchoredPosition,
                    x => dummyRect.anchoredPosition = x,
                    targetLocal + offsets[i],
                    m_itemFlyTime
                ).SetEase(Ease.InBack));

            seq.Insert(delay + m_plateEnterTime,
                dummyRect.DOScale(0.7f, m_itemFlyTime));

            seq.Insert(delay + m_plateEnterTime,
                dummyRect.DORotate(Vector3.zero, m_itemFlyTime));
        }

        // 4. Impact
        seq.AppendInterval(0.1f);
        seq.Append(plate.DOScale(0.85f, 0.1f));
        seq.Append(plate.DOScale(1f, 0.15f).SetEase(Ease.OutBack));

        // 5. Plate + dummy bay ra
        seq.Append(plate.DOAnchorPos(offScreenLocal, m_plateExitTime).SetEase(Ease.InBack));
        seq.Join(plate.DOScale(1f, m_plateExitTime));

        foreach (RectTransform dummyRect in activeDummies)
        {
            seq.Join(
                DOTween.To(
                    () => dummyRect.anchoredPosition,
                    x => dummyRect.anchoredPosition = x,
                    offScreenLocal,
                    m_plateExitTime
                ).SetEase(Ease.InBack));
        }

        // 6. Cleanup + callback
        seq.OnComplete(() =>
        {
            foreach (var slot in lockedSlots)
            {
                slot.Unlock();
            }
            foreach (var d in m_imgDummyList)
            {
                d.gameObject.SetActive(false);
                d.transform.localScale = Vector3.one;
                d.transform.rotation = Quaternion.identity;
            }


            Destroy(plate.gameObject);
            onComplete?.Invoke();
        });
    }

   

    private Image GetFreeDummy()
    {
        return m_imgDummyList.FirstOrDefault(x => !x.gameObject.activeSelf);
    }



}
