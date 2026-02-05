using UnityEngine;

namespace CodeBase.Logic
{
    public class LookAtCamera : MonoBehaviour
    {
        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void LateUpdate()
        {
            if (_cam == null) return;

            transform.forward = _cam.transform.forward;
        }
    }
}