using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
    /// Логика взаимодействия для PasswordRecovery.xaml
    /// </summary>
    public partial class PasswordRecovery : Page
    {
        private readonly int _userId;
        private readonly CadrAgencyEntities db = CadrAgencyEntities.GetContext();
        public PasswordRecovery(int userId)
        {
            InitializeComponent();
            _userId = userId;
        }
        //изменение пароля
        private void btnSaveNewPassword_Click(object sender, RoutedEventArgs e)
        {
            string newPass = pbNewPassword.Password?.Trim();
            string repeat = pbRepeatPassword.Password?.Trim();

            if (string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(repeat))
            {
                tbStatus.Text = "Заполните оба поля";
                return;
            }

            if (newPass.Length < 6)
            {
                tbStatus.Text = "Пароль слишком короткий (минимум 6 символов)";
                return;
            }

            if (newPass != repeat)
            {
                tbStatus.Text = "Пароли не совпадают";
                return;
            }

            var user = db.Users.FirstOrDefault(u => u.id == _userId); 
            if (user == null)
            {
                tbStatus.Text = "Пользователь не найден";
                return;
            }
            try
            {
                user.hashpass = Hash.HashPassword(newPass);
                db.SaveChanges();

                MessageBox.Show("Пароль обновлён");
                NavigationService.Navigate(new Autho());
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var sb = new StringBuilder();
                foreach (var eve in ex.EntityValidationErrors)
                {
                    sb.AppendLine($"Entity: {eve.Entry.Entity.GetType().Name}, State: {eve.Entry.State}");
                    foreach (var ve in eve.ValidationErrors)
                        sb.AppendLine($" - {ve.PropertyName}: {ve.ErrorMessage}");
                }

                MessageBox.Show(sb.ToString(), "Ошибки валидации EF");
            }


        }
    }
}
   