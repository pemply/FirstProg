using System;
using System.Collections.Generic;
using UnityEngine;
using CodeBase.StaticData; 

[Serializable]
public class EnemyMixEntry
{
    public MonsterTypeId Type;
    [Min(1)] public int Weight = 1;
}

[CreateAssetMenu(menuName = "StaticData/Waves/WaveConfig", fileName = "WaveConfig")]
public class WaveConfig : ScriptableObject
{
    [Min(1f)] public float Duration = 30f;
    [Min(0.1f)] public float SpawnInterval = 1f;
    [Min(1)] public int MaxAlive = 10;

    public List<EnemyMixEntry> Mix = new();

    public MonsterTypeId RollEnemy()
    {
        int total = 0;
        foreach (var e in Mix)
            total += Mathf.Max(0, e.Weight);

        if (total <= 0)
            return Mix.Count > 0 ? Mix[0].Type : default;

        int roll = UnityEngine.Random.Range(0, total);
        int acc = 0;

        foreach (var e in Mix)
        {
            acc += Mathf.Max(0, e.Weight);
            if (roll < acc)
                return e.Type;
        }

        return Mix[0].Type;
    }
}