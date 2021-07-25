using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nandro.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public ProductUnit Unit { get; set; }
    }

    public enum ProductUnit
    {
        Piece,
        Kg,
        Gr,
        Gl,
        L,
        Ml
    }


}
