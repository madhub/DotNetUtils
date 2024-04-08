using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp8Features;
internal class DefaultInterfaceTest
{
    static void Main(string[] args)
    {
        IDefaultInterfaceMethod anyClass = new AnyClass();
        anyClass.DefaultMethod();
        Console.ReadKey();
    }
}

interface IDefaultInterfaceMethod
{
    public void DefaultMethod()
    {
        Console.WriteLine("I am a default method in the interface!");
    }
}
class AnyClass : IDefaultInterfaceMethod
{
}