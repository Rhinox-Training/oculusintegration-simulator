using System;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using OVR.OpenVR;
using static OVRPlugin;

namespace Rhinox.XR.Oculus.Simulator
{
    public class OculusDeviceSimulator : MonoBehaviour
    {
        private OculusDeviceSimulatorControls _controls;
        private OVRManager _manager;

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

            _manager = OVRManager.instance;
            if (_manager == null)
                Debug.LogError($"Failed to get {nameof(_manager)}.");
        }

        protected virtual void OnEnable()
        {
            _isOculusConnected = OVRPlugin.initialized;
            TrySetCamera();
        }

        private void TrySetCamera()
        {
            if (CameraTransform == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                    CameraTransform = mainCamera.transform;
            }
        }

        protected virtual void Update()
        {
            if (_isOculusConnected)
                return;

            if (_controls.DesiredCursorLockMode != Cursor.lockState)
                Cursor.lockState = _controls.DesiredCursorLockMode;

            ProcessPoseInput();
            ProcessControlInput();
        }

        protected virtual void ProcessPoseInput()
        {
            if (CameraTransform == null)
                return;

            //var camera = CameraTransform;
            //var cameraParentRotation = CameraTransform != null ? CameraTransform.rotation : Quaternion.identity;
            var inverseCameraRotation = Quaternion.Inverse(CameraTransform.rotation);

            //movement
            if (Axis2DTargets.HasFlag(Axis2DTargets.Position))
            {
                // Determine frame of reference
                EnumHelper.GetAxes(_controls.KeyboardTranslateSpace, CameraTransform, _controls.ManipulationTarget, out var right, out var up, out var forward);

                // Keyboard translation
                var deltaPosition =
                    right * (_controls.ScaledKeyboardTranslateX * Time.deltaTime) +
                    up * (_controls.ScaledKeyboardTranslateY * Time.deltaTime) +
                    forward * (_controls.ScaledKeyboardTranslateZ * Time.deltaTime);

                ProcessDevicePositionForTarget(_controls.KeyboardTranslateSpace, inverseCameraRotation, deltaPosition);
            }

            //Mouse rotation
            var scaledMouseDeltaInput = _controls.GetScaledMouseRotateInput();
            Vector3 anglesDelta = _controls.GetScaledRotationDelta(scaledMouseDeltaInput);

            OVRPose rightHand = OVRPose.identity;
            OVRPose leftHand = OVRPose.identity;

            switch (_controls.ManipulationTarget)
            {
                case ManipulationTarget.RightHand:
                    rightHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.RightHand);
                    leftHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.RightHand);

                    _rightControllerEuler += anglesDelta;
                    rightHand.orientation = Quaternion.Euler(_rightControllerEuler);

                    OVRInput.SetOpenVRLocalPose(leftHand.position, rightHand.position, leftHand.orientation, rightHand.orientation);

