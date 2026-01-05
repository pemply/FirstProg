using System;
using UnityEngine;
using UnityEngine.UI;
namespace CodeBase.UI.over
{
    public class GameOverWindow : MonoBehaviour
    {
        [SerializeField] private Button _restartButton;
        private Action _onRestart;

        public void Construct(Action onRestart)
        {
            _onRestart = onRestart;

            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveAllListeners();
                _restartButton.onClick.AddListener(() => _onRestart?.Invoke());
            }
        }

        private void OnDestroy()
        {
            if (_restartButton != null)
                _restartButton.onClick.RemoveAllListeners();
        }
    }
}