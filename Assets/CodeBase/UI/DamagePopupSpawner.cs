using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Pool;
using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour, IDamagePopupService
{
    [SerializeField] private DamagePopupView _prefab;

    [Header("Jitter")]
    [SerializeField] private Vector2 _normalJitter = new(12f, 6f);
    [SerializeField] private Vector2 _critJitter   = new(28f, 16f);

    private Camera _cam;
    private IPoolService _pool;

    public void Construct(IPoolService pool)
    {
        _pool = pool;
        if (_cam == null) _cam = Camera.main;
    }

    private void Awake()
    {
        _cam = Camera.main;
    }

    public void Spawn(Vector3 worldPos, int damage, bool isCrit)
    {
        if (_prefab == null)
            return;

        if (_cam == null)
            _cam = Camera.main;

        if (_cam == null)
            return;

        Vector3 screen = _cam.WorldToScreenPoint(worldPos);

        Vector2 j = isCrit ? _critJitter : _normalJitter;
        screen.x += Random.Range(-j.x, j.x);
        screen.y += Random.Range(-j.y, j.y);

        DamagePopupView popup;

        if (_pool != null)
        {
            var go = _pool.Get(_prefab.gameObject, screen, Quaternion.identity, transform);
            popup = go.GetComponent<DamagePopupView>();
        }
        else
        {
            // якщо хтось забув Construct - буде видно по поведінці (але не краш)
            popup = Instantiate(_prefab, transform);
        }

        popup.Show(screen, damage, isCrit);
    }
 
}

public interface IDamagePopupService : IService
{
    void Spawn(Vector3 worldPos, int damage, bool isCrit);
}
