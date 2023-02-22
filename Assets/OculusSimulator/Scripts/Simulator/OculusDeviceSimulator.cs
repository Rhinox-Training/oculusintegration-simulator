using System;
using System.Linq;

using UnityEngine;
//using UnityEngine.InputSystem.XR;
//using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using static OVRPlugin;
//using OVR.OpenVR;
//using static OVRPlugin;

namespace Rhinox.XR.Oculus.Simulator
{
    public class OculusDeviceSimulator : MonoBehaviour
    {
        private OculusDeviceSimulatorControls _controls;
        //private OVRManager _manager;

        private OVRCameraRig _rig;
        public OVRCameraRig RIG => _rig;

        public Axis2DTargets Axis2DTargets { get; set; } = Axis2DTargets.Position;
        public const float HALF_SHOULDER_WIDTH = 0.18f;

        private Vector3 _leftControllerEuler;
        private Vector3 _rightControllerEuler;
        private Vector3 _centerEyeEuler;

        OVRPlugin.ControllerState5 _controllerState;

        private bool _isOculusConnected;
        public bool IsOculusConnected => _isOculusConnected;
        public bool IsRightTargeted => _controls == null || _controls.ManipulateRightControllerButtons;

        [Tooltip("The Transform that contains the Camera. This is usually the \"Head\" of XR Origins. Automatically set to the first enabled camera tagged MainCamera if unset.")]
        public Transform CameraTransform;

        protected virtual void Awake()
        {
            _controls = GetComponent<OculusDeviceSimulatorControls>();
            if (_controls == null)
                Debug.LogError($"Failed to get {nameof(_controls)}.");

            //OVRManager.instance

            _rig = FindObjectOfType<OVRCameraRig>();

            //_manager = OVRManager.instance;
            //if (_manager == null)
            //Debug.LogError($"Failed to get {nameof(_manager)}.");
        }

        protected virtual void OnEnable()
        {
            //_isOculusConnected = OVRPlugin.initialized;
            TrySetCamera();
        }

        private void TrySetCamera()
        {
            //if (CameraTransform == null)
            //{
            //    var mainCamera = Camera.main;
            //    if (mainCamera != null)
            //        CameraTransform = mainCamera.transform;
            //}
        }

        protected virtual void Update()
        {
            if (OVRPlugin.initialized)
                return;

            if (_controls.DesiredCursorLockMode != Cursor.lockState)
                Cursor.lockState = _controls.DesiredCursorLockMode;

            ProcessPoseInput();
            ProcessControlInput();
        }

