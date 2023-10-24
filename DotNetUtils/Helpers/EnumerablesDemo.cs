using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;
internal class EnumerablesDemo
{
    public void ChunkApiTest()
    {
        List<string> strings =  new List<string>{ "one", "two", "three", "four" };
        foreach (string[] chunk in strings.Chunk(3))
        {
            foreach (var item in chunk)
            {
                Console.WriteLine(item);
            }
        }
    }
}
