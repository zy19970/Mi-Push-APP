using System.Text.Json;

namespace MiPush.Cli;

internal sealed class XiaomiPushClient(HttpClient httpClient)
{
    public async Task<PushResult> SendAsync(
        AppConfig config,
        string appSecret,
        string regId,
        string title,
        string body,
        string? channelId,
        string? templateId,
        string? templateParamJson,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(config.PackageName))
            throw new InvalidOperationException("尚未配置 package name。请先运行 config init。 ");
        if (string.IsNullOrWhiteSpace(appSecret))
            throw new InvalidOperationException("缺少 AppSecret。设置 MIPUSH_APP_SECRET 或使用 --app-secret。 ");

        var fields = new Dictionary<string, string>
        {
            ["restricted_package_name"] = config.PackageName,
            ["registration_id"] = regId,
            ["title"] = title,
            ["description"] = body
        };

        if (!string.IsNullOrWhiteSpace(channelId)) fields["extra.channel_id"] = channelId;
        if (!string.IsNullOrWhiteSpace(templateId)) fields["extra.template_id"] = templateId;
        if (!string.IsNullOrWhiteSpace(templateParamJson))
        {
            _ = JsonDocument.Parse(templateParamJson);
            fields["extra.template_param"] = templateParamJson;
        }

        var endpoint = config.ApiBase.TrimEnd('/') + "/v3/message/regid";
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new FormUrlEncodedContent(fields)
        };
        request.Headers.TryAddWithoutValidation("Authorization", $"key={appSecret}");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        return PushResult.From(response.IsSuccessStatusCode, (int)response.StatusCode, raw);
    }
}

internal sealed record PushResult(bool HttpSuccess, int HttpStatus, int? Code, string? Result, string? Description, string Raw)
{
    public bool Success => HttpSuccess && (Code is null || Code == 0) && !string.Equals(Result, "error", StringComparison.OrdinalIgnoreCase);

    public static PushResult From(bool httpSuccess, int httpStatus, string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            int? code = root.TryGetProperty("code", out var codeNode) && codeNode.TryGetInt32(out var c) ? c : null;
            string? result = root.TryGetProperty("result", out var resultNode) ? resultNode.GetString() : null;
            string? description = root.TryGetProperty("description", out var descriptionNode) ? descriptionNode.GetString() : null;
            return new PushResult(httpSuccess, httpStatus, code, result, description, raw);
        }
        catch
        {
            return new PushResult(httpSuccess, httpStatus, null, null, null, raw);
        }
    }
}
