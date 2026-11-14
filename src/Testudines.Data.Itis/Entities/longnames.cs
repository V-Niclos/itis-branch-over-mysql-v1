using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[Index("tsn", "completename", Name = "tsn")]
[MySqlCollation("utf8mb4_bin")]
public partial class longnames
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int tsn { get; set; }

    [StringLength(164)]
    public string completename { get; set; } = null!;
}
