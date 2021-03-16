using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMMData.Models
{
    public class Link
    {
        public int Index;
        public string ID;
       
        public Link(int index, string id)
        {
            Index = index;
            ID = id;
        }
    }
}
