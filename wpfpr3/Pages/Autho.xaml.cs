using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
    /// 
    public partial class Autho : Page
    {
        int click;
        int count;
        DispatcherTimer timer;      
        int seconds;
        public string role;
        CadrAgencyEntities db = CadrAgencyEntities.GetContext();
        Random rnd = new Random();
        private string _recoveryCode;
        private int _recoveryUserId;
        private DateTime _recoveryCodeCreatedAt;
        private string _twoFactorCode;
        private int _twoFactorUserId;
        private DateTime _twoFactorCreatedAt;
        private User _pendingUser;       
        private string _pendingRole;     



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
            AppSecurityOptions.TwoFactorEnabledGlobally = false;


        }

        //таймер для отсчета секунд блокировки
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
        private async void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
                return;
            click += 1;
            string login = txtbLogin.Text.Trim();
            string password = txtbPassword.Text.Trim();

            CadrAgencyEntities db = CadrAgencyEntities.GetContext();
            string hashpassword = Hash.HashPassword(password);
            var user = db.Users.Where(x => x.email == login && x.hashpass == hashpassword).FirstOrDefault();
            //проверка доступа к системе
            if (click == 1)
            {
                if (user != null)
                {
                    if (!CanAccessSystem(DateTime.Now))
                    {
                        MessageBox.Show("Доступ к системе запрещён. Рабочее время с 10:00 до 19:00.");
                        return;
                    }
                    _pendingUser = user;
                    _pendingRole = GetRole(user);
                    if (!AppSecurityOptions.TwoFactorEnabledGlobally)
                    {
                        LoadPage(_pendingRole, user);
                        return;
                    }

                    await StartTwoFactorAsync(user);

                    return;
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
        //загрузка страницы в зависимости от роли
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
        //блокировка
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
        //приветствие в зависимости от времени суток
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
        //SMTP-взаимодействие
        private void forgotButton_Click(object sender, RoutedEventArgs e)
        {
            RecoveryPanel.Visibility = Visibility.Visible;
        }


        private async void btnSendCode_Click(object sender, RoutedEventArgs e)
        {
            string login = tbRecoverLogin.Text;

           var  recoveryUser = db.Users
                .FirstOrDefault(x => x.email == login);

            if (recoveryUser == null)
            {
                tbRecoverStatus.Text = "Пользователь не найден";
                return;
            }

            var recoveryCode = rnd.Next(1000, 9999).ToString();
            _recoveryCode = recoveryCode;
            _recoveryUserId = recoveryUser.id; 
            _recoveryCodeCreatedAt = DateTime.Now;


            try
            {
                MailMessage msg = new MailMessage(
                    "nsv154nikonorova@yandex.ru",
                    recoveryUser.email,
                    "Код восстановления",
                    $"Ваш код: {recoveryCode}");

                SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587);
                smtp.Credentials = new NetworkCredential("nsv154nikonorova@yandex.ru", "qsjrnykbkrgwxbjo");
                smtp.EnableSsl = true;

                await smtp.SendMailAsync(msg);

                tbRecoverStatus.Text = "Код отправлен на почту";
            }
            catch(Exception ex)
            {
                tbRecoverStatus.Text = "Ошибка отправки письма";
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var inputCode = tbCode.Text?.Trim();

            if (string.IsNullOrWhiteSpace(inputCode))
            {
                tbRecoverStatus.Text = "Введите код";
                return;
            }

            if (string.IsNullOrWhiteSpace(_recoveryCode) || _recoveryUserId == 0)
            {
                tbRecoverStatus.Text = "Сначала отправьте код";
                return;
            }

            // (не обязательно, но полезно) срок жизни кода
            if (DateTime.Now - _recoveryCodeCreatedAt > TimeSpan.FromMinutes(10))
            {
                tbRecoverStatus.Text = "Код истёк, запросите новый";
                _recoveryCode = null;
                _recoveryUserId = 0;
                return;
            }

            if (inputCode != _recoveryCode)
            {
                tbRecoverStatus.Text = "Неверный код";
                return;
            }
            NavigationService.Navigate(new PasswordRecovery(_recoveryUserId));

        }
        private async Task StartTwoFactorAsync(User user)
        {
            _twoFactorCode = rnd.Next(1000, 9999).ToString();
            _twoFactorUserId = user.id;
            _twoFactorCreatedAt = DateTime.Now;

            try
            {
                var msg = new MailMessage(
                    "nsv154nikonorova@yandex.ru",
                    user.email,
                    "Код доступа",
                    $"Ваш код для входа: {_twoFactorCode}");

                using (var smtp = new SmtpClient("smtp.yandex.ru", 587))
                {
                    smtp.Credentials = new NetworkCredential(
                        "nsv154nikonorova@yandex.ru",
                        "qsjrnykbkrgwxbjo"   // лучше потом вынести из кода
                    );
                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(msg);
                }

                // показываем панель 2FA
                TwoFactorPanel.Visibility = Visibility.Visible;
                tbTwoFactorStatus.Text = "";
                tbTwoFactorCode.Text = "";

                // (опционально) можно заблокировать поля логина, чтобы не путались
                txtbLogin.IsEnabled = false;
                txtbPassword.IsEnabled = false;
                btnEnter.IsEnabled = false;
                btnEnterGuests.IsEnabled = false;
            }
            catch (Exception ex)
            {
                TwoFactorPanel.Visibility = Visibility.Collapsed;
                tbTwoFactorStatus.Text = "";
                MessageBox.Show("Не удалось отправить код на почту: " + ex.Message);
            }
        }

        private void btnTwoFactorConfirm_Click(object sender, RoutedEventArgs e)
        {
            var input = tbTwoFactorCode.Text?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                tbTwoFactorStatus.Text = "Введите код";
                return;
            }

            if (string.IsNullOrWhiteSpace(_twoFactorCode) || _twoFactorUserId == 0 || _pendingUser == null)
            {
                tbTwoFactorStatus.Text = "Сначала войдите (логин и пароль)";
                return;
            }

  
            if (DateTime.Now - _twoFactorCreatedAt > TimeSpan.FromMinutes(5))
            {
                tbTwoFactorStatus.Text = "Код истёк. Войдите заново.";
                _twoFactorCode = null;
                _twoFactorUserId = 0;
                _pendingUser = null;
                _pendingRole = null;
                TwoFactorPanel.Visibility = Visibility.Collapsed;
                return;
            }

            if (input != _twoFactorCode)
            {
                tbTwoFactorStatus.Text = "Неверный код";
                return;
            }

    
            TwoFactorPanel.Visibility = Visibility.Collapsed;

            txtbLogin.IsEnabled = true;
            txtbPassword.IsEnabled = true;
            btnEnter.IsEnabled = true;
            btnEnterGuests.IsEnabled = true;

            _twoFactorCode = null;
            _twoFactorUserId = 0;

            var userToLogin = _pendingUser;
            var roleToLogin = _pendingRole ?? GetRole(userToLogin);

            _pendingUser = null;
            _pendingRole = null;

            LoadPage(roleToLogin, userToLogin);
        }
    }
}
