using iTextSharp.text;
using iTextSharp.text.pdf;
using ManagementLabel.Model;
using Microsoft.Extensions.Options;


namespace ManagementLabel.Components.InvoiceF
{
    public class InvoiceService(HttpClient http)
    {
        private readonly HttpClient _http = http;
     
        public List<BankTransferDetails> DownloadedBankTransferDetails = [];
        public async Task<ValidationResult> GetBankTransferDetailsAsync()
        {
            if (DownloadedBankTransferDetails.Count > 0)
            {
                return new ValidationResult { Result = true, Message = "Bank Transfer Details bereits geladen." };
            }

            try
            {
                var response = await _http.GetAsync("api/Invoices/getBankTransferDetails");
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler" };
                }

                var bankTransferDetails = await response.Content.ReadFromJsonAsync<List<BankTransferDetails>>() ?? null;
                if (bankTransferDetails == null)
                {
                    return null!;
                }

                DownloadedBankTransferDetails.AddRange(bankTransferDetails);
                return new ValidationResult { Result = true, Message = "Bank Transfer Details erfolgreich geladen." };

            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }

        private IOptions<ProjectInfo>? _projectInfo;
        public async Task<byte[]> InvoicePdfGeneration(Invoice invoice)
        {
            if (DownloadedBankTransferDetails.Count == 0)
            {
               await GetBankTransferDetailsAsync();
            }
            // get projectinfo
            _projectInfo = invoice.projectInfo;

            try
            {
                using var memoryStream = new MemoryStream();
                // إعداد المستند وحجم الصفحة وهوامش
                Document document = new(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                writer.PageEvent = new PdfEvent(DownloadedBankTransferDetails, _projectInfo);

                document.Open();

                // space table
                PdfPTable spaceTable = new (1)
                {
                    TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin,
                    LockedWidth = true,
                };
                Paragraph spaceP2 = new("  ", FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK));
                var spaceCell = new PdfPCell
                {

                    Border = Rectangle.NO_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                spaceCell.AddElement(spaceP2);
                spaceTable.AddCell(spaceCell);
                spaceTable.SpacingAfter = 35f;
                document.Add(spaceTable);

                // address 
                PdfPTable addressTable = new (2)
                {
                    TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin,
                    LockedWidth = true,
                };

                PdfPCell addressCell = new()
                {
                    Border = Rectangle.NO_BORDER
                };

                // absender
                string senderAddress = $"{invoice.projectInfo.Value.Name} {invoice.projectInfo.Value.Address.Replace("\n", "")}";
                Font font = FontFactory.GetFont(FontFactory.HELVETICA, 6, BaseColor.BLACK);
                Chunk underlinedSender = new (senderAddress, font);
                underlinedSender.SetUnderline(0.1f, -1f);
                Paragraph absenderP = new()
                {
                    SpacingAfter = 1f,
                };
                absenderP.Add(underlinedSender);
                addressCell.AddElement(absenderP);
                // empfanger
                if (invoice.order.Address != null)
                {
                    string recipientAddressName = $"{invoice.order.Address.FirstName} {invoice.order.Address.LastName}";
                    Paragraph p1 = new (recipientAddressName, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK))
                    {
                        SpacingAfter = 1f // مسافة بعد الفقرة
                    };
                    addressCell.AddElement(p1);
                    string recipientAddress = $"{invoice.order.Address.Street}";
                    Paragraph p2 = new (recipientAddress, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK))
                    {
                        SpacingAfter = 1f,

                    };
                    addressCell.AddElement(p2);
                    string zipCodeCity = $"{invoice.order.Address.ZipCode} {invoice.order.Address.City}";
                    Paragraph p3 = new (zipCodeCity, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK));
                    addressCell.AddElement(p3);
                }

                // add cell to table
                addressTable.AddCell(addressCell);


                // contact
                var contectCell = new PdfPCell
                {
                    Border = Rectangle.NO_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                string Email = $"E-Mail: {invoice.projectInfo.Value.Email}";
                Paragraph ContactP1 = new (Email, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK))
                {
                    SpacingAfter = 1f // مسافة بعد الفقرة
                };
                contectCell.AddElement(ContactP1);
                string Tel = $"Tel.: {invoice.projectInfo.Value.Phone}";
                Paragraph ContactP2 = new (Tel, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK));
                contectCell.AddElement(ContactP2);


                // add cell to table
                addressTable.AddCell(contectCell);
                // add to document
                addressTable.SpacingAfter = 40f;
                document.Add(addressTable);


