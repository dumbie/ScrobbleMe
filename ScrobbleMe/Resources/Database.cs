using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace ScrobbleMe
{
    partial class MainPage
    {
        //Database Context
        DBContext DBCon = new DBContext(@"Data Source=isostore:/ScrobbleMeDatabase.sdf;Max Database Size=512;");
        public class DBContext : DataContext
        {
            public DBContext(string DBCon) : base(DBCon) { }
            public Table<TblSong> TblSong;
        }

        //Database Load/Create
        public void LoadDatabase()
        {
            try { if (DBCon.DatabaseExists() == false) { DBCon.CreateDatabase(); } }
            catch
            {
                Dispatcher.BeginInvoke(delegate
                {
                    txt_ScrobbleStats.Text = "Failed to load the application database.";
                    txt_IgnoreStats.Text = "Failed to load the application database.";
                });
            }
        }

        //Database Songs Table
        [Table]
        public class TblSong
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
            public int Id { get; set; }
            [Column(CanBeNull = false)]
            public string Artist { get; set; }
            [Column(CanBeNull = false)]
            public string Album { get; set; }
            [Column(CanBeNull = false)]
            public string Title { get; set; }
            [Column(CanBeNull = true)]
            public int Track { get; set; }
            [Column(CanBeNull = false)]
            public string Duration { get; set; }
            [Column(CanBeNull = false)]
            public string Genre { get; set; }
            [Column(CanBeNull = true)]
            public int Plays { get; set; }
            [Column(CanBeNull = true)]
            public int Scrobbles { get; set; }
            [Column(CanBeNull = true)]
            public int Ignored { get; set; }
        }
    }
}