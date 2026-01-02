
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class AgentMoveToPlayer : MonoBehaviour
    {
        private const float MinimalDistance = 1f;

        public NavMeshAgent Agent;

        private Transform _heroTransform;
        
        public void Construct(Transform heroTransform)
        {
            _heroTransform = heroTransform;
            
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
        
      
        private bool HeroNotReached() =>
            Vector3.Distance(Agent.transform.position, _heroTransform.position) > MinimalDistance;
    }
}
