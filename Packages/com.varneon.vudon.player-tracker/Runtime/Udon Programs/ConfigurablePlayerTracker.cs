using JetBrains.Annotations;
using System;
using System.Linq;
using UdonSharp;
using UnityEngine;
using Varneon.VUdon.Editors;
using Varneon.VUdon.Logger.Abstract;
using VRC.SDKBase;

namespace Varneon.VUdon.PlayerTracker
{
    /// <summary>
    /// Configurable utility script for tracking Transforms to the local player's body, e.g. HMD, hand controllers and feet
    /// <para>Supports usage of interact tracker capsules on index fingers, knuckles and feet, providing a solution comparable to VRChat's Avatar Dynamics into worlds</para>
    /// <para>Optionally physical collider proxies can also be enabled as well, allowing players to kick objects in VR fullbody for example</para>
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100000000)] // Noclip + 900000000
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ConfigurablePlayerTracker : PlayerScaleUtility.Abstract.PlayerScaleCallbackReceiver
    {
        [Tooltip("Should the player's head be tracked and tracker transform to be aligned to the same orientation")]
        [FoldoutHeader("Head")]
        [FieldLabel("Track Head TrackingData")]
        [SerializeField]
        internal bool trackHead = true;

        [Tooltip("VRCPlayerApi.TrackingDataType.Head\n\nThis tracker's orientation will always match the player's HMD")]
        [FieldDisable(nameof(trackHead))]
        [FieldNullWarning]
        [SerializeField]
        internal Transform headTracker;

        [Tooltip("A child transform of head tracker which can be smoothed for comfort")]
        [FieldDisable(nameof(trackHead))]
        [FieldNullWarning]
        [SerializeField]
        internal Transform headTrackerSmoothedProxy;

        [Tooltip("Should the head tracker's proxy transform's rotation be smoothed")]
        [FieldLabel("Proxy Rotation Smoothing")]
        [FieldDisable(nameof(trackHead))]
        public bool HeadProxyRotationSmoothing;

        [Tooltip("How fast should the head rotation be smoothed\n\nNote: The default speed of 5 matches VRChat's steadycam in VR")]
        [FieldLabel("Smoothing Speed")]
        [SerializeField]
        [FieldRange(1f, 10f)]
        [FieldDisable(nameof(trackHead), nameof(HeadProxyRotationSmoothing))]
        internal float headSmoothingSpeed = 5f;

        [Tooltip("Should the head collider trigger TouchReceivers")]
        [SerializeField]
        internal bool headTriggersInteractions = true;

        [Tooltip("Should the hand controllers be tracked\n\nNote: This does not provide touch interactivity, only ability to parent objects to player's hands")]
        [FoldoutHeader("Hands (VR Only)")]
        [FieldLabel("Track Hand TrackingData")]
        [SerializeField]
        internal bool trackHands = true;

        [Tooltip("VRCPlayerApi.TrackingDataType.LeftHand\n\nThis Transform will always match player's left hand")]
        [FieldDisable(nameof(trackHands))]
        [FieldNullWarning]
        [SerializeField]
        internal Transform leftHandTracker;

        [Tooltip("VRCPlayerApi.TrackingDataType.RightHand\n\nThis Transform will always match player's right hand")]
        [FieldDisable(nameof(trackHands))]
        [FieldNullWarning]
        [SerializeField]
        internal Transform rightHandTracker;

        [Space]
        [Tooltip("Should the player's index fingers be tracked and trigger capsules to be aligned on them just like with VRChat's Avatar Dynamics\n\nThis provides touch interactivity with compatible implementations in the world utilizing TouchReceiver")]
        [FieldLabel("Track Index Finger Bones")]
        [SerializeField]
        internal bool trackIndexFingers = true;

        [Tooltip("Nearly identical right index finger to VRChat's Avatar Dynamics")]
        [FieldDisable(nameof(trackIndexFingers))]
        [FieldNullWarning]
        [SerializeField]
        internal Transform rightIndexFingerTracker;

        [Tooltip("Nearly identical left index finger to VRChat's Avatar Dynamics")]
        [FieldDisable(nameof(trackIndexFingers))]
        [FieldNullWarning]
        [SerializeField]
        internal Transform leftIndexFingerTracker;

        [Space]
        [Tooltip("Should the player's knuckles be tracked and trigger capsules to be aligned on them just like with VRChat's Avatar Dynamics\n\nThis provides touch interactivity with compatible implementations in the world utilizing TouchReceiver")]
        [FieldLabel("Track Knuckle Bones")]
        [SerializeField]
        internal bool trackKnuckles = true;

        [Tooltip("Nearly identical knuckle collider to VRChat's Avatar Dynamics")]
        [FieldDisable(nameof(trackKnuckles))]
        [FieldNullWarning]
        [SerializeField]
        internal Transform rightKnuckleTracker;

        [Tooltip("Nearly identical knuckle collider to VRChat's Avatar Dynamics")]
        [FieldDisable(nameof(trackKnuckles))]
        [FieldNullWarning]
        [SerializeField]
        internal Transform leftKnuckleTracker;

        [Space]
        [Tooltip("Should the knuckles affect physics objects in the world\n\nNote: Touch interactivity is enabled by default above, this is an extension of it allowing the physics to be manipulated")]
        [FieldLabel("Physical Knuckle Collisions")]
        [FieldDisable(nameof(trackKnuckles))]
        [SerializeField]
        internal bool physicalHands;

        [Tooltip("Physical collision proxy for pushing physics objects with right knuckle")]
        [FieldDisable(nameof(physicalHands), nameof(trackKnuckles))]
        [FieldNullWarning]
        [SerializeField]
        internal Rigidbody rightKnucklePhysicsCollider;

        [Tooltip("Physical collision proxy for pushing physics objects with left knuckle")]
        [FieldDisable(nameof(physicalHands), nameof(trackKnuckles))]
        [FieldNullWarning]
        [SerializeField]
        internal Rigidbody leftKnucklePhysicsCollider;

        [FoldoutHeader("Feet (VR Only)")]
        [Tooltip("Should the player's feet be tracked and trigger capsules to be aligned on them just like with VRChat's Avatar Dynamics\n\nThis provides touch interactivity with compatible implementations in the world utilizing TouchReceiver")]
        [FieldLabel("Track Foot Bones (+ Toes)")]
        [SerializeField]
        internal bool trackFeet = true;

        [Tooltip("Transform to be aligned to the player's right foot")]
        [FieldDisable(nameof(trackFeet))]
        [FieldNullWarning]
        [SerializeField]
        internal Transform rightFootTracker;

        [Tooltip("Transform to be aligned to the player's left foot")]
        [FieldDisable(nameof(trackFeet))]
        [FieldNullWarning]
        [SerializeField]
        internal Transform leftFootTracker;

        [Space]
        [Tooltip("Should the feet affect physics objects in the world\n\nNote: Touch interactivity is enabled by default above, this is an extension of it allowing the physics to be manipulated")]
        [FieldLabel("Physical Foot Collisions")]
        [FieldDisable(nameof(trackFeet))]
        [SerializeField]
        internal bool physicalFeet;

        [Tooltip("Physical collision proxy for kicking physics objects with right foot")]
        [FieldDisable(nameof(physicalFeet), nameof(trackFeet))]
        [FieldNullWarning]
        [SerializeField]
        internal Rigidbody rightFootPhysicsCollider;

        [Tooltip("Physical collision proxy for kicking physics objects with left foot")]
        [FieldDisable(nameof(physicalFeet), nameof(trackFeet))]
        [FieldNullWarning]
        [SerializeField]
        internal Rigidbody leftFootPhysicsCollider;

        [FoldoutHeader("Debug")]
        [Tooltip("Optional VUdon Logger for viewing avatar configuration reports in game")]
        [SerializeField]
        private UdonLogger logger;

        /// <summary>
        /// Tracker visualizers which will be visible for couple seconds every time an avatar changes and the tracker recalibrates
        /// </summary>
        [SerializeField, HideInInspector]
        private GameObject[] visualizers;

        private Quaternion lastHeadProxyRotation;

        /// <summary>
        /// Is the current avatar of the local player not humanoid
        /// </summary>
        private bool isNotHumanoid;

        /// <summary>
        /// Does the current avatar of the local player have a certain bone available
        /// </summary>
        private bool
            hasLeftIndex,
            hasRightIndex,
            hasLeftIndexDistal,
            hasRightIndexDistal,
            hasLeftLittleProximal,
            hasRightLittleProximal;

        private bool smoothHeadRotation;

        private VRCPlayerApi localPlayer;

        private CapsuleCollider
            leftIndexCollider,
            rightIndexCollider,
            leftKnuckleCollider,
            rightKnuckleCollider,
            leftFootCollider,
            rightFootCollider;

        private SphereCollider headCollider;

        private int
            avatarChangeCount,
            currentAvatarChangeId;

        private HumanBodyBones
            leftKnuckleBottomBone = HumanBodyBones.LeftLittleProximal,
            rightKnuckleBottomBone = HumanBodyBones.RightLittleProximal,
            leftFootFurthestBone = HumanBodyBones.LeftFoot,
            rightFootFurthestBone = HumanBodyBones.RightFoot;

        private const VRCPlayerApi.TrackingDataType
            TD_TYPE_HEAD = VRCPlayerApi.TrackingDataType.Head,
            TD_TYPE_LEFTHAND = VRCPlayerApi.TrackingDataType.LeftHand,
            TD_TYPE_RIGHTHAND = VRCPlayerApi.TrackingDataType.RightHand;

        private const HumanBodyBones
            BONE_LEFT_INDEX_INTERMEDIATE = HumanBodyBones.LeftIndexIntermediate,
            BONE_RIGHT_INDEX_INTERMEDIATE = HumanBodyBones.RightIndexIntermediate,
            BONE_LEFT_INDEX_DISTAL = HumanBodyBones.LeftIndexDistal,
            BONE_RIGHT_INDEX_DISTAL = HumanBodyBones.RightIndexDistal;

        private const string LOG_PREFIX = "[<color=#ABC>VUdon</color>][<color=#FF9600>ConfigurablePlayerTracker</color>] ";

        /// <summary>
        /// How long should the tracker visualizers stay visible after recalibrating
        /// </summary>
        private const float VISUALIZER_DURATION = 5f;

        private readonly Vector3 VECTOR3_ZERO = Vector3.zero;

        /// <summary>
        /// Reference to VRCSDK's default robot avatar mesh to be displayed in the editor as an example for where the trackers will be located on the player's body
        /// </summary>
        [NonSerialized]
        internal Mesh avatarMesh;

        private void Start()
        {
            if (trackHead = trackHead && headTracker)
            {
                smoothHeadRotation = HeadProxyRotationSmoothing && headTrackerSmoothedProxy != null;

                headCollider = headTracker.GetComponent<SphereCollider>();
            }

            if ((localPlayer = Networking.LocalPlayer).IsUserInVR())
            {
                trackHands = trackHands && leftHandTracker && rightHandTracker;

                if (trackIndexFingers = trackIndexFingers && leftIndexFingerTracker && rightIndexFingerTracker)
                {
                    leftIndexCollider = leftIndexFingerTracker.GetComponent<CapsuleCollider>();
                    rightIndexCollider = rightIndexFingerTracker.GetComponent<CapsuleCollider>();
                }

                if (trackKnuckles = trackKnuckles && leftKnuckleTracker && rightKnuckleTracker)
                {
                    leftKnuckleCollider = leftKnuckleTracker.GetComponent<CapsuleCollider>();
                    rightKnuckleCollider = rightKnuckleTracker.GetComponent<CapsuleCollider>();
                }

                if (trackFeet = trackFeet && leftFootTracker && rightFootTracker)
                {
                    leftFootCollider = leftFootTracker.GetComponent<CapsuleCollider>();
                    rightFootCollider = rightFootTracker.GetComponent<CapsuleCollider>();
                }

                physicalFeet = trackFeet && leftFootPhysicsCollider && rightFootPhysicsCollider;

                physicalHands = trackHands && leftKnucklePhysicsCollider && rightKnucklePhysicsCollider;
            }
            else
            {
                trackHands = false;

                trackKnuckles = false;

                trackIndexFingers = false;

                trackFeet = false;
            }

            if (trackHead)
            {
                SetGameObjectActive(headTracker, true);
            }

            if (trackHands)
            {
                SetGameObjectActive(leftHandTracker, true);
                SetGameObjectActive(rightHandTracker, true);
            }

            if (trackIndexFingers)
            {
                SetGameObjectActive(leftIndexFingerTracker, true);
                SetGameObjectActive(rightIndexFingerTracker, true);
            }

            if (trackKnuckles)
            {
                SetGameObjectActive(leftKnuckleTracker, true);
                SetGameObjectActive(rightKnuckleTracker, true);

                if (physicalHands)
                {
                    SetGameObjectActive(leftKnuckleCollider, true);
                    SetGameObjectActive(rightKnuckleCollider, true);
                }
            }

            if (trackFeet)
            {
                SetGameObjectActive(leftFootTracker, true);
                SetGameObjectActive(rightFootTracker, true);

                if (physicalFeet)
                {
                    SetGameObjectActive(leftFootCollider, true);
                    SetGameObjectActive(rightFootCollider, true);
                }
            }
        }

        private void FixedUpdate()
        {
            if (physicalHands)
            {
                leftKnucklePhysicsCollider.MovePosition(leftKnuckleTracker.position);
                leftKnucklePhysicsCollider.MoveRotation(leftKnuckleTracker.rotation);

                rightKnucklePhysicsCollider.MovePosition(rightKnuckleTracker.position);
                rightKnucklePhysicsCollider.MoveRotation(rightKnuckleTracker.rotation);
            }

            if (physicalFeet)
            {
                leftFootPhysicsCollider.MovePosition(leftFootTracker.position);
                leftFootPhysicsCollider.MoveRotation(leftFootTracker.rotation);

                rightFootPhysicsCollider.MovePosition(rightFootTracker.position);
                rightFootPhysicsCollider.MoveRotation(rightFootTracker.rotation);
            }
        }

        private void LateUpdate()
        {
            if (trackHead)
            {
                VRCPlayerApi.TrackingData td = localPlayer.GetTrackingData(TD_TYPE_HEAD);

                Quaternion rotation = td.rotation;

                headTracker.SetPositionAndRotation(td.position, rotation);

                if (smoothHeadRotation)
                {
                    lastHeadProxyRotation = Quaternion.RotateTowards(lastHeadProxyRotation, rotation, Time.deltaTime * headSmoothingSpeed * Quaternion.Angle(lastHeadProxyRotation, rotation));

                    headTrackerSmoothedProxy.rotation = lastHeadProxyRotation;
                }
            }

            if (trackHands)
            {
                VRCPlayerApi.TrackingData leftHandTD = localPlayer.GetTrackingData(TD_TYPE_LEFTHAND);
                VRCPlayerApi.TrackingData rightHandTD = localPlayer.GetTrackingData(TD_TYPE_RIGHTHAND);

                leftHandTracker.SetPositionAndRotation(leftHandTD.position, leftHandTD.rotation);
                rightHandTracker.SetPositionAndRotation(rightHandTD.position, rightHandTD.rotation);
            }
        }

        public override void PostLateUpdate()
        {
            if (isNotHumanoid) { return; }

            Vector3 leftIndexProximal = localPlayer.GetBonePosition(HumanBodyBones.LeftIndexProximal);
            Vector3 rightIndexProximal = localPlayer.GetBonePosition(HumanBodyBones.RightIndexProximal);

            if (trackIndexFingers)
            {
                Vector3 intermediate, distal;

                Quaternion curlAngle;

                if (hasRightIndex)
                {
                    if (hasRightIndexDistal)
                    {
                        intermediate = localPlayer.GetBonePosition(HumanBodyBones.RightIndexIntermediate);
                        distal = localPlayer.GetBonePosition(HumanBodyBones.RightIndexDistal);

                        curlAngle = GetCurlAngle(rightIndexProximal, intermediate, distal);

                        CalculateFingerTracker(rightIndexFingerTracker, intermediate, distal, curlAngle);
                    }
                    else
                    {
                        intermediate = localPlayer.GetBonePosition(HumanBodyBones.RightIndexProximal);
                        distal = localPlayer.GetBonePosition(HumanBodyBones.RightIndexIntermediate);

                        //curlAngle = GetCurlAngle(intermediate, distal, distal + localPlayer.GetBoneRotation(HumanBodyBones.RightIndexIntermediate) * Vector3.forward);

                        CalculateFingerTracker(rightIndexFingerTracker, intermediate, distal, Quaternion.identity);
                    }
                }

                if (hasLeftIndex)
                {
                    if (hasLeftIndexDistal)
                    {
                        intermediate = localPlayer.GetBonePosition(HumanBodyBones.LeftIndexIntermediate);
                        distal = localPlayer.GetBonePosition(HumanBodyBones.LeftIndexDistal);

                        curlAngle = GetCurlAngle(leftIndexProximal, intermediate, distal);

                        CalculateFingerTracker(leftIndexFingerTracker, intermediate, distal, curlAngle);
                    }
                    else
                    {
                        intermediate = localPlayer.GetBonePosition(HumanBodyBones.LeftIndexProximal);
                        distal = localPlayer.GetBonePosition(HumanBodyBones.LeftIndexIntermediate);

                        //curlAngle = GetCurlAngle(intermediate, distal, distal + localPlayer.GetBoneRotation(HumanBodyBones.LeftIndexIntermediate) * Vector3.forward);

                        CalculateFingerTracker(leftIndexFingerTracker, intermediate, distal, Quaternion.identity);
                    }
                }
            }

            if (trackKnuckles)
            {
                leftKnuckleTracker.SetPositionAndRotation(leftIndexProximal, Quaternion.LookRotation(leftIndexProximal - localPlayer.GetBonePosition(leftKnuckleBottomBone)));

                rightKnuckleTracker.SetPositionAndRotation(rightIndexProximal, Quaternion.LookRotation(rightIndexProximal - localPlayer.GetBonePosition(rightKnuckleBottomBone)));
            }

            if (trackFeet)
            {
                Vector3 leftFootFurthestPoint = localPlayer.GetBonePosition(leftFootFurthestBone);
                Vector3 rightFootFurthestPoint = localPlayer.GetBonePosition(rightFootFurthestBone);

                leftFootTracker.SetPositionAndRotation(leftFootFurthestPoint, Quaternion.LookRotation(leftFootFurthestPoint - localPlayer.GetBonePosition(HumanBodyBones.LeftFoot)));
                rightFootTracker.SetPositionAndRotation(rightFootFurthestPoint, Quaternion.LookRotation(rightFootFurthestPoint - localPlayer.GetBonePosition(HumanBodyBones.RightFoot)));
            }
        }

        private static Quaternion GetCurlAngle(Vector3 proximal, Vector3 intermediate, Vector3 distal)
        {
            return Quaternion.FromToRotation(proximal - intermediate, intermediate - distal);
        }

        private static Vector3 ExtrapolateDistal(Vector3 intermediate, Vector3 distal, Quaternion curlAngle)
        {
            return distal + curlAngle * (distal - intermediate);
        }

        public void CheckAvailableBones()
        {
            isNotHumanoid = localPlayer.GetBonePosition(HumanBodyBones.Hips).Equals(VECTOR3_ZERO);

            // Player is not humanoid and bones can't be tracked
            if (isNotHumanoid)
            {
                LogWarning("Avatar is not humanoid! Disabling humanoid tracking...");

                return;
            }

            hasLeftIndex = !localPlayer.GetBonePosition(BONE_LEFT_INDEX_INTERMEDIATE).Equals(VECTOR3_ZERO);
            hasRightIndex = !localPlayer.GetBonePosition(BONE_RIGHT_INDEX_INTERMEDIATE).Equals(VECTOR3_ZERO);

            hasLeftIndexDistal = !localPlayer.GetBonePosition(BONE_LEFT_INDEX_DISTAL).Equals(VECTOR3_ZERO);
            hasRightIndexDistal = !localPlayer.GetBonePosition(BONE_RIGHT_INDEX_DISTAL).Equals(VECTOR3_ZERO);

            hasLeftLittleProximal = !localPlayer.GetBonePosition(HumanBodyBones.LeftLittleProximal).Equals(VECTOR3_ZERO);
            hasRightLittleProximal = !localPlayer.GetBonePosition(HumanBodyBones.RightLittleProximal).Equals(VECTOR3_ZERO);

            leftKnuckleBottomBone = hasLeftLittleProximal ? HumanBodyBones.LeftLittleProximal : HumanBodyBones.LeftRingProximal;
            rightKnuckleBottomBone = hasRightLittleProximal ? HumanBodyBones.RightLittleProximal : HumanBodyBones.RightRingProximal;

            bool hasLeftToes = !localPlayer.GetBonePosition(HumanBodyBones.LeftToes).Equals(VECTOR3_ZERO);
            bool hasRightToes = !localPlayer.GetBonePosition(HumanBodyBones.RightToes).Equals(VECTOR3_ZERO);

            leftFootFurthestBone = hasLeftToes ? HumanBodyBones.LeftToes : HumanBodyBones.LeftFoot;
            rightFootFurthestBone = hasRightToes ? HumanBodyBones.RightToes : HumanBodyBones.RightFoot;

            Log($"Tracker calibration complete!\n\tHasIndexDistals: {ToLogString(hasLeftIndexDistal && hasRightIndexDistal)}\n\tHasLittleProximal: {ToLogString(hasLeftLittleProximal && hasRightLittleProximal)}\n\tHasToes: {ToLogString(hasLeftToes && hasRightToes)}");
        }

        public override void OnPlayerScaleChanged(float newPlayerScale)
        {
            CheckAvailableBones();

            // Player is not humanoid and bones can't be tracked
            if (isNotHumanoid) { return; }

            float fingerRadius = newPlayerScale * 0.01f;

            float rightDistance, leftDistance;

            if (trackIndexFingers)
            {
                rightDistance = Vector3.Distance(localPlayer.GetBonePosition(HumanBodyBones.RightIndexIntermediate), localPlayer.GetBonePosition(hasRightIndexDistal ? HumanBodyBones.RightIndexDistal : HumanBodyBones.RightIndexProximal));
                leftDistance = Vector3.Distance(localPlayer.GetBonePosition(HumanBodyBones.LeftIndexIntermediate), localPlayer.GetBonePosition(hasLeftIndexDistal ? HumanBodyBones.LeftIndexDistal : HumanBodyBones.LeftIndexProximal));

                Log("Apply Index Finger Collider Properties");

                ApplyFingerColliderProperties(leftIndexCollider, fingerRadius, leftDistance * 2f);
                ApplyFingerColliderProperties(rightIndexCollider, fingerRadius, rightDistance * 2f);
            }

            if (trackKnuckles)
            {
                rightDistance = Vector3.Distance(localPlayer.GetBonePosition(HumanBodyBones.RightIndexProximal), localPlayer.GetBonePosition(rightKnuckleBottomBone));
                leftDistance = Vector3.Distance(localPlayer.GetBonePosition(HumanBodyBones.LeftIndexProximal), localPlayer.GetBonePosition(leftKnuckleBottomBone));

                Log("Apply Knuckle Collider Properties");

                ApplyFingerColliderProperties(leftKnuckleCollider, fingerRadius * 2f, leftDistance);
                ApplyFingerColliderProperties(rightKnuckleCollider, fingerRadius * 2f, rightDistance);
            }

            if (trackFeet)
            {
                rightDistance = Vector3.Distance(localPlayer.GetBonePosition(HumanBodyBones.RightFoot), localPlayer.GetBonePosition(rightFootFurthestBone));
                leftDistance = Vector3.Distance(localPlayer.GetBonePosition(HumanBodyBones.LeftFoot), localPlayer.GetBonePosition(leftFootFurthestBone));

                float footRadius = newPlayerScale * 0.05f;

                Log("Apply Foot Collider Properties");

                ApplyFingerColliderProperties(leftFootCollider, footRadius, leftDistance);
                ApplyFingerColliderProperties(rightFootCollider, footRadius, rightDistance);
            }

            if (trackHead && headCollider)
            {
                headCollider.radius = newPlayerScale * 0.1f;
                headCollider.center = new Vector3(0f, -0.02f, -0.08f) * newPlayerScale;
            }

            EnableVisualizers();
        }

        private static void ApplyFingerColliderProperties(CapsuleCollider collider, float radius, float distance)
        {
            collider.radius = radius;

            collider.height = distance + radius * 2f;

            collider.center = new Vector3(0f, 0f, -distance / 2f);
        }

        private static void CalculateFingerTracker(Transform tracker, Vector3 intermediate, Vector3 distal, Quaternion curlAngle)
        {
            Vector3 extrapolatedDistal = ExtrapolateDistal(intermediate, distal, curlAngle);

            tracker.SetPositionAndRotation(extrapolatedDistal, Quaternion.LookRotation(extrapolatedDistal - intermediate));
        }

        private void SetGameObjectActive(Component component, bool active)
        {
            if(component == null) { return; }

            component.gameObject.SetActive(active);
        }

        private void EnableVisualizers()
        {
            avatarChangeCount++;

            SetVisualizersActive(true);

            SendCustomEventDelayedSeconds(nameof(TryDisableVisualizers), VISUALIZER_DURATION);
        }

        public void TryDisableVisualizers()
        {
            if(++currentAvatarChangeId < avatarChangeCount) { return; }

            SetVisualizersActive(false);
        }

        private void SetVisualizersActive(bool active)
        {
            foreach(GameObject visualizer in visualizers)
            {
                visualizer.SetActive(active);
            }
        }

        [PublicAPI]
        public bool TryGetTracker(TrackerType type, out Transform tracker)
        {
            switch (type)
            {
                case TrackerType.Head: tracker = headTracker; break;
                case TrackerType.HandLeft: tracker = leftHandTracker; break;
                case TrackerType.HandRight: tracker = rightHandTracker; break;
                case TrackerType.IndexFingerLeft: tracker = leftIndexFingerTracker; break;
                case TrackerType.IndexFingerRight: tracker = rightIndexFingerTracker; break;
                case TrackerType.FootLeft: tracker = leftFootTracker; break;
                case TrackerType.FootRight: tracker = rightFootTracker; break;
                default:
                    Debug.LogWarningFormat("{0}TrackerType '<color=#FEDCBA>{1}</color>' is not compatible with ConfigurablePlayerTracker!", LOG_PREFIX, type);
                    tracker = null;
                    break;
            }

            return tracker != null;
        }

        private void Log(string message)
        {
            if (logger)
            {
                logger.Log(string.Concat(LOG_PREFIX, message));
            }
        }

        private void LogWarning(string message)
        {
            if (logger)
            {
                logger.LogWarning(string.Concat(LOG_PREFIX, message));
            }
        }

        private static string ToLogString(bool value) => value ? "<color=#00FF00>True</color>" : "<color=#FF0000>False</color>";

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void OnDrawGizmosSelected()
        {
            if(avatarMesh)
            {
                Color originalColor = Gizmos.color;

                Gizmos.color = new Color(0.25f, 0.5f, 0.75f, 0.25f);

                Gizmos.DrawMesh(avatarMesh, 0, transform.position, transform.rotation);
                Gizmos.DrawMesh(avatarMesh, 1, transform.position, transform.rotation);

                Gizmos.color = originalColor;
            }
        }

        [UnityEditor.Callbacks.PostProcessScene(-1)]
        private static void InitializeOnBuild()
        {
            GameObject[] sceneRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (ConfigurablePlayerTracker tracker in sceneRoots.SelectMany(r => r.GetComponentsInChildren<ConfigurablePlayerTracker>(true)))
            {
                if (!tracker.headTriggersInteractions)
                {
                    Transform headTracker = tracker.headTracker;

                    DestroyImmediate(headTracker.GetComponent<InteractTracker>());
                    DestroyImmediate(headTracker.GetComponent<Collider>());
                    DestroyImmediate(headTracker.GetComponent<Rigidbody>());
                }

                ColliderVisualizer[] visualizers = tracker.GetComponentsInChildren<ColliderVisualizer>(true);

                foreach(ColliderVisualizer visualizer in visualizers)
                {
                    visualizer.InitializeOnBuild();
                }

                tracker.visualizers = visualizers.Select(v => v.gameObject).ToArray();

                for(int i = 0; i < tracker.transform.childCount; i++)
                {
                    tracker.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
#endif
    }
}
