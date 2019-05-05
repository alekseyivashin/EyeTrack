using System.Collections.Generic;
using System.Linq;
using Tobii.Research;

namespace EyeTrack.tracker
{
    public static class TrackerFinder
    {
        private static List<IEyeTracker> _trackers;

        public static IEnumerable<IEyeTracker> GetAllTrackers()
        {
            _trackers = EyeTrackingOperations.FindAllEyeTrackers().ToList();
            return _trackers;
        }
    }
}
