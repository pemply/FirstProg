using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Services.Input;
using UnityEngine;

namespace CodeBase.Logic
{
    public class PillarActivateFromInput : MonoBehaviour
    {
        private IInputService _input;
        private IPillarActivationService _pillars;

        private void Awake()
        {
            
            _input = AllServices.Container.Single<IInputService>();
            _pillars = AllServices.Container.Single<IPillarActivationService>();
        }


        private void Update()
        {
            if (!(_input.IsInteractButtonUp() || _input.IsAttackButtonUp()))
                return;

            var pillar = _pillars.Current;
            if (pillar == null)
                return;

            pillar.StartEncounter();
            _pillars.Clear(pillar);
        }

    }
}