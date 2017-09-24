BLE framework is a Unity library that you can use to easily manage the Bluetooth communication between your mobile device and a BLE module such as ReadBearLab BLE Mini.
The ReadBearLab BLE mini can be attached to a micro controller such as Arduino to allow simple wireless serial communication with your mobile device.

To get started you can use the sample Unity project in the folder Unity. It already contains everything you need to get started.

If you want to modify the plugin or study the source code, please continue reading.

Android plugin it is in binary form and it is compressed in a jar file. The project to generate the Android plugin is contained inside the folder AndroidPlugin.
To generate the plugin jar you need to install ant. You can download it from the official website:
http://ant.apache.org/bindownload.cgi

To generate the plugin launch this command inside the AndroidPlugin folder:

`ant jar`

Then, copy the generated `BleFrameworkPlugin.jar` file inside the `Unity/Assets/Plugins/Android` folder.

`cp bin/BleFrameworkPlugin.jar ../Unity/Assets/Plugins/Android`

Finally you can build the project in Unity for Android.

As regard iOS you can find the plugin source code inside the folder Unity/Assets/Plugins/iOS. In this case you don't need to create a binary. If you want to modify the plugin you can just modify the source code in this folder.

In Unity you can access the plugin by using the C# API contained inside the BLE folder. In particular you can use the static functions defined inside BLEController.cs and you can register to the events defined in BLEControllerEventHandler.cs
