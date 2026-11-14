using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[Index("tsn", "parent_tsn", Name = "taxon_unit_index1")]
[Index("tsn", "unit_name1", "name_usage", Name = "taxon_unit_index2")]
[Index("kingdom_id", "rank_id", Name = "taxon_unit_index3")]
[Index("tsn", "taxon_author_id", Name = "taxon_unit_index4")]
[MySqlCollation("utf8mb4_bin")]
public partial class taxonomic_units
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int tsn { get; set; }

    [StringLength(1)]
    public string? unit_ind1 { get; set; }

    [StringLength(35)]
    public string unit_name1 { get; set; } = null!;

    [StringLength(20)]
    public string? unit_ind2 { get; set; }

    [StringLength(35)]
    public string? unit_name2 { get; set; }

    [StringLength(20)]
    public string? unit_ind3 { get; set; }

    [StringLength(35)]
    public string? unit_name3 { get; set; }

    [StringLength(7)]
    public string? unit_ind4 { get; set; }

    [StringLength(35)]
    public string? unit_name4 { get; set; }

    [StringLength(1)]
    public string? unnamed_taxon_ind { get; set; }

    [StringLength(12)]
    public string name_usage { get; set; } = null!;

    [StringLength(50)]
    public string? unaccept_reason { get; set; }

    [StringLength(40)]
    public string credibility_rtng { get; set; } = null!;

    [StringLength(10)]
    public string? completeness_rtng { get; set; }

    [StringLength(7)]
    public string? currency_rating { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short? phylo_sort_seq { get; set; }

    public DateTime initial_time_stamp { get; set; }

    [Column(TypeName = "int(11)")]
    public int? parent_tsn { get; set; }

    [Column(TypeName = "int(11)")]
    public int? taxon_author_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int? hybrid_author_id { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short kingdom_id { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short rank_id { get; set; }

    public DateOnly update_date { get; set; }

    [StringLength(3)]
    public string? uncertain_prnt_ind { get; set; }

    [Column(TypeName = "mediumtext")]
    public string? n_usage { get; set; }

    [Column(TypeName = "text")]
    public string? complete_name { get; set; }
}
