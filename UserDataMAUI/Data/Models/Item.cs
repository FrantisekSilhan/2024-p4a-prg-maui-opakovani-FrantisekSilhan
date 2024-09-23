using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace UserDataMAUI.Data.Models
{
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public long ID { get; set; }
        [Column("title")]
        public string Title { get; set; } = string.Empty;
        [Column("description")]
        public string Description { get; set; } = string.Empty;
        [Column("author")]
        public long Author { get; set; }
    }
}
