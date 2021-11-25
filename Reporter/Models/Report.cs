namespace Reporter.Models;

public struct Report
{
    public int ID { get; set; } = -1;

    public ulong Moderator { get; set; } = 0;

    public string Username { get; set; } = "";

    public ReportType Type { get; set; } = ReportType.Other;

    public DateTime Time { get; set; } = DateTime.UtcNow;

    public string Punishment { get; set; } = "";

    public int BlocksBroken { get; set; } = 0;

    public string Note { get; set; } = "";

    public List<string> ProofURLs { get; set; } = new();

    public Report(int id, ulong agent, string username, ReportType type, DateTime time, string punishment, int blocksbroken = 0, string? note = null)
    {
        ID = id;
        Moderator = agent;
        Username = username;
        Type = type;
        Time = time;
        Punishment = punishment;
        Note = note ?? "";
        BlocksBroken = blocksbroken != 0 ? blocksbroken : 0;
    }

    public Report AddImages(List<Attachment> args)
    {
        foreach (Attachment attachment in args)
        {
            ProofURLs.Add(attachment.Url);
        }
        return this;
    }
}

public enum ReportType
{
    Grief,
    
    Tunnel,

    Chat,

    Hack,

    Other
}

