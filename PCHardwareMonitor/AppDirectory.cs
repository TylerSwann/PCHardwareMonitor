using System;



public struct AppDirectory
{
    public static string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).TrimEnd('\\');
    public static string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public static string rootDirectory = $"{appData}/PCHardwareMonitor";
    public static string userSettings = $"{rootDirectory}/user.json";
    public static string defaultSettings = $"{rootDirectory}/default.json";
}