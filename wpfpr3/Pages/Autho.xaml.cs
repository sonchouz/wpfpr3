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
using wpfpr3.Models;
using wpfpr3.Service;


namespace wpfpr3.Pages
{
    /// <summary>
    /// Логика взаимодействия для Autho.xaml
    /// </summary>
    public partial class Autho : Page
    {
        int click;
        public string role;
        public Autho()
        {
            InitializeComponent();
            click = 0;
        }

        private void btnEnterGuests_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Client(null, null));
        }
        private void GenerateCaptcha()
        {
            txtbCaptcha.Visibility = Visibility.Visible;
            txtBlockCaptcha.Visibility = Visibility.Visible;
            string capctchaText = CaptchaGenerator.GenerateCaptchaText(6);
            txtBlockCaptcha.Text = capctchaText;
            txtBlockCaptcha.TextDecorations = TextDecorations.Strikethrough;

        }
        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            click += 1;
            string login = txtbLogin.Text.Trim();
            string password = txtbPassword.Text.Trim();

            CadrAgencyEntities db = CadrAgencyEntities.GetContext();
            var user = db.Users.Where(x => x.email == login && x.hashpass == password).FirstOrDefault();
           
            if (click == 1)
            {
                if (user != null)
                {
                    if (user.roleID == 1)
                        role = "соискатель";
                    else if (user.roleID == 2)
                        role = "работодатель";
                    MessageBox.Show("Вы вошли под: " + role.ToString());
                    LoadPage(role.ToString(), user);
                }
                else
                {
                    MessageBox.Show("Вы ввели логин или пароль неверно!");
                    GenerateCaptcha();
                    txtbPassword.Clear();

                }

            }

            else if(click > 1)
            {
                if(user != null && txtbCaptcha.Text == txtBlockCaptcha.Text)
                {
                    MessageBox.Show("Вы вошли под: " + role.ToString());
                    LoadPage(role.ToString(), user);
                }
                else
                {
                    MessageBox.Show("Введите данные заново!");
                }

            }


        }

        private void LoadPage(string _role, User user)
        {
            click = 0;
            switch (_role)
            {
                case "соискатель":
                    NavigationService.Navigate(new Client(user, _role));
                    break;
                case "работодатель":
                    NavigationService.Navigate(new Client(user, _role));
                    break;
                default:
                    NavigationService.Navigate(new MainWindow());
                    break;
            }
        }




    }
}
