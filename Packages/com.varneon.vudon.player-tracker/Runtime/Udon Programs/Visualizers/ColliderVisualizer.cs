using UdonSharp;

namespace Varneon.VUdon.PlayerTracker
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class ColliderVisualizer : PlayerScaleUtility.Abstract.PlayerScaleCallbackReceiver
    {
        protected float scale;

        public override void OnPlayerScaleChanged(float newPlayerScale)
        {
            scale = newPlayerScale;

            SendCustomEventDelayedFrames(nameof(Refresh), 0);
        }

        public abstract void Refresh();

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        internal abstract void InitializeOnBuild();
#endif
    }
}
