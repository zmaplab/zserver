using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using ZServer.Interfaces.WMS;

namespace Client
{
    public class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await RunMainAsync();
        }

        private static Task<int> RunMainAsync()
        {
            try
            {
                var settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.None,
                    Async = true
                };


                XmlSerializer serializer = new XmlSerializer(typeof(StyledLayerDescriptor));
                var stream = File.ReadAllBytes("se.xml");
                var reader = new NamespaceIgnorantXmlTextReader(new MemoryStream(stream));
                var result = serializer.Deserialize(reader) as StyledLayerDescriptor;


                Console.ReadKey();

                // while (await reader.ReadAsync())
                // {
                //     switch (reader.NodeType)
                //     {
                //         case XmlNodeType.Element:
                //             Console.WriteLine("Start Element {0}", reader.Name);
                //             if (reader.Name == "StyledLayerDescriptor")
                //             {
                //                 styledLayerDescriptor = new StyledLayerDescriptor
                //                 {
                //                     NamedLayers = new List<NamedLayer>()
                //                 };
                //             }
                //             else if (reader.Name == "NamedLayer")
                //             {
                //                 NamedLayer namedLayer = await ReadNamedLayerAsync(reader);
                //             }
                //
                //             break;
                //         case XmlNodeType.Text:
                //             Console.WriteLine("Text Node: {0}",
                //                 await reader.GetValueAsync());
                //             break;
                //         case XmlNodeType.EndElement:
                //             if (reader.Name == "StyledLayerDescriptor")
                //             {
                //                  
                //             }
                //             else if (reader.Name == "NamedLayer")
                //             {
                //                 NamedLayer namedLayer = await ReadNamedLayerAsync(reader);
                //             }
                //             break;
                //         default:
                //             Console.WriteLine("Other node {0} with value {1}",
                //                 reader.NodeType, reader.Value);
                //             break;
                //     }
                // }

                // await using var client = await ConnectClient();
                // await DoClientWork(client);
                // Console.ReadKey();

                return Task.FromResult(0);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
                return Task.FromResult(1);
            }
        }

        // private static async Task<StyledLayerDescriptor> ReadStyledLayerDescriptorAsync(XmlReader reader)
        // {
        //     var styledLayerDescriptor =  new StyledLayerDescriptor();
        //
        //     await reader.ReadAsync();
        // }

        // private static async Task<IClusterClient> ConnectClient()
        // {
        //     var client = new HostBuilder()
        //         .UseLocalhostClustering()
        //         .Configure<ClusterOptions>(options =>
        //         {
        //             options.ClusterId = "dev";
        //             options.ServiceId = "ZServer";
        //         })
        //         .ConfigureLogging(logging => logging.AddConsole())
        //         .Build();
        //
        //     await client.Connect();
        //     Console.WriteLine("Client successfully connected to silo host \n");
        //     return client;
        // }

        private static Task DoClientWork(IClusterClient client)
        {
            // example of calling grains from the initialized client
            client.GetGrain<IWMSGrain>(Guid.NewGuid().ToString("N"));

            // foreach (var layer in layers.Split(',', StringSplitOptions.RemoveEmptyEntries))
            // {
            //     var info = layer.Split(':', StringSplitOptions.RemoveEmptyEntries);
            //     list.Add((info[0], info[1], filter));
            // }

            // var layers = new Layer[]
            // {
            //     new Layer("ah2021", "ygpd", "")
            // };
            // var response = await friend.GetMapAsync(layers,
            //     117.345703125F, 31.928339843750003F, 117.36767578125F, 31.950312500000003F, 1024, 1024, 180,
            //     "image/png",
            //     4326);
            //
            // await File.WriteAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "test.png"), response);
            // Console.WriteLine($"Get image success: {response.Length}");
            return Task.CompletedTask;
        }
    }
}