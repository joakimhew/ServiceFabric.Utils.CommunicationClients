using ProtoBuf;

namespace ServiceFabric.Utils.CommunicationClients.WebSocket
{
    [ProtoContract]
    public class WebSocketRequestMessage
    {
        [ProtoMember(1)] public int PartitionKey;
        [ProtoMember(2)] public string Operation;
        [ProtoMember(3)] public byte[] Value;
    }
}
