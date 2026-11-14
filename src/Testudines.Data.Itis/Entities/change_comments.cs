using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("change_track_id", "chg_cmt_id")]
[Index("change_track_id", "chg_cmt_id", Name = "change_comments_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class change_comments
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int change_track_id { get; set; }

    [Key]
    [Column(TypeName = "int(11)")]
    public int chg_cmt_id { get; set; }

    [StringLength(250)]
    public string change_detail { get; set; } = null!;

    public DateOnly update_date { get; set; }
}
