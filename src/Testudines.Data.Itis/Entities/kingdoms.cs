using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[Index("kingdom_id", "kingdom_name", Name = "kingdoms_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class kingdoms
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int kingdom_id { get; set; }

    [StringLength(10)]
    public string kingdom_name { get; set; } = null!;

    public DateOnly update_date { get; set; }
}
