using Microsoft.EntityFrameworkCore;
using Testudines.Data.Itis.Context;
using Testudines.Data.Itis.Models;

namespace Testudines.Data.Itis.Services;

/// <summary>
/// Provides access to the accepted taxon names and synonyms together with their hierarchy.
/// Ensures the supporting database view exists before querying it.
/// </summary>
public sealed class TsnHierarchyViewService(ItisDbContext dbContext)
{
    public const string ViewName = "TsnHierarchyNames";

    private const string CreateViewSql_old = """
        CREATE OR REPLACE VIEW `TsnHierarchyNames` AS
        SELECT
            h.`TSN` AS `tsn_accepted`,
            tu.`tsn` AS `tsn_name`,
            tu.`complete_name`,
            tut.`rank_name`,
            'Accepted' AS `name_status`,
            h.`hierarchy_string`
        FROM `hierarchy` AS h
        INNER JOIN `taxonomic_units` AS tu
            ON h.`TSN` = tu.`tsn`
        INNER JOIN `taxon_unit_types` AS tut
            ON tu.`kingdom_id` = tut.`kingdom_id`
            AND tu.`rank_id` = tut.`rank_id`

        UNION ALL

        SELECT
            sl.`tsn_accepted`,
            tu.`tsn` AS `tsn_name`,
            tu.`complete_name`,
            tut.`rank_name`,
            'Synonym' AS `name_status`,
            h.`hierarchy_string`
        FROM `synonym_links` AS sl
        INNER JOIN `hierarchy` AS h
            ON sl.`tsn_accepted` = h.`TSN`
        INNER JOIN `taxonomic_units` AS tu
            ON sl.`tsn` = tu.`tsn`
        INNER JOIN `taxon_unit_types` AS tut
            ON tu.`kingdom_id` = tut.`kingdom_id`
            AND tu.`rank_id` = tut.`rank_id`
        """;
    public const string CreateViewSql= """
        CREATE OR REPLACE VIEW `TsnHierarchyNames` AS
         
        
                -- 1. TAXONES ACEPTADOS
                SELECT
                    h.TSN AS Acepted_TSN,
                    tu.complete_name AS Acepted_complete_name,
                    tu_tAuthor.shortauthor AS Acepted_Autor,
                    tut.rank_name AS Accepted_RankName,
                    tu.tsn AS tu_TSN,
        	         tu.name_usage AS tu_IsValid,
         			  'Accepted' AS tu_IsValid_Reason,
                    tu.complete_name AS tu_complete_name,
                    tut.rank_name AS tu_rank_name,
                    h.hierarchy_string
                FROM hierarchy AS h
                INNER JOIN taxonomic_units AS tu
                    ON h.TSN = tu.tsn
                INNER JOIN taxon_unit_types AS tut
                    ON tu.kingdom_id = tut.kingdom_id
                    AND tu.rank_id = tut.rank_id
                 INNER JOIN strippedauthor AS tu_tAuthor
                 ON tu.taxon_author_id=tu_tAuthor.taxon_author_id
                UNION ALL

                -- 2. SINÓNIMOS
                SELECT
                    sl.tsn_accepted AS Acepted_TSN,
                    tu_valid.complete_name AS Acepted_complete_name,
                    tu_tAuthor.shortauthor AS Acepted_Autor,
                    tut_valid.rank_name AS Accepted_RankName,
                    tu.tsn AS tu_TSN,
        	 			tu.name_usage AS tu_IsValid,
         			   'Synonym' AS tu_IsValid_Reason,
                    tu.complete_name AS tu_complete_name,
                    tut.rank_name AS tu_rank_name,
                    h.hierarchy_string
                FROM synonym_links AS sl
                INNER JOIN hierarchy AS h
                    ON sl.tsn_accepted = h.TSN
                INNER JOIN taxonomic_units AS tu
                    ON sl.tsn = tu.tsn
                INNER JOIN taxon_unit_types AS tut
                    ON tu.kingdom_id = tut.kingdom_id
                    AND tu.rank_id = tut.rank_id
                INNER JOIN taxonomic_units AS tu_valid
                    ON sl.tsn_accepted = tu_valid.tsn
                INNER JOIN taxon_unit_types AS tut_valid
                    ON tu_valid.kingdom_id = tut_valid.kingdom_id
                    AND tu_valid.rank_id = tut_valid.rank_id
              INNER JOIN strippedauthor AS tu_tAuthor
                 ON tu.taxon_author_id=tu_tAuthor.taxon_author_id
                ORDER BY hierarchy_string, tu_IsValid_Reason;
       
        """;
    /// <summary>
    /// Creates or refreshes the view. The database account needs CREATE VIEW permission.
    /// </summary>
    public Task EnsureViewExistsAsync(CancellationToken cancellationToken = default) =>
        dbContext.Database.ExecuteSqlRawAsync(CreateViewSql, cancellationToken);

    /// <summary>
    /// Returns the accepted name and synonyms for an accepted TSN.
    /// </summary>
    public async Task<IReadOnlyList<TsnHierarchyName>> GetNamesForAcceptedTsnAsync(
        int acceptedTsn,
        CancellationToken cancellationToken = default)
    {
        await EnsureViewExistsAsync(cancellationToken);

        return await dbContext.Database
            .SqlQuery<TsnHierarchyName>($"""
                SELECT `tsn_accepted`, `tsn_name`, `complete_name`, `rank_name`, `name_status`, `hierarchy_string`
                FROM `TsnHierarchyNames`
                WHERE `tsn_accepted` = {acceptedTsn}
                ORDER BY `complete_name`, `name_status`
                """)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Finds rows where the TSN identifies either the accepted taxon or a name/synonym.
    /// </summary>
    public async Task<IReadOnlyList<vw_synonimous>> GetByTsnAsync(
        int tsn,
        CancellationToken cancellationToken = default)
    {
        await EnsureViewExistsAsync(cancellationToken);
        //Acepted_TSN
        /*
        Acepted_complete_name
        Acepted_Autor
Accepted_RankName
tu_TSN
tu_IsValid
tu_IsValid_Reason
tu_complete_name
tu_rank_name
hierarchy_string
       */

        return await dbContext.Database
            .SqlQuery<vw_synonimous>($"""
                SELECT *
                FROM `vw_synonimous`
                WHERE `Acepted_tsn` = {tsn} OR `tu_TSN` = {tsn}
                ORDER BY `tu_IsValid`, `tu_complete_name`
                """)
            .ToListAsync(cancellationToken);
    }
}

public sealed class TsnHierarchyName
{
    public int tsn_accepted { get; set; }

    public int tsn_name { get; set; }

    public string? complete_name { get; set; }

    public string? rank_name { get; set; }

    public string name_status { get; set; } = string.Empty;

    public string? hierarchy_string { get; set; }
}
