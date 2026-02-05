using CodeBase.Logic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _radius = 0.15f;   // товщина кулі
    [SerializeField] private float _maxLifeTime = 5f; // запас

    private float _damage;
    private float _speed;
    private int _enemyMask;
    private int _pierceLeft;
    private float _timeLeft;

    public void Construct(float damage, float range, float speed, int enemyMask, int pierce)
    {
        _damage = damage;
        _speed = Mathf.Max(0.01f, speed);
        _enemyMask = enemyMask;
        _pierceLeft = Mathf.Max(0, pierce);

        _timeLeft = Mathf.Min(_maxLifeTime, Mathf.Max(0.2f, range / _speed));
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
        Vector3 dir = (to - from);
        float dist = dir.magnitude;

        if (dist > 0.0001f)
        {
            if (Physics.SphereCast(from, _radius, dir.normalized, out RaycastHit hit, dist, _enemyMask))
            {
                var health = hit.collider.GetComponentInParent<IHealth>();
                if (health != null)
                {
                    health.TakeDamage(_damage);

                    if (_pierceLeft > 0)
                    {
                        _pierceLeft--;
                        // пересуваємось трохи вперед, щоб не застрягнути в тому ж колайдері
                        transform.position = hit.point + transform.forward * 0.02f;
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