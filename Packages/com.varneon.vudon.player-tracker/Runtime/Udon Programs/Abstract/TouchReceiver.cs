using System.Diagnostics.CodeAnalysis;
using UdonSharp;
using UnityEngine;

namespace Varneon.VUdon.PlayerTracker.Abstract
{
    /// <summary>
    /// Abstract receiver for physical interactions
    /// </summary>
    public abstract class TouchReceiver : UdonSharpBehaviour
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        public abstract void _OnInteractTrackerEntered(TrackerType type, Transform tracker);

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        public abstract void _OnInteractTrackerExited(TrackerType type);
    }
}
