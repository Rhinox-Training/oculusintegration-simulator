using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.XR.Oculus.Simulator
{
    public class OVRSimulatorControls : MonoBehaviour
    {
        [Tooltip("The desired cursor lock mode to toggle to from None (either Locked or Confined).")]
        public CursorLockMode DesiredCursorLockMode = CursorLockMode.Locked;

        [Tooltip("Speed of translation in the x-axis (left/right) when triggered by keyboard input.")]
        public float KeyboardXTranslateSpeed = 0.2f;

        [Tooltip("Speed of translation in the y-axis (up/down) when triggered by keyboard input.")]
        public float KeyboardYTranslateSpeed = 0.2f;

        [Tooltip("Speed of translation in the z-axis (forward/back) when triggered by keyboard input.")]
        public float KeyboardZTranslateSpeed = 0.2f;

        [Tooltip("Sensitivity of translation in the x-axis (left/right) when triggered by mouse input.")]
        public float MouseXTranslateSensitivity = 0.0004f;

        [Tooltip("Sensitivity of translation in the y-axis (up/down) when triggered by mouse input.")]
        public float MouseYTranslateSensitivity = 0.0004f;

        [Tooltip("Sensitivity of translation in the z-axis (forward/back) when triggered by mouse scroll input.")]
        public float MouseScrollTranslateSensitivity = 0.0002f;

        [Tooltip("Sensitivity of rotation along the x-axis (pitch) when triggered by mouse input.")]
        public float MouseXRotateSensitivity = 0.1f;

        [Tooltip("Sensitivity of rotation along the y-axis (yaw) when triggered by mouse input.")]
        public float MouseYRotateSensitivity = 0.1f;

        [Tooltip("Sensitivity of rotation along the z-axis (roll) when triggered by mouse scroll input.")]
        public float MouseScrollRotateSensitivity = 0.05f;

        [Tooltip("A boolean value of whether to invert the y-axis of mouse input when rotating by mouse input." +
                 "\nA false value (default) means typical FPS style where moving the mouse up/down pitches up/down." +
                 "\nA true value means flight control style where moving the mouse up/down pitches down/up.")]
        public bool MouseYRotateInvert;

        [Tooltip("The coordinate space in which keyboard translation should operate.")]
        public Space KeyboardTranslateSpace = Space.Local;

        [NonSerialized]
        public bool ManipulateRightControllerButtons = true;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
