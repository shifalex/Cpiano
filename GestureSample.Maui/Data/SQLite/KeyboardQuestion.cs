using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Models;
using SQLite;
using Supabase.Postgrest.Models;

namespace GestureSample.Maui.Data.SQLite
{
    [Table("KeyboardQuestion")]
    public class KeyboardQuestion
    {
        [PrimaryKey, AutoIncrement]
        public int QuestionID { get; set; }
        public int QuestionNumber { get; set; }
        public int AttemptNumber { get; set; } = 0;
        public string GameId { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public Guid UserId { get; set; } = Guid.Empty;
        public int ResultStatus { get; set; } = 0;
        public bool WasTutorialUsed { get; set; } = false;


        public int? aboveNumber { get; set; }
        public int? length { get; set; }
        public int? MoveByLength { get; set; }
        public int KeyboardRows { get; set; } = 1;
        public int KeyboardKeysInRow { get; set; } = 10;

        [Ignore]
        public Color RowBackgroundColor { get; set; } = Colors.White;


        //public string Op { get; set; } = Operation.Sum.ToString();

        // Ignore GameConfig during table creation
        [Ignore]
        public bool[] keyboard1 { get; set; }
        [Ignore]
        public bool[] keyboard2 { get; set; }
        [Ignore]
        public Direction dir { get; set; }
        [Ignore]
        public Direction MoveByDirection { get; set; } = Direction.Right;
        [Ignore]
        public bool[] SubmittedKeyboard { get; set; }

        // Serialize GameConfig as JSON for storage
        [Column("ConfigJson")]
        public string ConfigJson
        {
            get => JsonSerializer.Serialize(keyboard1);
            set => keyboard1 = value != null ? JsonSerializer.Deserialize<bool[]>(value) : null;
        }

        // Serialize GameConfig as JSON for storage
        [Column("ConfigJson2")]
        public string ConfigJson2
        {
            get => JsonSerializer.Serialize(keyboard2);
            set => keyboard2 = value != null ? JsonSerializer.Deserialize<bool[]>(value) : null;
        }
        [Ignore]
        public Operation Op { get; set; } = Operation.Sum;

        // Serialize GameConfig as JSON for storage
        [Column("ConfigJson4")]
        public string ConfigJson4
        {
            get => JsonSerializer.Serialize(Op);
            set => Op = value != null ? JsonSerializer.Deserialize<Operation>(value) : Operation.Copy;
        }

        // Serialize GameConfig as JSON for storage
        [Column("ConfigJson3")]
        public string ConfigJson3
        {
            get => JsonSerializer.Serialize(dir);
            set => dir = value != null ? JsonSerializer.Deserialize<Direction>(value) : Direction.Right;
        }

        [Column("SubmittedKeyboardJson")]
        public string SubmittedKeyboardJson
        {
            get => JsonSerializer.Serialize(SubmittedKeyboard);
            set => SubmittedKeyboard = value != null ? JsonSerializer.Deserialize<bool[]>(value) : null;
        }

        [Column("MoveByDirectionJson")]
        public string MoveByDirectionJson
        {
            get => JsonSerializer.Serialize(MoveByDirection);
            set => MoveByDirection = value != null ? JsonSerializer.Deserialize<Direction>(value) : Direction.Right;
        }

        public DateTime? SubmittedTime { get; set; }

        [Ignore]
        public bool HasSubmittedKeyboard => SubmittedKeyboard != null && SubmittedKeyboard.Length > 0;

        [Ignore]
        public bool HasSecondQuestionKeyboard => keyboard2 != null && keyboard2.Length > 0;

        [Ignore]
        public bool HasVisibleSecondQuestionKeyboard =>
            HasSecondQuestionKeyboard && GameConfig.Operations.LogicalDual.Contains(Op);

        [Ignore]
        public string SubmittedTimeText => SubmittedTime.HasValue ? SubmittedTime.Value.ToString("HH:mm:ss.fff") : "-";

        [Ignore]
        public string AttemptText => AttemptNumber > 0 ? $"Trial {AttemptNumber}" : "Trial";

        [Ignore]
        public bool HasArrowPrompt => aboveNumber.HasValue && length.HasValue;

        [Ignore]
        public bool ShowCombinedArrowKeyboard => HasArrowPrompt;

        [Ignore]
        public bool ShowSeparateQuestionKeyboard => !ShowCombinedArrowKeyboard;

        [Ignore]
        public bool ShowSeparateSubmittedKeyboard => !ShowCombinedArrowKeyboard && HasSubmittedKeyboard;

        [Ignore]
        public string CombinedArrowKeyboardLabel => HasSubmittedKeyboard ? "Q+A" : "Q";

        [Ignore]
        public string CombinedArrowLegendText =>
            HasSubmittedKeyboard ? "Blue = question, yellow = answer, green = both" : "Blue = question";

        [Ignore]
        public bool HasMoveByPrompt => MoveByLength.HasValue && Op == Operation.MoveBy;

        [Ignore]
        public Color[] CombinedArrowKeyboardColors
        {
            get
            {
                int lengthToUse = Math.Max(keyboard1?.Length ?? 0, SubmittedKeyboard?.Length ?? 0);
                if (lengthToUse == 0)
                    return Array.Empty<Color>();

                Color[] colors = Enumerable.Repeat(Colors.White, lengthToUse).ToArray();
                Color questionColor = Color.FromArgb("#D8ECFF");
                Color answerColor = Color.FromArgb("#FFF1A8");
                Color overlapColor = Color.FromArgb("#A9D8A5");

                for (int i = 0; i < lengthToUse; i++)
                {
                    bool hasQuestion = keyboard1 != null && i < keyboard1.Length && keyboard1[i];
                    bool hasAnswer = SubmittedKeyboard != null && i < SubmittedKeyboard.Length && SubmittedKeyboard[i];

                    colors[i] = (hasQuestion, hasAnswer) switch
                    {
                        (true, true) => overlapColor,
                        (true, false) => questionColor,
                        (false, true) => answerColor,
                        _ => Colors.White
                    };
                }

                return colors;
            }
        }

        [Ignore]
        public string QuestionCodeText
        {
            get
            {
                List<string> parts = new();

                parts.Add(Op.ToString());

                if (HasArrowPrompt)
                {
                    parts.Add($"Arrow {(dir == Direction.Right ? "->" : "<-")} key {aboveNumber} len {length}");
                }

                if (HasMoveByPrompt)
                {
                    parts.Add($"Shift {(MoveByDirection == Direction.Right ? "->" : "<-")} by {MoveByLength}");
                }

                if (HasSecondQuestionKeyboard)
                {
                    parts.Add("2 keyboards");
                }

                return string.Join("  |  ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
            }
        }

        [Ignore]
        public Color ResultLightColor => ResultStatus switch
        {
            1 => Colors.LightGreen,
            2 => Colors.Gold,
            _ when HasSubmittedKeyboard || SubmittedTime.HasValue => Colors.IndianRed,
            _ => Colors.LightGray
        };

        [Ignore]
        public string ResultStatusText => ResultStatus switch
        {
            1 => "Correct",
            2 => "Wrong Input",
            _ when HasSubmittedKeyboard || SubmittedTime.HasValue => "Wrong",
            _ => "Pending"
        };

        [Ignore]
        public string TutorialStatusText => WasTutorialUsed ? "Tutorial used" : string.Empty;

        public KeyboardConfig CreateKeyboardConfig()
        {
            int keysLength = keyboard1?.Length ?? SubmittedKeyboard?.Length ?? Math.Max(KeyboardKeysInRow, 1);
            int keysInRow = KeyboardKeysInRow > 0 ? KeyboardKeysInRow : Math.Max(keysLength, 1);
            int rows = KeyboardRows > 0 ? KeyboardRows : 1;

            if (keysInRow * rows < keysLength)
            {
                rows = (int)Math.Ceiling((double)keysLength / Math.Max(1, keysInRow));
            }

            return new KeyboardConfig
            {
                KeysInRow = Math.Max(1, keysInRow),
                Rows = Math.Max(1, rows)
            };
        }

    }
}
