using System;
using System.Collections.Generic;

namespace Rest_API.Models;

public partial class Player
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int TeamId { get; set; }

    public string? Position { get; set; }

    public virtual Team Team { get; set; } = null!;
}
