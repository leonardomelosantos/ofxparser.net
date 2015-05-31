using System;
using System.Collections.Generic;
using System.Text;

namespace OFXParser.Entities
{
    public class Bank
    {
        public int Code { get; set; }

        public String Name { get; set; }

        public Bank(int code, String name)
        {
            this.Code = code;
            this.Name = name;
        }
    }
}
