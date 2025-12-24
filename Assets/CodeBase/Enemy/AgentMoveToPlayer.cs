using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class AgentMoveToPlayer : MonoBehaviour
    {
        private const float MinimalDistance = 1f;

        public NavMeshAgent Agent;

        private Transform _heroTransform;
        private IGameFactory _gameFactory;

        private void Start()
        {
            _gameFactory = AllServices.Container.Single<IGameFactory>();

            if (_gameFactory.HeroGameObject != null)
                InitializeHeroTransform();
            else
                _gameFactory.HeroCreated += HeroCreated;
        }

        private void OnDestroy()
        {
            if (_gameFactory != null)
                _gameFactory.HeroCreated -= HeroCreated;
        }

        private void Update()
        {
            if (!Initialized())
                return;

            if (Agent == null || !Agent.enabled || !Agent.isOnNavMesh)
                return;

            if (HeroNotReached())
                Agent.SetDestination(_heroTransform.position);
        }

        private bool Initialized() =>
            _heroTransform != null;
        
        private void InitializeHeroTransform()
        {
            _heroTransform = _gameFactory.HeroTransform;
        }

        private void HeroCreated() =>
            InitializeHeroTransform();

        private bool HeroNotReached() =>
            Vector3.Distance(Agent.transform.position, _heroTransform.position) > MinimalDistance;
    }
}
