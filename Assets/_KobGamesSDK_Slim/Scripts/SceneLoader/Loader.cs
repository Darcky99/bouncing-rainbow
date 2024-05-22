using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    public string SceneName;
    public Slider Slider;
    public float DelayToSwitchScene = 2;

    private bool m_IsInitStop = false;
    private bool m_IsSwitchAllowed = false;

    private void Awake()
    {
        StartCoroutine(LoadScene());
        StartCoroutine(Progress());
    }

    IEnumerator LoadScene()
    {
        yield return null;

        //Begin to load the Scene you specify
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(SceneName);

        //Don't let the Scene activate until you allow it to
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            // Check if the load has finished
            if (asyncOperation.progress >= 0.9f)
            {
                if (!m_IsInitStop)
                {
                    m_IsInitStop = true;
                    StartCoroutine(nameof(StopProgress));
                }                

                if (m_IsSwitchAllowed)
                {
                    asyncOperation.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }

    IEnumerator Progress()
    {
        while (Slider.value < 0.9f)
        {
            Slider.value += 0.01f;

            yield return null;
        }

        yield return null;
    }

    IEnumerator StopProgress()
    {
        StopCoroutine(Progress());

        yield return new WaitForSeconds(DelayToSwitchScene);

        Slider.value = 1;

        m_IsSwitchAllowed = true;
    }
}
