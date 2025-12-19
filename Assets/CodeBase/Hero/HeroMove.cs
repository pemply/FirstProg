using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.Input;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroMove : MonoBehaviour
    {
        public CharacterController CharacterController;
        public float MovementSpeed = 5;
        private Camera _camera;
        private IInputService _inputService;

        private void Awake()
        {
            _inputService = Game.InputService;

            if (_camera == null)
                _camera = Camera.main;
        }

        private void Update()
        {
            if (Time.frameCount % 30 == 0)
                Debug.Log($"[HeroMove] me={transform.name} pos={transform.position} root={transform.root.name} rootPos={transform.root.position}");

            if (_camera == null)
                _camera = Camera.main;

            Vector3 movementVector = Vector3.zero;

            if (_inputService.Axis.sqrMagnitude > Constant.Epsilone)
            {
                movementVector = _camera.transform.TransformDirection(_inputService.Axis);
                movementVector.y = 0;
                movementVector.Normalize();

                transform.forward = movementVector;
            }

            movementVector += Physics.gravity;
            CharacterController.Move(MovementSpeed * movementVector * Time.deltaTime);
        }

       
    }
}