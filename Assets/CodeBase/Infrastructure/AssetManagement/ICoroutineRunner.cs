using System.Collections;
using UnityEngine;

namespace CodeBase.Infrastructure.AssetManagement
{
    public interface ICoroutineRunner
    {
        Coroutine StartCoroutine(IEnumerator coroutine);
    }
}