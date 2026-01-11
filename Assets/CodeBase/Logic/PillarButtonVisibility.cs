using CodeBase.Enemy;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.RunTime;
using UnityEngine;

namespace CodeBase.Logic
{
    public class PillarButtonVisibility : MonoBehaviour
    {
    private IPillarActivationService _pillars;

    private void Awake()
    {
        _pillars = AllServices.Container.Single<IPillarActivationService>();
        _pillars.Changed += OnChanged;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_pillars != null)
            _pillars.Changed -= OnChanged;
    }

    private void OnChanged(PillarEncounterSpawner pillar)
    {
        gameObject.SetActive(pillar != null);
    }
}
}