        protected virtual void ProcessPoseInput()
        {
            if (_rig == null)
                return;

            var cameraParent = CameraTransform.parent;
            var cameraParentRotation = cameraParent != null ? cameraParent.rotation : Quaternion.identity;
            var inverseCameraParentRotation = Quaternion.Inverse(cameraParentRotation);

            //var camera = _rig.centerEyeAnchor;
            //var cameraParentRotation = CameraTransform != null ? CameraTransform.rotation : Quaternion.identity;
            //var inverseCameraRotation = Quaternion.Inverse(camera.rotation);

            //movement
            if (Axis2DTargets.HasFlag(Axis2DTargets.Position))
            {
                // Determine frame of reference
                EnumHelper.GetAxes(_controls.KeyboardTranslateSpace, cameraParent, out var right, out var up, out var forward);

                // Keyboard translation
                var deltaPosition =
                    right * (_controls.ScaledKeyboardTranslateX * Time.deltaTime) +
                    up * (_controls.ScaledKeyboardTranslateY * Time.deltaTime) +
                    forward * (_controls.ScaledKeyboardTranslateZ * Time.deltaTime);

                ProcessDevicePositionForTarget(_controls.KeyboardTranslateSpace, inverseCameraParentRotation, deltaPosition);
            }

            //Mouse rotation
            var scaledMouseDeltaInput = _controls.GetScaledMouseRotateInput();
            Vector3 anglesDelta = _controls.GetScaledRotationDelta(scaledMouseDeltaInput);

            switch (_controls.ManipulationTarget)
            {
                case ManipulationTarget.RightHand:
                    break;
                case ManipulationTarget.LeftHand:
                    break;
                case ManipulationTarget.Head:
                    _centerEyeEuler += anglesDelta;
                    _rig.centerEyeAnchor.localRotation = Quaternion.Euler(_centerEyeEuler);
                    //HMDState.deviceRotation = HMDState.centerEyeRotation;

                    //_centerEyeEuler -= anglesDelta;
                    //_rig.centerEyeAnchor.localRotation = Quaternion.Euler(_centerEyeEuler);
                    break;
                case ManipulationTarget.All:
                    _centerEyeEuler -= anglesDelta;
                    _rig.centerEyeAnchor.localRotation = Quaternion.Euler(_centerEyeEuler);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //if (_controls.ResetInputTriggered())
            //    ResetControllers();

        }

        private void ProcessDevicePositionForTarget(Space manipulationSpace, Quaternion inverseCameraParentRotation, Vector3 deltaPosition)
        {
            Quaternion deltaRotation = Quaternion.identity;

            switch (_controls.ManipulationTarget)
            {
                case ManipulationTarget.RightHand:
                    break;

                case ManipulationTarget.LeftHand:
                    break;

                case ManipulationTarget.Head:
                    deltaRotation = GetDeltaRotation(manipulationSpace, _rig, inverseCameraParentRotation);
                    _rig.centerEyeAnchor.localPosition += deltaRotation * deltaPosition;
                    //HMDState.devicePosition = HMDState.centerEyePosition;

                    //_rig.centerEyeAnchor.localPosition += deltaRotation * deltaPosition;

                    //deltaRotation = GetDeltaRotation(manipulationSpace, _manager.headPoseRelativeOffsetRotation, inverseCameraParentRotation);
                    //_hmdState.centerEyePosition += deltaRotation * deltaPosition;
                    //_hmdState.devicePosition = _hmdState.centerEyePosition;
                    break;
                case ManipulationTarget.All:

                    _rig.centerEyeAnchor.localPosition += deltaPosition;
                    _rig.leftHandAnchor.localPosition += deltaPosition;
                    _rig.rightHandAnchor.localPosition += deltaPosition;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //static Quaternion GetDeltaRotation(Space translateSpace, in Quaternion controllerOrientation, in Quaternion inverseCameraParentRotation)
        //{
        //    switch (translateSpace)
        //    {
        //        case Space.Local:
        //            return controllerOrientation * inverseCameraParentRotation;
        //        case Space.Parent:
        //            return Quaternion.identity;
        //        case Space.Screen:
        //            return inverseCameraParentRotation;
        //        default:
        //            Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
        //            return Quaternion.identity;
        //    }
        //}

        static Quaternion GetDeltaRotation(Space translateSpace, in OVRCameraRig cameraRig, in Quaternion inverseCameraParentRotation)
        {
            //switch (translateSpace)
            //{
            //    case Space.Local:
            //        return Quaternion.Euler(headsetOrientation) * inverseCameraParentRotation;
            //    case Space.Parent:
            //        return Quaternion.identity;
            //    case Space.Screen:
            //        return inverseCameraParentRotation;
            //    default:
            //        Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
            //        return Quaternion.identity;
            //}

            switch (translateSpace)
            {
                case Space.Local:
                    return cameraRig.centerEyeAnchor.localRotation * inverseCameraParentRotation;
                case Space.Parent:
                    return Quaternion.identity;
                case Space.Screen:
                    return inverseCameraParentRotation;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
                    return Quaternion.identity;
            }
        }

        protected virtual void ProcessControlInput()
        {
            //    if (!_controls.ManipulateRightControllerButtons)
            //    {
            //        _controllerState = _controls.ProcessAxis2DControlInput(_controllerState);
            //        _controls.ProcessButtonControlInput(ref _controllerState);
            //    }
            //    else
            //    {
            //        _controllerState = _controls.ProcessAxis2DControlInput(_controllerState);
            //        _controls.ProcessButtonControlInput(ref _controllerState);
            //    }
        }

        private void ResetControllers()
        {
            //LeftControllerState.Reset();
            //RightControllerState.Reset();


            Vector3 baseHeadOffset = Vector3.forward * 0.25f + Vector3.down * 0.15f;
            Vector3 leftOffset = Vector3.left * HALF_SHOULDER_WIDTH + baseHeadOffset;
            Vector3 rightOffset = Vector3.right * HALF_SHOULDER_WIDTH + baseHeadOffset;

            var resetScale = _controls.GetResetScale();
            _rightControllerEuler = Vector3.Scale(_rightControllerEuler, resetScale);
            _leftControllerEuler = Vector3.Scale(_leftControllerEuler, resetScale);

            //PositionRelativeToHead(ref RightControllerState, rightOffset, Quaternion.Euler(_rightControllerEuler));
            //PositionRelativeToHead(ref LeftControllerState, leftOffset, Quaternion.Euler(_leftControllerEuler));
        }

    }
}