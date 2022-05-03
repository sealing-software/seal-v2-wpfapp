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
using SEAL_V2.model;
using SEAL_V2.db;
using System.Windows.Media.Animation;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for AccountPopupSecurity.xaml
    /// </summary>
    public partial class AccountPopupSecurity : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Account_Popup_Security_Page";
        private String name = "Account Security";
        private DatabaseInterface db = DatabaseInterface.Instance;
        public event EventHandler<StatusMessage> message;


        public AccountPopupSecurity()
        {
            InitializeComponent();

            loadObjectID();
        }

        public void loadObjectID()
        {
            objectID = ObjectIDManager.objectIDs[objectName];
        }

        public String getObjectName()
        {
            return objectName;
        }

        public long getObjectID()
        {
            return objectID;
        }

        public void refreshPage()
        {
            loadMainView();
        }

        public String getName()
        {
            return name;
        }

        public StatusMessage createMessage(object message, String objectName)
        {
            StatusMessage newMessage = new StatusMessage(ObjectIDManager.objectIDs[objectName], message, this.objectID);

            return newMessage;
        }

        public void sendMessage(StatusMessage newMessage)
        {
            if (message != null)
            {
                this.message(this, newMessage);
            }
        }

        public void receiveMessage(object sender, StatusMessage recevievedMessage)
        {
            if (MessageRelay.sendUp(this.objectID, recevievedMessage.getAddress()))
            {
                sendMessage(recevievedMessage);
            }
            else
            {

            }
        }

        private void EditUserButton_MouseEnter(object sender, MouseEventArgs e)
        {
            EditUserButton.Opacity = 0.5;
        }

        private void EditUserButton_MouseLeave(object sender, MouseEventArgs e)
        {
            EditUserButton.Opacity = 1;
        }

        private void loadMainView()
        {
            MainCard.Height = 170;
            mainview.Visibility = Visibility.Visible;
            passwordchangeview.Visibility = Visibility.Hidden;
            resetNewPasswordView();
        }

        private void loadPasswordChangeView()
        {
            mainview.Visibility = Visibility.Hidden;
            passwordchangeview.Visibility = Visibility.Visible;

            Storyboard showEditPassword = this.TryFindResource("showEditPassword") as Storyboard;
            showEditPassword.Begin();
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            loadPasswordChangeView();
        }

        private void passwordenter1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void passwordenter2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Storyboard hideEditPassword = this.TryFindResource("hideEditPassword") as Storyboard;
            hideEditPassword.Begin();

            loadMainView();
        }

        private void cancelButton_MouseEnter(object sender, MouseEventArgs e)
        {
            cancelButton.Opacity = 0.5;
        }

        private void cancelButton_MouseLeave(object sender, MouseEventArgs e)
        {
            cancelButton.Opacity = 1;
        }

        private void passwordenter1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (passwordenter1.Text.Equals("Enter new password"))
            {
                passwordenter1.Text = "";
                passwordenter1.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
        }

        private void passwordenter2_GotFocus(object sender, RoutedEventArgs e)
        {
            if (passwordenter2.Text.Equals("Enter new password again"))
            {
                passwordenter2.Text = "";
                passwordenter2.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
        }

        private void passwordenter1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (passwordenter1.Text.Equals(""))
            {
                passwordenter1.Text = "Enter new password";
                passwordenter1.Foreground = Brushes.Gray;
            }
        }

        private void passwordenter2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (passwordenter2.Text.Equals(""))
            {
                passwordenter2.Text = "Enter new password again";
                passwordenter2.Foreground = Brushes.Gray; ;
            }
        }

        private void submitButton_MouseEnter(object sender, MouseEventArgs e)
        {
            submitButton.Opacity = 0.5;
        }

        private void submitButton_MouseLeave(object sender, MouseEventArgs e)
        {
            submitButton.Opacity = 1;
        }

        private void resetNewPasswordView()
        {
            passwordenter1.Text = "Enter new password";
            passwordenter1.Foreground = Brushes.Gray;
            passwordenter2.Text = "Enter new password again";
            passwordenter2.Foreground = Brushes.Gray;
            passwordError.Text = "";
            submitButton.Visibility = Visibility.Hidden;
        }

        private void passwordenter1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (passwordError != null)
            {
                checkErrors();
            }

        }

        private void passwordenter2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (passwordError != null)
            {
                checkErrors();
            }
        }

        private void checkErrors()
        {
            if (passwordenter1.Text.Equals("Enter new password") || passwordenter2.Text.Equals("Enter new password again"))
            {
                passwordError.Text = "";
                submitButton.Visibility = Visibility.Hidden;
            }
            else if (passwordenter1.Text.Equals(""))
            {
                passwordError.Text = "The password cannot be blank";
                submitButton.Visibility = Visibility.Hidden;
            }
            else if (!passwordenter1.Text.Equals(passwordenter2.Text))
            {
                passwordError.Text = "The passwords much match";
                submitButton.Visibility = Visibility.Hidden;
            }
            else
            {
                passwordError.Text = "";
                submitButton.Visibility = Visibility.Visible;
            }
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            db.updateUserPassword(User.getUserId(), passwordenter2.Text);
            Storyboard hideEditPassword = this.TryFindResource("hideEditPassword") as Storyboard;
            hideEditPassword.Begin();
            loadMainView();
        }
    }
}
