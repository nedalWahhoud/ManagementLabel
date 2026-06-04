using ManagementLabel.Model;
using System.Data;
using System.Net;
using static System.Net.WebRequestMethods;

namespace ManagementLabel.Components.OneTimePaymentsF
{
    public class OneTimePaymentsService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public List<List<OneTimePaymentsGroupDto>> DownloadedGroups { get; private set; } = [];
        public async Task<ValidationResult> Add(OneTimePayment newOneTimePayment)
        {
            try
            {
                // löschen der Navigation Properties, da diese nicht übergeben werden sollen
                var paymentToSend = new OneTimePayment
                {
                    Id = newOneTimePayment.Id,
                    CustomerId = newOneTimePayment.CustomerId,
                    DistributionLineId = newOneTimePayment.DistributionLineId,
                    TotalAmount = newOneTimePayment.TotalAmount,
                    AmountCollected = newOneTimePayment.AmountCollected,
                    Status = newOneTimePayment.Status,
                    Notes = newOneTimePayment.Notes,
                    CreatedAt = newOneTimePayment.CreatedAt,
                    Customer = null!,
                    DistributionLine = null!
                };

                var response = await _http.PostAsJsonAsync("api/OneTimePayments/add", paymentToSend);

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode || result == null || !result.Result)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Fehler beim Hinzufügen der Einmalzahlung." };
                }

                // add to local
                paymentToSend.Id = result.NewId ?? 0;
                // die Navigation Properties müssen für die Gruppierung in der lokalen Liste gesetzt werden, damit die Anzeige korrekt funktioniert
                paymentToSend.Customer = newOneTimePayment.Customer;
                paymentToSend.DistributionLine = newOneTimePayment.DistributionLine;

