using System;
using UnityEngine;

namespace CodeBase.Enemy
{
    // Висить на ворогу (на root префабі)
    public class AliveCounterHandle : MonoBehaviour
    {
        private Action _onGone;
        private bool _gone;

        public void Construct(Action onGone)
        {
            _onGone = onGone;
            _gone = false;
        }

        // викликаємо, коли ворог "зник" (пул/дестрой) — НЕ обовʼязково смерть
        public void MarkGone()
        {
            if (_gone) return;
            _gone = true;
            _onGone?.Invoke();
        }

        // якщо об’єкт повернули в пул без явного MarkGone — підстрахуємось
        private void OnDisable()
        {
            // важливо: якщо ти десь просто вимикаєш GO / пул вимикає
            MarkGone();
        }
    }
}