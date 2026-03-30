using UnityEngine;

public class BootLoader : MonoBehaviour
{

    private string m_firstScene = CONSTANTS.HOMESCENE;

    void Start()
    {
        LoadingSceneManager.Instance.LoadFirstScene(m_firstScene);
    }
   
}
