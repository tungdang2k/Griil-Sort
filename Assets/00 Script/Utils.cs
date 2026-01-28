using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public static class Utils
{
    // lấy tất cả component T trong các con của Transform parent
    public static List<T> GetListInChild<T>(Transform parent) where T : Component
    {
        List<T> list = new List<T>();
        for (int i = 0; i < parent.childCount; i++)
        {
            var item = parent.GetChild(i).GetComponent<T>();
            if (item != null)
            {
                list.Add(item);
            }
        }
        return list;
    }

    public static List<T> TakeAndRemoveRandom<T>(List<T> source, int num)
    {
        List<T> result = new List<T>();
        num = Mathf.Min(num, source.Count); // đảm bảo không vượt quá số phần tử có trong source

        for (int i = 0; i < num; i++)
        {
            int randomIndex = Random.Range(0, source.Count);
            result.Add(source[randomIndex]);
            source.RemoveAt(randomIndex);
        }

        return result;
    }

    public static T GetRayCastUI<T>(Vector2 position) where T : MonoBehaviour
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = position;
        List<RaycastResult> list = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, list);

        if(list.Count > 0)
        {
            foreach (var item in list)
            {
                T component = item.gameObject.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
        }

        return null;


    }

}
