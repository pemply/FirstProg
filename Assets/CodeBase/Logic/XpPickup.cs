using CodeBase.Infrastructure.Services.Progress;
using UnityEngine;

namespace CodeBase.Logic
{
    public class XpPickup : MonoBehaviour
    {
        private int _amount;
        private IXpService _xp;

        public void Construct(int amount, IXpService xp)
        {
            _amount = amount;
            _xp = xp;
        }

        public void Collect()
        {
            _xp?.AddXp(_amount);
            Destroy(gameObject);
        }
    }
}