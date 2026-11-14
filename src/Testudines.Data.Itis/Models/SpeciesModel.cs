namespace Testudines.Data.Itis.Models;
public class SpeciesModel
{
    public int Tsn { get; set; }

    public string ScientificName { get; set; } = "";

    public string? Author { get; set; }

    public List<SynonymModel> Synonyms { get; set; } = [];
}