using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryApp
{
    public class Package
    {
        public string Id;
        public int Weight;
        public int Distance;
        public string OfferCode;

        public double Discount;
        public double TotalCost;
        public double DeliveryTime;

        public bool Delivered = false;
    }
}
