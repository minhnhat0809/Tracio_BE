using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class ServiceCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
