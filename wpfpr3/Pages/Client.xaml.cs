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

namespace wpfpr3.Pages
{
    /// <summary>
    /// Логика взаимодействия для Client.xaml
    /// </summary>
    public partial class Client : Page
    {
        public Client(User user, string role)
        {
            InitializeComponent();
            var candidate = CadrAgencyEntities.GetContext().Candidates.ToList();
            lviewCand.ItemsSource = candidate;
        }
    }
}
