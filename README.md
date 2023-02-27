# Oculus Integration - Simulator

Simulator to use Oculus Integration Toolkit without VR hardware
(mouse & keyboard)

# Dependencies

Oculus Integration Toolkit
https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022

## Setting up the simulator
You can either use the OVRCameraRig Variant which has the simulator included, this you can just drag into your scene.  
you can also use the normal OVRCameraRig and then attach the simulator prefab under it afterwards. 

![OVRCameraRig Variant](https://user-images.githubusercontent.com/76707656/221580217-dab6c536-2421-44da-9e55-314df09e616a.png)  
OVRCameraRig Variant prefab

![OVRCameraRig with Simulator attached](https://user-images.githubusercontent.com/76707656/221580431-1daaf75f-c4c9-40c7-93aa-33002002dd27.png)  
OVRCameraRig with Simulator prefab attached

In the OclulusSimulator prefab you have 3 scripts attached.  
The first script is `OculusDeviceSimulatorControls`

This script takes all the Input actions to:
- move the headset and contollers.
- change the space to either: local, parent or screenspace.
- change which controller's buttons you are manipulating. 

The 2 scripts below that are the simulator logic itself and a small script to enable the input actions.

![image](https://user-images.githubusercontent.com/76707656/221581792-b131b649-7964-4c06-a9f4-db5ebb231d73.png)

### NOTE
When adding the simulator prefab to an existing OVRCamerageRig then you must link the `CenterEyeAnchor` transform from the OVRCameraRig into the `Camera Transform` slot.


## Setting up the Recorder/Playback



## Setting up the Input Visualizer



# License

Apache-2.0 © Rhinox NV
