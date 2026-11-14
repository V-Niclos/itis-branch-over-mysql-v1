using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[Index("comment_id", Name = "comments_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class comments
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int comment_id { get; set; }

    [StringLength(100)]
    public string? commentator { get; set; }

    [Column(TypeName = "mediumtext")]
    public string comment_detail { get; set; } = null!;

    public DateTime comment_time_stamp { get; set; }

    public DateOnly update_date { get; set; }
}
