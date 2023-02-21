#if USING_XR_MANAGEMENT && USING_XR_SDK_OCULUS
#define USING_XR_SDK
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Assets.OculusSimulator.Scripts.Simulator
{
    [DefaultExecutionOrder(-1000000)]
    public class FixOVRSimulator : MonoBehaviour
    {
        private void Awake()
        {
#if USING_XR_SDK
            if (!OVRManager.OVRManagerinitialized && !OVRPlugin.initialized)
            {
                var method = typeof(OVRManager).GetMethod("InitOVRManager", BindingFlags.Instance | BindingFlags.NonPublic);
                if (method != null)
                {
                    var ovrManager = GameObject.FindObjectOfType<OVRManager>();
                    method.Invoke(ovrManager, null);
                }
            }
#endif
        }
    }
}
