using ManagementLabel.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using Org.BouncyCastle.Crypto.IO;
using System.Buffers.Text;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Web;
namespace ManagementLabel.Components.Share
{
    public class WhatsAppService(IJSRuntime JS, IOptions<AppConfig> appConfig)
    {
        private readonly IJSRuntime _JS = JS;
        private readonly IOptions<AppConfig> _appConfig = appConfig;
        public async Task<ValidationResult> SendCustomerInfo(Customers customer)
        {
            try
            {
                if (customer == null)
                {
                    return new ValidationResult { Result = false, Message = "Kundendaten sind null." };
                }

                string message = GetMessage(customer);

                await _JS.InvokeVoidAsync("whatsappRedirect.openWhatsAppWithoutNumber", message);

                return new ValidationResult { Result = true, Message = "WhatsApp-Nachricht wurde erfolgreich geöffnet." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }

        public async Task<ValidationResult> SendCustomerInfo(List<Customers> customers)
        {
            try
            {
                if (customers == null || customers.Count == 0)
                {
                    return new ValidationResult { Result = false, Message = "Keine Kundendaten vorhanden." };
                }

                // alle customers in eine Nachricht zusammenfassen
                var messageBuilder = new StringBuilder();
                foreach (var customer in customers)
                {
                    messageBuilder.AppendLine(GetMessage(customer));
                    messageBuilder.AppendLine("--------------------------------------------------");
                }

                string message = messageBuilder.ToString();

                //
                await _JS.InvokeVoidAsync("whatsappRedirect.openWhatsAppWithoutNumber", message);

                return new ValidationResult { Result = true, Message = "WhatsApp-Nachricht wurde erfolgreich geöffnet." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        private static string GetMessage(Customers customer)
        {
            string mapsLink;
            if (customer.Latitude != 0 && customer.Longitude != 0)
            {
                mapsLink = $"https://maps.google.com/?q={customer.Latitude.ToString(CultureInfo.InvariantCulture)},{customer.Longitude.ToString(CultureInfo.InvariantCulture)}";
            }
            else
            {
                string addressQuery = $"{customer.Street} {customer.BuildingNumber}, {customer.PostalCode} {customer.City}";
                // endcode the mapsLink
                string encodedAddress = WebUtility.UrlEncode(addressQuery);

                mapsLink = $"https://maps.google.com/?q={encodedAddress}";
            }

            string stopNumber = $"🛑 Stop-Nummer" +
                                $": {customer.StopNumber}\n";

            string distributionLineInfo = null!;
            if (customer.DistributionLine != null)
            {
                distributionLineInfo = $"🚚 Richtung: " +
                                       $"{customer.DistributionLine.LineName}\n";
            }



            string name = $"👤 Kunde:{customer.Name_de} " +
                          $": {customer.Name_ar}\n";

            string Address = null!;
            if (!string.IsNullOrEmpty(customer.Street) &&
               !string.IsNullOrEmpty(customer.BuildingNumber) &&
               !string.IsNullOrEmpty(customer.PostalCode) &&
               !string.IsNullOrEmpty(customer.City))
            {
                Address = $"📍 Adresse: {customer.Street} {customer.BuildingNumber}, " +
                          $"{customer.PostalCode} {customer.City}\n";
            }

            string location = $"🗺️ Standort: " +
                              $"{mapsLink}\n";

            string phone = null!;
            if (!string.IsNullOrEmpty(customer.PhoneNumber))
                phone = $"📞 Tel.: {customer.PhoneNumber}\n";

            string email = null!;
            if (!string.IsNullOrEmpty(customer.Email))
                email = $"📧 E-Mail: {customer.Email}\n";

            string notes_de = null!;
            if (!string.IsNullOrEmpty(customer.Notes_de))
                notes_de = $"📝 Note (DE): {customer.Notes_de}\n";
            string notes_ar = null!;
            if (!string.IsNullOrEmpty(customer.Notes_ar))
                notes_ar = $"📝 Note (AR): {customer.Notes_ar}";

            string message =
                             $"{stopNumber}" +
                             $"{distributionLineInfo}" +
                             $"{name}" +
                             $"{Address}" +
                             $"{location}" +
                             $"{phone}" +
                             $"{email}" +
                             $"{notes_de}" +
                             $"{notes_ar}";

            return message;
        }
        public async Task<ValidationResult> SendTransactionCustomerNotify(Customers customer, DebtCustomers debtCustomers, TransactionsCustomers transactionsCustomers, string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return new ValidationResult { Result = false, Message = "Token ist erforderlich." };
                }

                string encodedToken = HttpUtility.UrlEncode(token);
                string baseUrl = $"{_appConfig.Value.Domin}/customerDashboard";

                string urlWithToken = $"{baseUrl}?token={encodedToken}";
                string message =
                  $"Hallo {customer.Name_de} 👋 \n"
                + $"✅ Eine neue Schuldentransaktion wurde abgeschlossen.\n"
                + $"💰 Dein neuer Schuldenstand: {debtCustomers?.Balance ?? 0} €\n"
                + $"💵 Transaktionsbetrag: {transactionsCustomers.Amount} €\n"
                + "Hier klicken, um die Details zu sehen:\n"
                + $"{urlWithToken}";

                if (!string.IsNullOrEmpty(customer.PhoneNumber))
                    await JS.InvokeVoidAsync("whatsappRedirect.openWhatsApp", customer.PhoneNumber,message);
                else
                    await _JS.InvokeVoidAsync("whatsappRedirect.openWhatsAppWithoutNumber", message);
                return new ValidationResult { Result = true, Message = "WhatsApp-Nachricht wurde erfolgreich geöffnet." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
    }
}
