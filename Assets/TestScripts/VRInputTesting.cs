using OVR.OpenVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInputTesting : MonoBehaviour
{
    [SerializeField] float RotationSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.One))
        {
            transform.position -= new Vector3(0, .5f, 0);
        }
        else if (OVRInput.Get(OVRInput.Button.Two))
        {
            transform.position += new Vector3(0, .5f, 0);
        }

        var thumbMovement = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        thumbMovement *= Time.deltaTime * RotationSpeed;

        transform.rotation *= Quaternion.Euler(thumbMovement);
    }
}
