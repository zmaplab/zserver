using System.Collections.Generic;
using System.Text;
using Xunit;
using ZServer.Interfaces;

namespace ZServer.Tests;

public class XmlResultTests
{
    [Fact]
    public void Test1()
    {
        var exception = new ServerException
        {
            Code = "",
            Text = "this is a exception"
        };

        var report = new ServerExceptionReport
        {
            Exceptions = new List<ServerException>
            {
                exception
            }
        };
        Encoding.UTF8.GetString(report.Serialize());
    }
}