namespace Reporter.Models;

public struct Report
{
    /// <summary>
    /// The ID of the report (generated at creation)
    /// </summary>
    public long ID { get; set; } = -1;

    /// <summary>
    /// Moderator of the report, set as Discord user ID
    /// </summary>
    public ulong Moderator { get; set; } = 0;

    /// <summary>
    /// Username of the player to be reported
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// Type of report
    /// </summary>
    public ReportType Type { get; set; } = ReportType.Other;

    /// <summary>
    /// Time when offense was made
    /// </summary>
    public DateTime Time { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Punishment of the player
    /// </summary>
    public string Punishment { get; set; } = "";

    /// <summary>
    /// Total blocks broken
    /// </summary>
    public long BlocksBroken { get; set; } = 0;

    /// <summary>
    /// Additional notes
    /// </summary>
    public string Note { get; set; } = "";

    /// <summary>
    /// Array of images
    /// </summary>
    public string[] ProofURLs { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Creates a new report based on parameters included
    /// </summary>
    /// <param name="target"></param>
    /// <param name="moderator"></param>
    /// <param name="time"></param>
    /// <param name="punishment"></param>
    /// <param name="blocksbroken"></param>
    /// <param name="note"></param>
    /// <param name="id"></param>
    /// <param name="type"></param>
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

    /// <summary>
    /// Adds images from attachments
    /// </summary>
    /// <param name="args"></param>
    public void AddImages(IEnumerable<Attachment> args)
        => ProofURLs = ProofURLs.Concat(args.Select(x => x.Url)).ToArray();

    /// <summary>
    /// Addds images from links
    /// </summary>
    /// <param name="args"></param>
    public void AddImages(string[] args)
        => ProofURLs = ProofURLs.Concat(args).ToArray();
}

/// <summary>
/// The type of report
/// </summary>
public enum ReportType
{
    /// <summary>
    /// A grief report
    /// </summary>
    Grief,
    
    /// <summary>
    /// A tunnel, hellevator or terraform report
    /// </summary>
    Tunnel,

    /// <summary>
    /// NSFW, Toxicity or plain verbal madness in chat
    /// </summary>
    Chat,

    /// <summary>
    /// Hacking repor
    /// </summary>
    Hack,

    /// <summary>
    /// Anything else
    /// </summary>
    Other
}

