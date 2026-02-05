using System;
using UnityEngine;

namespace CodeBase.Infrastructure
{
    public class GameRunner : MonoBehaviour
    {
        public GameBootstrapper GameBootstrapperPrefab;
        private void Awake()
        {
            var bootstrapper = FindAnyObjectByType<GameBootstrapper>();
            
            if (bootstrapper == null)
                Instantiate(GameBootstrapperPrefab);
        }
    }
}