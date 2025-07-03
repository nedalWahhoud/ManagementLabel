using ManagementLabel.Model;

namespace ManagementLabel.Components.CartF
{
    public class CartService
    {
        public List<CartItem> CartItems { get; private set; } = [];

        public void AddToCart(CartItem cartItem)
        {
            var item = CartItems.FirstOrDefault(ci => ci.ProductId == cartItem.ProductId);
            if (item != null)
            {
                item.Quantity = cartItem.Quantity;
            }
            else
            {
                CartItems.Add(new CartItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Product = cartItem.Product
                });
            }
        }
        public void RemoveFromCart(int ProductId)
        {
            var item = CartItems.FirstOrDefault(ci => ci.ProductId == ProductId);
            if (item != null)
            {
                CartItems.Remove(item);
            }
        }
        public bool IsQuantityZero(CartItem cartItem)
        {
            if (cartItem.Quantity <= 0)
            {
                bool isProductAdded = IsProductAdded(cartItem.ProductId);
                if (isProductAdded)
                    RemoveFromCart(cartItem.ProductId);

                return true; // Do not add to cart if quantity is zero
            }
            return false; // Quantity is greater than zero, proceed with adding to cart
        }
        public void ClearCart(List<Products>? products = null)
        {
            if (products != null && products.Count > 0 && CartItems.Count > 0)
            {
                var productIdsInCart = CartItems.Select(ci => ci.ProductId).ToHashSet();

                foreach (var product in products)
                {
                    if (productIdsInCart.Contains(product.Id))
                    {
                        // nach reinigung von cartitems soll die quantity from alle Products 0 wiederstellen
                        product.CartItem.Quantity = 0;
                    }
                }
            }

            CartItems.Clear();
        }
        // 
        public int GetQuantityOfProduct(int productId)
        {
            return CartItems.FirstOrDefault(c => c.ProductId == productId)?.Quantity ?? 0;
        }
        public bool IsProductAdded(int productId)
        {
            return CartItems.Any(ci => ci.ProductId == productId);
        }
        public double GetTotalPrice()
        {
            return (double)(CartItems.Sum(ci => ci.Product?.SalePrice * ci.Quantity) ?? 0);
        }
        public double GetTotalTax()
        {
            return (double)Math.Round(CartItems.Sum(item => item.Product.SalePrice * (item.Product.TaxRate!.Rate / 100) * item.Quantity), 3);
        }
        public List<TaxRates> GetTaxRateGroups()
        {
            var taxRateGroups = CartItems
           .GroupBy(ci => ci.Product.TaxRate!.Rate)
           .Select(g =>
           {

               double taxAmount = Math.Round(g.Sum(ci => ci.Product.SalePrice * (ci.Product.TaxRate!.Rate / 100) * ci.Quantity), 3);
               double total = Math.Round(g.Sum(ci => ci.Quantity * ci.Product.SalePrice), 3);
               double netto = total - taxAmount;

               return new TaxRates
               {
                   TaxRate = g.Key,
                   NettoPrice = netto,
                   TaxAmount = taxAmount,
                   TotalPrice = total
               };
           })
           .ToList();


            return taxRateGroups;
        }
    }
}
