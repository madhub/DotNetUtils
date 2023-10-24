using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;
internal class HttpClientHelpers
{
    public async Task UsingConnectCallback()
    {
        https://www.meziantou.net/forcing-httpclient-to-use-ipv4-or-ipv6-addresses-1.htm
        var client = new HttpClient(new SocketsHttpHandler()
        {
            ConnectCallback = async (context, cancellationToken) =>
            {
                // Use DNS to look up the IP addresses of the target host:
                // - IP v4: AddressFamily.InterNetwork
                // - IP v6: AddressFamily.InterNetworkV6
                // - IP v4 or IP v6: AddressFamily.Unspecified
                // note: this method throws a SocketException when there is no IP address for the host
                var entry = await Dns.GetHostEntryAsync(context.DnsEndPoint.Host, AddressFamily.InterNetwork, cancellationToken);

                // Open the connection to the target host/port
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

                // Turn off Nagle's algorithm since it degrades performance in most HttpClient scenarios.
                socket.NoDelay = true;

                try
                {
                    await socket.ConnectAsync(entry.AddressList, context.DnsEndPoint.Port, cancellationToken);

                    // If you want to choose a specific IP address to connect to the server
                    // await socket.ConnectAsync(
                    //    entry.AddressList[Random.Shared.Next(0, entry.AddressList.Length)],
                    //    context.DnsEndPoint.Port, cancellationToken);

                    // Return the NetworkStream to the caller
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            }
        });

        var response = await client.GetStringAsync("https://google.com");
        Console.WriteLine(response);
    }
}
