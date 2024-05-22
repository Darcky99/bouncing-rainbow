using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundReSize : MonoBehaviour
{
    private SpriteRenderer m_SpriteRenderer;
    private Camera m_MainCamera;

    private void OnEnable()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        m_MainCamera = Camera.main;
        updateScale();
    }
    private void OnDisable()
    {
        
    }

    public void OnCameraPositionChanged(Vector2 i_NewPosition) {
        transform.position = i_NewPosition;
    }
    public void OnCameraSizeChanged() {
        updateScale();
    }
    private void updateScale()
    {
        // Get the size of the sprite in world units
        float spriteWidth = m_SpriteRenderer.sprite.bounds.size.x;
        float spriteHeight = m_SpriteRenderer.sprite.bounds.size.y;

        // Get the size of the camera view in world units
        float cameraHeight = m_MainCamera.orthographicSize * 2.0f;
        float cameraWidth = cameraHeight * m_MainCamera.aspect;

        // Calculate the scale needed to fit the sprite to the camera view
        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        // Set the sprite's scale to the calculated values
        transform.localScale = new Vector3(scaleX, scaleY, 1.0f);
    }
}
