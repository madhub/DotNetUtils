using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;
internal static class DockerClientHelper
{

    /// <summary>
    /// Create HttpClient to Docker Engine using NamedPipe & UnixDomain
    /// </summary>
    /// <returns></returns>
    public static HttpClient CreateHttpClientConnectionToDockerEngine()
    {
        SocketsHttpHandler socketsHttpHandler =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) switch
            {
                true => GetSocketHandlerForNamedPipe(),
                false => GetSocketHandlerForUnixSocket(),
            };
        return new HttpClient(socketsHttpHandler);

        // Local function to create Handler using NamedPipe
        static SocketsHttpHandler GetSocketHandlerForNamedPipe()
        {
            Console.WriteLine("Connecting to Docker Engine using Named Pipe:");
            SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler();
            // Custom connection callback that connects to NamedPiper server
            socketsHttpHandler.ConnectCallback = async (sockHttpConnContext, ctxToken) =>
            {
                Uri dockerEngineUri = new Uri("npipe://./pipe/docker_engine");
                NamedPipeClientStream pipeClientStream = new NamedPipeClientStream(dockerEngineUri.Host,
                                                        dockerEngineUri.Segments[2],
                                                        PipeDirection.InOut, PipeOptions.Asynchronous);
                await pipeClientStream.ConnectAsync(ctxToken);
                return pipeClientStream;
            };
            return socketsHttpHandler;
        }
        // Local function to create Handler using Unix Socket
        static SocketsHttpHandler GetSocketHandlerForUnixSocket()
        {
            Console.WriteLine("Connecting to Docker Engine using Unix Domain Socket:");
            SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler();
            // Custom connection callback that connects to Unixdomain Socket
            socketsHttpHandler.ConnectCallback = async (sockHttpConnContext, ctxToken) =>
            {
                Uri dockerEngineUri = new Uri("unix:///var/run/docker.sock");
                var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);

                var endpoint = new UnixDomainSocketEndPoint(dockerEngineUri.AbsolutePath);
                await socket.ConnectAsync(endpoint, ctxToken);
                return new NetworkStream(socket);
            };
            return socketsHttpHandler;
        }
    }
}
