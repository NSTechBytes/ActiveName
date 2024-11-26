using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Rainmeter;

namespace ActiveName
{
    internal class Measure
    {
        private string[] skinNames; // Store multiple skin names
        private List<string> activeSkins; // List of active skin names
        private int currentIndex = -1; // Index to track which skin to return next
        private API api;  // Store an instance of API

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
                api.Log(API.LogType.Error, "ActiveName.dll: SkinSection parameter is not specified or empty.");
                return;
            }

            // Split the skin names based on the delimiter | and trim whitespace
            skinNames = skinSectionParam.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (skinNames.Length == 0)
            {
                api.Log(API.LogType.Error, "ActiveName.dll: No valid skin names provided.");
            }
        }

        // Function to read the Rainmeter.ini file and check for active skins
        private List<string> GetActiveSections()
        {
            // Path to Rainmeter.ini file (default path in AppData folder)
            string iniFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rainmeter", "Rainmeter.ini");

            // Ensure the file exists
            if (!File.Exists(iniFilePath))
            {
                api.Log(API.LogType.Error, "ActiveName.dll: Rainmeter.ini file not found.");
                return new List<string>();
            }

            // Read all lines from the ini file
            string[] lines = File.ReadAllLines(iniFilePath);

            // List to store active skin names
            List<string> activeSkins = new List<string>();

            // Check each specified skin name
            foreach (string skinName in skinNames)
            {
                string targetHeader = $"[{skinName.Trim()}\\Main]"; // Automatically add \Main to each skin name
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
            // Get the active skin sections
            activeSkins = GetActiveSections();

            // Increment the index to return the next active skin name
            if (activeSkins.Count > 0)
            {
                currentIndex++;

                // Reset to the first skin if we've gone through all active skins
                if (currentIndex >= activeSkins.Count)
                {
                    currentIndex = 0;
                }

                // Return the active skin name one by one
                string activeSkin = activeSkins[currentIndex];
                api.Log(API.LogType.Debug, $"ActiveName.dll: Active skin: {activeSkin}"); // Updated logging level
                return currentIndex; // Return index to allow Rainmeter to request the next skin
            }

            // No active skins found, reset the index
            currentIndex = -1;
            return -1; // Indicate no active skins
        }

        // Called by Rainmeter to retrieve a string value (for the current active skin name)
        internal string GetString()
        {
            if (currentIndex >= 0 && currentIndex < activeSkins.Count)
            {
                return activeSkins[currentIndex]; // Return the current active skin name
            }
            return string.Empty; // No active skin available
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
