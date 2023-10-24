namespace Helpers;
public class FileUtils
{
    /// <summary>
    /// Generate Unique file name based on the timestamp
    /// </summary>
    /// <returns></returns>
    public static string GetFileNameTimeStampUtcNow()
    {
        // spell-checker:disable-next
        return DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
    }
}
