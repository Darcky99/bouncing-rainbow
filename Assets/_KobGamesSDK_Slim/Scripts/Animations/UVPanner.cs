using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class UVPanner : MonoBehaviour
    {
        public float AnimationSpeed;
        public Vector2 UVPanDirection;

        public Vector2 UVScale = new Vector2(1, 1);
        public MeshFilter MeshFilter;
        private Mesh m_Mesh;
        private Vector2[] m_UVs;

        // Use this for initialization
        void Start()
        {
            MeshFilter.sharedMesh = (Mesh)Instantiate(MeshFilter.sharedMesh);
            m_Mesh = MeshFilter.sharedMesh;
            m_UVs = m_Mesh.uv;
            for (int i = 0; i < m_UVs.Length; i++)
            {
                m_UVs[i] *= UVScale;
            }
            m_Mesh.uv = m_UVs;
        }

        private void Update()
        {
            for (int i = 0; i < m_UVs.Length; i++)
            {
                m_UVs[i] += UVPanDirection * AnimationSpeed * Time.deltaTime;
            }
            m_Mesh.uv = m_UVs;
        }
    }
}
