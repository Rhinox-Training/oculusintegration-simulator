using OVR.OpenVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInputTesting : MonoBehaviour
{
    [SerializeField] float RotationSpeed = 5f;


    private Vector3 _startPos;
    private Quaternion _Startrotation;
    private Vector3 _startScale;

    // Start is called before the first frame update
    void Start()
    {
        _startPos = gameObject.transform.position;
        _Startrotation = gameObject.transform.rotation;
        _startScale = gameObject.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        float deltaT = Time.deltaTime;

        //right controller A button
        if (OVRInput.Get(OVRInput.Button.One))
        {
            transform.position -= new Vector3(0f, .5f, 0f) * deltaT;
        }
        //right controller B button
        else if (OVRInput.Get(OVRInput.Button.Two))
        {
            transform.position += new Vector3(0f, .5f, 0f) * deltaT;
        }

        //lef controller X button
        if (OVRInput.Get(OVRInput.Button.Three))
        {
            transform.position -= new Vector3(.5f, 0f, 0f) * deltaT;
        }
        //lef controller Y button
        else if (OVRInput.Get(OVRInput.Button.Four))
        {
            transform.position += new Vector3(.5f, 0f, 0f) * deltaT;
        }

        if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick))
        {
            transform.position += new Vector3(0f, 0f, 1f);
        }
        else if (OVRInput.GetUp(OVRInput.Button.SecondaryThumbstick))
        {
            transform.position -= new Vector3(0f, 0f, 1f);
        }

        //reset when pressing down the left controller flush buttons
        if (OVRInput.GetDown(OVRInput.Button.Start))
        {
            gameObject.transform.position = _startPos;
            gameObject.transform.rotation = _Startrotation;
            gameObject.transform.localScale = _startScale;
        }


        var thumbMovement = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        thumbMovement *= RotationSpeed * deltaT;
        transform.rotation *= Quaternion.Euler(0, thumbMovement.x, thumbMovement.y);

        thumbMovement = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        thumbMovement *= RotationSpeed * deltaT;
        transform.rotation *= Quaternion.Euler(thumbMovement);

        //primary is left controller and secondary is right controllers
        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))//handtrigger is grip
        {
            transform.localScale += new Vector3(.1f, .1f, .1f);
        }
        else if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))//indextrigger is normal trigger
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
