using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;
internal class OtherApiDemo
{
    public void ConvertApi()
    {
        //public static byte[] FromHexString(ReadOnlySpan<char> chars);
        //+        public static byte[] FromHexString(string s);
        //+        public static string ToHexString(byte[] inArray);
        //+        public static string ToHexString(byte[] inArray, int offset, int length);
        //+        public static string ToHexString(ReadOnlySpan<byte> bytes);
    }
    public void ThreadSafeSudoRandomNumber()
    {
        var random = Random.Shared;
    }
    public void StringLineEnumerator(String contentWithLineSeparator)
    {
        // MemoryExtensions
        SpanLineEnumerator spanLineEnumerator = contentWithLineSeparator.AsSpan().EnumerateLines();
    }

    public void MathWithQuotientAndReminder()
    {
        (int Quotient, int Remainder) value = Math.DivRem(120, 200);
        
    }
}
