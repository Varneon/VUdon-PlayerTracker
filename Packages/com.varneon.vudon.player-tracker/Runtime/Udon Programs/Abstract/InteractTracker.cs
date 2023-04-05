using System.Diagnostics.CodeAnalysis;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Varneon.VUdon.PlayerTracker.Abstract
{
    /// <summary>
    /// Abstract tracker for physical interactions
    /// </summary>
    [DefaultExecutionOrder(-2146483648)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class InteractTracker : UdonSharpBehaviour
    {
        /// <summary>
        /// Type of this tracker
        /// </summary>
        public TrackerType TrackerType;

        [SuppressMessage("Performance", "UNT0026:GetComponent always allocates", Justification = "<Pending>")]
        public void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other) || other == null) { return; }

            TouchReceiver receiver = other.GetComponent<TouchReceiver>();

            if (receiver == null) { return; }

            OnInteractTrackerEntered(receiver);
        }

        [SuppressMessage("Performance", "UNT0026:GetComponent always allocates", Justification = "<Pending>")]
        public void OnTriggerExit(Collider other)
        {
            if (!Utilities.IsValid(other) || other == null) { return; }

            TouchReceiver receiver = other.GetComponent<TouchReceiver>();

            if (receiver == null) { return; }

            OnInteractTrackerExited(receiver);
        }

        protected virtual void OnInteractTrackerEntered(TouchReceiver receiver)
        {
            receiver._OnInteractTrackerEntered(TrackerType, transform);
        }

        protected virtual void OnInteractTrackerExited(TouchReceiver receiver)
        {
            receiver._OnInteractTrackerExited(TrackerType);
        }
    }
}
