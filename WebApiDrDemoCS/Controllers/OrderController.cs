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
        //public IEnumerable<Order> GetAllOrders()
        //{
        //    return GetOrders();
        //}
        public IEnumerable<Order> GetAllOrders()
        {
            using (productdbseaEntities ent = new productdbseaEntities())
            {
                string regionName;  
                regionName = System.Environment.GetEnvironmentVariable("REGION_NAME"); 
                if(regionName != null)
                    HttpContext.Current.Response.Headers.Add("Web-Region-Name", regionName);


                string sqlServerName = ent.Database.SqlQuery<string>("SELECT @@SERVERNAME").FirstOrDefault();
                if (sqlServerName != null)
                    HttpContext.Current.Response.Headers.Add("DB-Server-Name", sqlServerName);

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
            char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
             
            Random rand = new Random();
             
            for (int i = 1; i <= num_words; i++)
            { 
                string word = "";
                for (int j = 1; j <= num_letters; j++)
                { 
                    int letter_num = rand.Next(0, letters.Length - 1); 
                    word += letters[letter_num];
                }
                 
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
            string sqlServerName = null;
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

                sqlServerName = ent.Database.SqlQuery<string>("SELECT @@SERVERNAME").FirstOrDefault();
            }

            string regionName;
            regionName = System.Environment.GetEnvironmentVariable("REGION_NAME");  
              
            HttpResponseMessage resp = new HttpResponseMessage();
            resp.StatusCode = HttpStatusCode.Created;
            if (regionName != null)
                resp.Headers.Add("Web-Region-Name", regionName);
            if (sqlServerName != null)
                resp.Headers.Add("DB-Server-Name", sqlServerName);


            return resp;
        }
    }
}
