using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Mvc; 
using System.Web;
using System.Drawing;

namespace WebApiDrDemoCS.Controllers
{
    public class OrderController : ApiController
    {
        public IEnumerable<Order> GetOrders()
        {
            using (productdbseaEntities ent = new productdbseaEntities())
            {
                string serverName, regionName; 
                serverName = System.Environment.MachineName;
                regionName = System.Environment.GetEnvironmentVariable("REGION_NAME"); 
                 
                HttpContext.Current.Response.Headers.Add("Region-Name", regionName);


                var orders = ent.Orders.ToList();
                int count = 0;
                if(orders != null)
                {
                    count = orders.Count;
                }
                HttpContext.Current.Response.Headers.Add("Total-Records", count.ToString());

                

                return orders;
            }
        }

        private string GetRandomProductCodes()
        {
            int num_letters = 4;
            int num_words = 4;
            StringBuilder lstWords = new StringBuilder();
            // Make an array of the letters we will use.
            char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            // Make a random number generator.
            Random rand = new Random();

            // Make the words.
            for (int i = 1; i <= num_words; i++)
            {
                // Make a word.
                string word = "";
                for (int j = 1; j <= num_letters; j++)
                {
                    // Pick a random number between 0 and 25
                    // to select a letter from the letters array.
                    int letter_num = rand.Next(0, letters.Length - 1);

                    // Append the letter.
                    word += letters[letter_num];
                }

                // Add the word to the list.
                lstWords.Append(word);
            }

            return lstWords.ToString();
        }
        public HttpResponseMessage Add()
        {
            string productCode = GetRandomProductCodes();
            int quantity;
            Random rand = new Random();
            quantity = rand.Next(1, 100);

            return Add(productCode, quantity);
        }
        public HttpResponseMessage Add(string productCode, int quantity)
        {
            using (productdbseaEntities ent = new productdbseaEntities())
            {
                Order order = new Order()
                {
                    ProductCode = productCode,
                    Quantity = quantity,
                    OrderDateTime = DateTime.Now
                };
                ent.Orders.Add(order);
                ent.SaveChanges();
            }
            return new HttpResponseMessage() { StatusCode = HttpStatusCode.Created };
        }
    }
}
