# Oculus Integration - Simulator

Simulator to use Oculus Integration Toolkit without VR hardware
(mouse & keyboard)

# Dependencies

Oculus Integration Toolkit
https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022


# SETUP

## Setting up the Simulator
You can either use the **OVRCameraRig** Variant which has the simulator already included, this you can just drag and drop into your scene like the normal **OVRCameraRig**.  
You can also use the normal **OVRCameraRig** that may already exist in your scene and then attach the simulator prefab under it afterwards.

- **OVRCameraRig** Variant prefab  
![OVRCameraRig Variant](https://user-images.githubusercontent.com/76707656/221580217-dab6c536-2421-44da-9e55-314df09e616a.png)  

- **OVRCameraRig** with Simulator prefab attached  
![OVRCameraRig with Simulator attached](https://user-images.githubusercontent.com/76707656/221580431-1daaf75f-c4c9-40c7-93aa-33002002dd27.png)  


In the OclulusSimulator prefab there are 3 scripts attached.  
The first script is `OculusDeviceSimulatorControls`

![OVRSimulatorControls Script](https://user-images.githubusercontent.com/76707656/221784425-178b9119-b173-40ed-bd2a-cef7da064db6.png)

This script takes all the Input actions to:
- Move the headset and contollers.
- Change the space to either: local, parent or screen space.
- Change which controller's buttons you are manipulating. 
- Actually set the input buttons of the **OVRCameraRig**. 

#### NOTE
You can make your own input action mappings but they need to be slotted into the above places to correctly simulate the **OVRCameraRig** buttons.  
The Input Action Mapping also needs to be inserted in the below `Input Action Manager`.

The 2 scripts below here, are for the simulator logic itself and a small script to enable the input actions.

![Device simulator script and input action manager](https://user-images.githubusercontent.com/76707656/221581792-b131b649-7964-4c06-a9f4-db5ebb231d73.png)

#### NOTE
When adding the simulator prefab to an existing OVRCamerageRig then you must link the `CenterEyeAnchor` transform from the **OVRCameraRig** into the `Camera Transform` slot.


## Setting up the Recorder/Playback

Drag the `RecorderPlayback` prefab onto the **OVRCameraRig**.

![OVRCameraRig Variant with Recorder/Playback](https://user-images.githubusercontent.com/76707656/221591053-d9008172-86f0-4576-9c94-5a80c642bdfe.png)

### Recorder Settings

Under `Device Transforms`

Link the following:
- `Simulator` needs the `OculusSimulator` Script.
- `Head Transform` needs the `CenterEyeAnchor` from the **OVRCameraRig**.
- `Left Hand Transform` needs the `LeftHandAnchor` from the **OVRCameraRig**.
- `Right Hand Transform` needs the `RightHandAnchor` from the **OVRCameraRig**.

![Recorder](https://user-images.githubusercontent.com/76707656/221591706-a1ab8b98-a1c5-47fe-bca2-c336f7948c41.png)

### Playback Settings

The PlaybackScript under the `RecorderPlayback` only needs the `OculusSimulator` Script linked in the `Simulator` slot under the Input Parameters. 

![Playback](https://user-images.githubusercontent.com/76707656/221591866-594ab150-4a5f-48a1-8373-697045d74bc9.png)

## Setting up the Input Visualizer

The Input Visualizer can just be placed in the scene, but the follwing things need to be linked.

![Input Visualizer](https://user-images.githubusercontent.com/76707656/221597541-e3bee99b-dce6-49c7-8043-6f380652f999.png)

Link the following:
- `Device Simulator Controls` needs the `OculusSimulator (Oculus Device Simulator Controls)` Script.
- `Device Simulator` needs the `OculusSimulator (Oculus Device Simulator)` Script.
- `Recorder` needs the `Recorder` Script.
- `Playback` needs the `Playback` Script.


# USAGE

## The Simulator

![Editor Input Visualizer Window](https://user-images.githubusercontent.com/76707656/221621796-cb3a4893-8c3e-4d7c-a865-7310d59baab5.png)

When running the game inside the editor and having the InputVisualizer also included in the scene you can see the above window in the Top right conner of the game screen.  
Inside the square bracket is the button to trigger the action.

[Tab] Mode:
- All -> In this mode, when you move via the W,A,S,D keys both the headset and controllers will move. The same happens when moving the mouse and scroll wheel to rotate the view. 
- Head -> In this mode you only affect the heaset. The controllers will stay stationary in space.
- RightHand -> In this mode you only affect the Right Hand Controller. The other controller will stay stationary in space and the same goes for the headset.
- LeftHand -> In this mode you only affect the Left Hand Controller. The other controller will stay stationary in space and the same goes for the headset.

[T] Keyboard Space:
- Local -> When using the keyboard W,A,S,D keys you move the object (that was selected via the [Tab] Mode) in its local space, when pressing forward, you will go forward along its forward vector.
- Parent -> When using the keyboard W,A,S,D keys you move the object (that was selected via the [Tab] Mode) in its parent space, when pressing when pressing forward, you will go forward along the world forward vector.
- Screen -> When using the keyboard W,A,S,D keys you move the object (that was selected via the [Tab] Mode) in screen space, when pressing when pressing forward, you will go forward along the screens (headset cameras) forward vector.

[F] Controller Buttons:
- Right -> Manipulate the buttons and joystick of the Right controller.
- Left -> Manipulate the buttons and joystick of the Left controller.

## The Recorder/Playback
### Recorder
![Recorder](https://user-images.githubusercontent.com/76707656/221591706-a1ab8b98-a1c5-47fe-bca2-c336f7948c41.png)

First you need to select the Output Directory for the recordings, otherwise the recordings cannot be saved.

Below this is the name of the recording file, the textfield is to give in a new name for the recording and the dropdown menu right below it is to select an existing file to overwrite it.

##### Recording Parameters:
- `Start on Awake` will start the recording with the chosen name when the session is started.
- `End on Destroy` will stop the recording with the chosen name when the session is ended.

When setting the desired fps of the recording, also change the deadzones.  
The higher the desired fps, the lower the deadzone values should be. These values might take some trial and error to match perfectly.  
Some reference fps and thir deadzone value (positional deadzone and rotational deadzone are the same here):

- 120 fps -> 0.001
- 75 fps  -> 0.002
- 60 fps  -> 0.005
- 30 fps  -> 0.05
- 1 fps   -> 0.1

### Playback
![Playback](https://user-images.githubusercontent.com/76707656/221591866-594ab150-4a5f-48a1-8373-697045d74bc9.png)

To get a recording to play it back.  
Press the `Import file` button and go to the directory of the recorderd file.  
Double click the file to load it in.

### How to **Make** and **Play** a recording

There are 2 ways to start a recording.
- Check the `Start on Awake` box inside the recording paramters of the recording script.
- Press the `F1` button when running the game inside the editor.

There are 2 ways to save a recording.
- Check the `End on Destroy` box inside the recording paramters of the recording script.
- Press the `F2` button when running the game inside the editor.

To replay a recording.  
Press the `F3` button when running the game inside the editor.  
It will then dissable the simulators input and play the recording until it is done.

## The Input Visualizer

There isn't much extra usage other than showing all the metrics of where the headset and controllers are.
Also shows which one you are currently operating and in which space.

![Editor Input Visualizer Window](https://user-images.githubusercontent.com/76707656/221621796-cb3a4893-8c3e-4d7c-a865-7310d59baab5.png)

Under the **USED INPUT** there will be a list of all the buttons that are pressed in the simulation.


# License

Apache-2.0 © Rhinox NV
