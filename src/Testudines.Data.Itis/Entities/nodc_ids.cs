using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("nodc_id", "tsn")]
[Index("nodc_id", "tsn", Name = "nodc_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class nodc_ids
{
    [Key]
    [StringLength(12)]
    public string nodc_id { get; set; } = null!;

    public DateOnly update_date { get; set; }

    [Key]
    [Column(TypeName = "int(11)")]
    public int tsn { get; set; }
}
