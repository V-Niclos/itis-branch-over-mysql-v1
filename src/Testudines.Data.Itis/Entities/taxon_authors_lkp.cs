using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("taxon_author_id", "kingdom_id")]
[Index("taxon_author_id", "taxon_author", "kingdom_id", Name = "taxon_authors_id_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class taxon_authors_lkp
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int taxon_author_id { get; set; }

    [StringLength(100)]
    public string taxon_author { get; set; } = null!;

    public DateOnly update_date { get; set; }

    [Key]
    [Column(TypeName = "smallint(6)")]
    public short kingdom_id { get; set; }

    [Column(TypeName = "mediumtext")]
    public string? short_author { get; set; }
}
