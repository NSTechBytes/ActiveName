using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Rainmeter;

namespace ActiveName
{
    internal class Measure
    {
        private string[] skinSections;
        private API api;
        private bool isFirstCheck = true;
        private IntPtr activeSkinNamesPtr = IntPtr.Zero;

        internal Measure() { }

        internal void Reload(Rainmeter.API api, ref double maxValue)
        {
            this.api = api;
            string skinSectionParam = api.ReadString("SkinSection", "");
            if (string.IsNullOrEmpty(skinSectionParam))
            {
                api.Log(API.LogType.Error, "ActiveName.dll: SkinSection parameter is not specified or empty.");
                return;
            }
            skinSections = skinSectionParam.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (skinSections.Length == 0)
            {
                api.Log(API.LogType.Error, "ActiveName.dll: No valid skin sections provided.");
            }
            isFirstCheck = true;
            FreeActiveSkinNames();
        }

        private List<string> GetActiveSections()
        {
            string iniFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rainmeter", "Rainmeter.ini");
            if (!File.Exists(iniFilePath))
            {
                api.Log(API.LogType.Error, "ActiveName.dll: Rainmeter.ini file not found.");
                return new List<string>();
            }
            string[] lines = File.ReadAllLines(iniFilePath);
            List<string> activeSkins = new List<string>();

            foreach (string section in skinSections)
            {
                string targetHeader = $"[{section.Trim()}]";
                bool sectionFound = false;
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (trimmedLine.Equals(targetHeader, StringComparison.OrdinalIgnoreCase))
                    {
                        sectionFound = true;
                        continue;
                    }
                    if (sectionFound && trimmedLine.StartsWith("Active="))
                    {
                        string activeValue = trimmedLine.Substring("Active=".Length);
                        if (int.TryParse(activeValue, out int result) && result == 1 && !activeSkins.Contains(section))
                        {
                            activeSkins.Add(section);
                        }
                        break;
                    }
                }
            }
            return activeSkins;
        }

        internal double Update()
        {
            List<string> activeSections = GetActiveSections();
            string activeSkinNames = string.Join("|", activeSections);
            FreeActiveSkinNames();
            activeSkinNamesPtr = Marshal.StringToHGlobalUni(activeSkinNames);
            if (isFirstCheck)
            {
                string onFirstCheckAction = api.ReadString("OnFirstCheckAction", "");
                if (!string.IsNullOrEmpty(onFirstCheckAction))
                {
                    api.Execute(onFirstCheckAction);
                    api.Log(API.LogType.Debug, "ActiveName.dll: OnFirstCheckAction executed.");
                }
                isFirstCheck = false;
            }
            return activeSections.Count;
        }

        internal IntPtr GetStringPtr()
        {
            return activeSkinNamesPtr;
        }

        internal void FreeActiveSkinNames()
        {
            if (activeSkinNamesPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(activeSkinNamesPtr);
                activeSkinNamesPtr = IntPtr.Zero;
            }
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
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.FreeActiveSkinNames();
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Reload(new Rainmeter.API(rm), ref maxValue);
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
            return measure.GetStringPtr();
        }
    }
}
