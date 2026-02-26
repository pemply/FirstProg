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

        [Header("Rotation")]
        [SerializeField] private float _turnSpeed = 12f;
        [SerializeField] private float _backpedalDot = -0.15f;

        [Header("Auto Aim")]
        [SerializeField] private bool _autoAim = true;
        [SerializeField] private float _aimRefresh = 0.12f;
        [SerializeField] private LayerMask _enemyMask;

        private AttackRadiusZoneForHero _attackZone;

        private Vector3 _lookDir;
        private Transform _aimTarget;
        private float _aimTimer;

        private readonly Collider[] _aimHits = new Collider[24];

        private void Awake()
        {
            _camera = Camera.main;
            _inputService = AllServices.Container.Single<IInputService>();

            if (_heroAnimator == null)
                _heroAnimator = GetComponentInChildren<HeroAnimator>(true);

            _attackZone = GetComponentInChildren<AttackRadiusZoneForHero>(true);

            _lookDir = transform.forward;
            _lookDir.y = 0f;
            if (_lookDir.sqrMagnitude < Constant.Epsilone)
                _lookDir = Vector3.forward;
            _lookDir.Normalize();
        }

        private void Update()
        {
            Vector2 axis = _inputService.Axis;
            float speed01 = Mathf.Clamp01(axis.magnitude);

            // -------- MOVE DIR (camera-space) --------
            Vector3 moveDir = Vector3.zero;
            if (axis.sqrMagnitude > Constant.Epsilone)
            {
                moveDir = _camera.transform.TransformDirection(new Vector3(axis.x, 0f, axis.y));
                moveDir.y = 0f;
                moveDir.Normalize();
            }

            // -------- AUTO AIM --------
            if (_autoAim)
            {
                _aimTimer -= Time.deltaTime;
                if (_aimTimer <= 0f)
                {
                    _aimTarget = FindNearestEnemyInAttackRadius();
                    _aimTimer = _aimRefresh;
                }
            }
            else
            {
                _aimTarget = null;
            }

            // -------- LOOK DIR --------
            if (_aimTarget != null)
            {
                Vector3 to = _aimTarget.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > Constant.Epsilone)
                    _lookDir = to.normalized;
            }
            else
            {
                // fallback: backpedal від руху
                if (moveDir.sqrMagnitude > Constant.Epsilone)
                {
                    float dot = Vector3.Dot(_lookDir, moveDir);
                    if (dot > _backpedalDot)
                        _lookDir = moveDir;
                }
            }

            transform.forward = Vector3.Slerp(transform.forward, _lookDir, _turnSpeed * Time.deltaTime);

            // -------- ANIM --------
            _heroAnimator?.SetSpeed(speed01);

            // -------- MOVE --------
            Vector3 velocity = moveDir * MovementSpeed;
            velocity += Physics.gravity;
            CharacterController.Move(velocity * Time.deltaTime);
        }

        private Transform FindNearestEnemyInAttackRadius()
        {
            float r = (_attackZone != null) ? _attackZone.Radius : 0f;
            if (r <= 0.05f) return null;

            int count = Physics.OverlapSphereNonAlloc(transform.position, r, _aimHits, _enemyMask);
            if (count <= 0) return null;

            Transform best = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var c = _aimHits[i];
                if (c == null) continue;

                Transform t = c.transform.root;

                // якщо є EnemyHealth і він dead — пропуск
                var eh = t.GetComponentInChildren<Enemy.EnemyHealth>();
                if (eh != null && eh.IsDead) continue;

                Vector3 d = t.position - transform.position;
                d.y = 0f;
                float sqr = d.sqrMagnitude;

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = t;
                }
            }

            return best;
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