using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Data
{
    internal class KeyboardAnswerRepository : BaseRepository<QuestionAnswer>
    {
        public KeyboardAnswerRepository(SQLiteAsyncConnection database) : base(database) { }
    }
}
