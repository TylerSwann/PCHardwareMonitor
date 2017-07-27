using System;

public struct AppDirectory
{
    public static string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).TrimEnd('\\');
    public static string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public static string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    public static string startupFolder = $"{programData}/Microsoft/Windows/Start Menu/Programs/Startup";
    public static string startupExe = $"{startupFolder}/PC Hardware Monitor.exe";
    public static string rootDirectory = $"{appData}/PCHardwareMonitor";
    public static string userSettings = $"{rootDirectory}/user.json";
    public static string defaultSettings = $"{rootDirectory}/default.json";
    public static string applicationExe = $"{programFiles}/Tyler Swann/PC Hardware Monitor/PC Hardware Monitor.exe";
}