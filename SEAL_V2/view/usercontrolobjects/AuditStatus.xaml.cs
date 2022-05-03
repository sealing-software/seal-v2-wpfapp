using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for AuditStatus.xaml
    /// </summary>
    public partial class AuditStatus : UserControl
    {
        public AuditStatus()
        {
            InitializeComponent();
        }

        public void toggleOn()
        {
            MainGrid.Visibility = Visibility.Visible;
        }

        public void toggleOff()
        {
            MainGrid.Visibility = Visibility.Hidden;
        }
    }
}
