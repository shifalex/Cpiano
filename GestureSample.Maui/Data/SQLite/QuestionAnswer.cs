//using SQLite;
//using Microsoft.Data.Sqlite;
using SQLite;
using Supabase.Postgrest.Models;
using System.Text.Json;
//using Realms;

namespace GestureSample.Maui.Data.SQLite
{
    [Table("QuestionAnswer")]
    public class QuestionAnswer

    {
        [PrimaryKey, AutoIncrement]
        public int QuestionID { get; set; }
        public int QuestionNumber { get; set; }
        public string GameId { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public Guid UserId { get; set; }
        public int Sum { get; set; }
        public int Addend1 { get; set; }
        public int Addend2 { get; set; }
        public int SecondarySum { get; set; } = -1111;
        public int SecondaryAddend1 { get; set; } = -1111;
        public int SecondaryAddend2 { get; set; } = -1111;
        public bool SecondarySumEnabled { get; set; } = false;
        public bool SecondaryAddend1Enabled { get; set; } = false;
        public bool SecondaryAddend2Enabled { get; set; } = false;

        public int ResultStatus { get; set; } = 0;

        //public string Op { get; set; } = Operation.Sum.ToString();

        // Ignore GameConfig during table creation
        [Ignore]
        public Operation Op { get; set; } = Operation.Sum;

        // Serialize GameConfig as JSON for storage
        [Column("ConfigJson")]
        public string ConfigJson
        {
            get => JsonSerializer.Serialize(Op);
            set => Op = value != null ? JsonSerializer.Deserialize<Operation>(value) : Operation.Sum;
        }

        //public Color[] KeysPressed { get; set; }



    }

    public class ShowState : QuestionAnswer
    {
        private const int EmptyPpwValue = -1111;

        public ShowState(QuestionAnswer state)
        {
            QuestionNumber = state.QuestionNumber;
            Time = state.Time;
            UserId = state.UserId;
            Sum = state.Sum;
            Addend1 = state.Addend1;
            Addend2 = state.Addend2;
            SecondaryAddend1 = state.SecondaryAddend1;
            SecondaryAddend2 = state.SecondaryAddend2;
            SecondarySum = state.SecondarySum;
            SecondaryAddend1Enabled = state.SecondaryAddend1Enabled;
            SecondaryAddend2Enabled = state.SecondaryAddend2Enabled;
            SecondarySumEnabled = state.SecondarySumEnabled;
            Op = state.Op;
            ResultStatus = state.ResultStatus;
            OpDString = state.Op.ToDString();
        }

        public Color Addend1Color { get; set; } = Colors.White;
        public Color Addend2Color { get; set; } = Colors.White;
        public Color SumColor { get; set; } = Colors.White;
        public Color TimeColor { get { return TimeOnTask > 6 ? Colors.Yellow : Colors.White; } }
        public DateTimeOffset? StartTime { get; set; } = null;

        public Color RowBackgroundColor { get; set; }
        public double? TimeOnTask { get { if (StartTime == null) return null; return ((TimeSpan)(Time - StartTime)).TotalSeconds; } }
        public int SerialNumber { get; set; }
        public string OpDString { get; set; }
        public List<ShowQuestionAnswerPartRow> HelperRows { get; set; } = new();
        public bool HasHelperRows => HelperRows.Count > 0;
        public bool HasSecondaryPpw =>
            SecondaryAddend1 != EmptyPpwValue ||
            SecondaryAddend2 != EmptyPpwValue ||
            SecondarySum != EmptyPpwValue;
        public Color SecondaryAddend1BackgroundColor => SecondaryAddend1Enabled ? Colors.White : Color.FromArgb("#F1F1F1");
        public Color SecondaryAddend2BackgroundColor => SecondaryAddend2Enabled ? Colors.White : Color.FromArgb("#F1F1F1");
        public Color SecondarySumBackgroundColor => SecondarySumEnabled ? Colors.White : Color.FromArgb("#F1F1F1");
        public Color SecondaryAddend1TextColor => SecondaryAddend1Enabled ? Colors.Black : Colors.Gray;
        public Color SecondaryAddend2TextColor => SecondaryAddend2Enabled ? Colors.Black : Colors.Gray;
        public Color SecondarySumTextColor => SecondarySumEnabled ? Colors.Black : Colors.Gray;
        public string SecondaryAddend1Text => SecondaryAddend1 == EmptyPpwValue ? string.Empty : SecondaryAddend1.ToString();
        public string SecondaryAddend2Text => SecondaryAddend2 == EmptyPpwValue ? string.Empty : SecondaryAddend2.ToString();
        public string SecondarySumText => SecondarySum == EmptyPpwValue ? string.Empty : SecondarySum.ToString();

        public void SetHelperParts(IEnumerable<QuestionAnswerPart>? parts)
        {
            HelperRows = parts?
                .GroupBy(item => item.RowIndex)
                .OrderBy(group => group.Key)
                .Select(group => new ShowQuestionAnswerPartRow
                {
                    Parts = group
                        .OrderBy(item => item.ColumnIndex)
                        .Select(item => new ShowQuestionAnswerPartItem
                        {
                            ValueText = item.ValueText,
                            BackgroundColor = item.IsEnabled ? Colors.White : Color.FromArgb("#F1F1F1"),
                            TextColor = item.IsEnabled ? Colors.Black : Colors.Gray
                        })
                        .ToList()
                })
                .Where(row => row.Parts.Count > 0)
                .ToList()
                ?? new List<ShowQuestionAnswerPartRow>();
        }
    }

    public class ShowQuestionAnswerPartRow
    {
        public List<ShowQuestionAnswerPartItem> Parts { get; set; } = new();
    }

    public class ShowQuestionAnswerPartItem
    {
        public string ValueText { get; set; } = string.Empty;
        public Color BackgroundColor { get; set; } = Colors.White;
        public Color TextColor { get; set; } = Colors.Black;
    }
}
