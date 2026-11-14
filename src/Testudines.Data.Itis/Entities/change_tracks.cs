using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Testudines.Data.Itis.Entities;

[Index("change_track_id", Name = "change_tracks_index")]
[MySqlCollation("utf8mb4_bin")]
public partial class change_tracks
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int change_track_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int? old_tsn { get; set; }

    [StringLength(40)]
    public string change_reason { get; set; } = null!;

    [StringLength(100)]
    public string change_initiator { get; set; } = null!;

    [StringLength(100)]
    public string change_reviewer { get; set; } = null!;

    [StringLength(100)]
    public string change_certifier { get; set; } = null!;

    public DateTime change_time_stamp { get; set; }

    [Column(TypeName = "int(11)")]
    public int tsn { get; set; }

    public DateOnly update_date { get; set; }
}
