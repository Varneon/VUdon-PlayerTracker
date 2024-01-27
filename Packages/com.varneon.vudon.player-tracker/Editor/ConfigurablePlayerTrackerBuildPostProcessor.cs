using JetBrains.Annotations;
using System.Linq;
using UnityEditor.Callbacks;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Varneon.VUdon.PlayerTracker.Editor
{
    public static class ConfigurablePlayerTrackerBuildPostProcessor
    {
        [UsedImplicitly]
        [PostProcessScene(-1)]
        private static void PostProcessConfigurablePlayerTrackerOnBuild()
        {
            GameObject[] sceneRoots = SceneManager.GetActiveScene().GetRootGameObjects();

            ConfigurablePlayerTracker playerTracker = sceneRoots.SelectMany(r => r.GetComponentsInChildren<ConfigurablePlayerTracker>(true)).FirstOrDefault();

            if (playerTracker == null) { return; }

            foreach (ConfigurablePlayerTrackerConstraint constraint in sceneRoots.SelectMany(r => r.GetComponentsInChildren<ConfigurablePlayerTrackerConstraint>(true)))
            {
                if (constraint.Constraint == null)
                {
                    Debug.LogError("[<color=#ABC>VUdon</color>][<color=#FF9600>ConfigurablePlayerTracker</color>] Constraint hasn't been assigned to ConfigurablePlayerTrackerConstraint!", constraint.gameObject);

                    Object.DestroyImmediate(constraint);

                    continue;
                }

                if (!playerTracker.TryGetTracker(constraint.TrackerType, out Transform tracker))
                {
                    Debug.LogErrorFormat(constraint.gameObject, "[<color=#ABC>VUdon</color>][<color=#FF9600>ConfigurablePlayerTracker</color>] Unable to get tracker of type '<color=#FEDCBA>{0}</color>' from ConfigurablePlayerTracker!", constraint.TrackerType);

                    Object.DestroyImmediate(constraint);

                    continue;
                }

                constraint.Constraint.AddSource(new ConstraintSource() { sourceTransform = tracker, weight = 1f });

                Object.DestroyImmediate(constraint);
            }

            foreach (ConfigurablePlayerTrackerParentOnBuild constraint in sceneRoots.SelectMany(r => r.GetComponentsInChildren<ConfigurablePlayerTrackerParentOnBuild>(true)))
            {
                if (!playerTracker.TryGetTracker(constraint.Tracker, out Transform tracker))
                {
                    Debug.LogErrorFormat(constraint.gameObject, "[<color=#ABC>VUdon</color>][<color=#FF9600>ConfigurablePlayerTracker</color>] Unable to get tracker of type '<color=#FEDCBA>{0}</color>' from ConfigurablePlayerTracker!", constraint.Tracker);

                    Object.DestroyImmediate(constraint);

                    continue;
                }

                constraint.transform.parent = tracker;

                constraint.transform.localPosition = constraint.LocalPosition;

                constraint.transform.localEulerAngles = constraint.LocalRotation;

                Object.DestroyImmediate(constraint);
            }
        }
    }
}
