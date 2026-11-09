using System.Net;
using System.Text.Json;

namespace RaceDay.Mvc;

public sealed class ApiFailure(string message, HttpStatusCode status) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
}

public sealed class ApiClient(HttpClient http, IHttpContextAccessor context)
{
    private ISession Session => context.HttpContext!.Session;

    public Task<T> Get<T>(string path) => Read<T>(HttpMethod.Get, path);

    public async Task<T> Read<T>(HttpMethod method, string path, object? body = null)
    {
        using var response = await Send(method, path, body is null ? null : JsonContent.Create(body));
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public async Task Write(HttpMethod method, string path, object? body = null)
    {
        using var response = await Send(method, path, body is null ? null : JsonContent.Create(body));
    }

    public async Task Upload(string path, IFormFile file)
    {
        if (file.Length > 5_242_880)
            throw new ApiFailure("Choose an image of 5 MB or less.", HttpStatusCode.BadRequest);

        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(file.OpenReadStream()), "file", "image");
        using var response = await Send(HttpMethod.Post, path, form);
    }

    public async Task<(byte[] Bytes, string ContentType)> Image(string path)
    {
        using var response = await Send(HttpMethod.Get, path, null);
        return (await response.Content.ReadAsByteArrayAsync(), response.Content.Headers.ContentType!.MediaType!);
    }

    private async Task<HttpResponseMessage> Send(HttpMethod method, string path, HttpContent? content)
    {
        using var request = new HttpRequestMessage(method, path) { Content = content };
        var cookiesToSend = new[] { Session.GetString("RaceDay.Auth"), Session.GetString(".Assignment2.Api.Session") };
        var cookieHeader = string.Join("; ", cookiesToSend.OfType<string>());
        if (cookieHeader.Length > 0)
            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);

        HttpResponseMessage response;
        try { response = await http.SendAsync(request, context.HttpContext!.RequestAborted); }
        catch (HttpRequestException)
        {
            throw new ApiFailure("RaceDay is temporarily unavailable. Try again shortly.", HttpStatusCode.ServiceUnavailable);
        }
        catch (TaskCanceledException) when (!context.HttpContext!.RequestAborted.IsCancellationRequested)
        {
            throw new ApiFailure("RaceDay took too long to respond. Try again shortly.", HttpStatusCode.ServiceUnavailable);
        }

        if (response.IsSuccessStatusCode)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var header in cookies)
                {
                    var pair = header.Split(';', 2)[0];
                    var name = pair.Split('=', 2)[0];
                    if (name is "RaceDay.Auth" or ".Assignment2.Api.Session") Session.SetString(name, pair);
                }
            }
            return response;
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized && path != "api/auth/login")
                Session.Clear();

            var message = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Please check your sign-in details or sign in again.",
                HttpStatusCode.Forbidden => "You do not have access to this action.",
                HttpStatusCode.NotFound => "This item could not be found.",
                _ => "RaceDay could not complete the request."
            };

            if ((int)response.StatusCode < 500)
            {
                try
                {
                    var details = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (details.ValueKind == JsonValueKind.Object)
                    {
                        if (details.TryGetProperty("message", out var value)) message = value.GetString() ?? message;
                        else if (details.TryGetProperty("errors", out var errors))
                            message = string.Join(" ", errors.EnumerateObject().SelectMany(error =>
                                error.Value.EnumerateArray().Select(item => item.GetString())));
                    }
                }
                catch (JsonException) { }
                catch (NotSupportedException) { }
            }
            throw new ApiFailure(message, response.StatusCode);
        }
    }
}
