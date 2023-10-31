using System.Net.Sockets;
using System.Text.Json;
using Castle.DynamicProxy;

namespace RpcTest;

public class Rpc<T> where T : class
{
    public T Remote { get; private set; }

    private readonly T _local;
    private readonly Type _type;

    public Rpc(TcpClient tcpClient, T local)
    {
        _local = local;
        _type = typeof(T);
        Remote = new ProxyGenerator()
            .CreateInterfaceProxyWithoutTarget<T>(new Interceptor(tcpClient));
    }

    public void Process(string incommingMessage)
    {
        var message = JsonSerializer.Deserialize<RpcMessage>(incommingMessage);

        if (message is null)
            return;

        var method = _type.GetMethod(message.MethodName);

        if (method is null)
            return;

        var methodParameters = method.GetParameters();
        var parameters = new object[methodParameters.Length];

        for (var i = 0; i < method.GetParameters().Length; i++)
        {
            var parameterType = methodParameters[i].ParameterType;
            var jsonElement = message.Parameters[i];

            var parameter = jsonElement.Deserialize(parameterType);

            if (parameter is null)
                continue;

            parameters[i] = parameter;
        }

        method.Invoke(_local, parameters);
    }
}
