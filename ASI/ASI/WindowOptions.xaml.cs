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
using System.IO;

namespace ASI
{
    /// <summary>
    /// Logica di interazione per WindowOptions.xaml
    /// </summary>
    public partial class WindowOptions : Window
    {
        public WindowOptions()
        {
            InitializeComponent();
            LoadGraphicalSettings();
        }
        private void GetFreeAVWXToken_Click(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://account.avwx.rest/login");
        }
        private void GetFreeOpenAipToken_Click(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.openaip.net/users/clients ");
        }
        private void btnConfirmSettings_Click(object sender, RoutedEventArgs e)
        {
            //--- SAVING CREDENTIALS ---
            MainWindow.APP_SETTINGS.JP_USER = txbJeppesenUser.Text;
            MainWindow.APP_SETTINGS.JP_PASSWORD = txbJeppesenPassword.Text;
            MainWindow.APP_SETTINGS.LD_USER = txbLidoUser.Text;
            MainWindow.APP_SETTINGS.LD_PASSWORD = txbLidoPassword.Text;
            MainWindow.APP_SETTINGS.AVWX_TOKEN = txbAvwxToken.Text;
            MainWindow.APP_SETTINGS.OPENAIP_TOKEN = txbOpenAipToken.Text;
            //--- SAVING INTEGRATIONS ---
            //Information
            if (rbtnUseNoAirportData.IsChecked == true) MainWindow.APP_SETTINGS.IsInformationAVWX = MainWindow.APP_SETTINGS.IsInformationOpenAip = false;
            else if (rbtnUseAVWXAirportData.IsChecked == true) { MainWindow.APP_SETTINGS.IsInformationAVWX = true; MainWindow.APP_SETTINGS.IsInformationOpenAip = false; }
            else if (rbtnUseOpenAipAirportData.IsChecked == true) { MainWindow.APP_SETTINGS.IsInformationAVWX = false; MainWindow.APP_SETTINGS.IsInformationOpenAip = true; }
            //Charts
            if (rbtnNoCharts.IsChecked == true) MainWindow.APP_SETTINGS.IsChartServiceJeppesen = MainWindow.APP_SETTINGS.IsChartServiceLido = false;
            else if (rbtnUseJeppesenCharts.IsChecked == true) { MainWindow.APP_SETTINGS.IsChartServiceJeppesen = true; MainWindow.APP_SETTINGS.IsChartServiceLido = false; }
            else if (rbtnUseLidoCharts.IsChecked == true) { MainWindow.APP_SETTINGS.IsChartServiceJeppesen = false; MainWindow.APP_SETTINGS.IsChartServiceLido = true; }
            //Weather
            if (rbtnWeatherNo.IsChecked == true) MainWindow.APP_SETTINGS.IsWeatherAVWX = MainWindow.APP_SETTINGS.IsWeatherNOAA = MainWindow.APP_SETTINGS.IsWeatherIVAO = false;
            else if (rbtnWeatherAvwx.IsChecked == true) { MainWindow.APP_SETTINGS.IsWeatherAVWX = true; MainWindow.APP_SETTINGS.IsWeatherIVAO = MainWindow.APP_SETTINGS.IsWeatherNOAA = false; }
            else if (rbtnWeatherNoaa.IsChecked == true) { MainWindow.APP_SETTINGS.IsWeatherNOAA = true; MainWindow.APP_SETTINGS.IsWeatherIVAO = MainWindow.APP_SETTINGS.IsWeatherAVWX = false; }
            //Frequencies
            if (rbtnNoFrequencies.IsChecked == true) MainWindow.APP_SETTINGS.IsFrequenciesOpenAip = false;
            else if (rbtnUseOpenAipFrequencies.IsChecked == true) MainWindow.APP_SETTINGS.IsFrequenciesOpenAip = true;
            //Atis
            if (rbtnAtisNo.IsChecked == true) MainWindow.APP_SETTINGS.IsAtisIVAO = false;
            else if (rbtnAtisIvao.IsChecked == true) MainWindow.APP_SETTINGS.IsAtisIVAO = true;
            //--- SAVING UNITS ---

            //Saving on DB
            MainWindow.APP_SETTINGS.SaveSettings();
            this.Close();
        }
        private void btnChartsEmptyCache_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBoxResult.Yes == MessageBox.Show("This operation will delete the files of already downloaded charts, which means all charts will take longer to load as they will be downloaded first.\nAre you sure you want to delete the charts cache?", "Empty charts cache", MessageBoxButton.YesNo, MessageBoxImage.Asterisk))
            {
                try
                {
                    string[] chartsCacheFiles = Directory.GetFiles(MainWindow.APP_SETTINGS.DATA_PATH_CHARTS);
                    foreach (string c in chartsCacheFiles)
                        File.Delete(c);
                    MainWindow.HandleMessage("Cache has been deleted succesfully!", "Empty charts cache");
                }
                catch (Exception ex) { MainWindow.HandleException(ex); }
            }
        }

        private void LoadGraphicalSettings()
        {
            //--- LOADING CREDENTIALS ---
            txbJeppesenUser.Text = MainWindow.APP_SETTINGS.JP_USER;
            txbJeppesenPassword.Text = MainWindow.APP_SETTINGS.JP_PASSWORD;
            txbLidoUser.Text = MainWindow.APP_SETTINGS.LD_USER;
            txbLidoPassword.Text = MainWindow.APP_SETTINGS.LD_PASSWORD;
            txbAvwxToken.Text = MainWindow.APP_SETTINGS.AVWX_TOKEN;
            txbOpenAipToken.Text = MainWindow.APP_SETTINGS.OPENAIP_TOKEN;
            //--- LOADING SERVICES ---
            //Information
            if (!MainWindow.APP_SETTINGS.IsInformationOpenAip && !MainWindow.APP_SETTINGS.IsInformationAVWX) rbtnUseNoAirportData.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.IsInformationAVWX) rbtnUseAVWXAirportData.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.IsInformationOpenAip) rbtnUseOpenAipAirportData.IsChecked = true;
            //Charts
            if (!MainWindow.APP_SETTINGS.IsChartServiceJeppesen && !MainWindow.APP_SETTINGS.IsChartServiceLido) rbtnNoCharts.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.IsChartServiceJeppesen) rbtnUseJeppesenCharts.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.IsChartServiceLido) rbtnUseLidoCharts.IsChecked = true;
            //Weather
            if (!MainWindow.APP_SETTINGS.IsWeatherAVWX && !MainWindow.APP_SETTINGS.IsWeatherNOAA && !MainWindow.APP_SETTINGS.IsWeatherIVAO) rbtnWeatherNo.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.IsWeatherAVWX) rbtnWeatherAvwx.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.IsWeatherNOAA) rbtnWeatherNoaa.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.IsWeatherIVAO) rbtnWeatherIvao.IsChecked = true;
            //Frequencies
            if (!MainWindow.APP_SETTINGS.IsFrequenciesOpenAip) rbtnNoFrequencies.IsChecked = true;  
            else if (MainWindow.APP_SETTINGS.IsFrequenciesOpenAip) rbtnUseOpenAipFrequencies.IsChecked = true;
            //Atis
            if (!MainWindow.APP_SETTINGS.IsAtisIVAO) rbtnAtisNo.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.IsAtisIVAO) rbtnAtisIvao.IsChecked = true;
            //--- LOADING UNITS ---
            //Distance
            if (MainWindow.APP_SETTINGS.UNIT_DIST == "M") rbtnDistM.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_DIST == "FT") rbtnDistFt.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_DIST == "YD") rbtnDistYd.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_DIST == "NM") rbtnDistNm.IsChecked = true;
            //Wind speed
            if (MainWindow.APP_SETTINGS.UNIT_WIND == "MPS") rbtnWindMs.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_WIND == "KPH") rbtnWindKph.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_WIND == "MPS") rbtnWindMph.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_WIND == "KTS") rbtnWindKts.IsChecked = true;
            //Pressure
            if (MainWindow.APP_SETTINGS.UNIT_PRES == "HPA") rbtnPresHpa.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_PRES == "INHG") rbtnPresInhg.IsChecked = true;
            //Runway distance
            if (MainWindow.APP_SETTINGS.UNIT_RWY == "M") rbtnRwyM.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_RWY == "FT") rbtnRwyFt.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_RWY == "YD") rbtnRwyYd.IsChecked = true;
            //Runway elevation
            if (MainWindow.APP_SETTINGS.UNIT_ELEV == "M") rbtnRwyElM.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_ELEV == "FT") rbtnRwyElFt.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_ELEV == "YD") rbtnRwyElYd.IsChecked = true;
            //Temperature
            if (MainWindow.APP_SETTINGS.UNIT_TEMP == "C") rbtnTempC.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_TEMP == "F") rbtnTempF.IsChecked = true;
            //Visibility
            if (MainWindow.APP_SETTINGS.UNIT_VISIB == "M") rbtnVisibM.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_VISIB == "MI") rbtnVisibMi.IsChecked = true;
        }
    }
}