using System.Net;
using System.Text.Json;

namespace kc_yt_downloader.Model;

public static class CutTaskListener
{
    public static Task Listen(string prefix, Action<CutTaskRequest> callback, CancellationToken cancellationToken)
    {
        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var listener = new HttpListener
        {
            Prefixes = { prefix }
        };

        listener.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var context = listener.GetContext();

                var request = context.Request;
                var response = context.Response;

                response.AddHeader("Access-Control-Allow-Origin", "*");

                if (request.HttpMethod == "OPTIONS")
                {
                    response.AddHeader("Access-Control-Allow-Methods", "POST, OPTIONS");
                    response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
                    response.StatusCode = 204;
                    response.Close();
                    continue;
                }

                if (context.Request.HttpMethod != "POST")
                    continue;

                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var requestModel = JsonSerializer.Deserialize<CutTaskRequest>(body, jsonSerializerOptions);


                response.StatusCode = 200;
                response.Close();

                if (requestModel is not null)
                    callback?.Invoke(requestModel);
            }
            catch
            {

            }
        }

        return Task.CompletedTask;
    }
}
