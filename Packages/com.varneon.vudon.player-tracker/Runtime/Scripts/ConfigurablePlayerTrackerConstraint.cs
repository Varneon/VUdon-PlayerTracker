using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;

namespace Varneon.VUdon.PlayerTracker
{
    [AddComponentMenu("VUdon/Player Tracker/Configurable Player Tracker Constraint")]
    [ExcludeFromPreset]
    public class ConfigurablePlayerTrackerConstraint : MonoBehaviour
    {
        public IConstraint Constraint
        {
            get => constraint as IConstraint;
            set => constraint = value as Behaviour;
        }

        [SerializeField]
        private Behaviour constraint;

        [FormerlySerializedAs("trackerType")]
        public TrackerType TrackerType;

        private void OnValidate()
        {
            if(constraint == null || constraint is IConstraint) { return; }

            constraint = null;

            Debug.LogWarning("Constraint has to derive from UnityEngine.Animations.IConstraint!");
        }
    }
}
