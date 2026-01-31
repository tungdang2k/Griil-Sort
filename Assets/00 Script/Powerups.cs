using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Powerups : MonoBehaviour
{
    [SerializeField] private CountDowntimer m_timer;
    [SerializeField] private List<Image> m_imgDummyList = new List<Image>();
    [SerializeField] private  Transform m_magnetTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void OnMagnet()
    {
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

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < result.Count; i++)
        {
            int n = UnityEngine.Random.Range(0, result.Count);
            Sprite temp = result[i].sprite;
            result[i].sprite = result[n].sprite;
            result[n].sprite = temp;
            result[i].SetNativeSize();
            result[n].SetNativeSize();
        }

    }

    public void OnAddMoreGrill()
    {
        foreach (var grill in GameManager.Instance.ListGrill)
        {
            if (!grill.gameObject.activeInHierarchy)
            {
                grill.gameObject.SetActive(true);
                //fxNewGrill.transform.SetParent(grill.transform);
                //fxNewGrill.transform.localPosition = Vector3.zero;

                //fxNewGrill.transform.localScale = Vector3.zero;

                //fxNewGrill.Play();

                //fxNewGrill.transform
                //    .DOScale(1f, 0.3f)
                //    .SetEase(Ease.OutBack);
                break;
            }
        }
    }

    public void OnAddTime()
    {
        m_timer.AddTime(15f);
    }

}
