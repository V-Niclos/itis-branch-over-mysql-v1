using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("kingdom_id", "rank_id")]
[Index("kingdom_id", "rank_id", Name = "taxon_ut_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class taxon_unit_types
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int kingdom_id { get; set; }

    [Key]
    [Column(TypeName = "smallint(6)")]
    public short rank_id { get; set; }

    [StringLength(15)]
    public string rank_name { get; set; } = null!;

    [Column(TypeName = "smallint(6)")]
    public short dir_parent_rank_id { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short req_parent_rank_id { get; set; }

    public DateOnly update_date { get; set; }
}
