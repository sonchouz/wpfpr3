using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using wpfpr3.Models;

namespace wpfpr3.Pages
{
    public partial class AddEditCandidatePage : Page
    {
        private readonly CadrAgencyEntities _context = CadrAgencyEntities.GetContext();
        private readonly bool _isNew;
        private Candidate cand;

        public AddEditCandidatePage(Candidate currentcand = null)
        {
            InitializeComponent();
            _isNew = (currentcand == null || currentcand.id <= 0);

            if (_isNew)
            {
                cand = new Candidate
                {
                    User = new User()
                };

               
                cand.User.roleID = 1;
                btnDeleteCand.Visibility = Visibility.Collapsed;
            }
            else
            {
                
                cand = _context.Candidates
                    .Include(c => c.User)
                    .FirstOrDefault(c => c.id == currentcand.id);

                if (cand == null)
                {
                    MessageBox.Show("Кандидат не найден в базе.", "Ошибка");
                    NavigationService.GoBack();
                    return;
                }

                btnDeleteCand.Visibility = Visibility.Visible;
                txtPswd.Text = ""; 
            }

            
            cmbEdu.ItemsSource = _context.Educations.ToList();
            cmbEdu.DisplayMemberPath = "edulevel";
            cmbEdu.SelectedValuePath = "id";

            if (cand.educationID != null)
                cmbEdu.SelectedValue = cand.educationID;

            DataContext = cand;

            UpdateDeleteButtonVisibility();
        }

  
        private static readonly Regex PhoneAllowed = new Regex(@"^[0-9+\-\(\)\s]+$");

        private void Phone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !PhoneAllowed.IsMatch(e.Text);
        }

        private void AnyField_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDeleteButtonVisibility();
        }

        private void UpdateDeleteButtonVisibility()
        {
            
            if (!_isNew)
            {
                btnDeleteCand.Visibility = Visibility.Visible;
                return;
            }

            
            var surname = (cand?.User?.surname ?? "").Trim();
            var firstname = (cand?.User?.firstname ?? "").Trim();
            var phone = (cand?.User?.phone ?? "").Trim();
            var email = (cand?.User?.email ?? "").Trim();
            var city = (cand?.livingcity ?? "").Trim();
            var citizenship = (cand?.citizenship ?? "").Trim();

            if (surname == "" && firstname == "" && phone == "" && email == "" && city == "" && citizenship == "")
            {
                btnDeleteCand.Visibility = Visibility.Collapsed;
                return;
            }

            
            bool existsUser = _context.Users.Any(u =>
                (surname != "" && u.surname == surname) ||
                (firstname != "" && u.firstname == firstname) ||
                (phone != "" && u.phone == phone) ||
                (email != "" && u.email == email)
            );

            bool existsCandidate = _context.Candidates.Any(c =>
                (city != "" && c.livingcity == city) ||
                (citizenship != "" && c.citizenship == citizenship)
            );

            btnDeleteCand.Visibility = (existsUser || existsCandidate)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void btnEnterImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dlg.ShowDialog() == true)
            {
                img.Source = new BitmapImage(new Uri(dlg.FileName));
            }
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password ?? "");
                var hash = sha256.ComputeHash(bytes);

                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private void btnSaveCandidate_Click(object sender, RoutedEventArgs e)
        {
            if (cand?.User == null)
            {
                MessageBox.Show("Ошибка: User не создан.", "Ошибка");
                return;
            }

            var errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(cand.User.surname))
                errors.AppendLine("Фамилия обязательна.");

            if (string.IsNullOrWhiteSpace(cand.User.firstname))
                errors.AppendLine("Имя обязательно.");

            if (string.IsNullOrWhiteSpace(cand.User.phone))
                errors.AppendLine("Телефон обязателен.");

            if (string.IsNullOrWhiteSpace(cand.User.email))
                errors.AppendLine("Email обязателен.");

            if (!dpBirthday.SelectedDate.HasValue)
                errors.AppendLine("Дата рождения обязательна.");

            if (string.IsNullOrWhiteSpace(cand.citizenship))
                errors.AppendLine("Гражданство обязательно.");

            if (string.IsNullOrWhiteSpace(cand.livingcity))
                errors.AppendLine("Город проживания обязателен.");

            var selectedEdu = cmbEdu.SelectedItem as Education;
            if (selectedEdu == null)
                errors.AppendLine("Выберите образование.");

            if (_isNew && string.IsNullOrWhiteSpace(txtPswd.Text))
                errors.AppendLine("Пароль обязателен.");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка");
                return;
            }

            cand.User.birthday = dpBirthday.SelectedDate.Value;
            cand.educationID = selectedEdu.id;

    
            if (cand.statusID == 0 && _context.Statuses.Any())
                cand.statusID = _context.Statuses.Select(s => s.id).First();

            if (!string.IsNullOrWhiteSpace(txtPswd.Text))
                cand.User.hashpass = HashPassword(txtPswd.Text);

            try
            {
                if (_isNew)
                {
                    _context.Users.Add(cand.User);
                    _context.SaveChanges();

          
                    cand.id = cand.User.id;

                 
                    _context.Candidates.Add(cand);
                }

                _context.SaveChanges();

                MessageBox.Show("Информация сохранена.", "Ок");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка сохранения");
            }
        }

        private void btnDeleteCandidate_Click(object sender, RoutedEventArgs e)
        {
          
            if (!_isNew && cand?.User != null && cand.User.id > 0)
            {
                DeleteByUserId(cand.User.id);
                return;
            }

            
   
            var email = (cand?.User?.email ?? "").Trim();
            var phone = (cand?.User?.phone ?? "").Trim();
            var surname = (cand?.User?.surname ?? "").Trim();
            var firstname = (cand?.User?.firstname ?? "").Trim();

            int? foundUserId = null;

            if (email != "")
            {
                foundUserId = _context.Users
                    .Where(u => u.email == email)
                    .Select(u => (int?)u.id)
                    .FirstOrDefault();
            }

            if (foundUserId == null && phone != "")
            {
                foundUserId = _context.Users
                    .Where(u => u.phone == phone)
                    .Select(u => (int?)u.id)
                    .FirstOrDefault();
            }

            if (foundUserId == null && surname != "" && firstname != "")
            {
                foundUserId = _context.Users
                    .Where(u => u.surname == surname && u.firstname == firstname)
                    .Select(u => (int?)u.id)
                    .FirstOrDefault();
            }

            if (foundUserId == null)
            {
                MessageBox.Show("Ошибка: совпадающая запись не найдена в базе.", "Ошибка");
                return;
            }

            DeleteByUserId(foundUserId.Value);
        }

        private void DeleteByUserId(int userId)
        {
            if (MessageBox.Show(
                    "Удалить запись из базы?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                var userFromDb = _context.Users.FirstOrDefault(u => u.id == userId);

                if (userFromDb == null)
                {
                    MessageBox.Show("Пользователь не найден (возможно уже удалён).", "Информация");
                    NavigationService.GoBack();
                    return;
                }

                
                _context.Users.Remove(userFromDb);
                _context.SaveChanges();

                MessageBox.Show("Запись удалена.", "Ок");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка удаления");
            }
        }
    }
}