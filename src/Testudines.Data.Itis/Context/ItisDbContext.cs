using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Testudines.Data.Itis.Entities;

namespace Testudines.Data.Itis.Context;

public partial class ItisDbContext : DbContext
{
    public ItisDbContext(DbContextOptions<ItisDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<HierarchyToRank> HierarchyToRank { get; set; }

    public virtual DbSet<change_comments> change_comments { get; set; }

    public virtual DbSet<change_operations> change_operations { get; set; }

    public virtual DbSet<change_tracks> change_tracks { get; set; }

    public virtual DbSet<chg_operation_lkp> chg_operation_lkp { get; set; }

    public virtual DbSet<comments> comments { get; set; }

    public virtual DbSet<experts> experts { get; set; }

    public virtual DbSet<geographic_div> geographic_div { get; set; }

    public virtual DbSet<hierarchy> hierarchy { get; set; }

    public virtual DbSet<jurisdiction> jurisdiction { get; set; }

    public virtual DbSet<kingdoms> kingdoms { get; set; }

    public virtual DbSet<longnames> longnames { get; set; }

    public virtual DbSet<nodc_ids> nodc_ids { get; set; }

    public virtual DbSet<other_sources> other_sources { get; set; }

    public virtual DbSet<publications> publications { get; set; }

    public virtual DbSet<reference_links> reference_links { get; set; }

    public virtual DbSet<reviews> reviews { get; set; }

    public virtual DbSet<strippedauthor> strippedauthor { get; set; }

    public virtual DbSet<synonym_links> synonym_links { get; set; }

    public virtual DbSet<taxon_authors_lkp> taxon_authors_lkp { get; set; }

    public virtual DbSet<taxon_unit_types> taxon_unit_types { get; set; }

    public virtual DbSet<taxonomic_units> taxonomic_units { get; set; }

    public virtual DbSet<tu_comments_links> tu_comments_links { get; set; }

    public virtual DbSet<vern_ref_links> vern_ref_links { get; set; }

    public virtual DbSet<vernaculars> vernaculars { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<HierarchyToRank>(entity =>
        {
            entity.Property(e => e.credibility_rtng).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.kingdom_tsn).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.level).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.parent_tsn).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.rank_id).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.rank_name)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
            entity.Property(e => e.scientific_name).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.sort).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.synonyms).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.taxon_author).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.tsn).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<change_comments>(entity =>
        {
            entity.HasKey(e => new { e.change_track_id, e.chg_cmt_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
        });

        modelBuilder.Entity<change_operations>(entity =>
        {
            entity.HasKey(e => new { e.change_track_id, e.chg_op_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
        });

        modelBuilder.Entity<change_tracks>(entity =>
        {
            entity.HasKey(e => e.change_track_id).HasName("PRIMARY");

            entity.Property(e => e.change_track_id).ValueGeneratedNever();
            entity.Property(e => e.old_tsn).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<chg_operation_lkp>(entity =>
        {
            entity.HasKey(e => e.chg_op_id).HasName("PRIMARY");

            entity.Property(e => e.chg_op_id).ValueGeneratedNever();
        });

        modelBuilder.Entity<comments>(entity =>
        {
            entity.HasKey(e => e.comment_id).HasName("PRIMARY");

            entity.Property(e => e.comment_id).ValueGeneratedNever();
            entity.Property(e => e.commentator).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<experts>(entity =>
        {
            entity.HasKey(e => new { e.expert_id_prefix, e.expert_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.Property(e => e.expert_id_prefix).IsFixedLength();
            entity.Property(e => e.exp_comment).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<geographic_div>(entity =>
        {
            entity.HasKey(e => new { e.tsn, e.geographic_value })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
        });

        modelBuilder.Entity<hierarchy>(entity =>
        {
            entity.HasKey(e => e.hierarchy_string).HasName("PRIMARY");

            entity.Property(e => e.Parent_TSN).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<jurisdiction>(entity =>
        {
            entity.HasKey(e => new { e.tsn, e.jurisdiction_value })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
        });

        modelBuilder.Entity<kingdoms>(entity =>
        {
            entity.HasKey(e => e.kingdom_id).HasName("PRIMARY");

            entity.Property(e => e.kingdom_id).ValueGeneratedNever();
            entity.Property(e => e.kingdom_name).IsFixedLength();
        });

        modelBuilder.Entity<longnames>(entity =>
        {
            entity.HasKey(e => e.tsn).HasName("PRIMARY");

            entity.Property(e => e.tsn).ValueGeneratedNever();
        });

        modelBuilder.Entity<nodc_ids>(entity =>
        {
            entity.HasKey(e => new { e.nodc_id, e.tsn })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.Property(e => e.nodc_id).IsFixedLength();
        });

        modelBuilder.Entity<other_sources>(entity =>
        {
            entity.HasKey(e => new { e.source_id_prefix, e.source_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.Property(e => e.source_id_prefix).IsFixedLength();
            entity.Property(e => e.source_comment).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.source_type).IsFixedLength();
            entity.Property(e => e.version).IsFixedLength();
        });

        modelBuilder.Entity<publications>(entity =>
        {
            entity.HasKey(e => new { e.pub_id_prefix, e.publication_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.Property(e => e.pub_id_prefix).IsFixedLength();
            entity.Property(e => e.isbn).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.issn).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.listed_pub_date).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.pages).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.pub_comment).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.pub_place).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.publisher).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.title).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<reference_links>(entity =>
        {
            entity.HasKey(e => new { e.tsn, e.doc_id_prefix, e.documentation_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

            entity.Property(e => e.doc_id_prefix).IsFixedLength();
            entity.Property(e => e.change_track_id).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.init_itis_desc_ind)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
            entity.Property(e => e.original_desc_ind)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
            entity.Property(e => e.vernacular_name).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<reviews>(entity =>
        {
            entity.HasKey(e => new { e.tsn, e.review_start_date })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.Property(e => e.review_comment).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.review_end_date).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<strippedauthor>(entity =>
        {
            entity.HasKey(e => e.taxon_author_id).HasName("PRIMARY");

            entity.Property(e => e.taxon_author_id).ValueGeneratedNever();
        });

        modelBuilder.Entity<synonym_links>(entity =>
        {
            entity.HasKey(e => new { e.tsn, e.tsn_accepted })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
        });

        modelBuilder.Entity<taxon_authors_lkp>(entity =>
        {
            entity.HasKey(e => new { e.taxon_author_id, e.kingdom_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.Property(e => e.short_author).HasDefaultValueSql("'NULL'");
        });

        modelBuilder.Entity<taxon_unit_types>(entity =>
        {
            entity.HasKey(e => new { e.kingdom_id, e.rank_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.Property(e => e.rank_name).IsFixedLength();
        });

        modelBuilder.Entity<taxonomic_units>(entity =>
        {
            entity.HasKey(e => e.tsn).HasName("PRIMARY");

            entity.Property(e => e.tsn).ValueGeneratedNever();
            entity.Property(e => e.complete_name).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.completeness_rtng)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
            entity.Property(e => e.currency_rating)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
            entity.Property(e => e.hybrid_author_id).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.n_usage).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.parent_tsn).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.phylo_sort_seq).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.taxon_author_id).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.unaccept_reason).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.uncertain_prnt_ind)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
            entity.Property(e => e.unit_ind1)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
            entity.Property(e => e.unit_ind2)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
            entity.Property(e => e.unit_ind3).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.unit_ind4).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.unit_name1).IsFixedLength();
            entity.Property(e => e.unit_name2).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.unit_name3).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.unit_name4).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.unnamed_taxon_ind)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
        });

        modelBuilder.Entity<tu_comments_links>(entity =>
        {
            entity.HasKey(e => new { e.tsn, e.comment_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
        });

        modelBuilder.Entity<vern_ref_links>(entity =>
        {
            entity.HasKey(e => new { e.tsn, e.doc_id_prefix, e.documentation_id, e.vern_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0, 0 });

            entity.Property(e => e.doc_id_prefix).IsFixedLength();
        });

        modelBuilder.Entity<vernaculars>(entity =>
        {
            entity.HasKey(e => new { e.tsn, e.vern_id })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.Property(e => e.approved_ind)
                .HasDefaultValueSql("'NULL'")
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
