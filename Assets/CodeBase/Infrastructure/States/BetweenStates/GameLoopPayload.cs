using UnityEngine;

namespace CodeBase.Infrastructure.States.BetweenStates
{
    public readonly struct GameLoopPayload
    {
        public readonly GameObject Hero;
        public readonly GameObject Hud;

        public GameLoopPayload(GameObject hero, GameObject hud)
        {
            Hero = hero;
            Hud = hud;
        }
    }
}