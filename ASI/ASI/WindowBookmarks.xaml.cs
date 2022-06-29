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
using System.Windows.Shapes;

namespace ASI
{
    /// <summary>
    /// Logica di interazione per Bookmarks.xaml
    /// </summary>
    public partial class Bookmarks : Window
    {
        public Bookmarks()
        {
            InitializeComponent();
            LoadBookmarks();
        }

        private void LoadBookmarks()
        {
            lsbBookmarks.Items.Clear();
            foreach (string s in MainWindow.APP_SETTINGS.Favourites)
                lsbBookmarks.Items.Add(s);
        }

        private void btnDeleteBookmark_Click(object sender, RoutedEventArgs e)
        {
            string value = lsbBookmarks.SelectedValue.ToString();
            MainWindow.APP_SETTINGS.RemoveFavourite(value);
            LoadBookmarks();
        }

        private void btnOpenBookmark_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.IcaoFromBookmarks = lsbBookmarks.SelectedValue.ToString();
            this.Close();
        }
    }
}
