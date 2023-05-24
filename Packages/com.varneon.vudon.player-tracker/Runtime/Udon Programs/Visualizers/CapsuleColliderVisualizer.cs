using UnityEngine;

namespace Varneon.VUdon.PlayerTracker
{
    public class CapsuleColliderVisualizer : ColliderVisualizer
    {
        [SerializeField, HideInInspector]
        private new CapsuleCollider collider;

        [SerializeField, HideInInspector]
        private new LineRenderer renderer;

        public override void Refresh()
        {
            float doubleRadius = collider.radius * 2f;

            renderer.widthMultiplier = doubleRadius;

            renderer.SetPosition(1, new Vector3(0f, 0f, -collider.height + doubleRadius));
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        internal override void InitializeOnBuild()
        {
            collider = transform.parent.GetComponent<CapsuleCollider>();

            renderer = GetComponent<LineRenderer>();
        }
#endif
    }
}
