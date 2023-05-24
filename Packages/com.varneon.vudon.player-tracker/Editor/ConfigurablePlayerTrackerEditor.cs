using UnityEditor;
using UnityEngine;
using Varneon.VUdon.Editors.Editor;

namespace Varneon.VUdon.PlayerTracker.Editor
{
    [CustomEditor(typeof(ConfigurablePlayerTracker))]
    public class ConfigurablePlayerTrackerEditor : InspectorBase
    {
        [SerializeField]
        private Texture2D headerIcon;

        private ConfigurablePlayerTracker tracker;

        protected override string FoldoutPersistenceKey => "Varneon/VUdon/PlayerTracker/ConfigurablePlayerTracker/Editor/Foldouts";

        protected override InspectorHeader Header => new InspectorHeaderBuilder()
            .WithIcon(headerIcon)
            .WithTitle("VUdon - Configurable Player Tracker")
            .WithDescription("Configurable drag & drop player tracking solution for adding physical interactions for worlds")
            .WithURL("GitHub", "https://github.com/Varneon/VUdon-PlayerTracker")
            .Build();

        private static readonly Vector2
            TogglePositionOffset = new Vector2(-6f, -12f),
            ToggleSize = new Vector2(20f, 20f),
            DefaultLabelOffset = new Vector2(4f, 0f);

        private const string
            avatarGUID = "f950e9463c219da499826fbb7c22258c";

        private const string
            HEAD_TOOLTIP = "VRCPlayerApi.TrackingDataType.Head.\n\nTracker that always follows the player's HMD.\n\nIf you want to e.g. parent a HUD to the players head, it is recommended to parent it under the smoothed proxy.",
            KNUCKLE_TOOLTIP = "Capsule between two most distant proximal finger bones.",
            INDEX_FINGER_TOOLTIP = "Capsule between intermediate finger bone and extrapolated distal finger bone (approximate finger tip).\n\nIf distal bone doesn't exist, proximal and intermediate will be used instead.",
            FOOT_TOOLTIP = "Capsule between foot and toe bone.\n\nIf toe bone doesn't exist on the avatar, only foot bone will be used.";

        private Rect GetToggleRectFromWorldPoint(Vector3 point)
        {
            return new Rect(HandleUtility.WorldToGUIPoint(point) + TogglePositionOffset, ToggleSize);
        }

        private void DrawLabel(Vector3 point, string text, string tooltip, Vector2 offset, Object selectOnClick)
        {
            GUIContent content = new GUIContent(text, tooltip);

            Rect rect = HandleUtility.WorldPointToSizedRect(point, content, EditorStyles.textArea);

            rect.position += rect.size * offset + DefaultLabelOffset;

            if(GUI.Button(rect, content, EditorStyles.textArea))
            {
                Selection.activeObject = selectOnClick;
            };
        }

        private bool ShouldPointBeVisibleToCamera(Transform cameraTransform, Vector3 point)
        {
            return Vector3.Angle(cameraTransform.forward, point - cameraTransform.position) < 90f;
        }

        protected override void OnEnable()
        {
            tracker = (ConfigurablePlayerTracker)target;

            base.OnEnable();

            TryLoadPreviewAvatarAssets();
        }

        private bool TryLoadPreviewAvatarAssets()
        {
            string avatarPath = AssetDatabase.GUIDToAssetPath(avatarGUID);

            if (string.IsNullOrWhiteSpace(avatarPath)) { return false; }

            tracker.avatarMesh = AssetDatabase.LoadAssetAtPath<Mesh>(avatarPath);

            return true;
        }

