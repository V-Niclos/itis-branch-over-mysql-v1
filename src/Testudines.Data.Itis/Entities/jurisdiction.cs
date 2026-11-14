using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("tsn", "jurisdiction_value")]
[Index("tsn", "jurisdiction_value", Name = "jurisdiction_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class jurisdiction
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int tsn { get; set; }

    [Key]
    [StringLength(30)]
    public string jurisdiction_value { get; set; } = null!;

    [StringLength(19)]
    public string origin { get; set; } = null!;

    public DateOnly update_date { get; set; }
}
