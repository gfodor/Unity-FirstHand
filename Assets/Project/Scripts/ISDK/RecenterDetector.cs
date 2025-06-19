using UnityEngine;
using UnityEngine.XR;

namespace Oculus.Interaction.ComprehensiveSample
{
    /// <summary>
    /// Detects headset recentering by checking if the user has rotated
    /// more than 45 degrees in a single frame.
    /// </summary>
    public class RecenterDetector : MonoBehaviour
    {
        private Pose _lastPose;
        private static float _lastPauseTime;

        private void Start()
        {
            _lastPose = new Pose(InputTracking.GetLocalPosition(XRNode.Head),
                                 InputTracking.GetLocalRotation(XRNode.Head));
            _lastPauseTime = Time.time;
        }

        private void Update()
        {
            var newPose = new Pose(InputTracking.GetLocalPosition(XRNode.Head),
                                   InputTracking.GetLocalRotation(XRNode.Head));

            bool shouldRecenter = Vector3.Angle(
                    (_lastPose.rotation * Vector3.forward).SetY(0).normalized,
                    (newPose.rotation * Vector3.forward).SetY(0).normalized) > 45f;
            shouldRecenter &= Time.time - _lastPauseTime > 1f;
            _lastPose = newPose;
        }

        private void OnApplicationFocus(bool pause)
        {
            _lastPauseTime = Time.time;
        }
    }
}
