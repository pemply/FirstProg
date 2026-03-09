using System;
using CodeBase.GameLogic.Upgrade;
using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/Upgrade Visuals")]
    public class UpgradeVisualConfig : ScriptableObject
    {
        [Serializable]
        public struct UpgradeIconEntry
        {
            public UpgradeType Type;
            public Sprite Icon;
        }

        [Serializable]
        public struct RarityFrameEntry
        {
            public UpgradeRarity Rarity;
            public Sprite Frame;
        }

        [Header("Icons by upgrade type")]
        public UpgradeIconEntry[] Icons;

        [Header("Frames by rarity")]
        public RarityFrameEntry[] Frames;

        public Sprite GetIcon(UpgradeType type)
        {
            for (int i = 0; i < Icons.Length; i++)
            {
                if (Icons[i].Type == type)
                    return Icons[i].Icon;
            }

            return null;
        }

        public Sprite GetFrame(UpgradeRarity rarity)
        {
            for (int i = 0; i < Frames.Length; i++)
            {
                if (Frames[i].Rarity == rarity)
                    return Frames[i].Frame;
            }

            return null;
        }
    }
}