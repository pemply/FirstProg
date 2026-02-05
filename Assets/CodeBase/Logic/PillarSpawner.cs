using System;
using CodeBase.Enemy;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace CodeBase.Logic
{
    public class PillarSpawner : MonoBehaviour
    {
        private Action<PillarEncounterSpawner> _onPillarCompleted;
        [SerializeField] private GameObject _pillarPrefab;
        [SerializeField] private int _count = 3;
        [SerializeField] private float _minRadius = 12f;
        [SerializeField] private float _maxRadius = 25f;
        [SerializeField] private float _sampleDistance = 20f;

        private Transform _hero;
        private NavMeshQueryFilter _filter;
        private bool _filterReady;
        private bool _spawned;

        public void Construct(Transform hero, Action<PillarEncounterSpawner> onPillarCompleted)
        {
            _onPillarCompleted = onPillarCompleted;
            _hero = hero;

            // беремо agentTypeID з NavMeshSurface в сцені
            var surface = FindObjectOfType<NavMeshSurface>();
            if (surface != null)
            {
                _filter = new NavMeshQueryFilter
                {
                    agentTypeID = surface.agentTypeID,
                    areaMask = NavMesh.AllAreas
                };
                _filterReady = true;
         
            }
            else
            {
                _filterReady = false;
            }
        }

        public void Spawn()
        {
            if (_spawned) return;
            _spawned = true;

            if (_pillarPrefab == null || _hero == null) return;

            int spawned = 0;

            for (int i = 0; i < _count; i++)
            {
                if (TryGetPoint(_hero.position, out var pos))
                {
                    GameObject go = Instantiate(_pillarPrefab, pos, Quaternion.identity);
                    
                    var encounter = go.GetComponent<PillarEncounterSpawner>();
                    if (encounter != null)
                        encounter.Construct(_onPillarCompleted);

                    spawned++;
                }
            }
            
        }


        private bool TryGetPoint(Vector3 center, out Vector3 point)
        {
            for (int i = 0; i < 20; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float r = Random.Range(_minRadius, _maxRadius);
                Vector3 raw = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * r;

                bool ok = _filterReady
                    ? NavMesh.SamplePosition(raw, out var hit, _sampleDistance, _filter)
                    : NavMesh.SamplePosition(raw, out hit, _sampleDistance, NavMesh.AllAreas);

                if (ok)
                {
                    point = hit.position;
                    return true;
                }
            }

            point = default;
            return false;
        }
    }
}