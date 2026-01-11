using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using CodeBase.StaticData;
using UnityEngine;
using UnityEngine.AI;


namespace CodeBase.Enemy
{

    public class PillarEncounterSpawner : MonoBehaviour, ISavedProgress
    {
        [Header("Encounter")] public MonsterTypeId MonsterTypeId;
        public int TotalToSpawn = 10;
        public int MaxAlive = 4;
        public float SpawnInterval = 0.7f;

        [Header("Spawn on NavMesh around pillar")]
        public float MinRadius = 3f;

        public float MaxRadius = 7f;

        [Header("Reward placeholder")] public int RewardXp = 200;

        public bool Completed;
        public string Id { get; set; }

        private IGameFactory _factory;
        private readonly List<EnemyDeath> _tracked = new();
        public bool IsRunning => _running;

        private int _spawned;
        private int _alive;
        private bool _running;

        private void Awake()
        {
            var uid = GetComponent<UniqueId>();
            if (uid == null)
            {
                Debug.LogError("[PillarEncounterSpawner] UniqueId missing on Pillar prefab");
                return;
            }

            Id = uid.Id;
            _factory = AllServices.Container.Single<IGameFactory>();
        }

        public void LoadProgress(PlayerProgress progress)
        {
            if (Completed) return;

            if (progress.KillData.ClearedPillars.Contains(Id))
            {
                Completed = true;
                return;
            }

            // нічого не спавнимо на старті — чекаємо активації кнопкою
        }

        // виклич з PillarInteract по кнопці
        public void StartEncounter()
        {
            if (Completed || _running) return;

            _running = true;
            _spawned = 0;
            _alive = 0;

            StartCoroutine(SpawnLoop());
        }

        private System.Collections.IEnumerator SpawnLoop()
        {
            while (_spawned < TotalToSpawn)
            {
                if (_alive < MaxAlive)
                {
                    SpawnOne();
                    _spawned++;
                    _alive++;
                }

                yield return new WaitForSeconds(SpawnInterval);
            }

            // чекаємо поки всіх доб'ють
            while (_alive > 0)
                yield return null;

            Complete();
        }

        private void SpawnOne()
        {
            Vector3 pos = PickNavMeshPoint(transform.position);

            // parent можеш ставити null або enemiesRoot
            GameObject monster = _factory.CreateMonster(MonsterTypeId, null);
            monster.transform.position = pos;

            var death = monster.GetComponent<EnemyDeath>();
            if (death == null)
            {
                Debug.LogError("[PillarEncounter] EnemyDeath missing, alive will desync");
                _alive = Mathf.Max(0, _alive - 1);
                return;
            }

            _tracked.Add(death);
            death.DeathEvent += OnEnemyDied;
        }

        private void OnEnemyDied()
        {
            _alive = Mathf.Max(0, _alive - 1);
            // ВАЖЛИВО: тут ми не знаємо який саме death викликав (бо event без sender)
            // Тому просто зменшуємо alive. Для MVP цього достатньо.
        }

        private Vector3 PickNavMeshPoint(Vector3 center)
        {
            for (int i = 0; i < 20; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float r = Random.Range(MinRadius, MaxRadius);
                Vector3 raw = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * r;

                if (NavMesh.SamplePosition(raw, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    return hit.position;
            }

            return center;
        }

        private void Complete()
        {
            Completed = true;
            _running = false;
     

            _factory.CreateXpPickup(transform.position + Vector3.up * 0.5f, RewardXp);

            CleanupSubscriptions();
            Debug.Log($"[PillarEncounter] Completed pillar {Id}");
        }

        private void CleanupSubscriptions()
        {
            foreach (var d in _tracked)
                if (d != null)
                    d.DeathEvent -= OnEnemyDied;

            _tracked.Clear();
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            if (Completed && !progress.KillData.ClearedPillars.Contains(Id))
                progress.KillData.ClearedPillars.Add(Id);
        }

        private void OnDestroy() => CleanupSubscriptions();
    }
}