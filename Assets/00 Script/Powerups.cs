using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Powerups : MonoBehaviour
{
    [SerializeField] private CountDowntimer m_timer;
    [SerializeField] private List<Image> m_imgDummyList = new List<Image>();
    [SerializeField] private  Transform m_magnetTarget;
    [SerializeField] private TextMeshProUGUI m_txtAddTimePrefab;
    [SerializeField] private Transform m_popupRoot;
    [SerializeField] private float m_TimeBonus = 45;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void OnMagnet()
    {
        AudioManager.Instance.PlaySFX(SFXType.Merge);
        Dictionary<string, List<Image>> groups = new Dictionary<string, List<Image>>();

        foreach (var grill in GameManager.Instance.ListGrill)
        {
            if (grill.gameObject.activeInHierarchy)
            {
                foreach (var slot in grill.totalSlot)
                {
                    if (slot.HasFood())
                    {
                        string name = slot.GetSpriteFood().name;
                        if (!groups.ContainsKey(name))
                        {
                            groups.Add(name, new List<Image>());
                        }
                        groups[name].Add(slot.ImgFood);

                    }
                }


                TrayItem tray = grill.GetFistTray();

                if (tray != null)
                {
                    foreach (var img in tray.FoodList)
                    {
                        if (img.gameObject.activeInHierarchy)
                        {
                            string name = img.sprite.name;
                            if (!groups.ContainsKey(name))
                            {
                                groups.Add(name, new List<Image>());
                            }
                            groups[name].Add(img);
                        }
                    }
                }


            }



        }

        foreach (var kvp in groups)
        {
            if (kvp.Value.Count >= 3)
            {
                MagnetGroup(kvp.Value);
                break; // chỉ hút 1 group
            }
        }

    }

    private void MagnetGroup(List<Image> items)
    {
        if (items == null || items.Count < 3) return;
        if (m_imgDummyList.Count < 3) return;
        float duration = 0.35f;
        List<Image> foods = items.Take(3).ToList();
        for (int i = 0; i < foods.Count; i++)
        {
            Image imgFood = items[i];
            Image imgDummy = m_imgDummyList[i]; // pool sẵn 3 dummy


            // setup dummy
            imgDummy.sprite = imgFood.sprite;
            imgDummy.SetNativeSize();
            imgDummy.transform.position = imgFood.transform.position;
            imgDummy.transform.rotation = Quaternion.identity;
            imgDummy.color = Color.white;
            imgDummy.gameObject.SetActive(true);
            imgFood.gameObject.SetActive(false);

            imgDummy.transform.DOKill();

            Sequence seq = DOTween.Sequence();

            seq.Join(
                imgDummy.transform.DOMove(m_magnetTarget.position, duration)
                    .SetEase(Ease.InBack)
            );

            seq.Join(
                imgDummy.transform.DORotate(
                    new Vector3(0, 0, UnityEngine.Random.Range(-180, 180)),
                    duration,
                    RotateMode.FastBeyond360
                )
            );

            seq.OnComplete(() =>
            {

                imgDummy.gameObject.SetActive(false);
                imgDummy.transform.rotation = Quaternion.identity;
                TrayItem tray = imgFood.GetComponentInParent<TrayItem>();
                if (tray != null)
                {
                    tray.OnFoodRemoved();
                }
            });
        }

        // xử lý logic sau khi animation xong
        DOVirtual.DelayedCall(duration, () =>
        {
            List<Image> foods = items.Take(3).ToList();
            foreach (var img in foods)
            {
                FoodSlot slot = img.GetComponentInParent<FoodSlot>();
                if (slot != null)
                {
                    slot.ClearByMagnet();
                }
            }
            GameManager.Instance.OnMinusFood();

        });
    }

    public void OnShuffle()
    {
        AudioManager.Instance.PlaySFX(SFXType.Merge);
        StartCoroutine(IEShuffle());

    }

    private IEnumerator IEShuffle()
    {
        List<Image> result = new List<Image>();

        foreach (var grill in GameManager.Instance.ListGrill)
        {
            if (grill.gameObject.activeInHierarchy)
            {
                result.AddRange(grill.ListFoodActive());

            }
        }

        if (result.Count <= 1)
            yield break;

        yield return new WaitForSeconds(0.1f);

        float jumpTime = 0.25f;
        float fallTime = 0.35f;

        // 1️⃣ Jump lên đồng loạt
        foreach (var img in result)
        {
            img.transform
                .DOScale(1.15f, jumpTime)
                .SetEase(Ease.OutQuad);
        }

        yield return new WaitForSeconds(jumpTime * 0.6f);

        // 2️⃣ Shuffle sprite (instant)
        for (int i = 0; i < result.Count; i++)
        {
            int n = Random.Range(i, result.Count);

            Sprite temp = result[i].sprite;
            result[i].sprite = result[n].sprite;
            result[n].sprite = temp;

            result[i].SetNativeSize();
            result[n].SetNativeSize();
        }

        // 3️⃣ Rơi xuống đồng loạt
        foreach (var img in result)
        {
            img.transform
                .DOScale(1f, fallTime)
                .SetEase(Ease.OutBack);
        }


    }

    public void OnAddMoreGrill()
    {
        AudioManager.Instance.PlaySFX(SFXType.Drag);
        foreach (var grill in GameManager.Instance.ListGrill)
        {
            if (!grill.gameObject.activeInHierarchy)
            {
                grill.gameObject.SetActive(true);

                Transform t = grill.transform;
                t.localScale = Vector3.zero;

                t.DOScale(1f, 0.35f)
                 .SetEase(Ease.OutBack);
                break;
            }
        }
    }

    public void OnAddTime()
    {
        m_timer.AddTime(m_TimeBonus);
        ShowAddTimePopup(m_TimeBonus);
        AudioManager.Instance.PlaySFX(SFXType.TimeBonus);
    }

    private void ShowAddTimePopup(float seconds)
    {
        TextMeshProUGUI txt = Instantiate(m_txtAddTimePrefab, m_popupRoot);
        txt.text = $"+{seconds}s";

        RectTransform rt = txt.rectTransform;
        rt.localScale = Vector3.zero;

        rt.DOScale(1f, 0.2f).SetEase(Ease.OutBack)
          .OnComplete(() =>
          {
              rt.DOScale(0f, 0.15f).SetDelay(0.4f)
                .OnComplete(() => Destroy(txt.gameObject));
          });
    }


}
