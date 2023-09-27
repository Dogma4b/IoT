using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace IoT_Server.Modules
{
    class Crypto
    {

        public string sha512(string str)
        {
            return BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", String.Empty).ToLower();
        }
    }
}
