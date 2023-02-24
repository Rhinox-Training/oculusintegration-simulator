using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Rhinox.XR.Oculus.Simulator;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace Rhinox.XR.UnityXR.Simulator
{
    public class SimulationPlayback : MonoBehaviour
    {
        [Header("Input parameters")] 
        [SerializeField] private OculusDeviceSimulator _simulator;

        [HideInInspector][SerializeField] public string Path;
        [HideInInspector][SerializeField] public string RecordingName = "MyRecording";
        

        [Header("Playback Controls")] 
        public InputActionReference StartPlaybackActionReference;
        public InputActionReference ReimportRecordingActionReference;
        public InputActionReference AbortPlaybackActionReference;

        [HideInInspector] public bool IsPlaying;

        private SimulationRecording _currentRecording;
        private Stopwatch _playbackStopwatch;
        private float _frameInterval = float.MaxValue;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void Awake()
        {
            _playbackStopwatch = new Stopwatch();
        }

        private void OnEnable()
        {
            if (_simulator == null)
            {
                Debug.Log("_simulator has not been set,  disabling this SimulationPlayback.");
                this.gameObject.SetActive(false);
                return;
            }
            SimulatorUtils.Subscribe(StartPlaybackActionReference, StartPlayback);
            SimulatorUtils.Subscribe(ReimportRecordingActionReference, ImportRecording);
            SimulatorUtils.Subscribe(AbortPlaybackActionReference, AbortPlayback);
        }
        private void OnDisable()
        {
            SimulatorUtils.Unsubscribe(StartPlaybackActionReference, StartPlayback);
            SimulatorUtils.Subscribe(ReimportRecordingActionReference, ImportRecording);
            SimulatorUtils.Subscribe(AbortPlaybackActionReference, AbortPlayback);
        }
        
        private void ImportRecording(InputAction.CallbackContext ctx)
        {
            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(System.IO.Path.Combine(Path, $"{RecordingName}.xml"),
                FileMode.Open);
            _currentRecording = (SimulationRecording)serializer.Deserialize(stream);
            stream.Close();

            if (_currentRecording == null)
            {
                Debug.Log($"{nameof(SimulationPlayback)}, could not loud recording from XML file");
                return;
            }
            Debug.Log($"Imported recording of {_currentRecording.RecordingLength}");

            _frameInterval = 1.0f / _currentRecording.FrameRate;
        }
        private void ImportRecording()
        {
            //Read XML
            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(System.IO.Path.Combine(Path, $"{RecordingName}.xml"),
                FileMode.Open);
            _currentRecording = (SimulationRecording)serializer.Deserialize(stream);
            stream.Close();

            if (_currentRecording == null)
            {
                Debug.Log($"{nameof(SimulationPlayback)}, could not loud recording from XML file");
                return;
            }

            Debug.Log($"Imported recording of {_currentRecording.RecordingLength}");

            _frameInterval = 1.0f / _currentRecording.FrameRate;
        }

        private void AbortPlayback(InputAction.CallbackContext ctx)
        {
            StopAllCoroutines();
            EndPlayBack();
        }
        
        /// <summary>
        /// Disables input in the simulator and starts the playback of the current recording.
        /// </summary>
        /// <remarks>
        /// If there is no current recording or the current recordings frames are empty, the function returns early.
        /// </remarks>
        [ContextMenu("Start Playback")]
        private void StartPlayback(InputAction.CallbackContext ctx)
        {
            if (IsPlaying)
            {
                Debug.Log("Is currently playing, please wait until playback ends or stop the current playback");
                return;
            }
            // Import a recording if none is present.
            // If no recording could be imported, abort.
            if (_currentRecording == null)
            {
                ImportRecording();
                if (_currentRecording == null)
                    return;
            }
            if (_currentRecording.AmountOfFrames == 0 || _currentRecording.Frames.Count == 0)
            {
                _currentRecording = null;
                Debug.Log("Current recording is empty, abandoning playback. Please import a new recording.");
                return;
            }

            IsPlaying = true;
            
            Debug.Log("Started playback.");
            _simulator.IsInputEnabled = false;

            _playbackStopwatch.Restart();

            StartCoroutine(PlaybackRoutine());
        }

        private IEnumerator PlaybackRoutine()
        {
            // //Set first frame state
            // {
            //     var firstFrame = _currentRecording.Frames.First();
            //     foreach (var input in firstFrame.FrameInputs)
            //         ProcessFrameInput(input);
            //     _simulator.SetRigTransforms(firstFrame.HeadPosition, firstFrame.HeadRotation,
            //         firstFrame.LeftHandPosition, firstFrame.LeftHandRotation,
            //         firstFrame.RightHandPosition, firstFrame.RightHandRotation);
            //     yield return new WaitForSecondsRealtime(_frameInterval);
            // }

            int loopFrame = 0;
            int currentRecordedFrame = 0;
            OVRPlugin.ControllerState5 playbackState = default;
            while (loopFrame + 1 < _currentRecording.AmountOfFrames)
            {
                var currentFrame = _currentRecording.Frames[currentRecordedFrame];
                OvrFrameData nextOvrFrame;
                if (currentRecordedFrame + 1 != _currentRecording.Frames.Count)
                    nextOvrFrame = _currentRecording.Frames[currentRecordedFrame + 1];
                else
                {
                    //This is used when the last remaining frames are all empty
                    yield return new WaitForSecondsRealtime(_frameInterval);
                    loopFrame++;
                    continue;
                }

                foreach (var input in currentFrame.FrameInputs)
                {
                    switch (input.InputType)
                    {
                        case EnumHelper.SimulatorInputType.Button:
                            if (ProcessFrameInput(input, out OVRInput.RawButton btn))
                            {
                                if (input.IsInputStart)
                                    playbackState.Buttons |= (uint)btn;
                                else
                                    playbackState.Buttons &= ~(uint)btn;
                            }
                            break;
                        case EnumHelper.SimulatorInputType.Axis2D:
                            if (ProcessFrameInput(input, out OVRInput.RawAxis2D axis))
                            {
                                OVRPlugin.Vector2f result = default;
                                var commaSeparator = input.Value.LastIndexOf(',');
                                result.x = float.Parse(input.Value.Substring(1, commaSeparator - 1));
                                result.y = float.Parse(input.Value.Substring(commaSeparator + 1, input.Value.Length - commaSeparator - 2));                                
                                switch (axis)
                                {
                                    case OVRInput.RawAxis2D.LThumbstick:
                                        playbackState.LThumbstick = result;
                                        break;
                                    case OVRInput.RawAxis2D.LTouchpad:
                                        playbackState.LTouchpad = result;
                                        break;
                                    case OVRInput.RawAxis2D.RThumbstick:
                                        playbackState.RThumbstick = result;
                                        break;
                                    case OVRInput.RawAxis2D.RTouchpad:
                                        playbackState.RTouchpad = result;
                                        break;
                                }
                            }
                            break;
                        case EnumHelper.SimulatorInputType.Axis1D:
                            if(ProcessFrameInput(input,out OVRInput.RawAxis1D resultAxis))
                            {
                                var resultValue = float.Parse(input.Value);
                                switch (resultAxis)
                                {
                                    case OVRInput.RawAxis1D.LIndexTrigger:
                                        playbackState.LIndexTrigger = resultValue;
                                        break;
                                    case OVRInput.RawAxis1D.LHandTrigger:
                                        playbackState.LHandTrigger = resultValue;
                                        break;
                                    case OVRInput.RawAxis1D.RIndexTrigger:
                                        playbackState.RIndexTrigger = resultValue;
                                        break;
                                    case OVRInput.RawAxis1D.RHandTrigger:
                                        playbackState.RHandTrigger = resultValue;
                                        break;
                                    case OVRInput.RawAxis1D.LIndexTriggerCurl:
                                        playbackState.LIndexTriggerCurl = resultValue;
                                        break;
                                    case OVRInput.RawAxis1D.LIndexTriggerSlide:
                                        playbackState.LIndexTriggerSlide = resultValue;
                                        break;
                                    case OVRInput.RawAxis1D.LThumbRestForce:
                                        playbackState.LThumbRestForce = resultValue;
                                        break;
                                    case OVRInput.RawAxis1D.LStylusForce:
                                        playbackState.LStylusForce = resultValue;
                                        break;
                                    case OVRInput.RawAxis1D.RIndexTriggerCurl:
                                        playbackState.RIndexTriggerCurl = resultValue;
                                        break;
                                    case OVRInput.RawAxis1D.RIndexTriggerSlide:
                                        playbackState.RIndexTriggerSlide = resultValue;
                                        break;
                                    case OVRInput.RawAxis1D.RThumbRestForce:
                                        playbackState.RThumbRestForce = resultValue;
                                        break;
                                    case OVRInput.RawAxis1D.RStylusForce:
                                        playbackState.RStylusForce = resultValue;
                                        break;
                                }
                            }
                            break;
                    }
                   
                }
                
                _simulator.PushControllerState(playbackState);
                if (loopFrame == nextOvrFrame.FrameNumber - 1)
                {
                    currentRecordedFrame++;
                    yield return StartCoroutine(TransformLerpCoroutine(currentFrame, nextOvrFrame));
                }
                else
                    yield return new WaitForSecondsRealtime(_frameInterval);

                loopFrame++;
            }

            // //Set final frame state
            // {
            //     var final = _currentRecording.Frames.Last();
            //     foreach (var input in final.FrameInputs)
            //         ProcessFrameInput(input);
            //     _simulator.SetRigTransforms(final.HeadPosition, final.HeadRotation,
            //         final.LeftHandPosition, final.LeftHandRotation,
            //         final.RightHandPosition, final.RightHandRotation);
            //     yield return new WaitForSecondsRealtime(_frameInterval);
            // }
            
            EndPlayBack();

        }

        private IEnumerator TransformLerpCoroutine(OvrFrameData currentOvrFrame, OvrFrameData nextOvrFrame)
        {
            var timer = 0f;
            while (timer< _frameInterval)
            {
                var headPosition = Vector3.Lerp(currentOvrFrame.HeadPosition, nextOvrFrame.HeadPosition,
                    timer / _frameInterval);
                var headRotation = Quaternion.Lerp(currentOvrFrame.HeadRotation, nextOvrFrame.HeadRotation,
                    timer / _frameInterval);
                var leftPosition = Vector3.Lerp(currentOvrFrame.LeftHandPosition, nextOvrFrame.LeftHandPosition,
                    timer / _frameInterval);
                var leftRotation = Quaternion.Lerp(currentOvrFrame.LeftHandRotation, nextOvrFrame.LeftHandRotation,
                    timer / _frameInterval);
                var rightPosition = Vector3.Lerp(currentOvrFrame.RightHandPosition, nextOvrFrame.RightHandPosition,
                    timer / _frameInterval);
                var rightRotation = Quaternion.Lerp(currentOvrFrame.RightHandRotation, nextOvrFrame.RightHandRotation,
                    timer / _frameInterval);
                 _simulator.SetRigTransforms(headPosition,headRotation,leftPosition,leftRotation,rightPosition,rightRotation);
                timer += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Stops the current playback and re-enables input.
        /// </summary>
        private void EndPlayBack()
        {
            //----------------------------
            // CALCULATE LENGTH
            //----------------------------
            _playbackStopwatch.Stop();
            RecordingTime temp;
            temp.Milliseconds = _playbackStopwatch.Elapsed.Milliseconds;
            temp.Seconds = _playbackStopwatch.Elapsed.Seconds;
            temp.Minutes = _playbackStopwatch.Elapsed.Minutes;
            Debug.Log($"Ended playback of {temp}");
            _simulator.IsInputEnabled = true;
            IsPlaying = false;
        }

        private bool ProcessFrameInput(OvrFrameInput input, out OVRInput.RawButton resultButton)
        {
            //We need to convert the string from OVRInput.Button ot OVRInput.RawButton
            //Manipulate string, so it can be parsed into an OVRInput.RawButton object
            var buttonName = input.InputActionName;
            
            // The primary and secondary buttons have to be completely renamed
            buttonName = buttonName switch
            {
                "One" => "A",
                "Two" => "B",
                "Three" => "X",
                "Four" => "Y",
                _ => buttonName
            };

            // "Primary" should become "L"
            // "Secondary" should become "R"
            buttonName = buttonName.Replace("Primary", "L");
            buttonName = buttonName.Replace("Secondary", "R");

            if (Enum.TryParse(buttonName, out OVRInput.RawButton btn))
            {
                Debug.Log(btn);
                resultButton = btn;
                return true;
            }

            resultButton = OVRInput.RawButton.None;
            return false;
        }

        private bool ProcessFrameInput(OvrFrameInput input, out OVRInput.RawAxis2D resultAxis)
        {
            var axisName = input.InputActionName;
            // "Primary" should become "L"
            // "Secondary" should become "R"
            axisName = axisName.Replace("Primary", "L");
            axisName = axisName.Replace("Secondary", "R");
            if (Enum.TryParse(axisName, out OVRInput.RawAxis2D axis))
            {
                resultAxis = axis;
                return true;
            }
            resultAxis = OVRInput.RawAxis2D.None;
            return false;
        }

        private bool ProcessFrameInput(OvrFrameInput input, out OVRInput.RawAxis1D resultAxis)
        {
            var axisName = input.InputActionName;
            // "Primary" should become "L"
            // "Secondary" should become "R"
            axisName = axisName.Replace("Primary", "L");
            axisName = axisName.Replace("Secondary", "R");
            if (Enum.TryParse(axisName, out OVRInput.RawAxis1D axis))
            {
                resultAxis = axis;
                return true;
            }

            resultAxis = OVRInput.RawAxis1D.None;
            return false;
        }
        
    }
}