using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace auth_dotnet_api
{
    public static class Test
    {
        public static string GetTestMessage(this string message)
        {
            return "This is a test message from the Extension/test.cs file.";
        }
    }
}