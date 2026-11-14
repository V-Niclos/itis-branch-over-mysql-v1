using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("expert_id_prefix", "expert_id")]
[Index("expert_id_prefix", "expert_id", Name = "experts_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class experts
{
    [Key]
    [StringLength(3)]
    public string expert_id_prefix { get; set; } = null!;

    [Key]
    [Column(TypeName = "int(11)")]
    public int expert_id { get; set; }

    [StringLength(100)]
    public string expert { get; set; } = null!;

    [StringLength(500)]
    public string? exp_comment { get; set; }

    public DateOnly update_date { get; set; }
}
