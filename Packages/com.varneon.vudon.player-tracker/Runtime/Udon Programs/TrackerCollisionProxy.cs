using System.Linq;
using UdonSharp;
using UnityEngine;
using Varneon.VUdon.PlayerScaleUtility.Abstract;

namespace Varneon.VUdon.PlayerTracker
{
    [DefaultExecutionOrder(-100000001)] // ConfigurablePlayerTracker - 1
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TrackerCollisionProxy : PlayerScaleCallbackReceiver
    {
        [SerializeField]
        private CapsuleCollider parentCollider;

        [SerializeField, HideInInspector]
        private new CapsuleCollider collider;

        public override void OnPlayerScaleChanged(float newPlayerScale)
        {
            SendCustomEventDelayedFrames(nameof(CalibrateCollider), 0);
        }

        public void CalibrateCollider()
        {
            collider.radius = parentCollider.radius;
            collider.center = parentCollider.center;
            collider.height = parentCollider.height;
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        [UnityEditor.Callbacks.PostProcessScene(-1)]
        private static void InitializeOnBuild()
        {
            GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            System.Collections.Generic.IEnumerable<TrackerCollisionProxy> proxies = roots.SelectMany(r => r.GetComponentsInChildren<TrackerCollisionProxy>(true));

            foreach (TrackerCollisionProxy proxy in proxies)
            {
                proxy.collider = proxy.GetComponent<CapsuleCollider>();
            }
        }
#endif
    }
}
