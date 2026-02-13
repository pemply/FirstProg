using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.StaticData;
using CodeBase.StaticData.CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class HeroFactory
    {
        private readonly RunContextService _run;
        private readonly IStaticDataService _staticData;

        public HeroFactory(RunContextService run, IStaticDataService staticData)
        {
            _run = run;
            _staticData = staticData;
        }

        public GameObject CreateHero(GameObject at, out Transform heroTransform)
        {
            HeroId heroId = _run.SelectedHeroId;

            HeroConfig cfg = _staticData.ForHero(heroId);

            if (cfg == null)
            {
                Debug.LogError($"[HeroFactory] HeroConfig not found for id='{heroId}'. Check Resources/StaticData/Heroes and Id values.");
                heroTransform = at != null ? at.transform : null;
                return null;
            }

            if (cfg.Prefab == null)
            {
                Debug.LogError($"[HeroFactory] HeroConfig '{cfg.Id}' has no Prefab assigned.");
                heroTransform = at != null ? at.transform : null;
                return null;
            }

            Vector3 pos = at != null ? at.transform.position : Vector3.zero;
            GameObject hero = Object.Instantiate(cfg.Prefab, pos, Quaternion.identity);

            var cc = hero.GetComponentInChildren<CharacterController>();
            heroTransform = cc != null ? cc.transform : hero.transform;

            return hero;
        }
    }
}