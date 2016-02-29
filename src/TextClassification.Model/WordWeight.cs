using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TextClassification.Model
{
    public class WordWeight
    {
        [Key, Column("wrw_id")]
        public int Id { get; set; }

        [Column("wrw_text"), StringLength(50)]
        public string Text { get; set; }

        [Column("wrw_bag_id")]
        public int Bag { get; set; }

        [Column("wrw_weight")]
        public double Weight { get; set; }

        public WordWeight()
        {
            Text = string.Empty;
            Bag = 0;
            Weight = 0.0;
        }
    }
}
