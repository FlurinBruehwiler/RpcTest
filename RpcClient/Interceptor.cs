using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Castle.DynamicProxy;

namespace RpcTest;

public class Interceptor : IInterceptor
{
    private readonly TcpClient _tcpClient;

    public Interceptor(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
    }

    public void Intercept(IInvocation invocation)
    {
        if (!_tcpClient.Connected)
            return;

        var str = JsonSerializer.Serialize(new RpcMessage
        {
            MethodName = invocation.Method.Name,
            Parameters = invocation.Arguments.
                Select(x => JsonDocument
                    .Parse(JsonSerializer.Serialize(x)).RootElement)
                .ToArray()
        });
        var data = Encoding.UTF8.GetBytes(str);
        var stream = _tcpClient.GetStream();
        stream.Write(data, 0, data.Length);
        stream.Flush();
    }
}
