using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("tsn", "doc_id_prefix", "documentation_id")]
[Index("tsn", "doc_id_prefix", "documentation_id", Name = "reference_links_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class reference_links
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int tsn { get; set; }

    [Key]
    [StringLength(3)]
    public string doc_id_prefix { get; set; } = null!;

    [Key]
    [Column(TypeName = "int(11)")]
    public int documentation_id { get; set; }

    [StringLength(1)]
    public string? original_desc_ind { get; set; }

    [StringLength(1)]
    public string? init_itis_desc_ind { get; set; }

    [Column(TypeName = "int(11)")]
    public int? change_track_id { get; set; }

    [StringLength(80)]
    public string? vernacular_name { get; set; }

    public DateOnly update_date { get; set; }
}
