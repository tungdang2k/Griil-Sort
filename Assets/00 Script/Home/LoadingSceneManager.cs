using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : Singleton<LoadingSceneManager>
{
    [SerializeField] private GameObject m_loadingScene;
    [SerializeField] private Slider m_progressBar;

    public void SwichToScene(string name)
    {
        m_loadingScene.SetActive(true);

        m_progressBar.value = 0f;
        StartCoroutine(SwichToSceneAsyc(name));
    }

    private IEnumerator SwichToSceneAsyc(string name)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(name);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            m_progressBar.value = progress;
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        m_loadingScene.SetActive(false);
    }
}
