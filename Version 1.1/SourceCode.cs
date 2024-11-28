using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Rainmeter;

namespace PluginSkinStatus
{
    internal class Measure
    { 
        private string[] skinNames; // Store multiple skin names
        private string activeSkinNames; // To store active skin names
        private API api;  // Store an instance of API
        private bool isFirstCheck = true; // Track if this is the first update

        internal Measure()
        {
        }

        // Set up the measure with parameters from the Rainmeter skin
        internal void Reload(Rainmeter.API api, ref double maxValue)
        {
            this.api = api;  // Assign the API instance

            string skinSectionParam = api.ReadString("SkinSection", "");  // Read the skin names
            if (string.IsNullOrEmpty(skinSectionParam))
            {
                api.Log(API.LogType.Error, "PluginSkinStatus.dll: SkinSection parameter is not specified or empty.");
                return;
            }

            // Split the skin names based on the delimiter | and trim whitespace
            skinNames = skinSectionParam.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (skinNames.Length == 0)
            {
                api.Log(API.LogType.Error, "PluginSkinStatus.dll: No valid skin names provided.");
            }

            // Reset the first check flag when reloading
            isFirstCheck = true;
        }

        // Function to read the Rainmeter.ini file and check for active skins
        private List<string> GetActiveSections()
        {
            // Path to Rainmeter.ini file (default path in AppData folder)
            string iniFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rainmeter", "Rainmeter.ini");

            // Ensure the file exists
            if (!File.Exists(iniFilePath))
            {
                api.Log(API.LogType.Error, "PluginSkinStatus.dll: Rainmeter.ini file not found.");
                return new List<string>();
            }

            // Read all lines from the ini file
            string[] lines = File.ReadAllLines(iniFilePath);

            // List to store active skin names
            List<string> activeSkins = new List<string>();

            // Check each specified skin name
            foreach (string skinName in skinNames)
            {
                string targetHeader = $"[{skinName.Trim()}\\Main]"; // Automatically add \Main
                bool sectionFound = false;

                foreach (string line in lines)
                {
                    // Check if we're in the target section
                    if (line.Trim() == targetHeader)
                    {
                        sectionFound = true;
                        continue;
                    }

                    // Once in the section, find Active setting
                    if (sectionFound)
                    {
                        if (line.Trim().StartsWith("Active="))
                        {
                            string activeValue = line.Trim().Substring("Active=".Length);
                            if (int.TryParse(activeValue, out int result) && result == 1)
                            {
                                if (!activeSkins.Contains(skinName))
                                {
                                    activeSkins.Add(skinName); // Add unique active skin name
                                }
                            }
                            break; // Stop checking this section once Active is found
                        }
                    }
                }
            }

            return activeSkins;
        }

        // Called by Rainmeter to update the measure
        internal double Update()
        {
            // On first check, execute OnFirstCheckAction
            if (isFirstCheck)
            {
                string onFirstCheckAction = api.ReadString("OnFirstCheckAction", "");
                if (!string.IsNullOrEmpty(onFirstCheckAction))
                {
                    api.Execute(onFirstCheckAction); // Execute the action
                    api.Log(API.LogType.Debug, "PluginSkinStatus.dll: OnFirstCheckAction executed.");
                }
                isFirstCheck = false; // Reset the flag after the first execution
            }

            // Get the active skin sections
            List<string> activeSections = GetActiveSections();

            // Save the active skin names as a |-separated string
            activeSkinNames = string.Join("|", activeSections);

            return 0; // Placeholder since numeric output is no longer needed
        }

        // Called by Rainmeter to retrieve a string value
        internal string GetString()
        {
            return activeSkinNames; // Return the active skin names
        }
    }

    public static class Plugin
    {
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Reload(new Rainmeter.API(rm), ref maxValue);  // Pass the API instance here
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }

        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            string value = measure.GetString();
            return Marshal.StringToHGlobalUni(value);
        }
    }
}
