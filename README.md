

# ActiveName Plugin for Rainmeter

**ActiveName** is a Rainmeter plugin designed to check the active status of specific skins defined in your Rainmeter configuration file (`Rainmeter.ini`). It allows you to dynamically monitor and retrieve active skin names in a Rainmeter skin.

## Features
- **Supports Multiple Skins**: You can specify multiple skin names in the `SkinSection` parameter.
- **Checks Active Skin Status**: Reads the `Active=` setting from each skin's section in `Rainmeter.ini` to check if it is active.
- **Output Options**: Returns the names of all active skins.
  
## Installation

1. **Download or Clone the Repository**:  
   Clone or download the plugin repository to your Rainmeter plugin folder.

2. **Place the Plugin in the Rainmeter Plugin Folder**:  
   Move the compiled DLL (`ActiveName.dll`) to your Rainmeter `Plugins` folder.

3. **Configure Your Skin**:  
   Open your Rainmeter skin's `.ini` file and add the following:
   ```ini
   [MeasureActiveName]
   Measure=Plugin
   Plugin=ActiveName.dll
   SkinSection=ModernSearchBar|MiniAlarm
   ;only available in version 1.1
   OnFirstCheckAction=[!Log "Checking skins for the first time"]
   ```

   Replace `ModernSearchBar|MiniAlarm` with the names of the skins you want to monitor.

4. **Reload the Skin**:  
   Reload the skin in Rainmeter for changes to take effect.

## Parameters

- **SkinSection**: A list of skin names separated by a `|`. This plugin will check the active status of each skin section.
  
- **Output**: 
  - **Name**: Returns the name of the active skins one by one.
  - **Numeric**: This feature is disabled. For now, it only supports returning active skin names.

## Usage Example

### In your Rainmeter skin `.ini` file:

```ini
[MeasureActiveName]
Measure=Plugin
Plugin=ActiveName.dll
SkinSection=ModernSearchBar|MiniAlarm
;only available in version 1.1
 OnFirstCheckAction=[!Log "Checking skins for the first time"]
```

- This will return the names of all active skins in `ModernSearchBar` and `MiniAlarm`, and update accordingly.

## Logging

The plugin provides detailed logs via Rainmeter's `Debug` log level for troubleshooting. This helps in checking which skins are active and why certain skins may not be returning active status.

## License

This plugin is provided under the [Appache License](LICENSE).
