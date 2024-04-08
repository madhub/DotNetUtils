using GuerrillaNtp;


int Threshold = 30; // Threshold in seconds 30 0r 60 seconds
double ThresholdInPercentage = 0.05; // 5% threshold


var localTime = DateTime.UtcNow;
localTime.TimeOfDay.TotalSeconds
Console.WriteLine($"Current TIme in UTC: {localTime}");
// time.windows.com or time.aws.com or time.nist.gov
var servers = args.Length > 0 ? args : new[] { "pool.ntp.org" };
foreach (var host in servers)
{
    Console.WriteLine("");
    Console.WriteLine("Querying {0}...", host);
    try
    {
        // https://github.com/gmh5225/VPN-win-app/blob/master/src/ProtonVPN.App/Core/SystemTimeValidator.cs#L55
        var ntp = new NtpClient(servers[0]);
        var ntpTime = await ntp.QueryAsync();
        var response = ntpTime.Response;
        // Synchronized flagh indicate whether
        if (ntpTime.Synchronized)
        {
            var difference = Math.Abs(ntpTime.UtcNow.Subtract(localTime).TotalSeconds);
            Console.WriteLine($"Local clock drift {difference} seconds");
            // should check tolerance with 5 % upper or lower
            if (difference > Threshold)
            {
                Console.WriteLine("In sync. Difference: {0:0.00} seconds", difference);
            }
            else
            {
                
                Console.WriteLine("Incorrect system time detected .Out of sync. Difference: {0:0.00} seconds", difference);
            }

        }


        Console.WriteLine();
        Console.WriteLine("Received response");
        Console.WriteLine("-------------------------------------");
        Console.WriteLine("Synchronized:       {0}", ntpTime.Synchronized ? "yes" : "no");
        Console.WriteLine("Network time (UTC): {0:HH:mm:ss.fff}", ntpTime.UtcNow);
        Console.WriteLine("Network time:       {0:HH:mm:ss.fff}", ntpTime.Now);
        Console.WriteLine("Correction offset:  {0:s'.'FFFFFFF}", ntpTime.CorrectionOffset);
        Console.WriteLine("Round-trip time:    {0:s'.'FFFFFFF}", ntpTime.RoundTripTime);
        Console.WriteLine("Origin time:        {0:HH:mm:ss.fff}", response.OriginTimestamp);
        Console.WriteLine("Receive time:       {0:HH:mm:ss.fff}", response.ReceiveTimestamp);
        Console.WriteLine("Transmit time:      {0:HH:mm:ss.fff}", response.TransmitTimestamp);
        Console.WriteLine("Destination time:   {0:HH:mm:ss.fff}", response.DestinationTimestamp);
        Console.WriteLine("Leap second:        {0}", response.LeapIndicator);
        Console.WriteLine("Stratum:            {0}", response.Stratum);
        Console.WriteLine("Reference ID:       0x{0:x}", response.ReferenceId);
        Console.WriteLine("Reference time:     {0:HH:mm:ss.fff}", response.ReferenceTimestamp);
        Console.WriteLine("Root delay:         {0}ms", response.RootDelay.TotalMilliseconds);
        Console.WriteLine("Root dispersion:    {0}ms", response.RootDispersion.TotalMilliseconds);
        Console.WriteLine("Poll interval:      2^{0}s", response.PollInterval);
        Console.WriteLine("Precision:          2^{0}s", response.Precision);
    }
    catch (Exception ex)
    {
        Console.WriteLine("NTP query failed: {0}", ex.Message);
    }
}
