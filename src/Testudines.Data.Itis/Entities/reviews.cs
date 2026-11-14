using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("tsn", "review_start_date")]
[Index("tsn", "review_start_date", Name = "reviews_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class reviews
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int tsn { get; set; }

    [Key]
    public DateOnly review_start_date { get; set; }

    public DateOnly? review_end_date { get; set; }

    [StringLength(25)]
    public string review_reason { get; set; } = null!;

    [StringLength(100)]
    public string reviewer { get; set; } = null!;

    [StringLength(255)]
    public string? review_comment { get; set; }

    public DateOnly update_date { get; set; }
}
