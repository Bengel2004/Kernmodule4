﻿using Unity.Networking.Transport;

public class HitMonsterMessage : MessageHeader
{
    public override MessageType Type => MessageType.hitMonster;

    public int PlayerID { get; set; }
    public ushort Damage { get; set; }

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteInt(PlayerID);
        writer.WriteUShort(Damage);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        PlayerID = reader.ReadInt();
        Damage = reader.ReadUShort();
    }
}
