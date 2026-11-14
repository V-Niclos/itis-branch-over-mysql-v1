using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("source_id_prefix", "source_id")]
[Index("source_id_prefix", "source_id", Name = "other_sources_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class other_sources
{
    [Key]
    [StringLength(3)]
    public string source_id_prefix { get; set; } = null!;

    [Key]
    [Column(TypeName = "int(11)")]
    public int source_id { get; set; }

    [StringLength(10)]
    public string source_type { get; set; } = null!;

    [StringLength(64)]
    public string source { get; set; } = null!;

    [StringLength(10)]
    public string version { get; set; } = null!;

    public DateOnly acquisition_date { get; set; }

    [StringLength(500)]
    public string? source_comment { get; set; }

    public DateOnly update_date { get; set; }
}
