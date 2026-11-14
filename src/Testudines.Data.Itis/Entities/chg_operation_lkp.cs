using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[Index("chg_op_id", Name = "chg_operation_lkp_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class chg_operation_lkp
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int chg_op_id { get; set; }

    [StringLength(25)]
    public string change_operation { get; set; } = null!;

    public DateOnly update_date { get; set; }
}
