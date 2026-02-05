using CodeBase.Data;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Services.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Hero
{
    public class HeroMove : MonoBehaviour, ISavedProgress
    {
        [SerializeField] private HeroAnimator _heroAnimator;
        public CharacterController CharacterController;
        public float MovementSpeed = 5f;

        private Camera _camera;
        private IInputService _inputService;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private void Awake()
        {
            _camera = Camera.main;
            _inputService = AllServices.Container.Single<IInputService>();

            if (_heroAnimator == null)
                _heroAnimator = GetComponentInChildren<HeroAnimator>(true);
        }

        private void Update()
        {
            Vector3 input = Vector3.zero;

            Vector2 axis = _inputService.Axis;
            float speed01 = Mathf.Clamp01(axis.magnitude);

            if (axis.sqrMagnitude > Constant.Epsilone)
            {
                input = _camera.transform.TransformDirection(new Vector3(axis.x, 0f, axis.y));
                input.y = 0f;
                input.Normalize();

                transform.forward = input;
            }

            // ✅ анімація окремо
            _heroAnimator?.SetSpeed(speed01);

            // ✅ рух
            Vector3 velocity = input * MovementSpeed;
            velocity += Physics.gravity;
            CharacterController.Move(velocity * Time.deltaTime);
        }

        // ---------------- SAVE / LOAD ----------------

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.WorldData.PositionOnLevel =
                new PositionOnLevel(CurrentLevel(), transform.position.AsVectorData());
        }

        public void LoadProgress(PlayerProgress progress)
        {
            if (CurrentLevel() == progress.WorldData.PositionOnLevel.Level)
            {
                Vector3Data savedPosition = progress.WorldData.PositionOnLevel.Position;

                if (savedPosition != null)
                    Warp(savedPosition);
            }
        }

        private void Warp(Vector3Data to)
        {
            CharacterController.enabled = false;
            transform.position = to.AsUnityVector().AddY(CharacterController.height);
            CharacterController.enabled = true;
        }

        private static string CurrentLevel() =>
            SceneManager.GetActiveScene().name;
    }
}