        private void OnSceneGUI()
        {
            Transform sceneCameraTransform = SceneView.lastActiveSceneView.camera.transform;

            bool isCameraLookingForward = Quaternion.Angle(sceneCameraTransform.rotation, tracker.transform.rotation) < 90f;

            float leftOffsetMultiplier = isCameraLookingForward ? -1f : 0f;
            float rightOffsetMultiplier = isCameraLookingForward ? 0f : -1f;

            using (Handles.DrawingScope scope = new Handles.DrawingScope())
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;

                if (tracker.headTracker)
                {
                    Vector3 headPos = tracker.headTracker.position;

                    if (ShouldPointBeVisibleToCamera(sceneCameraTransform, headPos))
                    {
                        Vector3 headLabelPos = headPos + new Vector3(0f, HandleUtility.GetHandleSize(headPos), 0f);

                        string headLabelText = string.Concat("[", tracker.headTracker.name, "]\n\nSmooth Proxy Rotation: ", tracker.HeadProxyRotationSmoothing, "\nTrigger Interactions: ", tracker.headTriggersInteractions);

                        Color color = tracker.trackHead ? Color.cyan : Color.grey;

                        Handles.color = color;
                        GUI.color = color;

                        Handles.DrawLine(headPos, headLabelPos);

                        Handles.BeginGUI();
                        DrawLabel(headLabelPos, headLabelText, HEAD_TOOLTIP, new Vector2(-0.5f, -1f), tracker.headTracker);

                        using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
                        {
                            bool trackHead = GUI.Toggle(GetToggleRectFromWorldPoint(headPos), tracker.trackHead, GUIContent.none);

                            if (changeCheckScope.changed)
                            {
                                Undo.RecordObject(tracker, "Toggle Track Head");

                                tracker.trackHead = trackHead;
                            }
                        }

                        Handles.EndGUI();
                    }
                }

                if (tracker.leftHandTracker && tracker.rightHandTracker)
                {
                    Vector3 leftHandPos = tracker.leftHandTracker.position;
                    Vector3 rightHandPos = tracker.rightHandTracker.position;

                    Color color = tracker.trackHands ? Color.cyan : Color.grey;

                    Handles.color = color;
                    GUI.color = color;

                    if(ShouldPointBeVisibleToCamera(sceneCameraTransform, leftHandPos))
                    {
                        float sizeLeft = HandleUtility.GetHandleSize(leftHandPos) / 2f;
                        
                        Vector3 leftHandLabelPos = leftHandPos + new Vector3(0f, sizeLeft, 0f);

                        Handles.DrawLine(leftHandPos, leftHandLabelPos);

                        Handles.BeginGUI();

                        string leftHandLabelText = string.Concat("[", tracker.leftHandTracker.name, "]");

                        DrawLabel(leftHandLabelPos, leftHandLabelText, "VRCPlayerApi.TrackingDataType.LeftHand\n\nUsually an empty transform in case objects need to be parented to the player's controllers", new Vector2(leftOffsetMultiplier, -1f), tracker.leftHandTracker);

                        using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
                        {
                            bool trackHands = GUI.Toggle(GetToggleRectFromWorldPoint(leftHandPos), tracker.trackHands, GUIContent.none);

                            if (changeCheckScope.changed)
                            {
                                Undo.RecordObject(tracker, "Toggle Track Hands");

                                tracker.trackHands = trackHands;
                            }
                        }

                        Handles.EndGUI();
                    }

                    if (ShouldPointBeVisibleToCamera(sceneCameraTransform, rightHandPos))
                    {
                        float sizeRight = HandleUtility.GetHandleSize(rightHandPos) / 2f;

                        Vector3 rightHandLabelPos = rightHandPos + new Vector3(0f, sizeRight, 0f);

                        Handles.DrawLine(rightHandPos, rightHandLabelPos);

                        Handles.BeginGUI();

                        string rightHandLabelText = string.Concat("[", tracker.rightHandTracker.name, "]");

                        DrawLabel(rightHandLabelPos, rightHandLabelText, "VRCPlayerApi.TrackingDataType.RightHand\n\nUsually an empty transform in case objects need to be parented to the player's controllers", new Vector2(rightOffsetMultiplier, -1f), tracker.rightHandTracker);

                        using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
                        {
                            bool trackHands = GUI.Toggle(GetToggleRectFromWorldPoint(rightHandPos), tracker.trackHands, GUIContent.none);

                            if (changeCheckScope.changed)
                            {
                                Undo.RecordObject(tracker, "Toggle Track Hands");

                                tracker.trackHands = trackHands;
                            }
                        }

                        Handles.EndGUI();
                    }
                }

                if(tracker.leftKnuckleTracker && tracker.rightKnuckleTracker)
                {
                    Vector3 leftKnucklePos = tracker.leftKnuckleTracker.position;
                    Vector3 rightKnucklePos = tracker.rightKnuckleTracker.position;

                    Color color = tracker.trackKnuckles ? Color.green : Color.grey;

                    Handles.color = color;
                    GUI.color = color;

                    if(ShouldPointBeVisibleToCamera(sceneCameraTransform, leftKnucklePos))
                    {
                        float sizeLeft = HandleUtility.GetHandleSize(leftKnucklePos) / 2f;

                        Vector3 leftKnuckleLabelPos = leftKnucklePos + new Vector3(0, -sizeLeft, 0f);

                        Handles.DrawLine(leftKnucklePos, leftKnuckleLabelPos);

                        Handles.BeginGUI();

                        string leftKnuckleLabelText = string.Concat("[", tracker.leftKnuckleTracker.name, "]\n\nPhysical collisions: ", tracker.physicalHands);

                        DrawLabel(leftKnuckleLabelPos, leftKnuckleLabelText, KNUCKLE_TOOLTIP, new Vector2(leftOffsetMultiplier, 0f), tracker.leftKnuckleTracker);

                        using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
                        {
                            bool trackKnuckles = GUI.Toggle(GetToggleRectFromWorldPoint(leftKnucklePos), tracker.trackKnuckles, GUIContent.none);

                            if (changeCheckScope.changed)
                            {
                                Undo.RecordObject(tracker, "Toggle Track Knuckles");

                                tracker.trackKnuckles = trackKnuckles;
                            }
                        }

                        Handles.EndGUI();
                    }

                    if (ShouldPointBeVisibleToCamera(sceneCameraTransform, rightKnucklePos))
                    {
                        float sizeRight = HandleUtility.GetHandleSize(rightKnucklePos) / 2f;

                        Vector3 rightKnuckleLabelPos = rightKnucklePos + new Vector3(0, -sizeRight, 0f);

                        Handles.DrawLine(rightKnucklePos, rightKnuckleLabelPos);

                        Handles.BeginGUI();

                        string rightKnuckleLabelText = string.Concat("[", tracker.rightKnuckleTracker.name, "]\n\nPhysical collisions: ", tracker.physicalHands);

                        DrawLabel(rightKnuckleLabelPos, rightKnuckleLabelText, KNUCKLE_TOOLTIP, new Vector2(rightOffsetMultiplier, 0f), tracker.rightKnuckleTracker);

                        using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
                        {
                            bool trackKnuckles = GUI.Toggle(GetToggleRectFromWorldPoint(rightKnucklePos), tracker.trackKnuckles, GUIContent.none);

                            if (changeCheckScope.changed)
                            {
                                Undo.RecordObject(tracker, "Toggle Track Knuckles");

                                tracker.trackKnuckles = trackKnuckles;
                            }
                        }

                        Handles.EndGUI();
                    }
                }

                if(tracker.leftIndexFingerTracker && tracker.rightIndexFingerTracker)
                {
                    Vector3 leftIndexFingerPos = tracker.leftIndexFingerTracker.position;
                    Vector3 rightIndexFingerPos = tracker.rightIndexFingerTracker.position;

                    Color color = tracker.trackIndexFingers ? new Color(1f, 0f, 1f) : Color.grey;

                    Handles.color = color;
                    GUI.color = color;

                    if(ShouldPointBeVisibleToCamera(sceneCameraTransform, leftIndexFingerPos))
                    {
                        Vector3 leftIndexFingerLabelPos = leftIndexFingerPos - new Vector3(Mathf.Clamp01(HandleUtility.GetHandleSize(leftIndexFingerPos)), 0f, 0f);

                        Handles.DrawLine(leftIndexFingerPos, leftIndexFingerLabelPos);

                        Handles.BeginGUI();

                        string leftIndexFingerLabelText = string.Concat("[", tracker.leftIndexFingerTracker.name, "]");

                        DrawLabel(leftIndexFingerLabelPos, leftIndexFingerLabelText, INDEX_FINGER_TOOLTIP, new Vector2(leftOffsetMultiplier, -0.5f), tracker.leftIndexFingerTracker);

                        using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
                        {
                            bool trackIndexFingers = GUI.Toggle(GetToggleRectFromWorldPoint(leftIndexFingerPos), tracker.trackIndexFingers, GUIContent.none);

                            if (changeCheckScope.changed)
                            {
                                Undo.RecordObject(tracker, "Toggle Track IndexFingers");

                                tracker.trackIndexFingers = trackIndexFingers;
                            }
                        }

                        Handles.EndGUI();
                    }

                    if (ShouldPointBeVisibleToCamera(sceneCameraTransform, rightIndexFingerPos))
                    {
                        Vector3 rightIndexFingerLabelPos = rightIndexFingerPos + new Vector3(Mathf.Clamp01(HandleUtility.GetHandleSize(rightIndexFingerPos)), 0f, 0f);

                        Handles.DrawLine(rightIndexFingerPos, rightIndexFingerLabelPos);

                        Handles.BeginGUI();

                        string rightIndexFingerLabelText = string.Concat("[", tracker.rightIndexFingerTracker.name, "]");

                        DrawLabel(rightIndexFingerLabelPos, rightIndexFingerLabelText, INDEX_FINGER_TOOLTIP, new Vector2(rightOffsetMultiplier, -0.5f), tracker.rightIndexFingerTracker);

                        using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
                        {
                            bool trackIndexFingers = GUI.Toggle(GetToggleRectFromWorldPoint(rightIndexFingerPos), tracker.trackIndexFingers, GUIContent.none);

                            if (changeCheckScope.changed)
                            {
                                Undo.RecordObject(tracker, "Toggle Track IndexFingers");

                                tracker.trackIndexFingers = trackIndexFingers;
                            }
                        }

                        Handles.EndGUI();
                    }
                }

                if (tracker.leftFootTracker && tracker.rightFootTracker)
                {
                    Vector3 leftFootPos = tracker.leftFootTracker.position;
                    Vector3 rightFootPos = tracker.rightFootTracker.position;

                    Color color = tracker.trackFeet ? Color.green : Color.grey;

                    Handles.color = color;
                    GUI.color = color;

                    if(ShouldPointBeVisibleToCamera(sceneCameraTransform, leftFootPos))
                    {
                        float sizeLeft = HandleUtility.GetHandleSize(leftFootPos);

                        Vector3 leftFootLabelPos = leftFootPos + new Vector3(-Mathf.Clamp01(sizeLeft), sizeLeft - Mathf.Clamp(-1.5f + sizeLeft * 3f, 0f, float.MaxValue), 0f);

                        Handles.DrawLine(leftFootPos, leftFootLabelPos);

                        Handles.BeginGUI();

                        string leftFootLabelText = string.Concat("[", tracker.leftFootTracker.name, "]\n\nPhysical collisions: ", tracker.physicalFeet);

                        DrawLabel(leftFootLabelPos, leftFootLabelText, FOOT_TOOLTIP, new Vector2(leftOffsetMultiplier, 0f), tracker.leftFootTracker);

                        using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
                        {
                            bool trackFeet = GUI.Toggle(GetToggleRectFromWorldPoint(leftFootPos), tracker.trackFeet, GUIContent.none);

                            if (changeCheckScope.changed)
                            {
                                Undo.RecordObject(tracker, "Toggle Track Feet");

                                tracker.trackFeet = trackFeet;
                            }
                        }

                        Handles.EndGUI();
                    }

                    if (ShouldPointBeVisibleToCamera(sceneCameraTransform, rightFootPos))
                    {
                        float sizeRight = HandleUtility.GetHandleSize(rightFootPos);

                        Vector3 rightFootLabelPos = rightFootPos + new Vector3(Mathf.Clamp01(sizeRight), sizeRight - Mathf.Clamp(-1.5f + sizeRight * 3f, 0f, float.MaxValue), 0f);

                        Handles.DrawLine(rightFootPos, rightFootLabelPos);

                        Handles.BeginGUI();

                        string rightFootLabelText = string.Concat("[", tracker.rightFootTracker.name, "]\n\nPhysical collisions: ", tracker.physicalFeet);

                        DrawLabel(rightFootLabelPos, rightFootLabelText, FOOT_TOOLTIP, new Vector2(rightOffsetMultiplier, 0f), tracker.rightFootTracker);

                        using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
                        {
                            bool trackFeet = GUI.Toggle(GetToggleRectFromWorldPoint(rightFootPos), tracker.trackFeet, GUIContent.none);

                            if (changeCheckScope.changed)
                            {
                                Undo.RecordObject(tracker, "Toggle Track Feet");

                                tracker.trackFeet = trackFeet;
                            }
                        }

                        Handles.EndGUI();
                    }
                }

                GUI.color = Color.white;
            }
        }
    }
}
