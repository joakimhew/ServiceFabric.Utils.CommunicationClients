using ProtoBuf;

namespace ServiceFabric.Utils.CommunicationClients.WebSocket
{
    [ProtoContract]
    public class WebSocketResponseMessage
    {
        [ProtoMember(1)] public int Result;
        [ProtoMember(2)] public byte[] Value;
    }
}
