using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Pool;
using UnityEngine;


public class DamagePopupSpawner : MonoBehaviour, IDamagePopupService
{
    [SerializeField] private DamagePopupView _prefab;

    [Header("Jitter")]
    [SerializeField] private Vector2 _normalJitter = new(12f, 6f);
    [SerializeField] private Vector2 _critJitter   = new(28f, 16f);
    [SerializeField] private Vector2 _healJitter   = new(10f, 5f);

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
        if (_prefab == null) return;
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        Vector3 screen = _cam.WorldToScreenPoint(worldPos);

        Vector2 j = isCrit ? _critJitter : _normalJitter;
        screen.x += Random.Range(-j.x, j.x);
        screen.y += Random.Range(-j.y, j.y);

        DamagePopupView popup = SpawnPopup(screen);
        popup.Show(screen, damage, isCrit);
    }

    // ✅ NEW: heal з параметрами стилю
    public void SpawnHeal(Vector3 worldPos, int healAmount, float life, float floatSpeed)
    {
        if (_prefab == null) return;
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        if (healAmount <= 0) return;

        Vector3 screen = _cam.WorldToScreenPoint(worldPos);

        Vector2 j = _healJitter;
        screen.x += Random.Range(-j.x, j.x);
        screen.y += Random.Range(-j.y, j.y);

        DamagePopupView popup = SpawnPopup(screen);

        // 👇 віддаємо як "негативний damage" + кастом life/speed
        popup.ShowHealLike(screen, healAmount, life, floatSpeed);    }

    private DamagePopupView SpawnPopup(Vector3 screenPos)
    {
        DamagePopupView popup;

        if (_pool != null)
        {
            var go = _pool.Get(_prefab.gameObject, screenPos, Quaternion.identity, transform);
            popup = go.GetComponent<DamagePopupView>();
        }
        else
        {
            popup = Instantiate(_prefab, transform);
        }

        return popup;
    }
}
public interface IDamagePopupService : IService
{
    void Spawn(Vector3 worldPos, int damage, bool isCrit);

    void SpawnHeal(Vector3 worldPos, int healAmount, float life, float floatSpeed);
}