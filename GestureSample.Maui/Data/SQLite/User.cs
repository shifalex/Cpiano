using SQLite;
using Supabase;
using Supabase.Postgrest.Models;

namespace GestureSample.Maui.Data.SQLite
{
    [Table("User")]
    public class User
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AvatarUri { get; set; } // could be a URL or a local file path

        public DateTime LastLoginTime { get; set; } // New property
        public bool IsTeacher { get; set; } = false;
    }
}
