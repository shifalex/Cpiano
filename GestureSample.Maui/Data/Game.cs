//using SQLite;
//using Microsoft.Data.Sqlite;
//using MongoDB.Bson.IO;
using Microsoft.Maui.Platform;
using SQLite;
using System.Text.Json;
//using Realms;

namespace GestureSample.Maui.Data
{
    [Table("Game")]
    public class Game //: RealmObject

    {
        [PrimaryKey]
        public string Id { get; set; }
        public int index { get; set; }
        public DateTime TimeStart { get; set; } = DateTime.Now;
        public DateTime TimeEnd { get; set; } = DateTime.Now;//TODO: excgange into the last endtime of the game by calculating
        public int FinalStatus { get; set; } = -1;
        //public TimeSpan FinalTime { get; set; } = TimeSpan.Zero;
        public string UserId { get; set; }
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public string GameName { get; set; }

        public double MeasureTextWidth(string text, double fontSize)
        {
            var label = new Label { Text = text, FontSize = fontSize };
            var size = label.Measure(double.PositiveInfinity, double.PositiveInfinity);
            return size.Request.Width;
        }

        private string TruncateFormattedGameName(string formattedGameName, string prefix, string suffix, double fontSize, double availableWidth)
        {
            const string ellipsis = "...";
            string truncatedName = formattedGameName;
            string fullText = $"{prefix}{truncatedName}{suffix}";

            // Measure the widths of prefix and suffix
            double prefixWidth = MeasureTextWidth(prefix, fontSize);
            double suffixWidth = MeasureTextWidth(suffix, fontSize);
            double ellipsisWidth = MeasureTextWidth(ellipsis, fontSize);

            // Calculate available width for the game name
            double availableNameWidth = availableWidth - prefixWidth - suffixWidth;

            if (availableNameWidth <= 0)
            {
                // Not enough space to display the game name
                return $"{prefix}{ellipsis}{suffix}";
            }

            // If the full text fits, return it as is
            if (MeasureTextWidth(fullText, fontSize) <= availableWidth)
            {
                return fullText;
            }

            // Truncate the game name
            for (int i = formattedGameName.Length; i > 0; i--)
            {
                truncatedName = formattedGameName.Substring(0, i);
                double nameWidth = MeasureTextWidth(truncatedName, fontSize);

                if (nameWidth + ellipsisWidth <= availableNameWidth)
                {
                    return $"{prefix}{truncatedName}{ellipsis}{suffix}";
                }
            }

            // If no characters fit, return prefix and suffix with ellipsis
            return $"{prefix}{ellipsis}{suffix}";
        }

        public override string ToString()
        {
            string status =  FinalStatus switch { 0=>"Lose", 1=>"WIN!", _ => ""};
            string time = ((TimeSpan)(TimeEnd-TimeStart)).ToFormattedString("mm:ss");
            string formattedGameName = (GameName ?? string.Empty);

            string prefix = $"{index.ToString().PadLeft(3)} {TimeStart:t} ";
            string suffix = $"{status} {time} {(Wins-Losses)}/{Wins}".PadLeft(14);

            int availableCenterWidth = 17;
            if (formattedGameName.Length > availableCenterWidth)
            {
                // If there is enough space for the ellipsis
                if (availableCenterWidth >= 3)
                {
                    formattedGameName = formattedGameName.Substring(0, availableCenterWidth - 3) + "...";
                }
                else
                {
                    // Not enough space for the ellipsis, so use empty string or as much as fits
                    formattedGameName = formattedGameName.Substring(0, availableCenterWidth);
                }
            }
            int padding = availableCenterWidth - formattedGameName.Length;
            int padLeft = padding / 2;
            int padRight = padding - padLeft;

            string centeredGameName = formattedGameName.PadRight(availableCenterWidth);



            return $"{prefix} {centeredGameName} {suffix}";

            //string displayString = TruncateFormattedGameName(formattedGameName, prefix, suffix, Device.GetNamedSize(NamedSize.Default, typeof(Label)), 350);
            //return displayString;
        }


        // Ignore GameConfig during table creation
        [Ignore]
        public GameConfig Config { get; set; }

        // Serialize GameConfig as JSON for storage
        [Column("ConfigJson")]
        public string ConfigJson
        {
            get => Config != null ? JsonSerializer.Serialize(Config) : null;
            set => Config = value != null ? JsonSerializer.Deserialize<GameConfig>(value) : null;
        }

        //public Color[] KeysPressed { get; set; }



    }
}
