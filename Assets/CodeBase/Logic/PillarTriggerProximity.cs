using CodeBase.Enemy;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Logic.Interactor;
using UnityEngine;

namespace CodeBase.Logic
{

    [RequireComponent(typeof(Collider))]
    public class PillarTriggerProximity : MonoBehaviour
    {
        private IPillarActivationService _activation;
        private PillarEncounterSpawner _pillar;

        private IInteractor _insideInteractor;

        private void Awake()
        {
            _activation = AllServices.Container.Single<IPillarActivationService>();
            _pillar = GetComponentInParent<PillarEncounterSpawner>(); // <-- ВАЖЛИВО

            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
                Debug.LogWarning("[PillarTriggerProximity] Collider is not trigger");
        }


        private void OnTriggerEnter(Collider other)
        {
            if (_pillar == null || _pillar.Completed || _pillar.IsRunning)
                return;

            if (other.GetComponent<CharacterController>() == null)
                return;

            IInteractor interactor = other.GetComponentInParent<IInteractor>();
            if (interactor == null) return;

            if (_insideInteractor != null) return;

            _insideInteractor = interactor;
            _activation.SetCurrent(_pillar);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<CharacterController>() == null)
                return;

            IInteractor interactor = other.GetComponentInParent<IInteractor>();
            if (interactor == null) return;

            if (_insideInteractor != interactor)
                return;

            _insideInteractor = null;
            _activation.Clear(_pillar);
        }

    }
}