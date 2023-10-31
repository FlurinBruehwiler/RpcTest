using System.Text.Json;
using System.Text.Json.Nodes;

namespace RpcTest;

public class RpcMessage
{
    public string MethodName { get; set; }
    public JsonElement[] Parameters { get; set; }
}
