using System;
using System.Collections.Generic;

namespace Rest_API.Models;

public partial class Team
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string TeamPrincipal { get; set; } = null!;
    
    public virtual ICollection<Match> MatchAteams { get; } = new List<Match>();

    public virtual ICollection<Match> MatchBteams { get; } = new List<Match>();

    public virtual ICollection<Player> Players { get; } = new List<Player>();
}
