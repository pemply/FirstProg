using UnityEngine;

namespace CodeBase.Logic
{
    public class XpRewardHolder : MonoBehaviour
    {
        public int Xp { get; private set; }

        public void Set(int xp) =>
            Xp = xp;
    }
}