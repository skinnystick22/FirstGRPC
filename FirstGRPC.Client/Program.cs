using Basics;
using Grpc.Core;
using Grpc.Net.Client;

var options = new GrpcChannelOptions
{
    Credentials = ChannelCredentials.Insecure
};

using var channel = GrpcChannel.ForAddress("http://localhost:5242", options);
var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);

await ServerStreaming(client);
return;

static void Unary(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var request = new Request { Content = "Hello" };
    var response = client.Unary(request, deadline: DateTime.UtcNow.AddMilliseconds(3));

    Console.WriteLine(response.Message);
}

static async Task ClientStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var cancellationTokenSource = new CancellationTokenSource();
    using var call = client.ClientStream(cancellationToken: cancellationTokenSource.Token);
    for (var i = 0; i < 1_000; i++)
    {
        await call.RequestStream.WriteAsync(new Request { Content = i.ToString() }, cancellationTokenSource.Token);
    }

    await call.RequestStream.CompleteAsync();
    var response = await call;
    Console.WriteLine(response.Message);
}

static async Task ServerStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var request = new Request { Content = "Hello" };
    var cancellationTokenSource = new CancellationTokenSource();
    using var call = client.ServerStream(request, cancellationToken: cancellationTokenSource.Token);
    await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationTokenSource.Token))
    {
        if (string.Equals(response.Message, "2", StringComparison.InvariantCultureIgnoreCase))
        {
            cancellationTokenSource.Cancel();
        }
        Console.WriteLine(response.Message);
    }
}

static async Task BiDirectionalStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var cancellationTokenSource = new CancellationTokenSource();

    using var call = client.BiDirectionalStream(cancellationToken: cancellationTokenSource.Token);
    for (var i = 0; i < 10; i++)
    {
        var request = new Request { Content = i.ToString() };
        Console.WriteLine(request);
        await call.RequestStream.WriteAsync(request, cancellationTokenSource.Token);
    }

    while (await call.ResponseStream.MoveNext(cancellationTokenSource.Token))
    {
        var message = call.ResponseStream.Current;
        Console.WriteLine(message);
    }

    await call.RequestStream.CompleteAsync();
}