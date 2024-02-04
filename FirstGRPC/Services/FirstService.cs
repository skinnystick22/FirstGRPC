using Basics;
using Grpc.Core;

namespace FirstGRPC.Services;

public class FirstService : FirstServiceDefinition.FirstServiceDefinitionBase
{
    public override Task<Response> Unary(Request request, ServerCallContext context)
    {
        var response = new Response { Message = $"{request.Content} from server" };
        return Task.FromResult(response);
    }

    public override async Task<Response> ClientStream(IAsyncStreamReader<Request> requestStream,
        ServerCallContext context)
    {
        var response = new Response { Message = "Received." };
        while (await requestStream.MoveNext())
        {
            var requestPayload = requestStream.Current;
            Console.WriteLine($"Received: {requestPayload.Content}");
            response.Message = requestPayload.ToString();
        }

        return response;
    }

    public override async Task ServerStream(Request request, IServerStreamWriter<Response> responseStream,
        ServerCallContext context)
    {
        for (var i = 0; i < 100; i++)
        {
            var response = new Response { Message = i.ToString() };
            await responseStream.WriteAsync(response);
        }
    }

    public override async Task BiDirectionalStream(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Response> responseStream,
        ServerCallContext context)
    {
        var response = new Response { Message = string.Empty };
        while (await requestStream.MoveNext())
        {
            var requestPayload = requestStream.Current;
            response.Message = requestPayload.ToString();
            await responseStream.WriteAsync(response);
        }
    }
}