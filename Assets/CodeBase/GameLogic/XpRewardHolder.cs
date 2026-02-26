using UnityEngine;

namespace CodeBase.GameLogic
{
    public class XpRewardHolder : MonoBehaviour
    {
        public int Xp { get; private set; }

        public void Set(int xp) =>
            Xp = xp;
    }
}