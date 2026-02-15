using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;
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
        int count;
        DispatcherTimer timer;      
        int seconds;
        public string role;
        public Autho()
        {
            InitializeComponent();
            click = 0;
            count = 0;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            Timetxt.Visibility = Visibility.Hidden;
            Timetxt.Foreground = Brushes.Red;

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
           seconds--;

            if (seconds > 0)
            {
                Timetxt.Text = $" {seconds} ";
            }
            else
            {
                timer.Stop();
                Timetxt.Visibility = Visibility.Collapsed;

                count = 0;
                click = 0;

                SetControlsEnabled(true);
            }
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
        private string GetRole(User user)
        {
            if (user.roleID == 1)
                return "соискатель";
            if (user.roleID == 2)
                return "работодатель";
            return "";
        }
        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
                return;
            click += 1;
            string login = txtbLogin.Text.Trim();
            string password = txtbPassword.Text.Trim();

            CadrAgencyEntities db = CadrAgencyEntities.GetContext();
            string hashpassword = Hash.HashPassword(password);
            var user = db.Users.Where(x => x.email == login && x.hashpass == hashpassword).FirstOrDefault();
           
            if (click == 1)
            {
                if (user != null)
                {
                    if (!CanAccessSystem(DateTime.Now))
                    {
                        MessageBox.Show("Доступ к системе запрещён. Рабочее время с 10:00 до 19:00.");
                        return;
                    }
                    role = GetRole(user);
                    string greeting = GetGreeting(user, DateTime.Now);
                    MessageBox.Show($"{greeting}!\nВы вошли под ролью: {role}");
                    LoadPage(role.ToString(), user);
                }
                else
                {
                    MessageBox.Show("Вы ввели логин или пароль неверно!");
                    count++;
                    Console.WriteLine(count);
                    GenerateCaptcha();
                    txtbPassword.Clear();
                    CheckBlock();
 

                }

            }
            else if(click > 1)
            {
                if(user != null && txtbCaptcha.Text == txtBlockCaptcha.Text)
                {
                    string greeting = GetGreeting(user, DateTime.Now);
                    MessageBox.Show($"{greeting}!\nВы вошли под ролью: {role}");
                    LoadPage(role.ToString(), user);
                }
                else
                {
                    MessageBox.Show("Введите данные заново!");
                    count += 1;
                    txtbLogin.Clear();
                    txtbPassword.Clear();
                    txtBlockCaptcha.Visibility = Visibility.Hidden;
                    txtbCaptcha.Visibility = Visibility.Hidden;
                    CheckBlock();

                }

            }


        }

        private void LoadPage(string _role, User user)
        {
            click = 0;
            count = 0;
            switch (_role)
            {
                case "соискатель":
                    NavigationService.Navigate(new Client(user, _role));
                    break;
                case "работодатель":
                    NavigationService.Navigate(new Employer(user, _role));
                    break;
                default:
                    NavigationService.Navigate(new MainWindow());
                    break;
            }
        }

        private void SetControlsEnabled(bool isEnabled)
        {
            btnEnter.IsEnabled = isEnabled;
            btnEnterGuests.IsEnabled = isEnabled;
            txtbLogin.IsEnabled = isEnabled;
            txtbPassword.IsEnabled = isEnabled;
            txtbCaptcha.IsEnabled = isEnabled;
        }

        // Проверка, нужно ли блокировать окно
        private void CheckBlock()
        {
            if (count >= 3)
            {
                StartLock();
            }
        }

        private void StartLock()
        {
            seconds = 10;
            Timetxt.Visibility = Visibility.Visible;
            Timetxt.Text = $" {seconds} ";

            SetControlsEnabled(false);
            timer.Start();
        }
        private string GetFullName(User user)
        {
            if (user == null)
                return string.Empty;

            string fio = $"{user.surname} {user.firstname}"; 

           
            return fio;
        }
        private string GetGreeting(User user, DateTime now)
        {
            var time = now.TimeOfDay;
            string partOfDay;

           
            if (time >= new TimeSpan(10, 0, 0) && time <= new TimeSpan(12, 0, 0))
                partOfDay = "Доброе утро";
            
            else if (time > new TimeSpan(12, 0, 0) && time <= new TimeSpan(17, 0, 0))
                partOfDay = "Добрый день";
           
            else if (time > new TimeSpan(17, 0, 0) && time <= new TimeSpan(19, 0, 0))
                partOfDay = "Добрый вечер";
            else
                partOfDay = "Здравствуйте";

            string fullName = GetFullName(user);
            return $"{partOfDay}, {fullName}";
        }
        private bool CanAccessSystem(DateTime now)
        {
            var time = now.TimeOfDay;

            var start = new TimeSpan(10, 0, 0); 
            var end = new TimeSpan(19, 0, 0);   

            return time >= start && time <= end;
        }

        private void forgotButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ForgotPassword());
        }
    }
}
