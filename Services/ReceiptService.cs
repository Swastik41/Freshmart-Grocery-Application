using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using FreshMart.Models;

namespace FreshMart.Services
{
    public class ReceiptService
    {
        public byte[] GenerateReceipt(Order order, List<OrderItem> items, List<Product> products)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Header().Element(BuildHeader);
                    page.Content().Element(content => BuildContent(content, order, items, products));
                    page.Footer().Element(BuildFooter);
                });
            });

            return document.GeneratePdf();
        }

        // ---------------- HEADER ----------------
        private void BuildHeader(IContainer container)
        {
            container.PaddingBottom(20).Column(col =>
            {
                col.Item().Row(row =>
                {
                    // LEFT
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().Text("FreshMart")
                            .FontSize(32)
                            .Bold()
                            .FontColor("0a8a4e");

                        left.Item().Text("Premium Grocery Market")
                            .FontSize(12)
                            .FontColor("555555");

                        left.Item().Text("www.freshmart.com")
                            .FontSize(10)
                            .FontColor("777777");
                    });

                    // RIGHT LOGO BLOCK
                    row.ConstantItem(90).Padding(5)
                        .Background("0a8a4e")
                        .AlignCenter()
                        .AlignMiddle()
                        .Padding(10)
                        .Text("FM")
                            .FontColor("ffffff").FontSize(28).Bold();
                });

                col.Item().PaddingTop(10).LineHorizontal(1);

            });
        }

        // ---------------- CONTENT ----------------
        private void BuildContent(IContainer container, Order order, List<OrderItem> items, List<Product> products)
        {
            container.Column(col =>
            {
                // ORDER BOX
                col.Item().Padding(10)
                    .Border(1)
                    .Background("e9fff1")
                    .Padding(20)
                    .Column(box =>
                    {
                        box.Item().Text("Order Receipt")
                            .FontSize(20)
                            .Bold()
                            .FontColor("0a8a4e");

                        box.Item().Text($"Order ID: #{order.OrderId}").FontSize(11);
                        box.Item().Text($"Order Date: {order.OrderDate:MMM dd, yyyy h:mm tt}").FontSize(11);
                    });

                col.Item().PaddingTop(20);

                // TABLE HEADER
                col.Item().Row(row =>
                {
                    row.RelativeItem(3).Text("Product").Bold();
                    row.RelativeItem().Text("Qty").Bold();
                    row.RelativeItem().Text("Price").Bold();
                    row.RelativeItem().Text("Total").Bold();
                });

                col.Item().LineHorizontal(1);

                // ITEMS
                foreach (var item in items)
                {
                    var product = products.First(p => p.ProductId == item.ProductId);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text(product.Name).FontSize(11);
                        row.RelativeItem().Text(item.Quantity.ToString()).FontSize(11);
                        row.RelativeItem().Text($"${item.Price:0.00}").FontSize(11);
                        row.RelativeItem().Text($"${item.Price * item.Quantity:0.00}").FontSize(11);
                    });

                    col.Item().LineHorizontal(1);
                }

                // TOTALS
                col.Item().PaddingTop(20).AlignRight().Column(tot =>
                {
                    decimal tax = Math.Round(order.TotalAmount * 0.13m, 2);
                    decimal grand = order.TotalAmount + tax;

                    tot.Item().Text($"Subtotal: ${order.TotalAmount:0.00}")
                        .FontSize(12).FontColor("555555");

                    tot.Item().Text($"Tax (13%): ${tax:0.00}")
                        .FontSize(12).FontColor("555555");

                    tot.Item().PaddingTop(5).Text($"Grand Total: ${grand:0.00}")
                        .FontSize(18).Bold().FontColor("0a8a4e");
                });

                // THANK YOU BOX
                col.Item().PaddingTop(25)
                    .Background("0a8a4e")
                    .Padding(25)
                    .AlignCenter()
                    .Column(thx =>
                    {
                        thx.Item().Text("Thank you for shopping with FreshMart!")
                            .FontSize(16).Bold().FontColor("ffffff");

                        thx.Item().Text("We appreciate your business.")
                            .FontSize(11).FontColor("ffffff");
                    });
            });
        }

        // ---------------- FOOTER ----------------
        private void BuildFooter(IContainer container)
        {
            container.AlignCenter().PaddingTop(10)
                .Text("© 2025 FreshMart Grocery Store — All rights reserved.")
                .FontSize(10)
                .FontColor("888888");
        }
    }
}
