using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[PrimaryKey("pub_id_prefix", "publication_id")]
[Index("pub_id_prefix", "publication_id", Name = "publications_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class publications
{
    [Key]
    [StringLength(3)]
    public string pub_id_prefix { get; set; } = null!;

    [Key]
    [Column(TypeName = "int(11)")]
    public int publication_id { get; set; }

    [StringLength(100)]
    public string reference_author { get; set; } = null!;

    [StringLength(255)]
    public string? title { get; set; }

    [StringLength(255)]
    public string publication_name { get; set; } = null!;

    public DateOnly? listed_pub_date { get; set; }

    public DateOnly actual_pub_date { get; set; }

    [StringLength(80)]
    public string? publisher { get; set; }

    [StringLength(40)]
    public string? pub_place { get; set; }

    [StringLength(16)]
    public string? isbn { get; set; }

    [StringLength(16)]
    public string? issn { get; set; }

    [StringLength(15)]
    public string? pages { get; set; }

    [StringLength(500)]
    public string? pub_comment { get; set; }

    public DateOnly update_date { get; set; }
}
