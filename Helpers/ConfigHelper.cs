namespace Prism.Helpers;

internal static class ConfigHelper
{
    public static string GetConfigPath()
    {
        var configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        var configFilePath = Path.Combine(configDirectory, "Prism.json");
        if (!File.Exists(configFilePath))
        {
            File.WriteAllText(configFilePath, "{ \"Processes\": [] }");
        }

        return configFilePath;
    }
}