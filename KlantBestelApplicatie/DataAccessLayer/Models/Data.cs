﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Data
    {
        public string loggedInUser { get; set; }
        public bool isLoggedIn { get; set; }
        public Data()
        {
            loggedInUser = string.Empty;
            isLoggedIn = false;
        }
    }
}
