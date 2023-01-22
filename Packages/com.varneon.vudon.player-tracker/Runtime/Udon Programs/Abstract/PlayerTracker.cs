using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Varneon.VUdon.PlayerTracker.Abstract
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class PlayerTracker : UdonSharpBehaviour
    {
        [Header("Options")]
        [SerializeField]
        private bool headProxyRotationSmoothing;

        [SerializeField, Range(1f, 10f)]
        private float headSmoothingSpeed = 5f;

        [Space, Header("References")]
        [SerializeField]
        private Transform headTracker;

        [SerializeField]
        private Transform rightIndexFingerTracker;

        [SerializeField]
        private Transform leftIndexFingerTracker;

        [SerializeField]
        private Transform rightHandTracker;

        [SerializeField]
        private Transform leftHandTracker;

        private Transform headProxy;

        private Quaternion lastHeadProxyRotation;

        private bool
            trackHead,
            trackHands,
            trackIndexFingers;

        private bool smoothHeadRotation;

        private VRCPlayerApi localPlayer;

        private HumanBodyBones
            leftIndexFingerFurthestBone = HumanBodyBones.LeftIndexIntermediate,
            rightIndexFingerFurthestBone = HumanBodyBones.RightIndexIntermediate;

        private const VRCPlayerApi.TrackingDataType
            TD_TYPE_HEAD = VRCPlayerApi.TrackingDataType.Head,
            TD_TYPE_LEFTHAND = VRCPlayerApi.TrackingDataType.LeftHand,
            TD_TYPE_RIGHTHAND = VRCPlayerApi.TrackingDataType.RightHand;

        private const HumanBodyBones
            BONE_LEFT_INDEX_INTERMEDIATE = HumanBodyBones.LeftIndexIntermediate,
            BONE_RIGHT_INDEX_INTERMEDIATE = HumanBodyBones.RightIndexIntermediate,
            BONE_LEFT_INDEX_DISTAL = HumanBodyBones.LeftIndexDistal,
            BONE_RIGHT_INDEX_DISTAL = HumanBodyBones.RightIndexDistal;

        private const float BONE_CHECK_INTERVAL = 5f;

        private void Start()
        {
            if (trackHead = headTracker)
            {
                if (headTracker.childCount > 0)
                {
                    headProxy = headTracker.GetChild(0);
                }

                smoothHeadRotation = headProxyRotationSmoothing && headProxy != null;
            }

            if ((localPlayer = Networking.LocalPlayer).IsUserInVR())
            {
                trackHands = leftHandTracker && rightHandTracker;

                if (trackIndexFingers = leftIndexFingerTracker && rightIndexFingerTracker)
                {
                    _CheckAvailableBones();
                }
            }
        }

        public override void PostLateUpdate()
        {
            if (trackHead)
            {
                VRCPlayerApi.TrackingData td = localPlayer.GetTrackingData(TD_TYPE_HEAD);

                headTracker.SetPositionAndRotation(td.position, td.rotation);

                if (smoothHeadRotation)
                {
                    lastHeadProxyRotation = Quaternion.RotateTowards(lastHeadProxyRotation, td.rotation, Time.deltaTime * headSmoothingSpeed * Quaternion.Angle(lastHeadProxyRotation, td.rotation));

                    headProxy.rotation = lastHeadProxyRotation;
                }
            }

            if (trackHands)
            {
                VRCPlayerApi.TrackingData leftHandTD = localPlayer.GetTrackingData(TD_TYPE_LEFTHAND);
                VRCPlayerApi.TrackingData rightHandTD = localPlayer.GetTrackingData(TD_TYPE_RIGHTHAND);

                leftHandTracker.SetPositionAndRotation(leftHandTD.position, leftHandTD.rotation);
                rightHandTracker.SetPositionAndRotation(rightHandTD.position, rightHandTD.rotation);
            }

            if (trackIndexFingers)
            {
                leftIndexFingerTracker.position = localPlayer.GetBonePosition(leftIndexFingerFurthestBone);
                rightIndexFingerTracker.position = localPlayer.GetBonePosition(rightIndexFingerFurthestBone);
            }

            OnTrackingPostProcess();
        }

        private protected virtual void OnTrackingPostProcess() { }

        public void _CheckAvailableBones()
        {
            leftIndexFingerFurthestBone = localPlayer.GetBonePosition(BONE_LEFT_INDEX_DISTAL).Equals(Vector3.zero) ? BONE_LEFT_INDEX_INTERMEDIATE : BONE_LEFT_INDEX_DISTAL;
            rightIndexFingerFurthestBone = localPlayer.GetBonePosition(BONE_RIGHT_INDEX_DISTAL).Equals(Vector3.zero) ? BONE_RIGHT_INDEX_INTERMEDIATE : BONE_RIGHT_INDEX_DISTAL;

            SendCustomEventDelayedSeconds(nameof(_CheckAvailableBones), BONE_CHECK_INTERVAL);
        }

        /// <summary>
        /// Gets the tracking transform for provided type
        /// </summary>
        /// <param name="type">Type of tracking transform</param>
        /// <returns></returns>
        [PublicAPI]
        public Transform _GetTrackerTransform(TrackingTransformType type)
        {
            switch (type)
            {
                case TrackingTransformType.HeadRaw: return headTracker;
                case TrackingTransformType.HeadSmooth: return headProxy;
                case TrackingTransformType.HandLeft: return leftHandTracker;
                case TrackingTransformType.HandRight: return rightHandTracker;
                case TrackingTransformType.IndexFingerLeft: return leftIndexFingerTracker;
                case TrackingTransformType.IndexFingerRight: return rightIndexFingerTracker;
                default: return null;
            }
        }
    }
}
