using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour
{
    public static DamagePopupSpawner Instance { get; private set; }

    [SerializeField] private DamagePopupView _prefab;

    [Header("Jitter")]
    [SerializeField] private Vector2 _normalJitter = new Vector2(12f, 6f);
    [SerializeField] private Vector2 _critJitter   = new Vector2(28f, 16f);

    private Camera _cam;

    private void Awake()
    {
        Instance = this;
        _cam = Camera.main;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Spawn(Vector3 worldPos, int damage, bool isCrit)
    {
        if (_prefab == null || _cam == null) return;

        Vector3 screen = _cam.WorldToScreenPoint(worldPos);

        Vector2 j = isCrit ? _critJitter : _normalJitter;
        screen.x += Random.Range(-j.x, j.x);
        screen.y += Random.Range(-j.y, j.y);

        var popup = Instantiate(_prefab, transform);
        popup.Show(screen, damage, isCrit);
    }
}