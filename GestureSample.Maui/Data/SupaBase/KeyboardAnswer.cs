using System.Text.Json;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace GestureSample.Maui.Data.SupaBase
{
    [Table("KeyboardAnswer")]
    public class KeyboardAnswer : BaseModel
    {

        public string GameId { get; set; }

        public int QuestionNumber { get; set; }
        public DateTime AnswerTime { get; set; }
        public bool[] keyboardAnswer { get; set; }
        // Serialize GameConfig as JSON for storage
        [Column("KeyboardAnswerJson")]
        public string KeyboardAnswerJson
        {
            get => JsonSerializer.Serialize(keyboardAnswer);
            set => keyboardAnswer = value != null ? JsonSerializer.Deserialize<bool[]>(value) : null;
        }

    }
}
