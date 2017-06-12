using ProtoBuf;
using System.IO;
using System.Threading.Tasks;

namespace ServiceFabric.Utils.CommunicationClients.WebSocket

{
    /// <summary>
    /// <see cref="ProtoBuf"/> serialization implementation for WebSocket
    /// </summary>
    public class ProtobufWebSocketSerializer : IWebSocketSerializer
    {
        /// <summary>
        /// Deserializes a <see cref="ProtBuf"/> message with offset set to 0 and length set to <paramref name="serialized"/>
        /// </summary>
        /// <typeparam name="T">The type of the serialized message</typeparam>
        /// <param name="serialized">Serialized message</param>
        /// <returns></returns>
        public Task<T> DeserializeAsync<T>(byte[] serialized)
        {
            return DeserializeAsync<T>(serialized, 0, serialized.Length);
        }

        /// <summary>
        /// Deserializes a <see cref="ProtBuf"/> message
        /// </summary>
        /// <typeparam name="T">The type of the serialized message</typeparam>
        /// <param name="serialized">Serialized message</param>
        /// <param name="offset">Offset to start the memory stream at</param>
        /// <param name="length">Length of the stream</param>
        /// <returns></returns>
        public Task<T> DeserializeAsync<T>(byte[] serialized, int offset, int length)
        {
            using (MemoryStream ms = new MemoryStream(serialized, offset, length))
            {
                return Task.FromResult(Serializer.Deserialize<T>(ms));
            }
        }

        /// <summary>
        /// Serializes a message to <see cref="ProtoBuf"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Message to serialize</param>
        /// <returns></returns>
        public Task<byte[]> SerializeAsync<T>(T value)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize(ms, value);
                return Task.FromResult(ms.ToArray());
            }
        }
    }
}
