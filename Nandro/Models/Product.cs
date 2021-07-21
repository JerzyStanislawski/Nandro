namespace Nandro.Models
{
    public class Product
    {
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
