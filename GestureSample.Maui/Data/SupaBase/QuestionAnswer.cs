using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json;

namespace GestureSample.Maui.Data.SupaBase
{
    [Table("QuestionsAnswers")]
    public class QuestionAnswer : BaseModel

    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }
        [Column("questionNumber")]
        public int QuestionNumber { get; set; }
        [Column("gameId")]
        public Guid GameId { get; set; }
        [Column("time")]
        public DateTime Time { get; set; } = DateTime.Now;
        
        [Column("addend1")]
        public int Addend1 { get; set; }
        [Column("op")]
        public string Op { get; set; } = Operation.Sum.ToString();
        [Column("addend2")]
        public int Addend2 { get; set; }
        [Column("sum")]
        public int Sum { get; set; }
        [Column("secondarySum")]
        public int SecondarySum { get; set; } = -1111;
        [Column("secondaryAddend1")]
        public int SecondaryAddend1 { get; set; } = -1111;
        [Column("secondaryAddend2")]
        public int SecondaryAddend2 { get; set; } = -1111;
        [Column("secondarySumEnabled")]
        public bool SecondarySumEnabled { get; set; }
        [Column("secondaryAddend1Enabled")]
        public bool SecondaryAddend1Enabled { get; set; }
        [Column("secondaryAddend2Enabled")]
        public bool SecondaryAddend2Enabled { get; set; }
        [Column("resultStatus")]
        public int ResultStatus { get; set; } = 0;

    }
}
