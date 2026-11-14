using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("tsn", "vern_id")]
[Index("tsn", "vernacular_name", "language", Name = "vernaculars_index1")]
[Index("tsn", "vern_id", Name = "vernaculars_index2")]
[MySqlCollation("utf8mb4_bin")]
public partial class vernaculars
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int tsn { get; set; }

    [StringLength(80)]
    public string vernacular_name { get; set; } = null!;

    [StringLength(15)]
    public string language { get; set; } = null!;

    [StringLength(1)]
    public string? approved_ind { get; set; }

    public DateOnly update_date { get; set; }

    [Key]
    [Column(TypeName = "int(11)")]
    public int vern_id { get; set; }
}
