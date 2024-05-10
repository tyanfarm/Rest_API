using System;
using System.Collections.Generic;

namespace Rest_API.Models;

public partial class Match
{
    public int Id { get; set; }

    public int AteamId { get; set; }

    public int BteamId { get; set; }

    public DateTime Schedule { get; set; }

    public string Stadium { get; set; } = null!;

    public string Score { get; set; } = null!;

    public virtual Team Ateam { get; set; } = null!;

    public virtual Team Bteam { get; set; } = null!;
}
