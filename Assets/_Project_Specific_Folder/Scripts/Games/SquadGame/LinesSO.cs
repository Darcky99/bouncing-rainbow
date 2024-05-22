using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Line types", menuName = "Scriptable Objects/Line types")]
public class LinesSO : ScriptableObject
{
    public LineArcSO[] LinesList;
}
