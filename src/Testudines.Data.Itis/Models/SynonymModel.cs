namespace Testudines.Data.Itis.Models;

public class SynonymModel
{
    public int Tsn { get; set; }

    public string ScientificName { get; set; } = "";

    public string? Author { get; set; }

    public string? UnacceptabilityReason { get; set; }
}