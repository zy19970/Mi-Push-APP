using MiPush.Cli;

return await MainAsync(args);

static async Task<int> MainAsync(string[] args)
{
    try
    {
        if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
        {
            PrintHelp();
            return 0;
        }

        return args[0].ToLowerInvariant() switch
        {
            "config" => await HandleConfigAsync(args.Skip(1).ToArray()),
            "device" => await HandleDeviceAsync(args.Skip(1).ToArray()),
            "send" => await HandleSendAsync(args.Skip(1).ToArray()),
            _ => Fail($"未知命令：{args[0]}")
        };
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"错误：{ex.Message}");
        return 1;
    }
}

static async Task<int> HandleConfigAsync(string[] args)
{
    if (args.Length == 0) return Fail("用法：config init|show");
    var config = await ConfigStore.LoadAsync();

    if (args[0].Equals("show", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine($"配置文件：{ConfigStore.PathName}");
        Console.WriteLine($"Package: {config.PackageName}");
        Console.WriteLine($"API Base: {config.ApiBase}");
        Console.WriteLine($"Channel: {config.ChannelId ?? "(未设置)"}");
        Console.WriteLine($"Template: {config.TemplateId ?? "(未设置)"}");
        Console.WriteLine($"Devices: {config.Devices.Count}");
        Console.WriteLine("AppSecret: 不写入配置文件；读取 MIPUSH_APP_SECRET 或 --app-secret");
        return 0;
    }

    if (!args[0].Equals("init", StringComparison.OrdinalIgnoreCase))
        return Fail("用法：config init|show");

    var options = ParseOptions(args.Skip(1).ToArray());
    config.PackageName = RequiredOption(options, "package");
    if (options.TryGetValue("api-base", out var apiBase)) config.ApiBase = apiBase;
    if (options.TryGetValue("channel-id", out var channelId)) config.ChannelId = channelId;
    if (options.TryGetValue("template-id", out var templateId)) config.TemplateId = templateId;
    await ConfigStore.SaveAsync(config);
    Console.WriteLine($"已保存：{ConfigStore.PathName}");
    return 0;
}

static async Task<int> HandleDeviceAsync(string[] args)
{
    if (args.Length == 0) return Fail("用法：device add <name> <regid> | device list | device remove <name>");
    var config = await ConfigStore.LoadAsync();

    switch (args[0].ToLowerInvariant())
    {
        case "add":
            if (args.Length < 3) return Fail("用法：device add <name> <regid>");
            config.Devices[args[1]] = args[2];
            await ConfigStore.SaveAsync(config);
            Console.WriteLine($"已保存设备：{args[1]}");
            return 0;
        case "remove":
            if (args.Length < 2) return Fail("用法：device remove <name>");
            if (config.Devices.Remove(args[1])) await ConfigStore.SaveAsync(config);
            Console.WriteLine("完成。");
            return 0;
        case "list":
            foreach (var device in config.Devices)
                Console.WriteLine($"{device.Key}\t{Mask(device.Value)}");
            return 0;
        default:
            return Fail("用法：device add|list|remove");
    }
}

static async Task<int> HandleSendAsync(string[] args)
{
    if (args.Length < 3) return Fail("用法：send <device|regid> <title> <body> [options]");

    var target = args[0];
    var title = args[1];
    var body = args[2];
    var options = ParseOptions(args.Skip(3).ToArray());
    var config = await ConfigStore.LoadAsync();
    var regId = config.Devices.TryGetValue(target, out var savedRegId) ? savedRegId : target;
    var secret = options.GetValueOrDefault("app-secret") ?? Environment.GetEnvironmentVariable("MIPUSH_APP_SECRET") ?? "";
    var channelId = options.GetValueOrDefault("channel-id") ?? config.ChannelId;
    var templateId = options.GetValueOrDefault("template-id") ?? config.TemplateId;
    var templateParam = options.GetValueOrDefault("template-param-json");

    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    var client = new XiaomiPushClient(http);
    var result = await client.SendAsync(config, secret, regId, title, body, channelId, templateId, templateParam);

    Console.WriteLine($"HTTP {result.HttpStatus}");
    Console.WriteLine(result.Raw);
    return result.Success ? 0 : 2;
}

static Dictionary<string, string> ParseOptions(string[] args)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < args.Length; i++)
    {
        if (!args[i].StartsWith("--", StringComparison.Ordinal))
            throw new ArgumentException($"无法识别参数：{args[i]}");
        if (i + 1 >= args.Length)
            throw new ArgumentException($"参数缺少值：{args[i]}");
        result[args[i][2..]] = args[++i];
    }
    return result;
}

static string RequiredOption(Dictionary<string, string> options, string name) =>
    options.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new ArgumentException($"缺少 --{name}");

static int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}

static string Mask(string value)
{
    if (value.Length <= 12) return "***";
    return value[..6] + "..." + value[^6..];
}

static void PrintHelp()
{
    Console.WriteLine("""
MiPush.Cli - 本地设备直接调用 Xiaomi Push API

命令：
  config init --package <package> [--api-base <url>] [--channel-id <id>] [--template-id <id>]
  config show
  device add <name> <regid>
  device list
  device remove <name>
  send <device|regid> <title> <body> [--app-secret <secret>] [--channel-id <id>] [--template-id <id>] [--template-param-json <json>]

AppSecret 默认从环境变量 MIPUSH_APP_SECRET 读取，不写入本地配置文件。
""");
}
