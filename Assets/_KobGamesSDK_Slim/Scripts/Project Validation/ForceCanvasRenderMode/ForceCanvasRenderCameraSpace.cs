using UnityEngine;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim.ProjectValidator
{
    [ExecutionOrder(eExecutionOrderBase.ForceProjectSettings)]
    public class ForceCanvasRenderCameraSpace : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        private Canvas m_Canvas;

#if UNITY_EDITOR
        private void SetRefs()
        {
            m_Canvas = GetComponent<Canvas>();
            m_Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }
        
        private void OnValidate()
        {
            SetRefs();
        }
#endif
        private void Awake()
        {
            m_Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            if(CameraManager.Instance.UICamera == null)
                Debug.LogError("Something is wrong. Couldn't find UI Camera");
            
            m_Canvas.worldCamera = CameraManager.Instance.UICamera;
        }
    }
}
