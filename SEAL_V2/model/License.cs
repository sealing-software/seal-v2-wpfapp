using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace SEAL_V2.model
{
    public static class License
    {
        private static String licenseName = "";
        private static String licenseStatus = "";
        private static String association = "";
        private static PackIconKind statusIcon = PackIconKind.Cancel;
        private static Brush statusColor;
        private static bool activated = true;

        public static void licenseSetup()
        {
            //SETUP FOR PRODUCTION
            licenseName = "Developer " + "license";
            licenseStatus = "Activated";
            association = "Modus21";
            statusIcon = PackIconKind.Check;
            statusColor = ((Brush)Application.Current.Resources["NewItem"]);
        }

        public static String getLicenseName()
        {
            return licenseName;
        }

        public static String getLicenseStatus()
        {
            return licenseStatus;
        }

        public static PackIconKind getIcon()
        {
            return statusIcon;
        }

        public static Brush getLicenseStatusColor()
        {
            return statusColor;
        }

        public static String getAssociationName()
        {
            return association;
        }

        public static bool isActivated()
        {
            return activated;
        }
    }
}
