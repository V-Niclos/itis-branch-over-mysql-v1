using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[Index("taxon_author_id", "shortauthor", Name = "taxon_author_id")]
[MySqlCollation("utf8mb4_bin")]
public partial class strippedauthor
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int taxon_author_id { get; set; }

    [StringLength(100)]
    public string shortauthor { get; set; } = null!;
}
