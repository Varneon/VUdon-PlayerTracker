using Varneon.VUdon.PlayerTracker.Abstract;

namespace Varneon.VUdon.PlayerTracker
{
    public class InteractTracker : Abstract.InteractTracker
    {
        protected override void OnInteractTrackerEntered(TouchReceiver receiver)
        {
            base.OnInteractTrackerEntered(receiver);
        }

        protected override void OnInteractTrackerExited(TouchReceiver receiver)
        {
            base.OnInteractTrackerExited(receiver);
        }
    }
}
