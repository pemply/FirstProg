using System.Collections.Generic;
using CodeBase.Logic;
using CodeBase.UI;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _radius = 0.15f;
    [SerializeField] private float _maxLifeTime = 5f;

    private float _damage;
    private float _speed;
    private int _enemyMask;
    private int _pierceLeft;
    private float _timeLeft;

    private bool _isCrit;

    // ✅ щоб не дамажити того ж ворога двічі (через кілька колайдерів/кадрів)
    private readonly HashSet<int> _hitHealthIds = new HashSet<int>(16);

    public void SetCrit(bool isCrit) => _isCrit = isCrit;

    public void Construct(float damage, float range, float speed, int enemyMask, int pierce)
    {
        _damage = damage;
        _speed = Mathf.Max(0.01f, speed);
        _enemyMask = enemyMask;
        _pierceLeft = Mathf.Max(0, pierce);

        _timeLeft = Mathf.Min(_maxLifeTime, Mathf.Max(0.2f, range / _speed));

        _hitHealthIds.Clear();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        _timeLeft -= dt;
        if (_timeLeft <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 from = transform.position;
        Vector3 to = from + transform.forward * (_speed * dt);
        Vector3 dir = to - from;
        float dist = dir.magnitude;

        if (dist > 0.0001f)
        {
            if (Physics.SphereCast(from, _radius, dir.normalized, out RaycastHit hit, dist, _enemyMask))
            {
                var health = hit.collider.GetComponentInParent<IHealth>();
                if (health != null)
                {
                    int id = ((Component)health).GetInstanceID();

                    // ✅ вже били цього ворога цим снарядом — пропускаємо
                    if (_hitHealthIds.Contains(id))
                    {
                        // трохи проштовхнемось, щоб не застрягти в тому ж колайдері
                        transform.position = hit.point + transform.forward * 0.05f;
                        return;
                    }

                    _hitHealthIds.Add(id);

                    health.TakeDamage(_damage);

                    DamagePopupSpawner.Instance?.Spawn(hit.point, Mathf.RoundToInt(_damage), _isCrit);


                    if (_pierceLeft > 0)
                    {
                        _pierceLeft--;
                        transform.position = hit.point + transform.forward * 0.05f;
                        return;
                    }

                    Destroy(gameObject);
                    return;
                }
            }
        }

        transform.position = to;
    }
}
