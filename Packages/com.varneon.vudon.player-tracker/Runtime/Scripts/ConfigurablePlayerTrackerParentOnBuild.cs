using UnityEngine;

namespace Varneon.VUdon.PlayerTracker
{
    [AddComponentMenu("VUdon/Player Tracker/Configurable Player Tracker Parent On Build")]
    [DisallowMultipleComponent]
    [ExcludeFromPreset]
    public class ConfigurablePlayerTrackerParentOnBuild : MonoBehaviour
    {
        public TrackerType Tracker;

        public Vector3 LocalPosition;

        public Vector3 LocalRotation;
    }
}
