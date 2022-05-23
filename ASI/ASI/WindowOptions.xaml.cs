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
            LoadSettings();
        }
        private void GetFreeAVWXToken_Click(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://account.avwx.rest/login");
        }
        private void btnConfirmSettings_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter writer = null;
            try
            {
                //--- CHECKING DIRECTORY STRUCTURE ---
                CheckDirectoryStructure();
                writer = new StreamWriter(MainWindow.APP_SETTINGS.DATA_PATH_SETTINGS);

                //--- SAVING SERVICES ---
                writer.WriteLine("INFO_AVWX=1");
                if (rbtnUseJeppesenCharts.IsChecked == true) writer.WriteLine("JEPPESEN=1"); else writer.WriteLine("JEPPESEN=0");
                if (rbtnUseLidoCharts.IsChecked == true) writer.WriteLine("LIDO=1"); else writer.WriteLine("LIDO=0");
                if (chkUseOpenAIP.IsChecked == true) writer.WriteLine("OPENAIP=1"); else writer.WriteLine("OPENAIP=0");
                if (rbtnWeatherAvwx.IsChecked == true) writer.WriteLine("WEATHER_AVWX=1"); else writer.WriteLine("WEATHER_AVWX=0");
                if (rbtnWeatherIvao.IsChecked == true) writer.WriteLine("WEATHER_IVAO=1"); else writer.WriteLine("WEATHER_IVAO=0");
                if (rbtnWeatherNoaa.IsChecked == true) writer.WriteLine("WEATHER_NOAA=1"); else writer.WriteLine("WEATHER_NOAA=0");

                //--- SAVING JEPPESEN CHARTS DATA ---
                writer.WriteLine($"JP_USER={txbJeppesenUser.Text}");
                if (rbtnUseJeppesenCharts.IsChecked == true)
                    MainWindow.APP_SETTINGS.JP_USER = txbJeppesenUser.Text;
                writer.WriteLine($"JP_PASSWORD={txbJeppesenPassword.Text}");
                if (rbtnUseJeppesenCharts.IsChecked == true)
                    MainWindow.APP_SETTINGS.JP_PASSWORD = txbJeppesenPassword.Text;
                //--- SAVING LIDO CHARTS DATA ---
                writer.WriteLine($"LD_USER={txbLidoUser.Text}");
                if (rbtnUseLidoCharts.IsChecked == true)
                    MainWindow.APP_SETTINGS.JP_USER = txbLidoUser.Text;
                writer.WriteLine($"LD_PASSWORD={txbLidoPassword.Text}");
                if (rbtnUseLidoCharts.IsChecked == true)
                    MainWindow.APP_SETTINGS.JP_PASSWORD = txbLidoPassword.Text;
                //--- SAVING AVWX DATA ---
                MainWindow.APP_SETTINGS.AVWX_TOKEN = txbAvwxToken.Text;
                writer.WriteLine($"AVWX_TOKEN={MainWindow.APP_SETTINGS.AVWX_TOKEN}");
                //--- SAVING SETTINGS DATA ----
                //Distance
                if (rbtnDistM.IsChecked == true) writer.WriteLine("UNIT_DIST=M");
                else if (rbtnDistFt.IsChecked == true) writer.WriteLine("UNIT_DIST=FT");
                else if (rbtnDistYd.IsChecked == true) writer.WriteLine("UNIT_DIST=YD");
                else if (rbtnDistNm.IsChecked == true) writer.WriteLine("UNIT_DIST=NM");
                //Runway Length
                if (rbtnRwyM.IsChecked == true) writer.WriteLine("UNIT_RWY=M");
                else if (rbtnRwyFt.IsChecked == true) writer.WriteLine("UNIT_RWY=FT");
                else if (rbtnRwyYd.IsChecked == true) writer.WriteLine("UNIT_RWY=YD");
                //Wind speed
                if (rbtnWindMs.IsChecked == true) writer.WriteLine("UNIT_WIND=MS");
                else if (rbtnWindKph.IsChecked == true) writer.WriteLine("UNIT_WIND=KPH");
                else if (rbtnWindMph.IsChecked == true) writer.WriteLine("UNIT_WIND=MPH");
                else if (rbtnWindKts.IsChecked == true) writer.WriteLine("UNIT_WIND=KTS");
                //Runway Elevation
                if (rbtnRwyElM.IsChecked == true) writer.WriteLine("UNIT_ELEV=M");
                else if (rbtnRwyElFt.IsChecked == true) writer.WriteLine("UNIT_ELEV=FT");
                else if (rbtnRwyElYd.IsChecked == true) writer.WriteLine("UNIT_ELEV=YD");
                //Temperature
                if (rbtnTempC.IsChecked == true) writer.WriteLine("UNIT_TEMP=C");
                else if (rbtnTempF.IsChecked == true) writer.WriteLine("UNIT_TEMP=F");
                //Pressure
                if (rbtnPresHpa.IsChecked == true) writer.WriteLine("UNIT_PRES=HPA");
                else if (rbtnPresInhg.IsChecked == true) writer.WriteLine("UNIT_PRES=INHG");
                //Visibility
                if (rbtnVisibM.IsChecked == true) writer.WriteLine("UNIT_VISIB=M");
                else if (rbtnVisibMi.IsChecked == true) writer.WriteLine("UNIT_VISIB=MI");
                //Runway Wind
                if (chkShowWindOnRunway.IsChecked == true) writer.WriteLine("GR_RWY_WIND=1");
                else writer.WriteLine("GR_RWY_WIND=0");

                //--- DONE ---
                writer.Close();
                HandleMessage("All settings have been saved correctly! Restart the program to load new settings.", "Save settings");
                this.Close();
            }
            catch (InvalidDataException ex) { HandleWarning(ex.Message); }
            catch (Exception ex) { HandleException(ex); }
            finally { if (writer != null) writer.Close(); }
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
                    HandleMessage("Cache has been deleted succesfully!", "Empty charts cache");
                }
                catch (Exception ex) { HandleException(ex); }
            }
        }
        private void CheckDirectoryStructure()
        {
            //Checking dir structure
            if (!Directory.Exists(MainWindow.APP_SETTINGS.DATA_PATH_ROOT))
            {
                //Creating dir structure
                Directory.CreateDirectory(MainWindow.APP_SETTINGS.DATA_PATH_ROOT);
                Directory.CreateDirectory(MainWindow.APP_SETTINGS.DATA_PATH_ROOT + "Login");
                Directory.CreateDirectory(MainWindow.APP_SETTINGS.DATA_PATH_ROOT + "Charts");
            }
        }
        private void LoadSettings()
        {
            //Loading services
            rbtnUseJeppesenCharts.IsChecked = MainWindow.APP_SETTINGS.IsChartServiceJeppesen;
            rbtnUseLidoCharts.IsChecked = MainWindow.APP_SETTINGS.IsChartServiceLido;
            if (!MainWindow.APP_SETTINGS.IsChartServiceLido && !MainWindow.APP_SETTINGS.IsChartServiceJeppesen)
                rbtnNoCharts.IsChecked = true;
            chkUseOpenAIP.IsChecked = MainWindow.APP_SETTINGS.IsFrequenciesOpenAIP;
            rbtnWeatherAvwx.IsChecked = MainWindow.APP_SETTINGS.IsWeatherAVWX;
            rbtnWeatherIvao.IsChecked = MainWindow.APP_SETTINGS.IsWeatherIVAO;
            rbtnWeatherNoaa.IsChecked = MainWindow.APP_SETTINGS.IsWeatherNOAA;
            if (!MainWindow.APP_SETTINGS.IsWeatherAVWX && !MainWindow.APP_SETTINGS.IsWeatherIVAO && !MainWindow.APP_SETTINGS.IsWeatherNOAA)
                rbtnWeatherNo.IsChecked = true;
            //Loading Jeppesen charts credentials
            txbJeppesenUser.Text = MainWindow.APP_SETTINGS.JP_USER;
            txbJeppesenPassword.Text = MainWindow.APP_SETTINGS.JP_PASSWORD;
            //Loading Lido charts credentials
            txbLidoUser.Text = MainWindow.APP_SETTINGS.LD_USER;
            txbLidoPassword.Text = MainWindow.APP_SETTINGS.LD_PASSWORD;
            //Loading AVWX Token
            txbAvwxToken.Text = MainWindow.APP_SETTINGS.AVWX_TOKEN;
            //LOADING SETTINGS
            //Distance
            if (MainWindow.APP_SETTINGS.UNIT_DIST == "M") rbtnDistM.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_DIST == "FT") rbtnDistFt.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_DIST == "YD") rbtnDistYd.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_DIST == "NM") rbtnDistNm.IsChecked = true;
            //Runway Length
            if (MainWindow.APP_SETTINGS.UNIT_RWY == "M") rbtnRwyM.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_RWY == "FT") rbtnRwyFt.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_RWY == "YD") rbtnRwyYd.IsChecked = true;
            //Wind speed
            if (MainWindow.APP_SETTINGS.UNIT_WIND == "MS") rbtnWindMs.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_WIND == "KPH") rbtnWindKph.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_WIND == "MPH") rbtnWindMph.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_WIND == "KTS") rbtnWindKts.IsChecked = true;
            //Runway Elevation
            if (MainWindow.APP_SETTINGS.UNIT_ELEV == "M") rbtnRwyElM.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_ELEV == "FT") rbtnRwyElFt.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_ELEV == "YD") rbtnRwyElYd.IsChecked = true;
            //Temperature
            if (MainWindow.APP_SETTINGS.UNIT_TEMP == "C") rbtnTempC.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_TEMP == "F") rbtnTempF.IsChecked = true;
            //Pressure
            if (MainWindow.APP_SETTINGS.UNIT_PRES == "HPA") rbtnPresHpa.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_PRES == "INHG") rbtnPresInhg.IsChecked = true;
            //Visibility
            else if (MainWindow.APP_SETTINGS.UNIT_VISIB == "M") rbtnVisibM.IsChecked = true;
            else if (MainWindow.APP_SETTINGS.UNIT_VISIB == "MI") rbtnVisibMi.IsChecked = true;
            //Show wind on runway
            if (MainWindow.APP_SETTINGS.ShowWindOnRunway) chkShowWindOnRunway.IsChecked = true; else chkShowWindOnRunway.IsChecked = false;
        }

        #region Exceptions and messages
        private void HandleException(Exception ex)
        {
            MessageBox.Show(ex.Message, "Unhandled error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void HandleWarning(string msg)
        {
            MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private void HandleMessage(string msg, string title)
        {
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion
    }
}

/* 
 SETTINGS FILE STRUCTURE: 1 for enabled, 0 for disabled
JEPPESEN=1
LIDO=1
INFO_AVWX=1
OPENAIP=1
JP_USER=
JP_PASSWORD=
LD_USER=
LD_PASSWORD=
AVWX_TOKEN=
UNIT_DIST=
UNIT_RWY=
UNIT_WIND=
UNIT_ELEV=
UNIT_TEMP=
UNIT_PRES=
UNIT_VISIB=
GR_RWY_WIND=
 */