using Grpc.Core;
using Grpc.Net.Client;
using GRPC_Example_Client.Protos;

namespace GRPC_Example_Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:7082");
            var client = new BidirectionalStreaming.BidirectionalStreamingClient(channel);
            using var call = client.SendMessage();

            Console.WriteLine("Starting background task to receive messages");
            var readTask = Task.Run(async () =>
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    var msg = "Msg content: " + response.Content + " | info:" + response.Info.ToString();
                    await Console.Out.WriteLineAsync(msg);
                }

                Console.WriteLine("End receiving messages");
            });

            Console.WriteLine("Starting to send messages");
            Console.WriteLine("Type a message to echo then press enter.");
            while (true)
            {
                var result = Console.ReadLine();
                if (string.IsNullOrEmpty(result))
                {
                   break;
                }

                await call.RequestStream.WriteAsync(new ClientToServerMsg { Content = result, Info = "Client to Server" });
            }


            Console.WriteLine("Disconnecting");

            await Task.WhenAny(readTask, call.RequestStream.CompleteAsync());
        }
    }
}