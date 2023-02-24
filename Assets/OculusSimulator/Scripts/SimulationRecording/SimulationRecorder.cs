using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.XR.Oculus.Simulator;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace Rhinox.XR.UnityXR.Simulator
{
    /// <summary>
    /// This class records user input events and the transformation of the HMD and controllers.<br />It writes this recording to an xml file when the recording ends.
    /// </summary>
    /// <remarks>
    /// Control what gets captured by changing the booleans.
    /// </remarks>
    public class SimulationRecorder : MonoBehaviour
    {
        [Header("Device Transforms")]
        [SerializeField] private OculusDeviceSimulator _simulator;
        [SerializeField] private Transform _headTransform;
        [SerializeField] private Transform _leftHandTransform;
        [SerializeField] private Transform _rightHandTransform;

        [Header("Recording parameters")] 
        [Tooltip("Starts recording on awake.")]
        [SerializeField] private bool _startOnAwake;
        [Tooltip("End any running recording on destroy.")]
        [SerializeField] private bool _endOnDestroy;
        [SerializeField] private int _desiredFPS = 30;
        [Tooltip("Note: dead zone value should be very small for high frame rates!")]
        [SerializeField] private float _positionDeadZone = 0.005f;

        [Tooltip("Note: dead zone value should be very small for high frame rates!")]
        [SerializeField] private float _rotationDeadZone = 0.005f;

        [HideInInspector] public string Path;
        [HideInInspector] public string RecordingName = "NewRecording";

        [Header("Input actions")] 
        public InputActionReference BeginRecordingActionReference;
        public InputActionReference EndRecordingActionReference;
        [Space(15)]

        private float _frameInterval;

        [HideInInspector]
        public bool IsRecording;

        private Stopwatch _recordingStopwatch = new Stopwatch();
        private SimulationRecording _currentRecording ;
        private List<OvrFrameInput> _currentFrameInput = new List<OvrFrameInput>();

        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void Awake()
        {
            _frameInterval = 1 / (float)_desiredFPS;

            if (_startOnAwake)
                StartRecording(new InputAction.CallbackContext());
        }

        private void OnDestroy()
        {
            if(_endOnDestroy)
                EndRecording(new InputAction.CallbackContext());
        }

        private void OnEnable()
        {
            if (_simulator == null)
            {
                Debug.Log("_simulator has not been set,  disabling this SimulationRecorder.");
                this.gameObject.SetActive(false);
                return;
            }
            
            SubscribeRecorderActions();
        }
        private void OnDisable()
        {
            UnsubscribeRecorderActions();
        }

        private void SubscribeRecorderActions()
        {
            SimulatorUtils.Subscribe(BeginRecordingActionReference, StartRecording);
            SimulatorUtils.Subscribe(EndRecordingActionReference, EndRecording);
        }
        private void UnsubscribeRecorderActions()
        {
            SimulatorUtils.Unsubscribe(BeginRecordingActionReference, StartRecording);
            SimulatorUtils.Unsubscribe(EndRecordingActionReference, EndRecording);
        }

        /// <summary>
        /// Creates the recording and starts frame capturing.
        /// </summary>
        [ContextMenu("Start Recording")]
        private void StartRecording(InputAction.CallbackContext ctx)
        {
            IsRecording = true;
            _currentRecording = new SimulationRecording
            {
                FrameRate = _desiredFPS
            };
            _frameInterval = 1.0f / _desiredFPS;
            _recordingStopwatch.Restart();
            Debug.Log("Started recording.");
            
            StartCoroutine(RecordingCoroutine());
        }

        private IEnumerator RecordingCoroutine()
        {
            while (IsRecording)
            {
                var newFrame = new OvrFrameData
                            {
                                HeadPosition = _headTransform.position,
                                HeadRotation = _headTransform.rotation,
                                LeftHandPosition = _leftHandTransform.position,
                                LeftHandRotation = _leftHandTransform.rotation,
                                RightHandPosition = _rightHandTransform.position,
                                RightHandRotation = _rightHandTransform.rotation,
                                FrameInputs = new List<OvrFrameInput>(_currentFrameInput)
                            };
                var previousRecordedFrame = _currentRecording.Frames.LastOrDefault();
                //Temporarily set the frame number of the current frame to the previous recorded frame
                //Otherwise they will never be equal
                newFrame.FrameNumber = previousRecordedFrame.FrameNumber;
                
                if (newFrame.ApproximatelyEqual(previousRecordedFrame,_positionDeadZone,_rotationDeadZone) )
                {
                    _currentRecording.AddEmptyFrame();
                }
                else
                {
                    _currentRecording.AddFrame(newFrame);
                }
                _currentFrameInput.Clear();

                yield return new WaitForSecondsRealtime(_frameInterval);
            }
            
        }

        private void Update()
        {
            if (!IsRecording)
                return;
            
            //-------------------------------
            // RECORD INPUT FROM THIS FRAME
            //-------------------------------
            //Buttons
            foreach (OVRInput.Button inputButton in Enum.GetValues(typeof(OVRInput.Button)))
            {
                if (inputButton is OVRInput.Button.Any or OVRInput.Button.None)
                    continue;
                if (OVRInput.GetDown(inputButton))
                    RegisterFrameInput(inputButton, true);
                else if (OVRInput.GetUp(inputButton))
                    RegisterFrameInput(inputButton, false);
            }
            //Axis value
            foreach (OVRInput.Axis2D axis in Enum.GetValues(typeof(OVRInput.Axis2D)))
            {
                if (axis is OVRInput.Axis2D.Any or OVRInput.Axis2D.None)
                    continue;
                
                var state = OVRInput.Get(axis);
                if(Mathf.Approximately(state.x,0) && Mathf.Approximately(state.y,0))
                    continue;
                RegisterFrameInput(axis,state.ToString());
                
            }
        }

        private void RegisterFrameInput(OVRInput.Button button ,bool isInputStart)
        {
            var newInput = new OvrFrameInput()
            {
                InputType = EnumHelper.SimulatorInputType.Button,
                InputActionName = button.ToString(),
                IsInputStart = isInputStart
            };
            _currentFrameInput.Add(newInput);
        }

        private void RegisterFrameInput(OVRInput.Axis2D axis, string axisVal)
        {
            var newInput = new OvrFrameInput()
            {
                InputType = EnumHelper.SimulatorInputType.Axis2D,
                InputActionName = axis.ToString(),
                Value = axisVal
            };
            _currentFrameInput.Add(newInput);
        }
        
        /// <summary>
        /// End frame capturing and writes the recording to an XML file.
        /// </summary>
        [ContextMenu("End Recording")]
        private void EndRecording(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            //----------------------------
            // CALCULATE LENGTH
            //----------------------------
            RecordingTime temp;
            _recordingStopwatch.Stop();
            temp.Milliseconds = _recordingStopwatch.Elapsed.Milliseconds;
            temp.Seconds = _recordingStopwatch.Elapsed.Seconds;
            temp.Minutes = _recordingStopwatch.Elapsed.Minutes;
            _currentRecording.RecordingLength = temp;
            Debug.Log($"Ended recording of {_currentRecording.RecordingLength}");

            
            //----------------------------
            //Write to XML
            //----------------------------
            //Create the target directory just in case

            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(System.IO.Path.Combine(Path, $"{RecordingName}.xml"),
                FileMode.Create);
            serializer.Serialize(stream, _currentRecording);
            stream.Close();
            Debug.Log($"Wrote recording to: {Path}");
            // _simulator.InputEnabled = true;
            IsRecording = false;
        }
        
        
        //-----------------------
        // INPUT 
        //-----------------------
#if UNITY_EDITOR
        [ContextMenu("Import InputActionAsset")]
        private void ImportActionAsset()
        {
            EditorInputDialog.Create("Import InputActionAsset", "Reference File")
                .GenericUnityObjectField<InputActionAsset>("InputActionAsset:", out var actionAsset)
                .BooleanField("Overwrite:", out var overwriteVal)
                .OnAccept(() =>
                {
                    if (actionAsset == null || actionAsset.Value == null)
                        return;

                    var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    fields = fields.Where(x => x.GetReturnType() == typeof(InputActionReference)).ToArray();

                    foreach (var field in fields)
                    {
                        if (!overwriteVal.Value && field.GetValue(this) != null)
                            continue;
                        foreach (var map in actionAsset.Value.actionMaps)
                        {
                            foreach (var action in map.actions)
                            {
                                var actionName = action.name.Split('/').LastOrDefault();
                                if (actionName == null)
                                    continue;
                                var parts = actionName.SplitCamelCase(" ").Split(' ');
                                bool containsAll = true;
                                foreach (var part in parts)
                                {
                                    if (string.IsNullOrWhiteSpace(part))
                                        continue;


                                    if (!field.Name.Contains(part, StringComparison.InvariantCultureIgnoreCase))
                                        containsAll = false;
                                }

                                if (containsAll)
                                {
                                    var actionReference = GetReference(action);
                                    field.SetValue(this, actionReference);
                                    break;
                                }

                            }
                        }
                    }
                })
                .Show();
        }

        private static InputActionReference GetReference(InputAction action)
        {
            var assets = AssetDatabase.FindAssets($"t:{nameof(InputActionReference)}");
            foreach (var asset in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var refAssets = AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (var refAsset in refAssets.OfType<InputActionReference>())
                    if (refAsset.action.id == action.id)
                        return refAsset;
            }

            return InputActionReference.Create(action);
        }
#endif
    }
    
   

}
