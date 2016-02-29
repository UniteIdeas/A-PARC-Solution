using System.Configuration;
using System.Data.Entity;
using System.Data.SQLite;
using TextClassification.Model;

namespace TextClassification.Repository.Context
{
    public class SqLiteContext : DbContext
    {
        public DbSet<StemWord> StemWords { get; set; }
        public DbSet<WordWeight> WordWeights { get; set; }

		#region Construct 

        //public SqLiteContext()
        //{
        //    //var connectionString = ConfigurationManager.ConnectionStrings["DbConnection"];
        //    //CreateDatabaseIfNotExists<SqLiteContext>;
        //    Database.SetInitializer(new CreateInitializer());
        //}
        public SqLiteContext(string connString) : base(connString)
        {
            Database.SetInitializer(new CreateInitializer());
        }

        public class CreateInitializer : CreateDatabaseIfNotExists<SqLiteContext>
        {
            protected override void Seed(SqLiteContext context)
            {
                context.Seed(context);

                base.Seed(context);
            }
        }

		#endregion 

		#region init

		/// <summary>
		/// On Model Creating
		/// </summary>
		/// <param name="modelBuilder"></param>
		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
            modelBuilder.Entity<StemWord>().ToTable("frw_FrogWord");
            modelBuilder.Entity<WordWeight>().ToTable("wrw_WordWeight");
		}

        public void Seed (SqLiteContext context)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DbConnection"];
            const string dbname = "TextClassificationDatabase.sqlite";
            SQLiteConnection.CreateFile(dbname);
            var mDbConnection = new SQLiteConnection(connectionString.ConnectionString);
            mDbConnection.Open();
            const string sql = "CREATE TABLE frw_FrogWord (frw_text VARCHAR(50) PRIMARY KEY, frw_stem VARCHAR(50), frw_type VARCHAR(20))";
            var command = new SQLiteCommand(sql, mDbConnection);
            command.ExecuteNonQuery();
            mDbConnection.Close();
        }
		#endregion
    }
}
