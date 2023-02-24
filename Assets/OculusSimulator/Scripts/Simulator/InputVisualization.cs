//using Rhinox.XR.Oculus.Simulator;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

//using Rhinox.XR;
using Rhinox.XR.UnityXR.Simulator;
using Rhinox.XR.Oculus.Simulator;
using System;

/// <summary>
/// This component visualizes pressed input.A GUI is used to represent all keys.
/// </summary>
public class InputVisualization : MonoBehaviour
{
    [Header("Input parameters")]
    [SerializeField] private OculusDeviceSimulatorControls _deviceSimulatorControls;
    [SerializeField] private OculusDeviceSimulator _deviceSimulator;
    [SerializeField] private SimulationRecorder _recorder;
    [SerializeField] private SimulationPlayback _playback;

    private bool _leftGripPressed = false;
    private bool _rightGripPressed = false;
    private bool _leftTriggerPressed = false;
    private bool _rightTriggerPressed = false;
    private bool _leftPrimaryButtonPressed = false;
    private bool _rightPrimaryButtonPressed = false;
    private bool _leftSecondaryButtonPressed = false;
    private bool _rightSecondaryButtonPressed = false;

    /// <summary>
    /// See <see cref="MonoBehaviour"/>
    /// </summary>
    private void OnValidate()
    {
        Assert.AreNotEqual(_deviceSimulatorControls, null,
            $"{nameof(InputVisualization)}, device simulator controls not set!");
        Assert.AreNotEqual(_deviceSimulator, null, $"{nameof(InputVisualization)}, device simulator not set!");
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>
    /// </summary>
    private void OnGUI()
    {
        var titleStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,

            normal =
            {
                textColor = Color.white
            }
        };

        var subTitleStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Italic,
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,

            normal =
            {
                textColor = Color.white
            }
        };

        var windowRect = new Rect(Screen.width - 300, 0, 300, Screen.height);

        GUI.Box(windowRect, "");
        GUILayout.BeginArea(windowRect);

        //--------------------------
        // Simulator Controls
        //--------------------------
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_deviceSimulatorControls.ToggleManipulateAction)} Mode: {_deviceSimulatorControls.ManipulationTarget}");
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_deviceSimulatorControls.ToggleKeyboardSpaceAction)} Keyboard Space: {_deviceSimulatorControls.KeyboardTranslateSpace}");
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_deviceSimulatorControls.ToggleButtonControlTargetAction)} Controller Buttons: {(_deviceSimulatorControls.ManipulateRightControllerButtons ? "Right" : "Left")}");
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_recorder.BeginRecordingActionReference)} to start recording");
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_recorder.EndRecordingActionReference)} to end recording");
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_playback.StartPlaybackActionReference)} to start playback");

        //--------------------------
        // Simulator Info
        //--------------------------
        if (_recorder.IsRecording)
            GUILayout.Label("Currently recording.");
        if (_playback.IsPlaying)
            GUILayout.Label("Currently playing back.");
        //--------------------------
        // DEVICE POSITIONS
        //--------------------------
        GUILayout.Label("Device transforms", titleStyle);

        GUILayout.Label($"HMD position: {_deviceSimulator.RIG.centerEyeAnchor.localPosition}");
        GUILayout.Label($"HMD rotation: {_deviceSimulator.RIG.centerEyeAnchor.localEulerAngles}");

        //var rightHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.RightHand);
        GUILayout.Label($"Right controller position: {_deviceSimulator.RIG.rightHandAnchor.localPosition}");
        GUILayout.Label($"Right controller rotation: {_deviceSimulator.RIG.rightHandAnchor.localEulerAngles}");

        //var leftHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.LeftHand);
        GUILayout.Label($"Left controller position: {_deviceSimulator.RIG.leftHandAnchor.localPosition}");
        GUILayout.Label($"Left controller rotation: {_deviceSimulator.RIG.leftHandAnchor.localEulerAngles}");

        GUILayout.Space(10);
        GUILayout.Label(
            _deviceSimulatorControls.ManipulateRightControllerButtons
                ? $"Current manipulated controller: right"
                : $"Current manipulated controller: left");

        //--------------------------
        // INPUT 
        //--------------------------
        GUILayout.Space(10);
        GUILayout.Label("Used input", titleStyle);

        foreach (OVRInput.Button btn in Enum.GetValues(typeof(OVRInput.Button)))
        {
            if (btn is OVRInput.Button.Any or OVRInput.Button.None)
                continue;

            if (OVRInput.Get(btn))
                GUILayout.Label($"{btn} pressed");
        }

        //if (_deviceSimulatorControls.Axis2DInput.x != 0 || _deviceSimulatorControls.Axis2DInput.y != 0)
        //    GUILayout.Label($"2D axis input: {_deviceSimulatorControls.Axis2DInput.ToString()}");

        //if (_deviceSimulatorControls.RestingHandAxis2DInput.x != 0 || _deviceSimulatorControls.RestingHandAxis2DInput.y != 0)
        //    GUILayout.Label($"Resting hand 2D axis input: {_deviceSimulatorControls.RestingHandAxis2DInput.ToString()}");

        //if (_deviceSimulatorControls.GripInput)
        //    GUILayout.Label($"Grip pressed.");

        //if (_deviceSimulatorControls.TriggerInput)
        //    GUILayout.Label($"Trigger pressed.");

        //if (_deviceSimulatorControls.PrimaryButtonInput)
        //    GUILayout.Label($"Primary button pressed.");

        //if (_deviceSimulatorControls.SecondaryButtonInput)
        //    GUILayout.Label($"Secondary button pressed.");

        //if (_deviceSimulatorControls.MenuInput)
        //    GUILayout.Label($"Menu button pressed.");

        //if (_deviceSimulatorControls.Primary2DAxisClickInput)
        //    GUILayout.Label($"Primary 2D axis clicked.");
        //if (_deviceSimulatorControls.Secondary2DAxisClickInput)
        //    GUILayout.Label($"Secondary 2D axis clicked.");

        //if (_deviceSimulatorControls.Primary2DAxisTouchInput)
        //    GUILayout.Label($"Primary 2D axis touched.");
        //if (_deviceSimulatorControls.Secondary2DAxisTouchInput)
        //    GUILayout.Label($"Secondary 2D axis touched.");

        //if (_deviceSimulatorControls.PrimaryTouchInput)
        //    GUILayout.Label($"Primary touch pressed.");
        //if (_deviceSimulatorControls.SecondaryTouchInput)
        //    GUILayout.Label($"Secondary touch pressed.");

        //if (_leftGripPressed)
        //    GUILayout.Label("Left GRIP pressed.");
        //if (_rightGripPressed)
        //    GUILayout.Label("Right GRIP pressed.");

        //if (_leftTriggerPressed)
        //    GUILayout.Label("Left TRIGGER pressed.");
        //if (_rightTriggerPressed)
        //    GUILayout.Label("Right TRIGGER pressed.");

        //if (_leftPrimaryButtonPressed)
        //    GUILayout.Label("Left PRIMARY BUTTON pressed.");
        //if (_rightPrimaryButtonPressed)
        //    GUILayout.Label("Right PRIMARY BUTTON pressed.");

        //if (_leftSecondaryButtonPressed)
        //    GUILayout.Label("Left SECONDARY BUTTON pressed.");
        //if (_rightSecondaryButtonPressed)
        //    GUILayout.Label("Right SECONDARY BUTTON pressed.");

        GUILayout.EndArea();
    }
}