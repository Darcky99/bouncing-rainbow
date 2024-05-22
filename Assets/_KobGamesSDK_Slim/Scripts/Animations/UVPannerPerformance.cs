using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class UVPannerPerformance : MonoBehaviour
    {
        public Material Material;
        public string TextureName;

        public float AnimationSpeed;
        public Vector2 AnimationDir;

        private Vector2 m_TextureOffset;

        private void Update()
        {
            Material.SetTextureOffset(TextureName, m_TextureOffset);

            m_TextureOffset += AnimationDir * AnimationSpeed * Time.deltaTime;
        }
    }
}
