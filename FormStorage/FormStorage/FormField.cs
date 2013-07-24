using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormStorage
{
    public class FormField
    {
        public int ID;
        public string name = "";
        public string alias = "";
        public int sortOrder;
        public bool remove = false;
    }
}