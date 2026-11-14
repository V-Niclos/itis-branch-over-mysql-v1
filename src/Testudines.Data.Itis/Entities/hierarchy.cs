using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[Index("hierarchy_string", Name = "hierarchy_string")]
[MySqlCollation("utf8mb4_bin")]
public partial class hierarchy
{
    [Key]
    [StringLength(300)]
    public string hierarchy_string { get; set; } = null!;

    [Column(TypeName = "int(11)")]
    public int TSN { get; set; }

    [Column(TypeName = "int(11)")]
    public int? Parent_TSN { get; set; }

    [Column(TypeName = "int(11)")]
    public int level { get; set; }

    [Column(TypeName = "int(11)")]
    public int ChildrenCount { get; set; }
}
