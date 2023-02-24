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

        var thumbMovement = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        thumbMovement *= Time.deltaTime * RotationSpeed;
        transform.rotation *= Quaternion.Euler(0, thumbMovement.x, thumbMovement.y);

        thumbMovement = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        thumbMovement *= Time.deltaTime * RotationSpeed;
        transform.rotation *= Quaternion.Euler(thumbMovement);

        //primary is left controller and secondary is right controllers
        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
        {
            transform.localScale += new Vector3(.1f, .1f, .1f);
        }
        else if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            transform.localScale -= new Vector3(.1f, .1f, .1f);
        }
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            transform.localScale += new Vector3(.5f, .5f, .5f);
        }
        else if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            transform.localScale -= new Vector3(.5f, .5f, .5f);
        }
    }
}
