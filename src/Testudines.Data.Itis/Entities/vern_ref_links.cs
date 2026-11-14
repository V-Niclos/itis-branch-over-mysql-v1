using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("tsn", "doc_id_prefix", "documentation_id", "vern_id")]
[Index("tsn", "doc_id_prefix", "documentation_id", Name = "vern_rl_index1")]
[Index("tsn", "vern_id", Name = "vern_rl_index2")]
[MySqlCollation("utf8mb4_bin")]
public partial class vern_ref_links
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

    public DateOnly update_date { get; set; }

    [Key]
    [Column(TypeName = "int(11)")]
    public int vern_id { get; set; }
}
