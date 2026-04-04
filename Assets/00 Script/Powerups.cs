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
    [SerializeField] private PowerupShopPopup m_storagePopup;
    [SerializeField] private TextMeshProUGUI m_magnetCountTxt;
    [SerializeField] private TextMeshProUGUI m_shuffleCountTxt;
    [SerializeField] private TextMeshProUGUI m_timeCountTxt;
    [SerializeField] private Image m_magnetPlusIcon;
    [SerializeField] private Image m_shufflePlusIcon;
    [SerializeField] private Image m_timePlusIcon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(m_storagePopup == null)
        {   
            m_storagePopup = FindAnyObjectByType<PowerupShopPopup>();
        }

        UpdatePowerUpUI();
    }

    void UpdatePowerUpUI()
    {
        UpdateSingle(CONSTANTS.MAGNET, m_magnetCountTxt, m_magnetPlusIcon);
        UpdateSingle(CONSTANTS.SHUFFLE, m_shuffleCountTxt, m_shufflePlusIcon);
        UpdateSingle(CONSTANTS.ADDTIME, m_timeCountTxt, m_timePlusIcon);
    }

    void UpdateSingle(string id, TextMeshProUGUI txt, Image plusIcon)
    {
        int uses = PowerUpUsesManager.GetUses(id);

        txt.text = uses.ToString();

        if (uses > 0)
        {
            txt.text = uses.ToString();
            txt.transform.parent.gameObject.SetActive(true);
            plusIcon.gameObject.SetActive(false);
        }
        else
        {
            txt.transform.parent.gameObject.SetActive(false);
            plusIcon.gameObject.SetActive(true);
        }
    }
    public void OnClickMagnet()
    {
        HandlePowerUp(CONSTANTS.MAGNET, OnMagnet);
    }

    public void OnClickShuffle()
    {
        HandlePowerUp(CONSTANTS.SHUFFLE, OnShuffle);
    }

    public void OnClickAddTime()
    {
        HandlePowerUp(CONSTANTS.ADDTIME, OnAddTime);
    }

    void HandlePowerUp(string id, System.Action action)
    {
        int uses = PowerUpUsesManager.GetUses(id);

        if (uses <= 0)
        {
            m_storagePopup.Show(id, UpdatePowerUpUI);
            return;
        }
        if (id == CONSTANTS.MAGNET && !CanUseMagnet())
        {
            // TODO: hiện thông báo "Không có nhóm nào đủ 3 để hút"
            Debug.Log("Magnet: không có nhóm nào >= 3");
            return;
        }
        PowerUpUsesManager.AddUses(id, -1);
        UpdatePowerUpUI();
        action?.Invoke();
    }
    private bool CanUseMagnet()
    {
        var allFood = GetAllActiveFood();
        var groups = new Dictionary<string, int>();
        foreach (var (img, slot, tray) in allFood)
        {
            string name = img.sprite.name;
            if (!groups.ContainsKey(name)) groups[name] = 0;
            groups[name]++;
        }
        return groups.Any(kvp => kvp.Value >= 3);
    }

    private void OnMagnet()
    {

        AudioManager.Instance.PlaySFX(SFXType.Merge);

        var allFood = GetAllActiveFood();
        var groups = new Dictionary<string, List<Image>>();

        foreach (var (img, slot, tray) in allFood)
        {
            string name = img.sprite.name;
            if (!groups.ContainsKey(name))
                groups[name] = new List<Image>();
            groups[name].Add(img);
        }

        foreach (var kvp in groups)
        {
            if (kvp.Value.Count >= 3)
            {
                MagnetGroup(kvp.Value);
                break;
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
            Image imgFood = foods[i];
            Image imgDummy = m_imgDummyList[i];
            Image capturedDummy = imgDummy;

            imgDummy.sprite = imgFood.sprite;
            imgDummy.SetNativeSize();
            imgDummy.transform.position = imgFood.transform.position;
            imgDummy.transform.rotation = Quaternion.identity;
            imgDummy.color = Color.white;
            imgDummy.gameObject.SetActive(true);
            imgFood.gameObject.SetActive(false); // ẩn tạm để tween
            imgDummy.transform.DOKill();

            Sequence seq = DOTween.Sequence();
            seq.Join(capturedDummy.transform
                .DOMove(m_magnetTarget.position, duration)
                .SetEase(Ease.InBack));
            seq.Join(capturedDummy.transform.DORotate(
                new Vector3(0, 0, Random.Range(-180, 180)),
                duration, RotateMode.FastBeyond360));
            seq.OnComplete(() =>
            {
                capturedDummy.gameObject.SetActive(false);
                capturedDummy.transform.rotation = Quaternion.identity;
            });
            
        }

        DOVirtual.DelayedCall(duration, () =>
        {
            foreach (var img in foods)
            {
                // Ưu tiên xử lý FoodSlot trước
                FoodSlot slot = img.GetComponentInParent<FoodSlot>();
                if (slot != null)
                {
                    // ✅ Xóa food nhưng KHÔNG kéo tray lên ngay
                    slot.OnHideFood();
                    slot.ImgFood.sprite = null;
                    // Không gọi ClearByMagnet vì nó trigger OnCheckPrepareTray
                    // gây ra food mới xuất hiện và bị mất đếm
                    continue;
                }

                // Nếu food nằm trong Tray
                TrayItem tray = img.GetComponentInParent<TrayItem>();
                if (tray != null)
                {
                    img.gameObject.SetActive(false);
                    img.sprite = null;
                    tray.OnFoodRemoved();
                }
            }

            // ✅ Sau khi xóa hết 3 item mới check tray — tránh race condition
            foreach (var img in foods)
            {
                FoodSlot slot = img.GetComponentInParent<FoodSlot>();
                slot?.OnCheckPrepareTray();
            }

            GameManager.Instance.OnMinusFood();
        });
    }


    private List<(Image img, FoodSlot slot, TrayItem tray)> GetAllActiveFood()
    {
        var result = new List<(Image, FoodSlot, TrayItem)>();

        foreach (var grill in GameManager.Instance.ListGrill)
        {
            if (!grill.gameObject.activeInHierarchy) continue;

            foreach (var slot in grill.totalSlot)
            {
                if (slot.HasFood())
                    result.Add((slot.ImgFood, slot, null));
            }

            // ✅ m_stackTray thay vì m_totalTrays
            foreach (var tray in grill.totalTrays) // totalTrays là Stack
            {
                if (!tray.gameObject.activeInHierarchy) continue;
                foreach (var img in tray.FoodList)
                {
                    if (img.gameObject.activeInHierarchy)
                        result.Add((img, null, tray));
                }
            }
        }
        return result;
    }

   
    private void OnShuffle()
    {
        AudioManager.Instance.PlaySFX(SFXType.Shuffle);
        StartCoroutine(IEShuffle());

    }

    private IEnumerator IEShuffle()
    {
        List<Image> result = new List<Image>();
        foreach (var grill in GameManager.Instance.ListGrill)
        {
            if (grill.gameObject.activeInHierarchy)
                result.AddRange(grill.ListFoodActive());
        }

        if (result.Count <= 1)
            yield break;

        yield return new WaitForSeconds(0.1f);
        float jumpTime = 0.25f;
        float fallTime = 0.35f;

        foreach (var img in result)
            img.transform.DOKill();

        // 1️⃣ Jump lên đồng loạt (giữ nguyên)
        foreach (var img in result)
        {
            img.transform
                .DOScale(1.15f, jumpTime)
                .SetEase(Ease.OutQuad);
        }

        yield return new WaitForSeconds(jumpTime * 0.6f);

        // 2️⃣ Smart Shuffle sprite
        SmartShuffle(result);

        foreach (var img in result)
            img.transform.DOKill();

        // 3️⃣ Rơi xuống đồng loạt (giữ nguyên)
        foreach (var img in result)
        {
            img.transform
                .DOScale(1f, fallTime)
                .SetEase(Ease.OutBack);
        }
    }

    private void SmartShuffle(List<Image> result)
    {
        // B1: Lấy danh sách sprite hiện tại
        List<Sprite> sprites = result.Select(img => img.sprite).ToList();

        // B2: Đếm số lượng từng loại
        Dictionary<string, List<Sprite>> groups = new Dictionary<string, List<Sprite>>();
        foreach (var sp in sprites)
        {
            if (!groups.ContainsKey(sp.name))
                groups[sp.name] = new List<Sprite>();
            groups[sp.name].Add(sp);
        }

        // B3: Tách group có thể merge (>= 3) và group lẻ
        List<Sprite> mergeableSprites = new List<Sprite>();
        List<Sprite> remainingSprites = new List<Sprite>();

        foreach (var kvp in groups)
        {
            int fullGroups = kvp.Value.Count / 3;
            int leftover = kvp.Value.Count % 3;

            for (int i = 0; i < fullGroups * 3; i++)
                mergeableSprites.Add(kvp.Value[i]);

            for (int i = fullGroups * 3; i < kvp.Value.Count; i++)
                remainingSprites.Add(kvp.Value[i]);
        }

        // B4: Shuffle nội bộ từng nhóm
        mergeableSprites = mergeableSprites.OrderBy(_ => Random.value).ToList();
        remainingSprites = remainingSprites.OrderBy(_ => Random.value).ToList();

        // B5: Ghép lại — mergeableSprites đứng đầu, lẻ đứng sau
        // → đảm bảo các slot đầu tiên (visible) chứa food có thể merge
        List<Sprite> finalOrder = new List<Sprite>();
        finalOrder.AddRange(mergeableSprites);
        finalOrder.AddRange(remainingSprites);

        // B6: Gán lại sprite
        for (int i = 0; i < result.Count; i++)
        {
            result[i].sprite = finalOrder[i];
            result[i].SetNativeSize();
        }
    }

    private void OnAddMoreGrill()
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