                    //RightControllerState.deviceRotation = Quaternion.Euler(_rightControllerEuler);
                    break;
                case ManipulationTarget.LeftHand:
                    rightHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.RightHand);
                    leftHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.RightHand);

                    _leftControllerEuler += anglesDelta;
                    leftHand.orientation = Quaternion.Euler(_leftControllerEuler);

                    OVRInput.SetOpenVRLocalPose(leftHand.position, rightHand.position, leftHand.orientation, rightHand.orientation);

                    //_leftControllerEuler += anglesDelta;
                    //LeftControllerState.deviceRotation = Quaternion.Euler(_leftControllerEuler);
                    break;
                case ManipulationTarget.Head:
                    _centerEyeEuler -= anglesDelta;

                    _manager.headPoseRelativeOffsetRotation = _centerEyeEuler;


                    //HMDState.centerEyeRotation = Quaternion.Euler(_centerEyeEuler);
                    //HMDState.deviceRotation = HMDState.centerEyeRotation;
                    break;
                case ManipulationTarget.All:
                    //var matrixL = GetRelativeMatrixFromHead(ref LeftControllerState);
                    //var matrixR = GetRelativeMatrixFromHead(ref RightControllerState);
                    //_centerEyeEuler += anglesDelta;
                    //HMDState.centerEyeRotation = Quaternion.Euler(_centerEyeEuler);
                    //HMDState.deviceRotation = HMDState.centerEyeRotation;
                    //PositionRelativeToHead(ref LeftControllerState, matrixL.GetColumn(3), matrixL.rotation);
                    //PositionRelativeToHead(ref RightControllerState, matrixR.GetColumn(3), matrixR.rotation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //if (_controls.ResetInputTriggered())
            //    ResetControllers();





            #region "old Test Code"
            //Vector3 emulatedTranslation = _manager.headPoseRelativeOffsetTranslation;
            //float deltaMouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
            //float emulatedHeight = deltaMouseScrollWheel * MOUSE_SCALE_HEIGHT;
            //var deltaPosition =
            //right * (_controls.ScaledKeyboardTranslateX * Time.deltaTime) +
            //up * (_controls.ScaledKeyboardTranslateY * Time.deltaTime) +
            //forward * (_controls.ScaledKeyboardTranslateZ * Time.deltaTime);

            //emulatedTranslation.y += emulatedHeight;
            //_manager.headPoseRelativeOffsetTranslation = emulatedTranslation;

            //OVRManager.set

            //var cameraParent = CameraTransform.parent;
            //var cameraParentRotation = cameraParent != null ? cameraParent.rotation : Quaternion.identity;
            //var inverseCameraParentRotation = Quaternion.Inverse(cameraParentRotation);

            //if (Axis2DTargets.HasFlag(Axis2DTargets.Position))
            //{
            //    // Determine frame of reference
            //    EnumHelper.GetAxes(_controls.KeyboardTranslateSpace, CameraTransform, out var right, out var up, out var forward);

            //    // Keyboard translation
            //    var deltaPosition =
            //        right * (_controls.ScaledKeyboardTranslateX * Time.deltaTime) +
            //        up * (_controls.ScaledKeyboardTranslateY * Time.deltaTime) +
            //        forward * (_controls.ScaledKeyboardTranslateZ * Time.deltaTime);

            //    ProcessDevicePositionForTarget(_controls.KeyboardTranslateSpace, inverseCameraParentRotation, deltaPosition);
            //}
            #endregion
        }

        private void ProcessDevicePositionForTarget(Space manipulationSpace, Quaternion inverseCameraParentRotation, Vector3 deltaPosition)
        {
            Quaternion deltaRotation = Quaternion.identity;
            OVRPose rightHand = OVRPose.identity;
            OVRPose leftHand = OVRPose.identity;

            switch (_controls.ManipulationTarget)
            {
                case ManipulationTarget.RightHand:
                    rightHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.RightHand);
                    leftHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.LeftHand);

                    //deltaRotation = GetDeltaRotation(manipulationSpace, leftHand.orientation, inverseCameraParentRotation);

                    rightHand.position += /*deltaRotation **/ deltaPosition;
                    OVRInput.SetOpenVRLocalPose(leftHand.position, rightHand.position, leftHand.orientation, rightHand.orientation);
                    break;

                case ManipulationTarget.LeftHand:
                    rightHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.RightHand);
                    leftHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.LeftHand);

                    //deltaRotation = GetDeltaRotation(manipulationSpace, leftHand.orientation, inverseCameraParentRotation);

                    leftHand.position += /*deltaRotation **/ deltaPosition;
                    OVRInput.SetOpenVRLocalPose(leftHand.position, rightHand.position, leftHand.orientation, rightHand.orientation);
                    break;

                case ManipulationTarget.Head:
                    deltaRotation = GetDeltaRotation(manipulationSpace, _manager.headPoseRelativeOffsetRotation, inverseCameraParentRotation);

                    _manager.headPoseRelativeOffsetTranslation += /*deltaRotation **/ deltaPosition;
                    //_hmdState.centerEyePosition += deltaRotation * deltaPosition;
                    //_hmdState.devicePosition = _hmdState.centerEyePosition;
                    break;
                case ManipulationTarget.All:

                    _manager.headPoseRelativeOffsetTranslation += /*deltaRotation **/ deltaPosition;




                    rightHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.RightHand);
                    leftHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.RightHand);

                    Vector3 relativeRightPosition = rightHand.position - _manager.headPoseRelativeOffsetTranslation;
                    Vector3 relativeLeftPosition = leftHand.position - _manager.headPoseRelativeOffsetTranslation;

                    deltaRotation = GetDeltaRotation(manipulationSpace, _manager.headPoseRelativeOffsetRotation, inverseCameraParentRotation);

                    //_hmdState.centerEyePosition += deltaRotation * deltaPosition;

                    //_manager.headPoseRelativeOffsetTranslation += deltaRotation * deltaPosition;
                    //Vector3 newDevicePosition = _hmdState.centerEyePosition;
                    //_hmdState.devicePosition = newDevicePosition;

                    //_rightControllerState.devicePosition = newDevicePosition + relativeRightPosition;
                    //_leftControllerState.devicePosition = newDevicePosition + relativeLeftPosition;

                    OVRInput.SetOpenVRLocalPose(_manager.headPoseRelativeOffsetTranslation + leftHand.position,
                                                _manager.headPoseRelativeOffsetTranslation + rightHand.position,
                                                leftHand.orientation, rightHand.orientation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static Quaternion GetDeltaRotation(Space translateSpace, in Quaternion controllerOrientation, in Quaternion inverseCameraParentRotation)
        {
            switch (translateSpace)
            {
                case Space.Local:
                    return controllerOrientation * inverseCameraParentRotation;
                case Space.Parent:
                    return Quaternion.identity;
                case Space.Screen:
                    return inverseCameraParentRotation;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
                    return Quaternion.identity;
            }
        }

        static Quaternion GetDeltaRotation(Space translateSpace, in Vector3 headsetOrientation, in Quaternion inverseCameraParentRotation)
        {
            switch (translateSpace)
            {
                case Space.Local:
                    return Quaternion.Euler(headsetOrientation) * inverseCameraParentRotation;
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