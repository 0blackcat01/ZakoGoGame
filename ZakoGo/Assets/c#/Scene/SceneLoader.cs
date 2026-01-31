using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    private static SceneLoader instance;
    private bool animationComplete;

    private AsyncOperation loadingOperation;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }


        DontDestroyOnLoad(this.gameObject);
        instance = this;
    }

    public static void LoadScene(string sceneName)
    {
        string sceneName0 = SceneManager.GetActiveScene().name;
        if (sceneName0 != "Main")
        {
            GameObject.FindGameObjectWithTag("UIcontrol").GetComponent<CanvasGroup>().interactable = false;
            GameObject.FindGameObjectWithTag("UIcontrol").GetComponent<CanvasGroup>().alpha = 0;
        }
        
        instance.StartCoroutine(instance.LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 加载loading场景
        yield return SceneManager.LoadSceneAsync("Loading");

        // 模拟动画播放
        yield return new WaitForSeconds(0.5f);

        // 设置动画播放完成的标志
        animationComplete = true;

        // 加载目标场景
        loadingOperation = SceneManager.LoadSceneAsync(sceneName);
        loadingOperation.allowSceneActivation = false;

        // 等待目标场景加载完成
        while (!loadingOperation.isDone)
        {
            // 如果动画播放完成，则激活目标场景
            if (animationComplete && loadingOperation.progress >= 0.9f)
            {
                loadingOperation.allowSceneActivation = true;
                
            }

            yield return null;
        }
        GameObject.FindGameObjectWithTag("UIcontrol").GetComponent<CanvasGroup>().interactable = true;
        GameObject.FindGameObjectWithTag("UIcontrol").GetComponent<CanvasGroup>().alpha = 1f;
    }
}

