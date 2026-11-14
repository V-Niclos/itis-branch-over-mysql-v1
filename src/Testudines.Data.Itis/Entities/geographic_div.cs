using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("tsn", "geographic_value")]
[Index("tsn", "geographic_value", Name = "geographic_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class geographic_div
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int tsn { get; set; }

    [Key]
    [StringLength(45)]
    public string geographic_value { get; set; } = null!;

    public DateOnly update_date { get; set; }
}
