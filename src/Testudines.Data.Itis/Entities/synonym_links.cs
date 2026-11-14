using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("tsn", "tsn_accepted")]
[Index("tsn", "tsn_accepted", Name = "synonym_links_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class synonym_links
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int tsn { get; set; }

    [Key]
    [Column(TypeName = "int(11)")]
    public int tsn_accepted { get; set; }

    public DateOnly update_date { get; set; }
}
