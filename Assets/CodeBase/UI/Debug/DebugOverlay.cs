using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Infrastructure.Services.RunTime;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Debug
{
   
    public class DebugOverlay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        private static DebugOverlay _instance;
        private IDifficultyScalingService _difficulty;
       // private RunTimerService _runTimer;
        private IXpService _xp;
        


      private void Awake()
      {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
    gameObject.SetActive(false);
    return;
#endif

          if (_instance != null)
          {
              Destroy(gameObject);
              return;
          }

          _instance = this;
          DontDestroyOnLoad(gameObject);
      }

        private void Start()
        {
            // Беремо сервіси з контейнера (як ти вже робиш у проекті)
            _difficulty = AllServices.Container.Single<IDifficultyScalingService>();
         //   _runTimer = AllServices.Container.Single<RunTimerService>();
            _xp = AllServices.Container.Single<IXpService>();
            
          //  _dps = FindObjectOfType<HeroDpsMeter>();
        }

        private void Update()
        {
            if (_text == null) return;

         //   float t = _runTimer != null ? _runTimer.ElapsedSeconds : 0f;

            string xpLine;
            try
            {
                // навіть якщо _xp != null, всередині може бути null progress
                xpLine = $"Level: {_xp.Level}  XP: {_xp.CurrentXpInLevel}/{_xp.RequiredXp}";
            }
            catch
            {
                xpLine = "Level: n/a  XP: n/a (progress not ready)";
            }

            _text.text =
                $"<b>DEBUG</b>\n" +
              //  $"Time: {t:F1}s\n" +
                $"Tier: {_difficulty?.Tier ?? 0}\n" +
                $"HP: {_difficulty?.HpMult ?? 1f:F2}  DMG: {_difficulty?.DmgMult ?? 1f:F2}  XP: {_difficulty?.XpMult ?? 1f:F2}\n" +
                $"{xpLine}\n";

        }

        
        }
    
}