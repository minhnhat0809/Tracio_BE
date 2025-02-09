using System;
using System.Collections.Generic;

namespace ContentService.Domain.Entities;

public partial class BlogPrivacy
{
    public int BlogId { get; set; }

    public int CyclistId { get; set; }

    public string CyclistName { get; set; } = null!;

    public virtual Blog Blog { get; set; } = null!;
}
