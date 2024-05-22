using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotatorV2 : MonoBehaviour
{
    public float Speed;
    public Vector3 RotationVector;

    void Update()
    {
        transform.Rotate(RotationVector * Speed * Time.deltaTime);
    }
}
