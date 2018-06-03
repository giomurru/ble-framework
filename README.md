BLE framework is a Unity library that you can use to easily manage the Bluetooth communication between your mobile device and a ReadBearLab BLE boards.
The supported boards are:
* BLE Shield
* BLE Mini
* Blend Micro
* Blend
* BLE Nano
* RBL nRF51822

The ReadBearLab BLE boards can be attached to a micro controller such as Arduino to allow simple wireless serial communication with your mobile device.

The sample Unity project inside the folder Unity contains everything you need to get started.

Continue reading only if you want to modify the plugins or contribute to the development.

Android plugin it is in binary form and it is compressed in a jar file. The Android Studio project to generate the Android plugin is contained inside the folder AndroidPluginProject.

Copy the generated `BleFrameworkPlugin.jar` file inside the `Unity/Assets/Plugins/Android` folder in order to test with the sample app.

As regard iOS you can find the plugin source code inside the folder Unity/Assets/Plugins/iOS. In this case you don't need to create a binary. If you want to modify the plugin you can just modify the source code in this folder.

In Unity you can access the plugin by using the C# API contained inside the BLE folder. In particular you can use the static functions defined inside BLEController.cs and you can register to the events defined in BLEControllerEventHandler.cs
