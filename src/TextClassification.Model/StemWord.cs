using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TextClassification.Model
{
    public class StemWord
    {
        [Key, Column("frw_text"), StringLength(50)]
        public string Text { get; set; }

        [Column("frw_stem"), StringLength(50)]
        public string Stem { get; set; }

        [Column("frw_type"), StringLength(20)]
        public string Type { get; set; }

        public StemWord()
        {
            Text = string.Empty;
            Stem = string.Empty;
            Type = string.Empty;
        }
    }
}
