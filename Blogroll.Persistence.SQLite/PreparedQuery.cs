using System.Data.SQLite;
using System.Linq;

namespace Blogroll.Persistence.SQLite
{
    internal sealed class PreparedQuery
    {
        public PreparedQuery(SQLiteConnection db,
            string sql, params (string Key, object Value)[] placeHoldersWithValues)
        {
            _db = db;
            _sql = sql;
            _placeHoldersWithValues = placeHoldersWithValues;
        }

        private readonly SQLiteConnection _db;
        private readonly string _sql;
        private readonly (string Key, object Value)[] _placeHoldersWithValues;

        public SQLiteCommand Command()
        {
            var cmd = new SQLiteCommand(_sql, _db);
            _placeHoldersWithValues.ToList().ForEach(kvp => cmd.Parameters.AddWithValue(kvp.Key, kvp.Value));
            return cmd;
        }
    }
}