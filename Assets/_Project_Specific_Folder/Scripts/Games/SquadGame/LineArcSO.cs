using Crosstales.OnlineCheck.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArcLine", menuName = "Scriptable Objects/ArcLine")]
public class LineArcSO : ScriptableObject
{
    public int Level;
    public Color LineColor;
    public float Speed;
    public AudioClip HitSound;
    public float Pitch;
}
