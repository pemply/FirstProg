using CodeBase.Enemy;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.UI;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace CodeBase.Infrastructure.Factory
{
    public class MonsterFactory
    {
        private readonly IStaticDataService _staticData;
        private readonly IDifficultyScalingService _difficulty;
        private readonly EliteConfig _elite;

        public MonsterFactory(IStaticDataService staticData, IDifficultyScalingService difficulty)
        {
            _staticData = staticData;
            _difficulty = difficulty;

            _elite = Resources.Load<EliteConfig>(AssetsPath.EliteConfigPath);
           
        }

       public GameObject CreateMonster(MonsterTypeId monsterTypeId, Transform parent, Transform heroTransform)
        {
            MonsterStaticData monsterData = _staticData.ForMonster(monsterTypeId);

            Vector3 spawnPos = parent != null ? parent.position : Vector3.zero;
            Quaternion spawnRot = parent != null ? parent.rotation : Quaternion.identity;

            GameObject monster = Object.Instantiate(monsterData.PrefabReference, spawnPos, spawnRot, parent);

            bool isElite = _elite != null && Random.value <= _elite.Chance;

            float hp = CalcAndApplyRewards(monsterTypeId, monsterData, monster, isElite, out float dmg);

            if (isElite && _elite != null)
                monster.transform.localScale *= _elite.ScaleMult;

            // Health
            IHealth health = monster.GetComponent<IHealth>();
            if (health == null)
                Debug.LogError($"[MonsterFactory] IHealth missing on {monster.name}");
            else
            {
                health.currentHealth = hp;
                health.maxHealth = hp;
            }

            // UI / Move
            monster.GetComponent<ActorUI>()?.Construct(health);
            monster.GetComponent<AgentMoveToPlayer>()?.Construct(heroTransform);

            var agent = monster.GetComponent<NavMeshAgent>();
            if (agent != null)
                agent.speed = monsterData.MoveSpeed;

            // Base/Tank attack
            var baseAttack = monster.GetComponent<EnemyAttack>();
            if (baseAttack != null)
            {
                baseAttack.Construct(heroTransform);
                baseAttack.Damage = dmg;
                baseAttack.AttackColdown = monsterData.AttackCooldown;
                baseAttack.Cleavage = monsterData.Cleavage;
                baseAttack.EffectiveDistance = monsterData.EffectiveDistance;
            }

            // Kamikaze
            var kamikaze = monster.GetComponent<KamikazeAttack>();

            if (kamikaze != null)
            {
                if (baseAttack != null)
                    baseAttack.enabled = false;

                kamikaze.enabled = true;
                kamikaze.Construct(heroTransform);

                kamikaze.Damage = dmg;
                kamikaze.AttackColdown = monsterData.AttackCooldown;
                kamikaze.Cleavage = monsterData.Cleavage;
                kamikaze.EffectiveDistance = monsterData.EffectiveDistance;

                kamikaze.SetConfig(monsterData.Kamikaze);
            }

            return monster;
        }

        private float CalcAndApplyRewards(MonsterTypeId monsterTypeId, MonsterStaticData monsterData, GameObject monster,
            bool isElite, out float dmg)
        {
            float hp = monsterData.Hp * _difficulty.HpMult;
            dmg = monsterData.Damage * _difficulty.DmgMult;
            int xp = Mathf.RoundToInt(monsterData.XpReward * _difficulty.XpMult);

            if (isElite && _elite != null)
            {
                hp *= _elite.HpMult;
                dmg *= _elite.DmgMult;
                xp = Mathf.RoundToInt(xp * _elite.XpMult);
            }

            var holder = monster.GetComponent<XpRewardHolder>() ?? monster.AddComponent<XpRewardHolder>();
            holder.Set(xp);
            
            return hp;
        }
    }
}
