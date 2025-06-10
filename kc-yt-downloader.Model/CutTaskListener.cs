using System.Net;
using System.Text.Json;

namespace kc_yt_downloader.Model;

public class CutTaskListener
{        
    public CutTaskListener()
    {
        
    }

    public Task Listen(Action<CutTaskRequest> callback, CancellationToken cancellationToken)
    {
        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var listener = new HttpListener
        { 
            Prefixes = { "http://localhost:5000/api/cut/" }
        };

        listener.Start();

        while (!cancellationToken.IsCancellationRequested) 
        {
            try
            {
                var context = listener.GetContext();

                var request = context.Request;
                var response = context.Response;

                if (request.HttpMethod == "OPTIONS")
                {
                    response.AddHeader("Access-Control-Allow-Origin", "*");
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
            catch (HttpListenerException ex)
            {                
                
            }
        }

        return Task.CompletedTask;
    }
}
