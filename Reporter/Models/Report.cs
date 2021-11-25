namespace Reporter.Models;

public struct Report
{
    public long ID { get; set; } = -1;

    public ulong Moderator { get; set; } = 0;

    public string Username { get; set; } = "";

    public ReportType Type { get; set; } = ReportType.Other;

    public DateTime Time { get; set; } = DateTime.UtcNow;

    public string Punishment { get; set; } = "";

    public long BlocksBroken { get; set; } = 0;

    public string Note { get; set; } = "";

    public string[] ProofURLs { get; set; } = Array.Empty<string>();

    public Report(string target, ulong moderator, DateTime time, string punishment, long blocksbroken = 0, string note = "", long id = -1, ReportType? type = null)
    {
        ID = id;
        Moderator = moderator;
        Username = target;
        Type = type ?? ReportType.Other;
        Time = time;
        Punishment = punishment;
        Note = note;
        BlocksBroken = blocksbroken;
    }

    public void AddImages(IEnumerable<Attachment> args)
        => ProofURLs = ProofURLs.Concat(args.Select(x => x.Url)).ToArray();

    public void AddImages(string[] args)
        => ProofURLs = ProofURLs.Concat(args).ToArray();
}

public enum ReportType
{
    Grief,
    
    Tunnel,

    Chat,

    Hack,

    Other
}

