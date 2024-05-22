using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PcBuild : MonoBehaviour
{
#if UNITY_STANDALONE
    //public List<Canvas> CanvasesToExclude;
    //private Canvas m_CurrentCanvas;
    public List<GameObject> GameObjectUI;
    public List<string> GameObjectUINames;

    public List<string> GameObjectUIDebugNames;

    private void Start()
    {
        //m_CurrentCanvas = this.GetComponent<Canvas>();

        SetResolution1080x1920();
    }

    public void TogglePause()
    {
        Time.timeScale = Time.timeScale == 1 ? 0 : 1;
    }

    public void ToggleMusic()
    {
        //MKAudioManager.Instance.IsMusicMute = !MKAudioManager.Instance.IsMusicMute;
    }

    public void ToggleSFX()
    {
        //MKAudioManager.Instance.IsSoundMute = !MKAudioManager.Instance.IsSoundMute;
    }

    public void SetResolution2048x2732()
    {
        Screen.SetResolution(2048, 2732, false);
        //Display.main.SetRenderingResolution(2048, 2732);
    }

    public void SetResolution1242x2208()
    {
        Screen.SetResolution(1242, 2208, false);
        //Display.main.SetRenderingResolution(1242, 2208);
    }

    public void SetResolution1242xx2688()
    {
        Screen.SetResolution(1242, 2688, false);
    }

    public void SetResolution1080x1920()
    {
        Screen.SetResolution(1080, 1920, false);
        //Display.main.SetRenderingResolution(1080, 1920);
    }

    public void SetResolution1200x1600()
    {
        Screen.SetResolution(1200, 1600, false);
        //Display.main.SetRenderingResolution(1200, 1600);
    }

    public void SetResolution886x1920()
    {
        Screen.SetResolution(886, 1920, false);
        //Display.main.SetRenderingResolution(1200, 1600);
    }


    //[ShowInInspector] private Canvas[] m_Canvases;
    //private bool m_IsCanvasesVisible = true;
    //public void ToggleCanvases()
    //{
    //    if (m_IsCanvasesVisible)
    //    {
    //        m_Canvases = GameObject.FindObjectsOfType<Canvas>();
    //    }

    //    foreach (var canvas in m_Canvases)
    //    {
    //        if (canvas != m_CurrentCanvas && !CanvasesToExclude.Contains(canvas))
    //        {
    //            canvas.gameObject.SetActive(!canvas.gameObject.activeSelf);
    //        }
    //    }

    //    m_IsCanvasesVisible = !m_IsCanvasesVisible;
    //}

    [Button]
    public void ToggleUIDebug()
    {
        foreach (var go in GameObjectUIDebugNames)
        {
            if (go != null)
            {
                GameObject _go = null;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    foreach (var rootGO in SceneManager.GetSceneAt(i).GetRootGameObjects())
                    {
                        //Debug.Log(z.gameObject.scene.name);
                        _go = rootGO.FindObject<GameObject>(go, true);

                        if (_go != null)
                            break;
                    }
                    if (_go != null)
                        break;
                }

                if (_go != null)
                {
                    _go.SetActive(!_go.activeSelf);
                }
            }
        }
    }

    [Button]
    public void ToggleUI()
    {
        foreach (var go in GameObjectUI)
        {
            if (go != null)
            {
                go.SetActive(!go.activeSelf);
            }
        }

        foreach (var go in GameObjectUINames)
        {
            if (go != null)
            {
                GameObject _go = null;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    foreach (var rootGO in SceneManager.GetSceneAt(i).GetRootGameObjects())
                    {
                        //Debug.Log(z.gameObject.scene.name);
                        _go = rootGO.FindObject<GameObject>(go, true);

                        if (_go != null)
                            break;
                    }
                    if (_go != null)
                        break;
                }
                
                if (_go != null)
                {
                    _go.SetActive(!_go.activeSelf);
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleUI();
        }
    }
#endif
}
