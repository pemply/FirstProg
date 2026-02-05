using UnityEngine;

namespace CodeBase.Logic.Interactor
{
    public class HeroInteractor : MonoBehaviour, IInteractor
    {
        public Transform Transform => transform;
    }
    
    public interface IInteractor
    {
        Transform Transform { get; }
    }
}