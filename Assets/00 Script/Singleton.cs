using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {

                _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);

                if (_instance == null)
                {
                    Debug.LogError($"Không tìm thấy {typeof(T)} trong Scene!");
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
}