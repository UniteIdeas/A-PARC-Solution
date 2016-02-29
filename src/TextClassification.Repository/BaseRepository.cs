using System;
using System.Data;
using System.Data.SQLite;
using TextClassification.Repository.Context;

namespace TextClassification.Repository
{
    public class BaseRepository
    {
        internal static SqLiteContext Context
        {
            get { return new SqLiteContext("DbConnection"); }
        }

        internal static SQLiteParameter SqlParameter(object value, string name, DbType dbType = DbType.Int32, int size = 0,
                                                                         ParameterDirection direction = ParameterDirection.Input)
        {

            var result = new SQLiteParameter
            {
                DbType = dbType,
                ParameterName = name,
                Value = value,
                Direction = direction,
                Size = size
            };
            if (value == null) result.Value = DBNull.Value;

            return result;
        }

        internal void Dispose()
        {
            Context.Dispose();
        }
    }
}
