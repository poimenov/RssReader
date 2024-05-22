using System;
using System.IO;
using System.Text.Json;
using Avalonia.Styling;

namespace RssReader.MVVM;

public class AppSettings
{
    public const string APPLICATION_NAME = "RssReader";
    public const string JSON_FILE_NAME = "appsettings.json";
    public static string AvaResPath => $"avares://{APPLICATION_NAME}.MVVM/Assets";
    public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APPLICATION_NAME);
    public string? DefaultLanguage { get; set; }
    public string? DefaultTheme { get; set; }
    public ThemeVariant GetTheme()
    {
        switch (DefaultTheme)
        {
            case "Light":
                return ThemeVariant.Light;
            case "Dark":
                return ThemeVariant.Dark;
            default:
                return ThemeVariant.Default;
        }
    }
    public void SetTheme(ThemeVariant theme)
    {
        DefaultTheme = theme?.Key?.ToString();
    }

    public void Save()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, JSON_FILE_NAME);
        if (File.Exists(filePath))
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, jsonString);
        }
    }
}
