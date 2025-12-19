using UnityEngine;

namespace CodeBase.Services.Input
{
    public interface IInputService
    {
       public Vector2 Axis {get;}
       bool IsAttackButtonUp();

    }
}