using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine.InputSystem.LowLevel;

namespace Rhinox.XR.UnityXR.Simulator
{
    /// <summary>
    /// This class holds the data of a simulation recording.
    /// </summary>
    [XmlRoot(ElementName = "SimulationRecording")]
    public class SimulationRecording
    {
        [XmlElement(ElementName = "FrameRate", Order = 1)]
        public int FrameRate = 30;
        
        [XmlElement(ElementName = "AmountOfFrames", Order = 2)]
        public int AmountOfFrames { get; set; }

        [XmlElement(ElementName = "RecordingLength", Order = 3)]
        public RecordingTime RecordingLength { get; set; }
        
        [XmlArray(ElementName = "Frames", Order = 4)] 
        public List<OvrFrameData> Frames = new List<OvrFrameData>();

        /// <summary>
        /// Adds a new frame to the recording. 
        /// </summary>
        /// <param name="newOvrFrame">The frame that gets appended to the recording.</param>
        public void AddFrame(OvrFrameData newOvrFrame)
        {
            newOvrFrame.FrameNumber = AmountOfFrames;

            //Add the frame.
            Frames.Add(newOvrFrame);
            AmountOfFrames++;
        }

        public void AddEmptyFrame()
        {
            AmountOfFrames++;
        }
    }

    public struct RecordingTime
    {
        [XmlElement(ElementName = "Minutes",Order = 1)]
        public int Minutes;

        [XmlElement(ElementName = "Seconds", Order = 2)]
        public int Seconds;

        [XmlElement(ElementName = "Milliseconds", Order = 3)]
        public int Milliseconds;

        public override string ToString()
        {
            return $"minutes: {Minutes}, seconds: {Seconds}, milliseconds: {Milliseconds}";
        }
    }
}