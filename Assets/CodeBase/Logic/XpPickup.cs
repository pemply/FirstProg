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
            Debug.Log($"[XpPickup] Construct amount={_amount}, xpNull={_xp == null}");
        }

        public void Collect()
        {
            Debug.Log($"[XpPickup] Collect amount={_amount}, xpNull={_xp == null}");
            _xp?.AddXp(_amount);
            Destroy(gameObject);
        }
    }
}