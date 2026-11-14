using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("change_track_id", "chg_op_id")]
[Index("change_track_id", "chg_op_id", Name = "change_operations_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class change_operations
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int change_track_id { get; set; }

    [Key]
    [Column(TypeName = "int(11)")]
    public int chg_op_id { get; set; }

    public DateOnly update_date { get; set; }
}
