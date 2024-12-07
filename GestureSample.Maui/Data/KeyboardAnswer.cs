using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.Text.Json;

namespace GestureSample.Maui.Data
{
    [Table("KeyboardAnswer")]
    public class KeyboardAnswer
    {

        public string GameId { get; set; }

        public int QuestionNumber { get; set; }
        public DateTime AnswerTime { get; set; }
        [Ignore]
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
