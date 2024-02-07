﻿using Basics;
using Grpc.Core;
using Grpc.Net.Client;

var options = new GrpcChannelOptions
{
    Credentials = ChannelCredentials.Insecure
};

using var channel = GrpcChannel.ForAddress("http://localhost:5242", options);
var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);

await BiDirectionalStreaming(client);
return;

static void Unary(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var request = new Request { Content = "Hello" };
    var response = client.Unary(request);
}

static async Task ClientStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using var call = client.ClientStream();
    for (var i = 0; i < 1_000; i++)
    {
        await call.RequestStream.WriteAsync(new Request { Content = i.ToString() });
    }

    await call.RequestStream.CompleteAsync();
    var response = await call;
    Console.WriteLine(response.Message);
}

static async Task ServerStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var request = new Request { Content = "Hello" };
    using var call = client.ServerStream(request);
    await foreach (var response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(response.Message);
    }
}

static async Task BiDirectionalStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using var call = client.BiDirectionalStream();
    for (var i = 0; i < 10; i++)
    {
        var request = new Request { Content = i.ToString() };
        Console.WriteLine(request);
        await call.RequestStream.WriteAsync(request);
    }

    while (await call.ResponseStream.MoveNext())
    {
        var message = call.ResponseStream.Current;
        Console.WriteLine(message);
    }

    await call.RequestStream.CompleteAsync();
}