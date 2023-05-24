using UnityEngine;

namespace Varneon.VUdon.PlayerTracker
{
    public class SphereColliderVisualizer : ColliderVisualizer
    {
        [SerializeField, HideInInspector]
        private new SphereCollider collider;

        public override void Refresh()
        {
            transform.localPosition = collider.center;

            transform.localScale = 0.2f * scale * Vector3.one;
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        internal override void InitializeOnBuild()
        {
            collider = transform.parent.GetComponent<SphereCollider>();
        }
#endif
    }
}
