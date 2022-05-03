using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SEAL_V2.model
{
    public class Group
    {
        public int ID { get; set; }
        public String name { get; set; }
        public String colorString { get; set; }
        public int qty { get; set; }
        public SolidColorBrush brushColor { get; set; }

        public Group(int ID, String name, String color)
        {
            this.ID = ID;
            this.name = name;
            this.colorString = color;
            createColor();
        }

        public void setQty(int qty)
        {
            this.qty = qty;
        }

        private void createColor()
        {
            brushColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorString));
        }

        public bool isAdmin()
        {
            bool result = false;

            if (this.ID == 1)
            {
                result = true;
            }

            return result;
        }
    }
}
