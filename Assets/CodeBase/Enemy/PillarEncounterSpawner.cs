using System;
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
        private Action<PillarEncounterSpawner> _onCompletedReward;

        [SerializeField] private GameObject _prompt;

        [Header("Encounter")] 
        public MonsterTypeId MonsterTypeId;
        public int TotalToSpawn = 10;
        public int MaxAlive = 4;
        public float SpawnInterval = 0.7f;

        [Header("Spawn on NavMesh around pillar")]
        public float MinRadius = 3f;
        public float MaxRadius = 7f;

        [Header("Reward placeholder")] 
        public int RewardXp = 200;

        public bool Completed = false;
        public string Id { get; set; }

        private IGameFactory _factory;
        private readonly List<EnemyDeath> _tracked = new();

        public bool IsRunning => _running;

        private int _spawned;
        private int _alive;
        private bool _running;

        private Coroutine _loop;
        private bool _completedSent;

        public void Construct(Action<PillarEncounterSpawner> onCompletedReward)
        {
            _onCompletedReward = onCompletedReward;
        }

        private void Awake()
        {
         

            var uid = GetComponent<UniqueId>();
            if (uid == null)
            {
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
        }

        public void StartEncounter()
        {
            if (Completed || _running) return;

            _running = true;
            _spawned = 0;
            _alive = 0;

            if (_loop != null)
                StopCoroutine(_loop);

            _loop = StartCoroutine(SpawnLoop());
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

            while (_alive > 0)
                yield return null;

            Complete();
        }

        private void SpawnOne()
        {
            Vector3 pos = PickNavMeshPoint(transform.position);

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
        }

        private Vector3 PickNavMeshPoint(Vector3 center)
        {
            for (int i = 0; i < 20; i++)
            {
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float r = UnityEngine.Random.Range(MinRadius, MaxRadius);
                Vector3 raw = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * r;

                if (NavMesh.SamplePosition(raw, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    return hit.position;
            }

            return center;
        }

        private void Complete()
        {
            if (_completedSent) return;
            _completedSent = true;

            Completed = true;
            _running = false;

            if (_loop != null)
            {
                StopCoroutine(_loop);
                _loop = null;
            }

            CleanupSubscriptions();

            _onCompletedReward?.Invoke(this);
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

        public void SetPrompt(bool on)
        {
            if (_prompt != null)
                _prompt.SetActive(on);
        }
    }
}