                // Invoice Info
                PdfPTable InvoiceInfoTable = new (4)
                {
                    TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin,
                    LockedWidth = true,
                };
                // rechnung text
                string Invoice = "Rechnung";
                Paragraph InvoiceP = new (Invoice, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK))
                {
                    Alignment = Element.ALIGN_LEFT,
                    SpacingAfter = 10f
                };

                var InvoiceTextCell = new PdfPCell(InvoiceP)
                {
                    Border = Rectangle.NO_BORDER,
                    Colspan = 4,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                InvoiceInfoTable.AddCell(InvoiceTextCell);

                // Rechnung info
                var InvoiceInfoCell = new PdfPCell
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                Font font1 = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

                // إنشاء الخلايا الأربعة في صف واحد
                PdfPCell cell1 = new (new Phrase($"Rechnungsnummer\n{invoice.InvoceeNumber}", font1))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 5f
                };

                PdfPCell cell2 = new (new Phrase($"Rechnungsdatum\n{DateTime.Today:dd.MM.yyyy}", font1))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 5f
                };

                PdfPCell cell3 = new (new Phrase($"Order Id\n{invoice.order.Id}", font1))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 5f
                };

                PdfPCell cell4 = new (new Phrase($"Ordersdatum\n{invoice.order.OrderDate:dd.MM.yyyy HH:mm}", font1))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 5f
                };

                // add to cell
                InvoiceInfoTable.AddCell(cell1);
                InvoiceInfoTable.AddCell(cell2);
                InvoiceInfoTable.AddCell(cell3);
                InvoiceInfoTable.AddCell(cell4);
                // add ti table
                InvoiceInfoTable.AddCell(InvoiceInfoCell);
                document.Add(InvoiceInfoTable);

                // order items
                PdfPTable orderItemsTable = new(6)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 20f
                };
                // set widths to table
                orderItemsTable.SetWidths([1f, 4f, 1f, 2f, 1f, 2f]);
                // font
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                Font cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                // header column
                string[] headers = ["Pos.", "Produkt", "Menge", "Einzelpreis", "Steuer", "Preis"];
                foreach (string header in headers)
                {
                    PdfPCell headerCell = new (new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(230, 230, 230),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    orderItemsTable.AddCell(headerCell);
                }
                // add order items 
                double TotalGross = 0;
                double GetTaxAmountRate19 = 0;
                double TotalPriceOfTaxRate19 = 0;
                double GetTaxAmountRate7 = 0;
                double TotalPriceOfTaxRate7 = 0;
                for (int i = 0; i < invoice.order.OrderItems.Count; i++)
                {
                    var item = invoice.order.OrderItems[i];
                    var itemPreis = item.UnitPrice * item.Quantity;
                    TotalGross += itemPreis;
                    if (item.Product?.TaxRate != null && item.Product?.TaxRate?.Rate == 19)
                    {
                        if (invoice.order.DiscountCode != null)
                        {
                            TotalPriceOfTaxRate19 += itemPreis - ((itemPreis / 100) * invoice.order.DiscountCode.DiscountPercentage);
                        }
                        else if (invoice.order.DiscountCategory != null)
                        {
                            TotalPriceOfTaxRate19 += itemPreis - ((itemPreis / 100) * invoice.order.DiscountCategory.DiscountPercentage);
                        }
                        else
                            TotalPriceOfTaxRate19 += itemPreis;
                    }
                    else if (item.Product?.TaxRate != null && item.Product?.TaxRate?.Rate == 7)
                    {
                        if (invoice.order.DiscountCode != null)
                        {
                            TotalPriceOfTaxRate7 += itemPreis - ((itemPreis / 100) * invoice.order.DiscountCode.DiscountPercentage);
                        }
                        else if (invoice.order.DiscountCategory != null)
                        {
                            TotalPriceOfTaxRate7 += itemPreis - ((itemPreis / 100) * invoice.order.DiscountCategory.DiscountPercentage);
                        }
                        else
                            TotalPriceOfTaxRate7 += itemPreis;
                    }
                    orderItemsTable.AddCell(new PdfPCell(new Phrase((i + 1).ToString(), cellFont)) { Padding = 5 });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase(item.Product?.Name_de ?? "Fehler", cellFont)) { Padding = 5 });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), cellFont)) { Padding = 5 });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase(item.UnitPrice.ToString("C", new System.Globalization.CultureInfo("de-DE")), cellFont)) { Padding = 5 });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase(item.Product?.TaxRate?.Rate.ToString() != null ? item.Product.TaxRate.Rate.ToString() : "", cellFont)) { Padding = 5 });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase(itemPreis.ToString("C", new System.Globalization.CultureInfo("de-DE")), cellFont)) { Padding = 5 });
                }
                // Rabbat
                if (invoice.order.DiscountCode != null)
                {
                    var (DiscountPercentage, DiscountValue) = GetDetailsDiscountCode(invoice.order);
                    orderItemsTable.AddCell(new PdfPCell(new Phrase("Rabbat: ", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase($"{DiscountPercentage}% ({DiscountValue:C})", cellFont)) { Colspan = 4, Padding = 5 });
                    // Den Rabatt vom Betrag abziehen 
                    TotalGross -= DiscountValue;
                }
                else if (invoice.order.DiscountCategory != null)
                {
                    var (DiscountPercentage, DiscountValue, categoryName) = GetDetailsDiscountCategory(invoice.order);
                    orderItemsTable.AddCell(new PdfPCell(new Phrase("Rabbat: ", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase($"K.: {categoryName} \n{DiscountPercentage}% ({DiscountValue:C})", cellFont)) { Colspan = 4, Padding = 5 });
                    // Den Rabatt vom Betrag abziehen 
                    TotalGross -= DiscountValue;
                }
                else
                {
                    orderItemsTable.AddCell(new PdfPCell(new Phrase("Rabbat: ", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase("0 €", cellFont)) { Colspan = 4, Padding = 5 });
                }
                // Preis
                orderItemsTable.AddCell(new PdfPCell(new Phrase("Preis: ", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                orderItemsTable.AddCell(new PdfPCell(new Phrase(TotalGross.ToString("C", new System.Globalization.CultureInfo("de-DE")), cellFont)) { Colspan = 4, Padding = 5 });
                // Versandkosten
                orderItemsTable.AddCell(new PdfPCell(new Phrase("Versandskosten: ", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                orderItemsTable.AddCell(new PdfPCell(new Phrase(invoice.order?.ShippingCost.ToString("C", new System.Globalization.CultureInfo("de-DE")), cellFont)) { Colspan = 4, Padding = 5 });
                // Gesamtbrutto
                orderItemsTable.AddCell(new PdfPCell(new Phrase("Gesamtbrutto: ", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                orderItemsTable.AddCell(new PdfPCell(new Phrase((TotalGross + invoice.order?.ShippingCost ?? 0).ToString("C", new System.Globalization.CultureInfo("de-DE")), cellFont)) { Colspan = 4, Padding = 5 });
                // Steuer 
                GetTaxAmountRate19 = (TotalPriceOfTaxRate19 / 100) * 19;
                GetTaxAmountRate7 = (TotalPriceOfTaxRate7 / 100) * 7;
                // 19%
                orderItemsTable.AddCell(new PdfPCell(new Phrase($"MwSt. 19% von {TotalPriceOfTaxRate19.ToString("C", new System.Globalization.CultureInfo("de-DE"))} :", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                orderItemsTable.AddCell(new PdfPCell(new Phrase(GetTaxAmountRate19.ToString("C", new System.Globalization.CultureInfo("de-DE")), cellFont)) { Colspan = 4, Padding = 5 });
                // 7%
                orderItemsTable.AddCell(new PdfPCell(new Phrase($"MwSt. 7% von {TotalPriceOfTaxRate7.ToString("C", new System.Globalization.CultureInfo("de-DE"))} :", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                orderItemsTable.AddCell(new PdfPCell(new Phrase(GetTaxAmountRate7.ToString("C", new System.Globalization.CultureInfo("de-DE")), cellFont)) { Colspan = 4, Padding = 5 });
                // Netto
                orderItemsTable.AddCell(new PdfPCell(new Phrase("Gesamtnetto: ", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                orderItemsTable.AddCell(new PdfPCell(new Phrase((TotalGross - (GetTaxAmountRate19 + GetTaxAmountRate7)).ToString("C", new System.Globalization.CultureInfo("de-DE")), cellFont)) { Colspan = 4, Padding = 5 });
                document.Add(orderItemsTable);


                document.Close();

                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler bei der PDF-Erstellung: " + ex.Message);
            }
        }
        public (int DiscountPercentage, double DiscountValue) GetDetailsDiscountCode(Order order)
        {

            (int DiscountPercentage, double DiscountValue) discountDetails = default!;
            double originalTotal = order.TotalPrice / (1 - ((order.DiscountCode?.DiscountPercentage ?? 0) / 100.0));
            double discountValue = originalTotal - order.TotalPrice;

            discountDetails.DiscountPercentage = order.DiscountCode?.DiscountPercentage ?? 0;
            discountDetails.DiscountValue = discountValue;
            return discountDetails;
        }
        public (int DiscountPercentage, double DiscountValue, string categoryName) GetDetailsDiscountCategory(Order order)
        {
            (int DiscountPercentage, double DiscountValue, string categoryName) discountDetails = default!;

            // get discount details for the order by category
            double categoryitemsPrice = 0;
            foreach (var item in order.OrderItems)
            {
                if ((item.Product?.CategoryId ?? 0) == (order.DiscountCategory?.CategoriesId ?? 0))
                {
                    categoryitemsPrice += item.UnitPrice * item.Quantity;
                }
            }
            // get discount category
            double categoryDiscountValue = categoryitemsPrice * (order.DiscountCategory?.DiscountPercentage ?? 0) / 100.0;

            discountDetails.DiscountPercentage = order.DiscountCategory?.DiscountPercentage ?? 0;
            discountDetails.DiscountValue = categoryDiscountValue;

            discountDetails.categoryName = order.DiscountCategory?.Category?.Name_de ?? "Kein Kategorie";
            var matchedItem = order.OrderItems
            .FirstOrDefault(o => o.Product?.CategoryId == order.DiscountCategory?.CategoriesId);
            discountDetails.categoryName = matchedItem?.Product?.Category?.Name_de ?? "null";

            return discountDetails;
        }
    }
    public class PdfEvent(List<BankTransferDetails> bankDetails, IOptions<ProjectInfo> projectInfo) : PdfPageEventHelper
    {

        private readonly List<BankTransferDetails> _bankDetails = bankDetails;
        private readonly IOptions<ProjectInfo> _projectInfo = projectInfo;

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            PdfPTable headerTable = new (1)
            {
                TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin
            };

            var cell = new PdfPCell(new Phrase("Syriana", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 25, BaseColor.BLACK)))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT,
                PaddingBottom = 10
            };

            headerTable.AddCell(cell);

            // نرسم الجدول في رأس الصفحة (على اليسار والمسافة من أعلى الصفحة)
            headerTable.WriteSelectedRows(0, -5, document.LeftMargin - 20, document.PageSize.Height - 10, writer.DirectContent);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            for (int i = 0; i < _bankDetails.Count; i++)
            {
                var detail = _bankDetails[i]; // مثال: عرض أول عنصر فقط

                string BankDetails = $"Bankverbindung \n" +
                    $"{detail.AccountHolderName}\n " +
                    $"{detail.BankName}\n" +
                    $"{detail.IBAN}\n" +
                    $"{detail.BIC}";

                string projectInfo1 = $"{_projectInfo.Value.Name}\n" +
                    $"{_projectInfo.Value.Address}\n" +
                    $"{_projectInfo.Value.Steuernummer}\n" +
                    $"{_projectInfo.Value.UStIdNr}";

                PdfPTable footerTable = new (2)
                {
                    TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin,
                };

                footerTable.SetWidths([1f, 1f]); // 50% - 50% توزيع الأعمدة

                var leftCell = new PdfPCell(new Phrase(projectInfo1, FontFactory.GetFont(FontFactory.HELVETICA, 8)))
                {
                    Border = Rectangle.TOP_BORDER,
                    BorderWidthTop = 0.5f,
                    BorderColorTop = BaseColor.GRAY,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_TOP,
                };

                var rightCell = new PdfPCell(new Phrase(BankDetails, FontFactory.GetFont(FontFactory.HELVETICA, 8)))
                {
                    Border = Rectangle.TOP_BORDER,
                    BorderWidthTop = 0.5f,
                    BorderColorTop = BaseColor.GRAY,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_TOP,
                };

                footerTable.AddCell(leftCell);
                footerTable.AddCell(rightCell);

                // موقع الجدول في أسفل الصفحة: من اليسار LeftMargin، وعلى ارتفاع BottomMargin
                float footerY = document.BottomMargin;
                footerTable.WriteSelectedRows(0, -1, document.LeftMargin, footerY, writer.DirectContent);
            }
        }
    }
}
