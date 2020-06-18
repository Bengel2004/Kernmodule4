﻿using Unity.Networking.Transport;

public class HitByMonsterMessage : MessageHeader
{
    public override MessageType Type => MessageType.hitByMonster;

    public int PlayerID { get; set; }
    public ushort Health { get; set; }

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteInt(PlayerID);
        writer.WriteUShort(Health);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        PlayerID = reader.ReadInt();
        Health = reader.ReadUShort();
    }
}
