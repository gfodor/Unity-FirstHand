// Copyright (c) Meta Platforms, Inc. and affiliates.

using Oculus.Interaction.Locomotion;
using System.Collections;
using UnityEngine;

namespace Oculus.Interaction.ComprehensiveSample
{
    /// <summary>
    /// Mirrors the OVRRigs position when it locomotes, then offsets it vertically if seated mode is enabled
    /// </summary>
    public class SeatedMode : MonoBehaviour
    {
        private const string PLAYERPREFS_KEY = "settings.seated_mode";

        [SerializeField]
        [SerializeField]
        private Transform _rigRoot; // Root of the XR rig
        [SerializeField]
        private PlayerLocomotor _locomotor;
        [SerializeField]
        private Transform _averageEyeLevel; //assumed to be a child
        [SerializeField]
        private float _seatedEyeHeight = 1.63f;

        private static bool? _isOn;
        public static bool IsOn
        {
            get
            {
                if (!_isOn.HasValue) _isOn = Store.GetInt(PLAYERPREFS_KEY) > 0;
                return _isOn.Value;
            }
        }

        private IEnumerator Start()
        {
            yield return null; //takes 2 frame for the camera to get its position
            yield return null;
            if (_rigRoot == null && Camera.main)
            {
                _rigRoot = Camera.main.transform.parent;
            }
            _locomotor.WhenLocomotionEventHandled += SyncPosition;
            SyncPosition();
            UpdateCameraRigHeight();
        }

        private void Update()
        {
            var head = Camera.main?.transform;
            if (!head) return;
            var eyePose = PoseUtils.Delta(_rigRoot, head);
            eyePose.position = eyePose.position.SetY(_seatedEyeHeight);
            _averageEyeLevel.SetPose(eyePose, Space.Self);
        }

        private void OnDestroy()
        {
            _locomotor.WhenLocomotionEventHandled -= SyncPosition;
        }

        private void SyncPosition(LocomotionEvent locomotion, Pose _)
        {
            if (locomotion.IsTeleport())
            {
                SyncPosition();
            }
            else if (locomotion.IsSnapTurn())
            {
                if (_rigRoot) transform.rotation = _rigRoot.rotation;
            }
        }

        private void SyncPosition()
        {
            if (_rigRoot) transform.SetPose(_rigRoot.GetPose());
            UpdateCameraRigHeight();
        }

        private void UpdateCameraRigHeight()
        {
            if (IsOn)
            {
                var head = Camera.main?.transform;
                if (!head || !_rigRoot) return;
                float playerHeight = head.position.y - _rigRoot.position.y;
                float diff = Mathf.Max(_seatedEyeHeight - playerHeight, 0);
                _rigRoot.position = transform.position + Vector3.up * diff;
            }
            else
            {
                if (_rigRoot) _rigRoot.SetPose(transform.GetPose());
            }
        }

        public static void SetSeatedMode(bool value)
        {
            _isOn = value;
            Store.SetInt(PLAYERPREFS_KEY, value ? 1 : 0);

            var instances = FindObjectsOfType<SeatedMode>();
            for (int i = 0; i < instances.Length; i++)
            {
                instances[i].UpdateCameraRigHeight();
            }
        }
    }
}
