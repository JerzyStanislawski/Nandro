using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nandro.Models
{
    public class Product
    {
        private string _name;

        public EventHandler<EventArgs> NameChanged;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NameChanged?.Invoke(this, new EventArgs());
            }
        }
        public decimal Price { get; set; }
        public ProductUnit Unit { get; set; }

        public override string ToString()
        {
            return Name;
        }
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
