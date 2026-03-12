using CodeBase.Enemy;
using CodeBase.GameLogic;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.Pool;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.StaticData;
using CodeBase.UI;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Infrastructure.Factory
{
    public class MonsterFactory
    {
        private readonly IStaticDataService _staticData;
        private readonly IDifficultyScalingService _difficulty;
        private readonly EliteConfig _elite;
        private readonly IPoolService _pool;
        private IDamagePopupService _popupService;

        public MonsterFactory(IStaticDataService staticData, IDifficultyScalingService difficulty, IPoolService pool, IDamagePopupService popupService)
        {
            _staticData = staticData;
            _difficulty = difficulty;
            _pool = pool;
            _popupService = popupService;

            _elite = Resources.Load<EliteConfig>(AssetsPath.EliteConfigPath);
        }
        public void SetDamagePopups(IDamagePopupService popups) =>
            _popupService = popups;
        public GameObject CreateMonster(MonsterTypeId monsterTypeId, Transform parent, Transform heroTransform)
        {
           

            MonsterStaticData monsterData = _staticData.ForMonster(monsterTypeId);

            Vector3 spawnPos = parent != null ? parent.position : Vector3.zero;
            Quaternion spawnRot = parent != null ? parent.rotation : Quaternion.identity;

            if (!TryGetValidSpawnPoint(spawnPos, NavMesh.AllAreas, out spawnPos))
            {
                Debug.LogWarning($"[MonsterFactory] Invalid spawn point: {spawnPos}");
                return null;
            }

            GameObject monster = _pool.Get(monsterData.PrefabReference, spawnPos, spawnRot, null);
            monster.transform.localScale = monsterData.PrefabReference.transform.localScale;

            ResetPooledMonster(monster);
            
            bool isElite = _elite != null && Random.value <= _elite.Chance;

            float hp = CalcAndApplyRewards(monsterTypeId, monsterData, monster, isElite, out float dmg);

            if (isElite && _elite != null)
                monster.transform.localScale *= _elite.ScaleMult;
           
            // HEALTH
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
            {
                agent.speed = monsterData.MoveSpeed;

                if (TryGetSpawnOnNavMesh(monster.transform.position, agent.areaMask, out Vector3 fixedSpawn))
                    agent.Warp(fixedSpawn);
            }
            
            // ATTACK COMPONENTS

            var baseAttack = monster.GetComponent<EnemyAttack>();
            var kamikaze = monster.GetComponent<KamikazeAttack>();
            var areaAttack = monster.GetComponent<EnemyAreaAttack>();

            // SENSOR RADIUS
            var sensor = monster.GetComponent<AttackSensorRadiusApplier>();
            if (sensor != null)
            {
                if (areaAttack != null && monsterData.AreaAttack != null)
                    sensor.Apply(monsterData.AreaAttack.SensorRadius, 0f);
                else
                    sensor.Apply(monsterData.EffectiveDistance, monsterData.Cleavage);
            }

            // BASE MELEE ATTACK
            if (baseAttack != null)
            {
                baseAttack.Construct(heroTransform);
                baseAttack.Damage = dmg;
                baseAttack.AttackAnimSpeed = monsterData.AnimAttackSpeed;
                baseAttack.AttackColdown = monsterData.AttackCooldown;
                baseAttack.Cleavage = monsterData.Cleavage;
                baseAttack.EffectiveDistance = monsterData.EffectiveDistance;
            }

            // KAMIKAZE

            if (kamikaze != null)
            {
                if (baseAttack != null)
                    baseAttack.enabled = false;

                kamikaze.enabled = true;
                kamikaze.Construct(heroTransform);

                kamikaze.Damage = dmg;
                kamikaze.EffectiveDistance = monsterData.EffectiveDistance;

                kamikaze.ExplosionRadius = monsterData.Cleavage; // ✅ ось це

                kamikaze.SetConfig(monsterData.Kamikaze);
            }

            // AREA / RANGED AOE
            if (areaAttack != null)
            {
                if (baseAttack != null)
                    baseAttack.enabled = false;

                areaAttack.enabled = true;
                areaAttack.Construct(heroTransform, _pool);
                areaAttack.SetConfig(monsterData.AreaAttack);
            }
            else if (baseAttack != null && kamikaze == null)
            {
                baseAttack.enabled = true;
            }

            // HEALER

            var healer = monster.GetComponent<EnemyHealer>();
            if (healer != null)
            {
                healer.enabled = true;
                healer.Construct(heroTransform, _popupService);
                healer.SetConfig(monsterData.Healer);
            }

            return monster;
        }


        private void ResetPooledMonster(GameObject monster)
        {
            
            // 1) Re-enable behaviours
            var mover = monster.GetComponent<AgentMoveToPlayer>();
            if (mover != null) mover.enabled = true;

            var healer = monster.GetComponent<EnemyHealer>();
            if (healer != null) healer.enabled = true;

            var anim = monster.GetComponent<EnemyAnimator>();
            if (anim != null)
            {
                anim.Construct(_pool);
                anim.ResetForReuse(); // можна лишити як було, або тут після Construct
            }
            // 3) Reset attacks (IMPORTANT: implement ResetForReuse below)
            var baseAttack = monster.GetComponent<EnemyAttack>();
            if (baseAttack != null) baseAttack.ResetForReuse();

            var kamikaze = monster.GetComponent<KamikazeAttack>();
            if (kamikaze != null) kamikaze.ResetForReuse();

            var area = monster.GetComponent<EnemyAreaAttack>();
            if (area != null) area.ResetForReuse();

            // 4) NavMeshAgent state reset
            var agent = monster.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                if (!agent.enabled)
                    agent.enabled = true;

                agent.isStopped = false;
                agent.ResetPath();

                if (!agent.isOnNavMesh)
                {
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(monster.transform.position, out hit, 2f, NavMesh.AllAreas))
                        agent.Warp(hit.position);
                }
            }
        }

        private float CalcAndApplyRewards(
            MonsterTypeId monsterTypeId,
            MonsterStaticData monsterData,
            GameObject monster,
            bool isElite,
            out float dmg)
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
        private bool TryGetSpawnOnNavMesh(Vector3 desiredPos, int areaMask, out Vector3 result)
        {
            if (NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, 1.5f, areaMask))
            {
                result = hit.position;
                return true;
            }

            result = desiredPos;
            return false;
        }
        private bool TryGetValidSpawnPoint(Vector3 desiredPos, int areaMask, out Vector3 result)
        {
            result = desiredPos;

            if (!NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, 1.5f, areaMask))
                return false;

            result = hit.position;

            int obstacleMask = LayerMask.GetMask("Obstacle");

            if (Physics.CheckSphere(result, 0.5f, obstacleMask, QueryTriggerInteraction.Ignore))
                return false;

            return true;
        }
    }
    
}