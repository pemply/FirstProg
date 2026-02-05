using UnityEngine;

namespace CodeBase.Services.Input
{
    public abstract class InputService : IInputService
    {
        protected const string Horizontal = "Horizontal";
        protected const string Vertical = "Vertical";
        private const string Button = "Fire1";

        public abstract Vector2 Axis { get; }

        public bool IsAttackButtonUp() => 
            SimpleInput.GetButtonUp(Button);
        
        public virtual bool IsInteractButtonUp() =>
            false; 
        
        protected static Vector2 SimpleInputAxis() => 
            new(SimpleInput.GetAxis(Horizontal), SimpleInput.GetAxis(Vertical));
    }
}