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
using System.Globalization;

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
        public bool WasHeaderResultToggleUsed { get; set; } = false;


        public int? aboveNumber { get; set; }
        public int? length { get; set; }
        public int? MoveByLength { get; set; }
        public int KeyboardRows { get; set; } = 1;
        public int KeyboardKeysInRow { get; set; } = 10;
        public bool ShowNumbersOnKeys { get; set; } = false;
        public string? QuestionPromptText { get; set; }

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
        [Ignore]
        public int[]? KeyboardWeights { get; set; }
        [Ignore]
        public bool[]? InitialKeyboardState { get; set; }
        [Ignore]
        public Color[]? QuestionKeyboardColors { get; set; }
        [Ignore]
        public Color[]? QuestionKeyboardColors2 { get; set; }
        [Ignore]
        public Color[]? SubmittedKeyboardColors { get; set; }
        [Ignore]
        public Color[]? InitialKeyboardColors { get; set; }

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

        [Column("KeyboardWeightsJson")]
        public string KeyboardWeightsJson
        {
            get => JsonSerializer.Serialize(KeyboardWeights);
            set => KeyboardWeights = value != null ? JsonSerializer.Deserialize<int[]>(value) : null;
        }

        [Column("InitialKeyboardStateJson")]
        public string InitialKeyboardStateJson
        {
            get => JsonSerializer.Serialize(InitialKeyboardState);
            set => InitialKeyboardState = value != null ? JsonSerializer.Deserialize<bool[]>(value) : null;
        }

        [Column("QuestionKeyboardColorsJson")]
        public string QuestionKeyboardColorsJson
        {
            get => JsonSerializer.Serialize(SerializeColors(QuestionKeyboardColors));
            set => QuestionKeyboardColors = DeserializeColors(value);
        }

        [Column("QuestionKeyboardColorsJson2")]
        public string QuestionKeyboardColorsJson2
        {
            get => JsonSerializer.Serialize(SerializeColors(QuestionKeyboardColors2));
            set => QuestionKeyboardColors2 = DeserializeColors(value);
        }

        [Column("SubmittedKeyboardColorsJson")]
        public string SubmittedKeyboardColorsJson
        {
            get => JsonSerializer.Serialize(SerializeColors(SubmittedKeyboardColors));
            set => SubmittedKeyboardColors = DeserializeColors(value);
        }

        [Column("InitialKeyboardColorsJson")]
        public string InitialKeyboardColorsJson
        {
            get => JsonSerializer.Serialize(SerializeColors(InitialKeyboardColors));
            set => InitialKeyboardColors = DeserializeColors(value);
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
        public bool HasQuestionKeyboard => keyboard1 != null && keyboard1.Length > 0;

        [Ignore]
        public bool HasInitialKeyboardState => InitialKeyboardState != null && InitialKeyboardState.Length > 0;

        [Ignore]
        public bool HasQuestionKeyboardColors => QuestionKeyboardColors != null && QuestionKeyboardColors.Length > 0;

        [Ignore]
        public bool HasSecondQuestionKeyboardColors => QuestionKeyboardColors2 != null && QuestionKeyboardColors2.Length > 0;

        [Ignore]
        public bool HasSubmittedKeyboardColors => SubmittedKeyboardColors != null && SubmittedKeyboardColors.Length > 0;

        [Ignore]
        public bool HasInitialKeyboardColors => InitialKeyboardColors != null && InitialKeyboardColors.Length > 0;

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
        public bool HasPromptText => !string.IsNullOrWhiteSpace(QuestionPromptText);

        [Ignore]
        public bool IsSpecialArrowPrompt =>
            HasPromptText &&
            (QuestionPromptText!.Contains("|--->", StringComparison.Ordinal) ||
             QuestionPromptText.Contains("(ordinal)", StringComparison.OrdinalIgnoreCase));

        [Ignore]
        public bool ShowCombinedArrowKeyboard => HasArrowPrompt;

        [Ignore]
        public bool ShowSeparateQuestionKeyboard => !ShowCombinedArrowKeyboard && HasQuestionKeyboard;

        [Ignore]
        public bool ShowSeparateSubmittedKeyboard => !ShowCombinedArrowKeyboard && HasSubmittedKeyboard;

        [Ignore]
        public bool ShowPrimaryQuestionKeyboard => ShowSeparateQuestionKeyboard && !HasInitialKeyboardState;

        [Ignore]
        public bool ShowInitialKeyboardPreview => !ShowCombinedArrowKeyboard && HasInitialKeyboardState;

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
                if (HasQuestionKeyboardColors || HasSubmittedKeyboardColors)
                {
                    int colorLength = Math.Max(QuestionKeyboardColors?.Length ?? 0, SubmittedKeyboardColors?.Length ?? 0);
                    if (colorLength > 0)
                    {
                        Color[] combinedColors = Enumerable.Repeat(Colors.White, colorLength).ToArray();
                        for (int i = 0; i < colorLength; i++)
                        {
                            bool hasQuestion = QuestionKeyboardColors != null && i < QuestionKeyboardColors.Length && !IsFreeColor(QuestionKeyboardColors[i]);
                            bool hasAnswer = SubmittedKeyboardColors != null && i < SubmittedKeyboardColors.Length && !IsFreeColor(SubmittedKeyboardColors[i]);

                            if (hasQuestion && hasAnswer)
                            {
                                Color currentQuestionColor = QuestionKeyboardColors![i];
                                Color currentAnswerColor = SubmittedKeyboardColors![i];
                                combinedColors[i] = ColorsMatch(currentQuestionColor, currentAnswerColor)
                                    ? currentAnswerColor
                                    : Color.FromArgb("#A9D8A5");
                            }
                            else if (hasQuestion)
                            {
                                combinedColors[i] = QuestionKeyboardColors![i];
                            }
                            else if (hasAnswer)
                            {
                                combinedColors[i] = SubmittedKeyboardColors![i];
                            }
                        }

                        return combinedColors;
                    }
                }

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
        public Color[]? QuestionSnapshotColors => HasQuestionKeyboardColors ? QuestionKeyboardColors : BuildColorsFromBits(keyboard1);

        [Ignore]
        public Color[]? SecondQuestionSnapshotColors => HasSecondQuestionKeyboardColors ? QuestionKeyboardColors2 : BuildColorsFromBits(keyboard2);

        [Ignore]
        public Color[]? SubmittedKeyboardSnapshotColors => HasSubmittedKeyboardColors ? SubmittedKeyboardColors : BuildColorsFromBits(SubmittedKeyboard);

        [Ignore]
        public Color[]? InitialKeyboardSnapshotColors => HasInitialKeyboardColors ? InitialKeyboardColors : BuildColorsFromBits(InitialKeyboardState);

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

                if (HasPromptText)
                {
                    parts.Add($"Prompt {QuestionPromptText}");
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
                Rows = Math.Max(1, rows),
                ShowNumbersOnKeys = ShowNumbersOnKeys || (KeyboardWeights != null && KeyboardWeights.Length > 0),
                WeightsArray = KeyboardWeights?.ToArray(),
                IsMulticolor = HasMulticolorSnapshots()
            };
        }

        private bool HasMulticolorSnapshots()
        {
            return HasMulticolorSnapshot(QuestionKeyboardColors) ||
                   HasMulticolorSnapshot(QuestionKeyboardColors2) ||
                   HasMulticolorSnapshot(SubmittedKeyboardColors) ||
                   HasMulticolorSnapshot(InitialKeyboardColors) ||
                   Op == Operation.GroupByColor;
        }

        private static bool HasMulticolorSnapshot(Color[]? colors)
        {
            if (colors == null || colors.Length == 0)
                return false;

            int usedColorCount = colors
                .Where(color => !IsFreeColor(color))
                .Select(ToColorToken)
                .Distinct(StringComparer.Ordinal)
                .Count();

            return usedColorCount > 1;
        }

        private static Color[]? BuildColorsFromBits(bool[]? bits)
        {
            if (bits == null || bits.Length == 0)
                return null;

            Color[] colors = new Color[bits.Length];
            for (int i = 0; i < bits.Length; i++)
                colors[i] = bits[i] ? Colors.Yellow : Colors.White;

            return colors;
        }

        private static string[]? SerializeColors(Color[]? colors)
        {
            return colors?.Select(ToColorToken).ToArray();
        }

        private static Color[]? DeserializeColors(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            string[]? tokens = JsonSerializer.Deserialize<string[]>(json);
            if (tokens == null || tokens.Length == 0)
                return null;

            return tokens.Select(ParseColorToken).ToArray();
        }

        private static string ToColorToken(Color color)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.######},{1:0.######},{2:0.######},{3:0.######}",
                color.Red,
                color.Green,
                color.Blue,
                color.Alpha);
        }

        private static Color ParseColorToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Colors.White;

            string[] parts = token.Split(',');
            if (parts.Length != 4)
                return Colors.White;

            return new Color(
                float.Parse(parts[0], CultureInfo.InvariantCulture),
                float.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture),
                float.Parse(parts[3], CultureInfo.InvariantCulture));
        }

        private static bool IsFreeColor(Color color) => ColorsMatch(color, Colors.White) || ColorsMatch(color, Colors.Transparent);

        private static bool ColorsMatch(Color a, Color b)
        {
            const float epsilon = 0.01f;
            return Math.Abs(a.Red - b.Red) < epsilon &&
                   Math.Abs(a.Green - b.Green) < epsilon &&
                   Math.Abs(a.Blue - b.Blue) < epsilon &&
                   Math.Abs(a.Alpha - b.Alpha) < epsilon;
        }

    }
}
