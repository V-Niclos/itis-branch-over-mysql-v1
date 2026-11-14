using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("tsn", "comment_id")]
[Index("tsn", "comment_id", Name = "tu_comments_links_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class tu_comments_links
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int tsn { get; set; }

    [Key]
    [Column(TypeName = "int(11)")]
    public int comment_id { get; set; }

    public DateOnly update_date { get; set; }
}
