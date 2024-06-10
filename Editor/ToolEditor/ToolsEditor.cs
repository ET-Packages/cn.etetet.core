﻿namespace ET
{
    public static class ToolsEditor
    {
        public static void ExcelExporter()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            const string tools = "./Tool";
#else
            const string tools = "dotnet.exe";
#endif
            ShellHelper.Run($"{tools} ./Bin/ET.ExcelExporter.dll", "./");
        }
        
        public static void Proto2CS()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            const string tools = "./Tool";
#else
            const string tools = "dotnet.exe";
#endif
            ShellHelper.Run($"{tools} ./Bin/ET.Proto2CS.dll", "./");
        }
    }
}