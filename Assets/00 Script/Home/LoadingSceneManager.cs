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

    public void LoadFirstScene(string name)
    {
        StartCoroutine(SwichToSceneAsyc(name)); 
    }

    
    private IEnumerator SwichToSceneAsyc(string name)
    {

        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(name);


        operation.allowSceneActivation = false;

        float displayProgress = 0f;
        float targetProgress = 0f;

        while (operation.progress < 0.9f)
        {
            targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, Time.deltaTime * 2f);
            m_progressBar.value = displayProgress;
            yield return null;
        }


        m_progressBar.value = 1f;
        yield return new WaitForSeconds(0.5f); 

        operation.allowSceneActivation = true; // chuyển scene

        while (!operation.isDone)
            yield return null;

        m_loadingScene.SetActive(false);
    }
}
