using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.StaticData
{
  
    [CreateAssetMenu(menuName = "StaticData/Waves/WaveSequence", fileName = "WaveSequence")]
    public class WaveSequence : ScriptableObject
    {
        public List<WaveConfig> Waves = new();
    }
}