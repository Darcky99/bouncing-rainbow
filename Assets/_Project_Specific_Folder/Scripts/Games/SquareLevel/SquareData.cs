using System;
using UnityEngine;

[Serializable]
public class SquareData
{
    [SerializeField] private int m_Level;
    [SerializeField] private Color m_Color;
    [SerializeField] private float m_Speed;
    [SerializeField] private AudioClip m_Sound;

    public int Level { get { return m_Level; } }
    public Color Color { get { return m_Color; } }
    public float Speed { get { return m_Speed; } }
    public AudioClip Sound { get { return m_Sound; } }

    public SquareData(int i_Level, Color i_Color, float i_Speed, AudioClip i_Sound)
    {
        m_Level = i_Level;
        m_Color = i_Color;
        m_Speed = i_Speed;
        m_Sound = i_Sound;
    }
}
