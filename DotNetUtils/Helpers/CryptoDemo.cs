using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;
internal class CryptoDemo
{
    public void LoadFromPem()
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem("location to pem cert");
        // or
        rsa.ImportFromEncryptedPem("locatio", "password");
    }
}
