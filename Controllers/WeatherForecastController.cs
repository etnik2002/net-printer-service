using Microsoft.AspNetCore.Mvc;
using System;
using PrinterUtility;
using ESC_POS_USB_NET.Printer;
using System.Text;


namespace test_2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        [HttpPost("print/{order_id}")]
        public IActionResult PrintOrder(string order_id, [FromBody] OrderModel order)
        {
            try
            {
                if (order == null || order.Items == null || order.Items.Count == 0)
                {
                    return BadRequest("Invalid order data.");
                }

                Printer printer = new Printer("EPSON TM-m30 Bluetooth", "UTF-8");

                PrintReceipt(printer, "Insyllium Restaurant", "Tetovo", $"Order #{order_id}");

                foreach (var item in order.Items)
                {
                    PrintItem(printer, item.name, $"Qty: {item.quantity}", $"Price: ${item.price:0.00}", $"Total: ${item.quantity * item.price:0.00}");
                }

                decimal subtotal = order.Items.Sum(item => item.quantity * item.price);
                decimal tax = subtotal;
                decimal total = subtotal + tax;

                PrintTotal(printer, $"Subtotal: ${subtotal:0.00}", $"Tax (10%): ${tax:0.00}", $"Total: ${total:0.00}");

                printer.FullPaperCut();
                printer.PrintDocument();
                var data = new { Message = "Receipt printed" };

                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.ReadLine();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("print/test")]
        public IActionResult PrintTestOrder()
        {
            try
            {
                Printer printer = new Printer("EPSON TM-m30 Receipt", "UTF-8");
                printer.AlignCenter();
                printer.Append("Test print");
                printer.NewLine();
                printer.NewLine();
                printer.NewLine();
                printer.NewLine();
                printer.NewLine();
                printer.QrCode("https://insyllium.com");
                printer.FullPaperCut();
                printer.PrintDocument();
                return Ok("Printed test document");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("")]
        public IActionResult Test()
        {
            return Ok("API WORKS");
        }

        public class OrderModel
        {
            public DateTime OrderDate { get; set; }
            public List<ItemModel> Items { get; set; }
        }

        public class ItemModel
        {
            public string name { get; set; }
            public int quantity { get; set; }
            public decimal price { get; set; }
        }


        static void PrintReceipt(Printer printer, params string[] headerLines)
        {
            printer.AlignCenter();
            foreach (var line in headerLines)
            {
                printer.Append(line);
            }
            printer.NewLine();
        }

        static void PrintItem(Printer printer, string itemName, string quantity, string unitPrice, string total)
        {
            printer.AlignLeft();
            printer.Append(itemName);
            printer.AlignRight();
            printer.Append(unitPrice);
            printer.AlignRight();
            printer.NewLine();
        }

        static void PrintTotal(Printer printer, params string[] totalLines)
        {
            printer.AlignLeft();
            foreach (var line in totalLines)
            {
                printer.Append(line);
            }
            printer.NewLine();
            printer.QrCode("https://insyllium.com");
        }
    }


}

