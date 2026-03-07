using System.Collections;
using CodeBase;
using UnityEngine;
using CodeBase.CameraLogic;

public class CombatJuice : MonoBehaviour
{
    [Header("Crit Shake")]
    [SerializeField] private float _critShakeAmp = 0.12f;
    [SerializeField] private float _critShakeDur = 0.09f;

    [Header("Hit Stop")]
    [SerializeField] private float _critTimeScale = 0.08f;
    [SerializeField] private float _critStopRealSeconds = 0.03f;

    [Header("Spam protection")]
    [SerializeField] private float _critCooldown = 0.07f;

    private CritFlash _flash;
    private CameraFollow _follow;
    private Coroutine _hitStopCo;
    private float _cd;

    private void Awake()
    {
        _follow = GetComponent<CameraFollow>();
    }

    public void BindHud(GameObject hud)
    {
        _flash = hud != null ? hud.GetComponentInChildren<CritFlash>(true) : null;
        Debug.Log($"[CombatJuice] bind flash: {_flash != null}", this);
    }

    private void Update()
    {
        if (_cd > 0f)
            _cd -= Time.unscaledDeltaTime;

        if (Input.GetKeyDown(KeyCode.F))
            _flash?.Flash();
    }

    public void OnCrit()
    {
        if (GamePause.IsHardPaused)
            return;

        if (_cd > 0f)
            return;

        _cd = _critCooldown;

        _flash?.Flash();
        _follow?.Shake(_critShakeAmp, _critShakeDur);

        if (_hitStopCo != null)
            StopCoroutine(_hitStopCo);

        _hitStopCo = StartCoroutine(HitStopRoutine());
    }

    private IEnumerator HitStopRoutine()
    {
        float prev = Time.timeScale;
        if (GamePause.IsHardPaused || prev <= Constant.Epsilone)
        {
            _hitStopCo = null;
            yield break;
        }
        Time.timeScale = _critTimeScale;
        yield return new WaitForSecondsRealtime(_critStopRealSeconds);
        if (!GamePause.IsHardPaused)
            Time.timeScale = prev;

        _hitStopCo = null;
    }

    private void OnDisable()
    {
        if (_hitStopCo != null)
        {
            StopCoroutine(_hitStopCo);
            _hitStopCo = null;
        }

        if (!GamePause.IsHardPaused)
            Time.timeScale = 1f;
    }
    public static class GamePause
    {
        public static bool IsHardPaused; 
    }
}