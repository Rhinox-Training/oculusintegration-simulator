using System;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using OVR.OpenVR;

namespace Rhinox.XR.Oculus.Simulator
{
    public class OculusDeviceSimulator : MonoBehaviour
    {
        private OculusDeviceSimulatorControls _controls;
        private OVRManager _manager;

        public Axis2DTargets Axis2DTargets { get; set; } = Axis2DTargets.Position;

        private Vector3 _leftControllerEuler;
        private Vector3 _rightControllerEuler;
        private Vector3 _centerEyeEuler;


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
            //ProcessControlInput();
        }

        protected virtual void ProcessPoseInput()
        {
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
        }

        private void ProcessDevicePositionForTarget(Space manipulationSpace, Quaternion inverseCameraParentRotation, Vector3 deltaPosition)
        {
            Quaternion deltaRotation = Quaternion.identity;
            switch (_controls.ManipulationTarget)
            {
                case ManipulationTarget.RightHand:
                    deltaRotation = GetDeltaRotation(manipulationSpace, inverseCameraParentRotation);
                    //_rightControllerState.devicePosition += deltaRotation * deltaPosition;
                    break;
                case ManipulationTarget.LeftHand:
                    deltaRotation = GetDeltaRotation(manipulationSpace, inverseCameraParentRotation);
                    //_leftControllerState.devicePosition += deltaRotation * deltaPosition;
                    break;
                case ManipulationTarget.Head:
                    deltaRotation = GetDeltaRotation(manipulationSpace, inverseCameraParentRotation);
                    //_hmdState.centerEyePosition += deltaRotation * deltaPosition;
                    //_hmdState.devicePosition = _hmdState.centerEyePosition;
                    break;
                case ManipulationTarget.All:

                    //Vector3 relativeRightPosition = _rightControllerState.devicePosition - _hmdState.devicePosition;
                    //Vector3 relativeLeftPosition = _leftControllerState.devicePosition - _hmdState.devicePosition;

                    deltaRotation = GetDeltaRotation(manipulationSpace, inverseCameraParentRotation);
                    //_hmdState.centerEyePosition += deltaRotation * deltaPosition;
                    //Vector3 newDevicePosition = _hmdState.centerEyePosition;
                    //_hmdState.devicePosition = newDevicePosition;

                    //_rightControllerState.devicePosition = newDevicePosition + relativeRightPosition;
                    //_leftControllerState.devicePosition = newDevicePosition + relativeLeftPosition;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static Quaternion GetDeltaRotation(Space translateSpace, in Quaternion inverseCameraParentRotation)
        {

            switch (translateSpace)
            {
                //TODO: FIX this \/
                //case Space.Local:
                //return state.deviceRotation * inverseCameraParentRotation;
                case Space.Parent:
                    return Quaternion.identity;
                case Space.Screen:
                    return inverseCameraParentRotation;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
                    return Quaternion.identity;
            }
        }

        //static Quaternion GetDeltaRotation(Space translateSpace, in Quaternion inverseCameraParentRotation)
        //{
        //    switch (translateSpace)
        //    {
        //        //TODO: FIX this \/
        //        //case Space.Local:
        //            //return state.centerEyeRotation * inverseCameraParentRotation;
        //        case Space.Parent:
        //            return Quaternion.identity;
        //        case Space.Screen:
        //            return inverseCameraParentRotation;
        //        default:
        //            Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
        //            return Quaternion.identity;
        //    }
        //}
    }
}