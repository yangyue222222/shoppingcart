﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopCart.Models
{
    public class Product
    {
        public int pro_id { get; set; }
        public string pro_name { get; set; }
        public string pro_desc { get; set; }
        public int pro_price { get; set; }
        public string pro_image { get; set; }
    }
}