

namespace Testudines.Data.Itis.Models
{
    public class vw_synonimous
    {
        public int Acepted_TSN { get; set; } = 0;
        public string? Acepted_complete_name{ get; set; }
        public string? Acepted_Autor { get; set; }
        public string? Accepted_RankName { get; set; }
        public int tu_TSN { get; set; } = 0;
        public string? tu_IsValid { get; set; }
        public string? tu_IsValid_Reason { get; set; }
        public string? tu_complete_name { get; set; }
        public string? tu_rank_name { get; set; }
        public string? hierarchy_string { get; set; }
    }
}
