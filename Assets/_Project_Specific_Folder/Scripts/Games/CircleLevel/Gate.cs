using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    //[SerializeField] private SpriteRenderer m_SpriteRenderer;

    private Vector3 m_Rotation;
    public void SetAngle(float i_SectionValue)
    {
        float i_degrees = i_SectionValue * 360f;
        i_degrees = 180f - i_degrees;
        m_Rotation.z = i_degrees;
        transform.localEulerAngles = m_Rotation;
    }
}