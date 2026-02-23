using ManagementLabel.Model;

namespace ManagementLabel.Components.ReceiptF
{
    public class ReceiptService
    {
        public bool IsValidProduct(Receipt receipt, out string errorMessage)
        {
            if (receipt == null)
            {
                errorMessage = "Receipt is empty";
                return false;
            }
            if (receipt.PaymentMethod == null || receipt.PaymentMethodId <= 0)
            {
                errorMessage = "Bitte wähle eine Zahlungsmethode aus";
                return false;
            }
            if (receipt.Products == null || receipt.Products.Count <= 0)
            {
                errorMessage = "Bitte wähle die Products aus";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

    }
}
