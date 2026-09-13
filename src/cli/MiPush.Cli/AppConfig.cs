using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiPush.Cli;

internal sealed class AppConfig
{
    public string PackageName { get; set; } = "";
    public string ApiBase { get; set; } = "https://api.xmpush.xiaomi.com";
    public string? ChannelId { get; set; }
    public string? TemplateId { get; set; }
    public Dictionary<string, string> Devices { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal static class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string PathName
    {
        get
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrWhiteSpace(root))
                root = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(root, "MiPushApp", "config.json");
        }
    }

    public static async Task<AppConfig> LoadAsync()
    {
        if (!File.Exists(PathName)) return new AppConfig();
        await using var stream = File.OpenRead(PathName);
        return await JsonSerializer.DeserializeAsync<AppConfig>(stream, JsonOptions) ?? new AppConfig();
    }

    public static async Task SaveAsync(AppConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(PathName)!);
        await using var stream = File.Create(PathName);
        await JsonSerializer.SerializeAsync(stream, config, JsonOptions);
    }
}
