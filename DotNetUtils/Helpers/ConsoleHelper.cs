using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helpers;
/// <summary>
///  https://www.meziantou.net/handling-cancelkeypress-using-a-cancellationtoken.htm
/// </summary>
internal class ConsoleHelper 
{
    CancellationTokenSource cts = new CancellationTokenSource();
    const int WaitIndefinitely = -1;
    private bool disposedValue;

    public void SetupKeyPressHandler()
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            // We'll stop the process manually by using the CancellationToken
            e.Cancel = true;

            // Change the state of the CancellationToken to "Canceled"
            // - Set the IsCancellationRequested property to true
            // - Call the registered callbacks
            cts.Cancel();
        };
    }
    public async Task WaitForCancelAsync()
    {
        try
        {
            // code using the cancellation token
            Console.WriteLine("Waiting");
            await Task.Delay(WaitIndefinitely, cts.Token);

        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation canceled");
        }
    }

    // experiment using posix signal
    private void UsingPosixSignal()
    {
        using var registration = PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx => {
            Console.WriteLine($"PosixSignal.SIGTERM received");
            Console.WriteLine($"{ctx.Signal}");
        });

        using var registration1 = PosixSignalRegistration.Create(PosixSignal.SIGHUP, ctx => {
            Console.WriteLine($"PosixSignal.SIGHUP received");
            Console.WriteLine($"{ctx.Signal}");

        });
    }
}
