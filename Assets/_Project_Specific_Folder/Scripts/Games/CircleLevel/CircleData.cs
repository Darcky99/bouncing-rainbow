using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CircleData
{
    [SerializeField] private int m_Level;
    [SerializeField] private Color m_Color;
    [SerializeField] private float m_LineLenght;
    [SerializeField] private float m_Speed;
    [SerializeField] private AudioClip m_Sound;

    public int Level { get { return m_Level; } }
    public Color Color { get { return m_Color; } }
    public float LineLenght { get { return m_LineLenght; } }
    public float Speed { get { return m_Speed; } }
    public AudioClip Sound { get { return m_Sound; } }
    public string Identifier { get { return $"Level {m_Level}"; } }

    public CircleData(int i_Level, Color i_Color, float i_LineLenght, float i_Speed)
    {
        m_Level = i_Level;
        m_Color = i_Color;
        m_LineLenght = i_LineLenght;
        m_Speed = i_Speed;
    }
}
