using Unity.Networking.Transport;
using UnityEngine;

public class StayAliveMessage : MessageHeader
{
    public override MessageType Type => MessageType.none;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);
    }
}
