// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.XR;

namespace Oculus.Interaction.ComprehensiveSample
{
    /// <summary>
    /// Returns true if the attached collider contains the main camera
    /// </summary>
    public class ContainsPlayerActiveState : MonoBehaviour, IActiveState
    {
        private static Camera _mainCamera;
        private static Transform _origin;

        [SerializeField]
        ActiveStateExpectation _head = ActiveStateExpectation.True;
        [SerializeField]
        ActiveStateExpectation _hands = ActiveStateExpectation.Any;
        [SerializeField]
        ActiveStateExpectation _rig = ActiveStateExpectation.Any;
        [SerializeField]
        private bool _workIfColliderIsDisabled;

        private Collider _collider;

        public bool Active
        {
            get
            {
                if (!_collider) return false;

                bool wasEnabled = _collider.enabled;
                _collider.enabled |= _workIfColliderIsDisabled;

                if (!_collider.enabled) return false;

                try
                {
                    if (!_mainCamera) _mainCamera = Camera.main;
                    if (!_mainCamera) return false;
                    if (_origin == null) _origin = _mainCamera.transform.parent;

                    if (_head != ActiveStateExpectation.Any)
                    {
                        var playerPos = _mainCamera.transform.position;
                        if (!_head.Matches(Contains(playerPos))) return false;
                    }

                    if (_rig != ActiveStateExpectation.Any)
                    {
                        var rigPos = _origin ? _origin.position : _mainCamera.transform.position;
                        if (!_rig.Matches(Contains(rigPos))) return false;
                    }

                    if (_hands != ActiveStateExpectation.Any)
                    {
                        Vector3 localOffset = new Vector3(-0.1f, 0, 0);
                        bool containsAny =
                            Contains(GetNodeWorldPosition(XRNode.LeftHand) + localOffset) ||
                            Contains(GetNodeWorldPosition(XRNode.RightHand) + localOffset);

                        if (!_hands.Matches(containsAny)) return false;
                    }

                    return true;
                }
                finally
                {
                    _collider.enabled = wasEnabled;
                }
            }
        }

        bool Contains(Vector3 point)
        {
            return _collider.IsPointWithinCollider(point);
        }

        private Vector3 GetNodeWorldPosition(XRNode node)
        {
            var localPos = InputTracking.GetLocalPosition(node);
            return _origin ? _origin.TransformPoint(localPos) : localPos;
        }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void OnDrawGizmosSelected()
        {
            _collider = GetComponent<Collider>();
            if (!Active) return;

            Gizmos.color = Color.yellow;

            switch (_collider)
            {
                case BoxCollider box:
                    Gizmos.matrix = Matrix4x4.TRS(box.transform.position, box.transform.rotation, box.transform.lossyScale);
                    Gizmos.DrawWireCube(box.center, box.size);
                    break;
            }
        }
    }
}
