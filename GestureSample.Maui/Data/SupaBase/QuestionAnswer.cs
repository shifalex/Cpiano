using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json;

namespace GestureSample.Maui.Data.SupaBase
{
    [Table("QuestionsAnswers")]
    public class QuestionAnswer : BaseModel

    {
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
        [Column("resultStatus")]
        public int ResultStatus { get; set; } = 0;

    }
}
