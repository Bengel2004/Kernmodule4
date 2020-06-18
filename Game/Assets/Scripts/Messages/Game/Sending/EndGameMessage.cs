using Unity.Networking.Transport;

public class EndGameMessage : MessageHeader
{
    public override MessageType Type => MessageType.endGame;

    public byte NumberOfScores { get; set; }
    public Score[] playerIDScores { get; set; }

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteByte(NumberOfScores);
        for (int i = 0; i < NumberOfScores; i++)
        {
            writer.WriteInt(playerIDScores[i].playerID);
            writer.WriteUShort(playerIDScores[i].score);
        }
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        NumberOfScores = reader.ReadByte();
        playerIDScores = new Score[NumberOfScores];
        for (int i = 0; i < NumberOfScores; i++)
        {
            playerIDScores[i].playerID = reader.ReadInt();
            playerIDScores[i].score = reader.ReadUShort();
        } 
    }
}

public struct Score
{
    public int playerID;
    public ushort score;
}
