using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;
internal class ConsoleHelpe2
{
    TaskCompletionSource tcs = new TaskCompletionSource();
    bool sigintReceived = false;
    //Console.WriteLine("Waiting for SIGINT/SIGTERM");
    public void SetupKeyPressHandler()
    {
        Console.CancelKeyPress += (_, ea) =>
        {
            // Tell .NET to not terminate the process
            ea.Cancel = true;
            Console.WriteLine("Received SIGINT (Ctrl+C)");
            tcs.SetResult();
            sigintReceived = true;
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            if (!sigintReceived)
            {
                Console.WriteLine("Received SIGTERM");
                tcs.SetResult();
            }
            else
            {
                Console.WriteLine("Received SIGTERM, ignoring it because already processed SIGINT");
            }
        };

    }
    public async Task WaitForCancelAsync()
    {
        await tcs.Task;
    }
}
