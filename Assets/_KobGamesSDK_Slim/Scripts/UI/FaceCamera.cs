using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KobGamesSDKSlim.Collectable
{
    public class FaceCamera : MonoBehaviour
    {
        private void OnEnable()
        {
            rotateTowardsCamera();
        }

        void FixedUpdate()
        {
            rotateTowardsCamera();
        }

        private void rotateTowardsCamera()
        {
            transform.rotation = CameraManager.Instance.MainCamera.transform.rotation;
        }
    }
}
