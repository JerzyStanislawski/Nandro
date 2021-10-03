using ReactiveUI;
using System;

namespace Nandro.Models
{
    public class CartItem : ReactiveObject
    {
        public EventHandler<EventArgs> CountChanged;

        public Product Product { get; set; }
        public decimal Price { get; set; }
        public ProductUnit? Unit { get; set; }

        private decimal _count;
        public decimal Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                CountChanged?.Invoke(this, new EventArgs());
            }
        }
    }
}
