using UnityEngine;

namespace CodeBase.Logic
{
    public class AutoGroundVisual : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private float _extraLift = 0f; // якщо треба трошки підняти над землею

        private void Awake()
        {
            if (_visualRoot == null)
                return;

            // Зібрати всі Renderer-и візуалу
            var renderers = _visualRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
                return;

            // Об'єднати bounds у WORLD space
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);

            // Нижня точка моделі (world)
            float minY = b.min.y;

            // Y root (world)
            float rootY = transform.position.y;

            // Скільки треба підняти, щоб minY став на rootY
            float delta = (rootY - minY) + _extraLift;

            // Піднімаємо visualRoot (world shift)
            _visualRoot.position += Vector3.up * delta;
        }
    }
}