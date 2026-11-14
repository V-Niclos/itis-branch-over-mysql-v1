using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[Keyless]
[MySqlCollation("utf8mb4_bin")]
public partial class HierarchyToRank
{
    [Column(TypeName = "int(11)")]
    public int? kingdom_tsn { get; set; }

    [Column(TypeName = "int(11)")]
    public int? level { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short? rank_id { get; set; }

    [StringLength(15)]
    public string? rank_name { get; set; }

    [Column(TypeName = "int(11)")]
    public int? tsn { get; set; }

    [Column(TypeName = "int(11)")]
    public int? parent_tsn { get; set; }

    [StringLength(163)]
    public string? scientific_name { get; set; }

    [StringLength(100)]
    public string? taxon_author { get; set; }

    [StringLength(40)]
    public string? credibility_rtng { get; set; }

    [StringLength(400)]
    public string? sort { get; set; }

    [Column(TypeName = "int(11)")]
    public int DirectChildrenCount { get; set; }

    [StringLength(1000)]
    public string? synonyms { get; set; }
}
