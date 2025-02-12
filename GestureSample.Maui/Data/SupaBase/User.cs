using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace GestureSample.Maui.Data.SupaBase
{
    [Table("Users")]
    public class User : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("avatarUri")]
        public string AvatarUri { get; set; } // could be a URL or a local file path
        
        [Column("lastLogin")]
        public DateTime LastLoginTime { get; set; } // New property
    }
}
