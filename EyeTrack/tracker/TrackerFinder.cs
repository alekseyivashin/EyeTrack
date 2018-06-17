using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static IEyeTracker GetByName(string name)
        {
            return _trackers.Find(tracker => tracker.DeviceName == name);
        }
    }
}
