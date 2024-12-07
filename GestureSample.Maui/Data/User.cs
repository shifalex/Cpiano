using SQLite;

namespace GestureSample.Maui.Data
{
    [Table("User")]
    public class User
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AvatarUri { get; set; } // could be a URL or a local file path
    }
}
