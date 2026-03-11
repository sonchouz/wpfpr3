using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
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

namespace wpfpr3.Pages
{
   
    public partial class Employer : Page
    {
        private readonly User _currentUser;
        private List<Candidate> _allCandidates = new List<Candidate>();

       //фильтры и сортировка
        public List<string> SortingList { get; } = new List<string>
        {
            "Без сортировки",
            "Фамилия (А-Я)",
            "Фамилия (Я-А)",
            "Дата рождения (новые)",
            "Дата рождения (старые)"
        };

        public List<string> FilterList { get; } = new List<string>
        {
            "Все",
            "основное общее",
            "среднее общее",
            "среднее профессиональное",
            "высшее"
        };
        public Employer(User user, string role)
        {
            InitializeComponent();
            DataContext = this;


            LoadCandidates();
            ApplyFilters();


        }
        //применение фильтров
        private void ApplyFilters()
        {
            if (_allCandidates == null) return;

            var query = _allCandidates.AsQueryable();

           
            var searchText = txtFullName.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();

                query = query.Where(c =>
                    (c.User.firstname != null && c.User.firstname.ToLower().Contains(searchText)) ||
                    (c.User.surname != null && c.User.surname.ToLower().Contains(searchText)));
            }

     
            switch (cmbFilter.SelectedIndex)
            {
                case 1: 
                    query = query.Where(c => c.Education != null &&
                                             c.Education.edulevel != null &&
                                             c.Education.edulevel.ToLower().Contains("основное общее"));
                    break;
                case 2: 
                    query = query.Where(c => c.Education == null ||
                                             c.Education.edulevel == null ||
                                             c.Education.edulevel.ToLower().Contains("среднее общее"));
                    break;
                case 3:
                    query = query.Where(c => c.Education == null ||
                                             c.Education.edulevel == null ||
                                             c.Education.edulevel.ToLower().Contains("среднее профессиональное"));
                    break;
                case 4:
                    query = query.Where(c => c.Education == null ||
                                             c.Education.edulevel == null ||
                                             c.Education.edulevel.ToLower().Contains("высшее"));
                    break;


            }

           
            switch (cmbSorting.SelectedIndex)
            {
                case 1: 
                    query = query.OrderBy(c => c.User.surname);
                    break;
                case 2: 
                    query = query.OrderByDescending(c => c.User.surname);
                    break;
                case 3: 
                    query = query.OrderByDescending(c => c.User.birthday);
                    break;
                case 4: 
                    query = query.OrderBy(c => c.User.birthday);
                    break; 
            }

            var result = query.ToList();
            lViewCand.ItemsSource = result;

            txtResultCount.Text = result.Count.ToString();
        }
        //загрузка кандидатов
        private void LoadCandidates()
        {
            var ctx = CadrAgencyEntities.GetContext();
            _allCandidates = ctx.Candidates.ToList();
            txtAllAmount.Text = _allCandidates.Count.ToString();

        }

        private void lViewCand_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var cand = lViewCand.SelectedItem as Candidate;
            if (cand == null) return;
            NavigationService.Navigate(new
           AddEditCandidatePage());
        }
        //добавление кандидата
        private void btnAddEmp_Click(object sender, RoutedEventArgs e)
        {

            NavigationService.Navigate(new AddEditCandidatePage());
        }
        //изменение состояния
        private void cmbSorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void txtFullName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void btnEmp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtFullName_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void btnAddNewEmp_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditCandidatePage());
        }

        private void PrintListButton_Click(object sender, RoutedEventArgs e)
        {
            FlowDocument doc = fdr.Document;
            if(doc == null)
            {
                MessageBox.Show("Документ не найден");
                return;
            }

            PrintDialog pd  = new PrintDialog();
             if(pd.ShowDialog() == true)
            {
                FlowDocument printDoc = fdr.Document;

                printDoc.PageHeight = pd.PrintableAreaHeight;
                printDoc.PageWidth = pd.PrintableAreaWidth;
                printDoc.PagePadding = new Thickness(40);
                printDoc.ColumnGap = 0;
                printDoc.ColumnWidth = pd.PrintableAreaWidth;

                IDocumentPaginatorSource idpSource = doc;
                pd.PrintDocument(idpSource.DocumentPaginator, "Список сотрудников");
            }
        }
    } 
    
}