                DownloadedGroups.Add([new OneTimePaymentsGroupDto {
                                    GroupStartDate = paymentToSend.CreatedAt ?? DateTime.Now,
                                    Payments = [paymentToSend] 
                }]);



                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }

        public async Task<ValidationResult> GetGroupedPaymentsByLineId(int lineId)
        {
            // bevor neue Daten geladen werden, werden die alten Daten gelöscht, damit keine veralteten Daten angezeigt werden
            DownloadedGroups.Clear();

            try
            {
                var response = await _http.GetAsync($"api/OneTimePayments/getGroupedPaymentsByLineId/{lineId}");
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Fehler beim Abrufen der Einmalzahlungen." };
                }
                var groupedPayment = await response.Content.ReadFromJsonAsync<List<OneTimePaymentsGroupDto>>();
                if (groupedPayment == null || groupedPayment.Count == 0)
                {
                    return new ValidationResult { Result = false, Message = "Keine Einmalzahlungen für diese Linie." };
                }
                // add to Local
                DownloadedGroups.Add(groupedPayment);

                return new ValidationResult { Result = true, Message = "Einmalzahlungen erfolgreich abgerufen." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }

        public async Task<ValidationResult> UpdateOneTimePaymentAsync(OneTimePayment editOneTimePayment)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/OneTimePayments/updateStauts", editOneTimePayment);
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode || result == null || !result.Result)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Fehler beim Aktualisieren des Zahlungsstatus." };
                }
                // update  in locally list
                List<OneTimePaymentsGroupDto> targetLineList = null!;
                OneTimePaymentsGroupDto targetGroup = null!;
                OneTimePayment oldPayment = null!;

                foreach (var lineList in DownloadedGroups)
                {
                    foreach (var group in lineList)
                    {
                        var payment = group.Payments.FirstOrDefault(p => p.Id == editOneTimePayment.Id);
                        if (payment != null)
                        {
                            targetLineList = lineList;
                            targetGroup = group;
                            oldPayment = payment;
                            break;
                        }
                    }
                    if (oldPayment != null) break;
                }

                // 2. Falls der alte Batch gefunden wird, aktualisiere ihn in der Liste.
                if (oldPayment != null && targetGroup != null)
                {
                    var index = targetGroup.Payments.IndexOf(oldPayment);
                    if (index != -1)
                    {
                        // Ersetze die alten Daten durch die aktualisierten Daten vom Server.
                        targetGroup.Payments[index] = editOneTimePayment;
                    }
                }


                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }
        public async Task<ValidationResult> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/OneTimePayments/{id}");
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (!response.IsSuccessStatusCode || result == null || !result.Result)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Fehler beim Löschen der Einmalzahlung." };
                }

                if (result.Result == true)
                {
                    // delete in locally list
                    List<OneTimePaymentsGroupDto> targetLineList = null!;
                    OneTimePaymentsGroupDto targetGroup = null!;
                    OneTimePayment paymentToRemove = null!;

                    foreach (var lineList in DownloadedGroups)
                    {
                        foreach (var group in lineList)
                        {
                            var payment = group.Payments.FirstOrDefault(p => p.Id == id);
                            if (payment != null)
                            {
                                targetLineList = lineList;
                                targetGroup = group;
                                paymentToRemove = payment;
                                break;
                            }
                        }
                        if (paymentToRemove != null) break;
                    }

                    //  Wenn wir den Batch finden, löschen wir ihn und bereinigen die Struktur.
                    if (paymentToRemove != null)
                    {
                        // Lösche den Batch aus seiner Sammlung
                        targetGroup.Payments.Remove(paymentToRemove);

                        // Wenn die Gruppe vollständig leer ist und keine neuen Beiträge mehr erhält, löschen Sie die Gruppe selbst.
                        if (targetGroup.Payments.Count == 0)
                        {
                            targetLineList.Remove(targetGroup);
                        }

                        //Wenn die Schriftartenliste keine Gruppen mehr enthält, löschen Sie die gesamte Schriftart aus dem Cache.
                        if (targetLineList.Count == 0)
                        {
                            DownloadedGroups.Remove(targetLineList);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }
        // loacl
        public OneTimePayment? GetOneTimePaymentByIdLocal(int id)
        {
            return DownloadedGroups
                .SelectMany(lineList => lineList)
                .SelectMany(group => group.Payments) 
                .FirstOrDefault(p => p.Id == id); 
        }
        public string GetEnumDisplayName(OneTimePaymentStatus status)
        {
            var field = status.GetType().GetField(status.ToString());
            var attribute = (System.ComponentModel.DataAnnotations.DisplayAttribute)Attribute.GetCustomAttribute(field!, typeof(System.ComponentModel.DataAnnotations.DisplayAttribute))!;
            return attribute?.Name ?? status.ToString();
        }
        public string GetStatusClass(OneTimePaymentStatus status,bool isDropdown, bool isBaseClass = true)
        {
            string dropdownClass = isDropdown ? "dropdown-toggle" : "";

            string baseClass = $"btn btn-sm badge {dropdownClass}";

            string textColor = "text-white";
            string colorClass = status switch
            {
                OneTimePaymentStatus.Offen => "bg-secondary",
                OneTimePaymentStatus.TeilweiseInkassiert => "bg-warning",
                OneTimePaymentStatus.VollstaendigInkassiert => "bg-success",
                OneTimePaymentStatus.Verschoben => "bg-danger",
                OneTimePaymentStatus.Ueberzahlt => "bg-info",
                _ => "bg-danger"
            };

            return isBaseClass ? $"{baseClass} {colorClass} {textColor}" : $"{colorClass} {textColor}";
        }
        public bool IsEdited(OneTimePayment original, OneTimePayment edited)
        {
            return original.CustomerId != edited.CustomerId ||
                   original.DistributionLineId != edited.DistributionLineId ||
                   original.TotalAmount != edited.TotalAmount ||
                   original.AmountCollected != edited.AmountCollected ||
                   original.Status != edited.Status ||
                   original.Notes != edited.Notes;
        }

        public class CachedLine
        {
            public int LineId { get; set; }
            public bool NeedServerRefresh { get; set; } = true;
            public List<OneTimePaymentsGroupDto> Groups { get; set; } = [];
        }
    }
}
