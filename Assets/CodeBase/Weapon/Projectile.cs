using System.Collections.Generic;
using CodeBase;
using CodeBase.Enemy;
using CodeBase.GameLogic;
using CodeBase.GameLogic.Pool;
using CodeBase.Infrastructure.Services.Pool;
using CodeBase.Logic;
using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float _radius = 0.15f;
    [SerializeField] private float _maxLifeTime = 5f;

    private static int _alive;
    private static int _returned;
    private static int _destroyed;

    private float _damage;
    private float _speed;
    private int _enemyMask;
    private int _pierceLeft;
    private float _timeLeft;

    private bool _isCrit;
    private bool _despawned;

    private GameObject _impactFx;
    private GameObject _muzzleFx;
    private Transform _ownerRoot;

    private readonly HashSet<int> _hitHealthIds = new HashSet<int>(16);
    private IPoolService _pool;
    private PooledObject _pooled;
    private IDamagePopupService _damagePopups;
    private float _knockback;
    private float _knockbackChance;
    private TrailRenderer _trail;
    private ParticleSystem _ps;

    private void OnEnable()
    {
        _alive++;
        Debug.Log($"[PROJ] alive={_alive}");
    }

    private void OnDisable()
    {
        _alive--;
        Debug.Log($"[PROJ] alive={_alive}");
    }

    private void OnDestroy()
    {
        _destroyed++;
        Debug.Log($"[PROJ] destroyed={_destroyed}");
    }

    private void Awake()
    {
        _trail = GetComponentInChildren<TrailRenderer>(true);
        _ps = GetComponentInChildren<ParticleSystem>(true);
    }

    public void OnSpawned()
    {
        _despawned = false;
        _hitHealthIds.Clear();
        // ресет візуалів (якщо є)
        if (_trail != null)
        {
            _trail.Clear();
            _trail.emitting = true;
        }

        if (_ps != null)
        {
            _ps.Clear(true);
            _ps.Play(true);
        }
    }

    public void OnDespawned()
    {
        // щоб не тягнути старе
        _damagePopups = null;
        _hitHealthIds.Clear();
        _returned++;
        // Debug.Log($"[PROJ] returned={_returned} alive={_alive}");
        if (_trail != null) _trail.emitting = false;
        if (_ps != null) _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void SetCrit(bool isCrit) => _isCrit = isCrit;

    public void Construct(float damage,
        float range,
        float speed,
        int enemyMask,
        int pierce,
        float knockback,
        float knockbackChance,
        IDamagePopupService damagePopups,
        GameObject impactFx,
        GameObject muzzleFx,
        Transform ownerRoot,
        IPoolService pool)
    {
        _damage = damage;
        _speed = Mathf.Max(0.01f, speed);
        _enemyMask = enemyMask;
        _pierceLeft = Mathf.Max(0, pierce);
        _knockback = knockback;
        _knockbackChance = knockbackChance;
        _timeLeft = Mathf.Min(_maxLifeTime, Mathf.Max(0.2f, range / _speed));
        _hitHealthIds.Clear();

        _damagePopups = damagePopups;

        _impactFx = impactFx;
        _muzzleFx = muzzleFx;
        _ownerRoot = ownerRoot;
        _pool = pool;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        _timeLeft -= dt;
        if (_timeLeft <= 0f)
        {
            Despawn();
            return;
        }

        Vector3 from = transform.position;
        Vector3 to = from + transform.forward * (_speed * dt);
        Vector3 dir = to - from;
        float dist = dir.magnitude;

        if (dist > Constant.Epsilone)
        {
            if (Physics.SphereCast(from, _radius, dir.normalized, out RaycastHit hit, dist, _enemyMask))
            {
                var health = hit.collider.GetComponentInParent<IHealth>();
                if (health != null)
                {
                    int id = ((Component)health).GetInstanceID();

                    if (_hitHealthIds.Contains(id))
                    {
                        transform.position = hit.point + transform.forward * 0.05f;
                        return;
                    }

                    _hitHealthIds.Add(id);
                    var healthComponent = (Component)health;


                    if (_ownerRoot != null && healthComponent.transform.root == _ownerRoot)
                    {
                        transform.position = hit.point + transform.forward * 0.05f;
                        return;
                    }

                    health.TakeDamage(_damage);
                    TryApplyKnockback(hit.collider);
                    SpawnImpact(hit.point, hit.normal);
                    if (_damagePopups != null && IsUnityObjectAlive(_damagePopups))
                        _damagePopups.Spawn(hit.point, Mathf.RoundToInt(_damage), _isCrit);
                    else
                        _damagePopups = null;

                    if (_pierceLeft > 0)
                    {
                        _pierceLeft--;
                        transform.position = hit.point + transform.forward * 0.05f;
                        return;
                    }

                    Despawn();
                    return;
                }
            }
        }

        transform.position = to;
    }

    private void SpawnImpact(Vector3 point, Vector3 normal)
    {
        if (_impactFx == null || _pool == null) return;
        _pool.Get(_impactFx, point, Quaternion.LookRotation(normal));
    }

    private void SpawnMuzzle()
    {
        if (_muzzleFx == null || _pool == null) return;
        _pool.Get(_muzzleFx, transform.position, transform.rotation);
    }

    private bool IsUnityObjectAlive(object service)
    {
        var uo = service as UnityEngine.Object;
        return uo == null || uo;
    }

    private void Despawn()
    {
        if (_despawned) return;
        _despawned = true;

        if (_pooled == null)
            _pooled = GetComponent<PooledObject>();

        Debug.Log(
            $"[PROJ] Despawn {name} pooled={(_pooled != null)} alive={_alive} returned={_returned} destroyed={_destroyed}");

        if (_pooled != null) _pooled.Release();
        else Destroy(gameObject);
    }
    private void TryApplyKnockback(Collider col)
    {
        if (_knockback <= 0f)
            return;

        float chancePercent = Mathf.Clamp(_knockbackChance, 0f, 100f);
        if (chancePercent <= 0f)
            return;

        float roll = Random.Range(0f, 100f);
        if (roll >= chancePercent)
            return;

        EnemyKnockback knockback = col.GetComponentInParent<EnemyKnockback>();
        if (knockback == null)
            return;

        Vector3 dir = transform.forward;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        knockback.Push(dir.normalized, _knockback);
    }
}