using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShopCart.Models;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace ShopCart.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Login(string username, string password)
        {
            if (username == null)
                return View();

            Customer customer = GetCustomer(username);
            if (customer.password != CalculateMD5Hash(password).ToLower())
                return View();

            List<Product> prolist = callme();
            ViewData["pl"] = prolist;
            string sessionId = CreateSession(customer.customer_id);
            ViewData["sessionId"] = sessionId;
            ViewBag.a = 1;
            ViewBag.customer = customer.customer_id;
            return View("Gallery");
        }
        //public ActionResult Gallery(string sessionId)
        //{
        //    ViewData["sessionId"] = sessionId;
        //    return View();
        //}
        public ActionResult Partial(string search)
        {
            Debug.WriteLine(search);
            List<Product> pro = new List<Product>();

            using (SqlConnection conn = new SqlConnection(("Server=DESKTOP-C2V6TC0; Database=ShoppingCartT4; Integrated Security=true")))
            {
                conn.Open();

                string sql = @"select * from Product where pro_desc like '%" + search + "%'";
                SqlCommand cmd = new SqlCommand(sql, conn);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Product p = new Product()
                    {
                        pro_id = (int)reader["pro_id"],
                        pro_name = (string)reader["pro_name"],
                        pro_desc = (string)reader["pro_desc"],
                        pro_price = (int)reader["pro_price"],
                        pro_image = (string)reader["pro_image"]

                    };
                    pro.Add(p);
                    Debug.WriteLine(p.pro_id);
                }
            }
            ViewData["partial"] = pro;
            ViewBag.a = 2;
            return View("Gallery");
        }
        public ActionResult Purchase(string customer_id)
        {
            List<Product> pro = new List<Product>();
            List<PurchaseItems> items = new List<PurchaseItems>();

            Debug.WriteLine(customer_id);
            using (SqlConnection conn = new SqlConnection(("Server=DESKTOP-C2V6TC0; Database=ShoppingCartT4; Integrated Security=true")))
            {
                conn.Open();

                string sql = @"select * from Purchaseitem p join Customer c on c.customer_id = p.customer_id join Product po on po.pro_id = p.pro_id where c.customer_id = '" + customer_id + "'";
                SqlCommand cmd = new SqlCommand(sql, conn);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Product p = new Product();
                    PurchaseItems i = new PurchaseItems();

                    p.pro_id = (int)reader["pro_id"];
                    p.pro_name = (string)reader["pro_name"];
                    p.pro_desc = (string)reader["pro_desc"];
                    p.pro_price = (int)reader["pro_price"];
                    p.pro_image = (string)reader["pro_image"];
                    i.pro_id = (int)reader["pro_id"];
                    i.activation_code = (string)reader["activation_code"];
                    i.purchase_time = (DateTime)reader["purchase_time"];

                    pro.Add(p);
                    items.Add(i);
                }
            }
            ViewBag.Products = pro;
            ViewBag.Purchases = items;
            return View();
        }
        public List<Product> callme()
        {
            List<Product> pro = new List<Product>();

            using (SqlConnection conn = new SqlConnection(("Server=DESKTOP-C2V6TC0; Database=ShoppingCartT4; Integrated Security=true")))
            {
                conn.Open();

                string sql = @"SELECT * from Product";
                SqlCommand cmd = new SqlCommand(sql, conn);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Product p = new Product()
                    {
                        pro_id = (int)reader["pro_id"],
                        pro_name = (string)reader["pro_name"],
                        pro_desc = (string)reader["pro_desc"],
                        pro_price = (int)reader["pro_price"],
                        pro_image = (string)reader["pro_image"]
                        
                    };
                    pro.Add(p);
                    Debug.WriteLine(p.pro_id);
                }
            }
            return pro;
        }

        public ActionResult Logout(string sessionId)
        {
            RemoveSession(sessionId);
            return View("Login");
        }
        public static Customer GetCustomer(string username)
        {
            Customer customer = null;
            List<Customer> customerlist = new List<Customer>();
            using (SqlConnection conn = new SqlConnection(("Server=DESKTOP-C2V6TC0; Database=ShoppingCartT4; Integrated Security=true")))
            {
                conn.Open();

                string sql = @"SELECT customer_id, username, password from Customer WHERE username = '" + username + "'";
                SqlCommand cmd = new SqlCommand(sql, conn);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    customer = new Customer()
                    {
                        customer_id = (int)reader["customer_id"],
                        username = (string)reader["username"],
                        password = (string)reader["password"]
                    };
                }

            }
            return customer;
        }
        public static string CreateSession(int customer_id)
        {
            string sessionId = Guid.NewGuid().ToString();

            using (SqlConnection conn = new SqlConnection("Server=DESKTOP-C2V6TC0; Database=ShoppingCartT4; Integrated Security=true"))
            {
                conn.Open();
                string sql = @"UPDATE Customer SET session_id = '" + sessionId + "'" + " WHERE customer_id =" + customer_id;
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }

            return sessionId;
        }

        public static void RemoveSession(string sessionId)
        {
            Debug.WriteLine(sessionId);
            using (SqlConnection conn = new SqlConnection("Server=DESKTOP-C2V6TC0; Database=ShoppingCartT4; Integrated Security=true"))
            {
                conn.Open();
                string sql = @"UPDATE Customer SET session_id = NULL WHERE sessionid = '" + sessionId + "'";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }

        public string CalculateMD5Hash(string input)
        {

            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

    }
}
