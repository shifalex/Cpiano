using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Models;
using SQLite;

SQLitePCL.Batteries_V2.Init();
int checks = 0;
void Check(bool condition, string message)
{
    checks++;
    if (!condition) throw new Exception(message);
}

using var db = new SQLiteConnection(":memory:");
db.Execute("CREATE TABLE KeyEvent (id INTEGER PRIMARY KEY AUTOINCREMENT, GameId TEXT, QuestionNumber INTEGER, AttemptNumber INTEGER, EventType INTEGER, KeyNumber INTEGER, Row INTEGER, EventTime BIGINT, RelativeX REAL, RelativeY REAL)");
db.Insert(new LegacyEvent { GameId = "legacy", EventType = 1, KeyNumber = 1 });
db.CreateTable<KeyEvent>();
var legacy = db.Table<KeyEvent>().First();
Check(legacy.ContactId == null && legacy.EventSequence == 0 && legacy.FromKeyNumber == null,
    "Existing rows remain readable after adding replay columns");
var time = DateTime.Now;
var events = new[]
{
    new KeyEvent { EventType = 1, KeyNumber = 1, ContactId = "A", EventSequence = 1, ElapsedMs = 0 },
    new KeyEvent { EventType = 1, KeyNumber = 2, ContactId = "B", EventSequence = 2, ElapsedMs = 10 },
    new KeyEvent { EventType = 4, FromKeyNumber = 1, KeyNumber = 3, ContactId = "A", EventSequence = 3, ElapsedMs = 180 },
    new KeyEvent { EventType = 4, FromKeyNumber = 2, KeyNumber = 4, ContactId = "B", EventSequence = 4, ElapsedMs = 180 },
    new KeyEvent { EventType = 0, KeyNumber = 3, ContactId = "A", EventSequence = 5, ElapsedMs = 520 },
    new KeyEvent { EventType = 0, KeyNumber = 4, ContactId = "B", EventSequence = 6, ElapsedMs = 530 }
};
// Insert in reverse order to simulate synchronization assigning fresh IDs.
foreach (var item in events.Reverse())
{
    item.GameId = "new"; item.QuestionNumber = 1; item.EventTime = time;
    db.Insert(item);
}
var loaded = db.Table<KeyEvent>().Where(e => e.GameId == "new").ToList()
    .OrderBy(e => e.EventTime).ThenBy(e => e.EventSequence).ThenBy(e => e.id).ToList();
Check(loaded.Select(e => e.EventSequence).SequenceEqual(new long[] { 1, 2, 3, 4, 5, 6 }),
    "Same-time events replay in capture order after reload");
Check(loaded[2].ContactId == "A" && loaded[2].FromKeyNumber == 1 && loaded[2].ElapsedMs == 180,
    "Glide identity, source and elapsed time survive SQLite roundtrip");
Check(loaded[2].EventTypeText == "Glide", "Glide is identified separately in the event log");
Check(loaded.Count(e => e.EventType == 1) == 2, "Two glides do not add key-downs");
bool[] state = new bool[4];
foreach (var item in loaded)
{
    if (item.EventType == 1) state[item.KeyNumber - 1] = true;
    else if (item.EventType == 4) KeyboardReplayTransitions.ApplyGlide(state, item.FromKeyNumber, item.KeyNumber, false);
    else if (item.EventType == 0) state[item.KeyNumber - 1] = false;
    if (item.EventSequence == 4)
        Check(state.SequenceEqual(new[] { false, false, true, true }), "Two simultaneous glides reconstruct both destinations");
}
Check(state.All(pressed => !pressed), "Release at destination clears both moved contacts");
Console.WriteLine($"Passed {checks} replay persistence checks.");

[Table("KeyEvent")]
class LegacyEvent
{
    [PrimaryKey, AutoIncrement] public int id { get; set; }
    public string GameId { get; set; } = "";
    public int EventType { get; set; }
    public int KeyNumber { get; set; }
}
