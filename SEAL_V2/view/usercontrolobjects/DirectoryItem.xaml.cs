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
using MaterialDesignThemes.Wpf;

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for DirectoryItem.xaml
    /// </summary>
    public partial class DirectoryItem : UserControl
    {
        public String name { get; set; }
        public int id { get; set; }
        public int parentID { get; set; }
        public bool sequenceApplied { get; set; }
        public int sequenceID { get; set; }
        public bool selected = false;

        public DirectoryItem(int id, String directoryName, int parentID, int sequenceApplied, int sequenceID)
        {
            InitializeComponent();
            name = directoryName;
            this.id = id;
            this.parentID = parentID;
            DirectoryName.Text = name;

            if (sequenceApplied == 1)
            {
                this.sequenceApplied = true;
            }
            else
            {
                this.sequenceApplied = false;
            }

            this.sequenceID = sequenceID;
        }

        private void ItemGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            ItemGrid.Opacity = 0.5;
            icon.Kind = PackIconKind.FolderOpen;
            selected = true;
        }

        private void ItemGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            ItemGrid.Opacity = 1;
            icon.Kind = PackIconKind.Folder;
            selected = false;
        }

    }
}
