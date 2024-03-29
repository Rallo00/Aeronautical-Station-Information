﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;

namespace ASI
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // SETTINGS
        public static Settings APP_SETTINGS = new Settings();
        private readonly DispatcherTimer TimeClock = new DispatcherTimer();
        private Airport currentAirport = null;
        //Charts-related
        private int CHART_ROTATION = 0;
        private string PATH_SELECTED_CHART = null;
        private Point chartOrigin, chartStart;
        private ChartJeppesen JeppChart = null;
        private ChartLufthansa lidoChart = null;
        private List<ChartJeppesen.Chart> lastDownloadedJeppCharts = new List<ChartJeppesen.Chart>();
        private List<ChartLufthansa.Chart> lastDownloadedLidoCharts = new List<ChartLufthansa.Chart>();
        csharp_metar_decoder.entity.DecodedMetar metarInfo = null;
        public static string IcaoFromBookmarks = null;
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            TimeClock.Tick += new EventHandler(TimeClock_Tick);
            TimeClock.Interval = new TimeSpan(0, 0, 1);
            TimeClock.Start();
            txtCurrentRegion.Text = RegionInfo.CurrentRegion.EnglishName;
            AIRACdetails();
            ClearGUIvalues();
            txbSearchICAO.Focus();
        }

        private void btnSearchICAO_Click(object sender, RoutedEventArgs e)
        {
            string inputData = txbSearchICAO.Text;
            if (inputData.Length != 4)
                HandleWarning("Inserisci un codice ICAO valido.");
            else
            {
                //Clearing old labels
                ClearGUIvalues();
                //Getting station info
                try
                {
                    GetStationInfo(inputData);
                    GetStationMetarTaf(inputData);
                    GetStationCharts(inputData);
                    //Focusing on station search
                    txbSearchICAO.Focus();
                }
                catch (Exception err) { MessageBox.Show("Unable to get station information. Make sure you entered a valid ICAO station. Also check the device internet connection or your proxy settings if you are using one.\n\n" + "Error: " + err.Message + "\n\nCaller: " + err.TargetSite, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        #region Timer for Time
        private void TimeClock_Tick(object sender, EventArgs e)
        {
            //Printing time on left sidebar
            string hour = DateTime.Now.Hour.ToString(), minutes = DateTime.Now.Minute.ToString(), hourUtc = DateTime.UtcNow.Hour.ToString();
            hour = (hour.Length == 1) ? "0" + hour : hour;
            hourUtc = (hourUtc.Length == 1) ? "0" + hourUtc : hourUtc;
            minutes = (minutes.Length == 1) ? "0" + minutes : minutes;
            txtLocalTime.Text = hour + ":" + minutes;
            txtZuluTime.Text = hourUtc + ":" + minutes;
        }
        #endregion

        #region Exceptions and messages
        public static void HandleException(Exception ex)
        {
            MessageBox.Show(ex.Message, "Unhandled error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void HandleWarning(string msg)
        {
            MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public static void HandleMessage(string msg, string title)
        {
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region ToolBar
        private void ToolBarRefresh_Click(object sender, RoutedEventArgs e)
        {
            //Loading
            APP_SETTINGS = new Settings();
            AIRACdetails();
            ClearGUIvalues();
            txbSearchICAO.Focus();
        }
        private void ToolBarChartTurn_Click(object sender, RoutedEventArgs e)
        {
            if (lsbCharts.SelectedIndex != -1)
            {
                TransformedBitmap transformBmp = new TransformedBitmap();
                transformBmp.BeginInit();
                if (tbtnChartDarkMode.IsChecked == true)
                    transformBmp.Source = new BitmapImage(new Uri(ConvertPNGtoDARK(PATH_SELECTED_CHART)));
                else
                    transformBmp.Source = new BitmapImage(new Uri(PATH_SELECTED_CHART));
                RotateTransform transform = new RotateTransform(CHART_ROTATION += 90);
                transformBmp.Transform = transform;
                transformBmp.EndInit();
                imgChart.Source = transformBmp;
            }
        }
        private void ToolBarChartOpen_Click(object sender, RoutedEventArgs e)
        {
            if (lsbCharts.SelectedIndex != -1)
            {
                if (tbtnChartDarkMode.IsChecked == true)
                    System.Diagnostics.Process.Start(ConvertPNGtoDARK(PATH_SELECTED_CHART));
                else
                    System.Diagnostics.Process.Start(PATH_SELECTED_CHART);
            }
        }
        private void ToolBarChartZoomIn_Click(object sender, RoutedEventArgs e)
        {
            Point center = imgChart.TransformToAncestor(chartBorder).Transform(new Point(imgChart.ActualWidth / 2, imgChart.ActualHeight / 2));
            Matrix m = imgChart.RenderTransform.Value;
            m.ScaleAtPrepend(1.2, 1.2, center.X, center.Y);
            imgChart.RenderTransform = new MatrixTransform(m);
        }
        private void ToolBarChartZoomOut_Click(object sender, RoutedEventArgs e)
        {
            Point center = imgChart.TransformToAncestor(chartBorder).Transform(new Point(imgChart.ActualWidth / 2, imgChart.ActualHeight / 2));
            Matrix m = imgChart.RenderTransform.Value;
            m.ScaleAtPrepend(1 / 1.2, 1 / 1.2, center.X, center.Y);
            imgChart.RenderTransform = new MatrixTransform(m);
        }
        private void ToolBarChartFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(MainWindow.APP_SETTINGS.DATA_PATH_CHARTS);
        }
        private void ToolBarSettings_Click(object sender, RoutedEventArgs e)
        {
            WindowOptions preferencesWindow = new WindowOptions();
            preferencesWindow.ShowDialog();
            tabAirportInfo.Focus();
        }
        private void ToolBarFavourite_Click(object sender, RoutedEventArgs e)
        {
            if (currentAirport != null)
            {
                //If already favourite, cancel
                if (currentAirport.IsFavourite)
                {
                    APP_SETTINGS.RemoveFavourite(currentAirport.ICAOCode.ToUpper());
                    if (currentAirport != null)
                        currentAirport.IsFavourite = false;
                    imgToolbarBtnFavourite.Source = new BitmapImage(new Uri(APP_SETTINGS.GetApplicationFolderPath() + "\\Icons\\48x48_bookmark_empty.png", UriKind.Absolute));
                }
                else //Not favourite, add new
                {
                    APP_SETTINGS.AddFavourite(currentAirport.ICAOCode.ToUpper());
                    if (currentAirport != null)
                        currentAirport.IsFavourite = true;
                    imgToolbarBtnFavourite.Source = new BitmapImage(new Uri(APP_SETTINGS.GetApplicationFolderPath() + "\\Icons\\48x48_bookmark_full.png", UriKind.Absolute));
                }
            }
        }
        private void ToolBarFavourites_Click(object sender, RoutedEventArgs e)
        {
            Bookmarks formBookmarks = new Bookmarks();
            formBookmarks.ShowDialog();
            if (IcaoFromBookmarks != null)
            {
                txbSearchICAO.Text = IcaoFromBookmarks;
                IcaoFromBookmarks = null;
                btnSearchICAO_Click(sender, e);

            }
        }
        #endregion

        #region Charts related
        private void lsbCharts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lsbCharts.SelectedIndex != -1 && lsbCharts.SelectedValue.ToString() != "No charts available!")
            {
                string ProcedureCode = lsbCharts.SelectedValue.ToString().Split(':')[0];
                ChartJeppesen.Chart chartJeppToDownload = new ChartJeppesen.Chart();
                ChartLufthansa.Chart chartLidoToDownload = new ChartLufthansa.Chart();

                if (MainWindow.APP_SETTINGS.IsChartServiceJeppesen)
                {
                    foreach (ChartJeppesen.Chart c in lastDownloadedJeppCharts)
                        if (ProcedureCode == c.indexNum)
                        { chartJeppToDownload = c; break; }
                }
                else if (MainWindow.APP_SETTINGS.IsChartServiceLido)
                    foreach (ChartLufthansa.Chart c in lastDownloadedLidoCharts)
                        if (ProcedureCode == c.Name)
                        { chartLidoToDownload = c; break; }
                string fileName = null;
                try
                {
                    if (MainWindow.APP_SETTINGS.IsChartServiceJeppesen)
                        fileName = JeppChart.GetChartFileName(chartJeppToDownload);
                    else if (MainWindow.APP_SETTINGS.IsChartServiceLido)
                        fileName = lidoChart.GetChartFileName(chartLidoToDownload);
                    //Rotating Chart to default
                    imgChart.RenderTransform = new RotateTransform(0, 0, imgChart.Height / 2);
                    //Showing Chart
                    if (tbtnChartDarkMode.IsChecked == true)
                        imgChart.Source = new BitmapImage(new Uri(ConvertPNGtoDARK(fileName), UriKind.Absolute));
                    else
                        imgChart.Source = new BitmapImage(new Uri(fileName, UriKind.Absolute));
                    PATH_SELECTED_CHART = fileName;
                    CHART_ROTATION = 0;
                }
                catch (System.Net.WebException err) { MessageBox.Show("The server might not have the resource you requested.\n" + err.Message, "Server error", MessageBoxButton.OK, MessageBoxImage.Error); }
                catch (Exception err) { MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }
        private void ChartCheckbox_CheckChanged(object sender, RoutedEventArgs e) { ShowSelectedCharts(); }
        private void ShowSelectedCharts()
        {
            if (JeppChart != null || lidoChart != null)
            {
                bool showDeparture = (bool)chkChartDeparture.IsChecked,
                     showArrivals = (bool)chkChartArrival.IsChecked,
                     showAirport = (bool)chkChartAirport.IsChecked,
                     showApproach = (bool)chkChartApproach.IsChecked,
                     showReference = (bool)chkChartAirspace.IsChecked;
                lsbCharts.Items.Clear();
                if(MainWindow.APP_SETTINGS.IsChartServiceJeppesen)
                    foreach (var chart in lastDownloadedJeppCharts)
                    {
                        switch (chart.category.ToUpper())
                        {
                            case "AIRPORT":
                                if (showAirport) lsbCharts.Items.Add(chart.indexNum + ": " + chart.procId);
                                break;
                            case "ARRIVAL":
                                if (showArrivals) lsbCharts.Items.Add(chart.indexNum + ": " + chart.procId);
                                break;
                            case "APPROACH":
                                if (showApproach) lsbCharts.Items.Add(chart.indexNum + ": " + chart.procId);
                                break;
                            case "DEPARTURE":
                                if (showDeparture) lsbCharts.Items.Add(chart.indexNum + ": " + chart.procId);
                                break;
                            default:
                                if (showReference) lsbCharts.Items.Add(chart.indexNum + ": " + chart.procId);
                                break;
                        }
                    }
                else if(MainWindow.APP_SETTINGS.IsChartServiceLido)
                    foreach (var chart in lastDownloadedLidoCharts)
                    {
                        switch (chart.Category.ToUpper())
                        {
                            case "AIRPORT":
                                if (showAirport) lsbCharts.Items.Add(chart.Name);
                                break;
                            case "ARRIVAL":
                                if (showArrivals) lsbCharts.Items.Add(chart.Name);
                                break;
                            case "APPROACH":
                                if (showApproach) lsbCharts.Items.Add(chart.Name);
                                break;
                            case "DEPARTURE":
                                if (showDeparture) lsbCharts.Items.Add(chart.Name);
                                break;
                            default:
                                if (showReference) lsbCharts.Items.Add(chart.Name);
                                break;
                        }
                    }
            }
        }
        private string ConvertPNGtoDARK(string path)
        {
            return System.IO.Path.ChangeExtension(path, "dark.png");
        }
        private void btnChartOpen_Click(object sender, RoutedEventArgs e)
        {
            if (lsbCharts.SelectedIndex != -1)
            {
                if (tbtnChartDarkMode.IsChecked == true)
                    System.Diagnostics.Process.Start(ConvertPNGtoDARK(PATH_SELECTED_CHART));
                else
                    System.Diagnostics.Process.Start(PATH_SELECTED_CHART);
            }
        }
        private void btnChartTurn_Click(object sender, RoutedEventArgs e)
        {
            if (lsbCharts.SelectedIndex != -1)
            {
                TransformedBitmap transformBmp = new TransformedBitmap();
                transformBmp.BeginInit();
                if (tbtnChartDarkMode.IsChecked == true)
                    transformBmp.Source = new BitmapImage(new Uri(ConvertPNGtoDARK(PATH_SELECTED_CHART)));
                else
                    transformBmp.Source = new BitmapImage(new Uri(PATH_SELECTED_CHART));
                RotateTransform transform = new RotateTransform(CHART_ROTATION += 90);
                transformBmp.Transform = transform;
                transformBmp.EndInit();
                imgChart.Source = transformBmp;
            }
        }
        private void imgChart_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(imgChart);
            Matrix m = imgChart.RenderTransform.Value;
            if (e.Delta > 0)
                m.ScaleAtPrepend(1.2, 1.2, p.X, p.Y);
            else
                m.ScaleAtPrepend(1 / 1.2, 1 / 1.2, p.X, p.Y);
            imgChart.RenderTransform = new MatrixTransform(m);
        }
        private void imgChart_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (imgChart.IsMouseCaptured) return;
            imgChart.CaptureMouse();

            chartStart = e.GetPosition(chartBorder);
            chartOrigin.X = imgChart.RenderTransform.Value.OffsetX;
            chartOrigin.Y = imgChart.RenderTransform.Value.OffsetY;
        }
        private void imgChart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            imgChart.ReleaseMouseCapture();
            chartOrigin = chartStart;
        }
        private void imgChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!imgChart.IsMouseCaptured) return;
                Point p = e.MouseDevice.GetPosition(chartBorder);
                Matrix m = imgChart.RenderTransform.Value;
                m.OffsetX = chartOrigin.X + (p.X - chartStart.X);
                m.OffsetY = chartOrigin.Y + (p.Y - chartStart.Y);
                imgChart.RenderTransform = new MatrixTransform(m);
            }
        }
        #endregion

        #region Custom GUI Functions
        private void txtWebsite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Go to airport website
            try { System.Diagnostics.Process.Start(txtWebsite.Text.ToString()); } catch { }
        }
        private void txbSearchICAO_GotFocus(object sender, RoutedEventArgs e)
        {
            txbSearchICAO.Clear();
            txbSearchICAO.FontFamily = new FontFamily("Consolas");
            txbSearchICAO.FontStyle = FontStyles.Normal;
            txbSearchICAO.FontSize = 24;
        }
        private void txbSearchICAO_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txbSearchICAO.Text.Length == 0)
            {
                txbSearchICAO.FontFamily = new FontFamily("Segoe UI");
                txbSearchICAO.FontStyle = FontStyles.Italic;
                txbSearchICAO.FontSize = 12;
                txbSearchICAO.Text = "Search here...";
            }
        }
        private void ClearGUIvalues()
        {
            //Clearing airport data
            txtAirportTitle.Text = "Search a station to begin";
            txtATIS.Text = txtCity.Text = txtCountry.Text = txtElevation.Text = txtIATA.Text = txtLatitude.Text = txtLongitude.Text = txtRealBearing.Text = txtSurface.Text = txtLength.Text = txtNotes.Text = txtWebsite.Text = txtWidth.Text = "";
            cbxRunways.Items.Clear();
            //Clearing metar/taf data
            txbMetar.Clear(); txbTaf.Clear();
            //Clearing weather data
            txtWeatherClouds.Text = txtWeatherDewPoint.Text = txtWeatherPressure.Text = txtWeatherTemperature.Text = txtWeatherTime.Text = txtWeatherVisibility.Text = txtWeatherWind.Text = "";
            //Clearing frequencies data
            grdFrequencies.ItemsSource = null;
            //Clearing charts data
            chkChartAirport.IsChecked = true;
            chkChartAirspace.IsChecked = false;
            chkChartApproach.IsChecked = false;
            chkChartArrival.IsChecked = false;
            chkChartDeparture.IsChecked = false;
            //Clear runway rectangle
            runwayRectangle.Visibility = Visibility.Hidden;
            runwayRectangle.RenderTransform = new RotateTransform(0);
            txtRwy1.Visibility = Visibility.Hidden;
            txtRwy2.Visibility = Visibility.Hidden;
            imgWind.Source = null;
            //Clear metar
            metarInfo = null;
            //Focus on search
            txbSearchICAO.Focus();
        }
        private void txbSearchICAO_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                btnSearchICAO_Click(sender, e);
        }
        private void cbxRunways_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxRunways.SelectedIndex != -1)
            {
                //GESTIRE DIVERSE RUNWAY
                if (APP_SETTINGS.IsInformationAVWX || APP_SETTINGS.IsInformationOpenAip)
                {
                    //Handling runway information based on selected runway in listbox
                    string runwayIdent1 = cbxRunways.SelectedValue.ToString().Split('/')[0];
                    //Selecting the correct runway to show properties
                    Runway selectedRunway = null;
                    foreach (Runway r in currentAirport.Runways)
                        if (r.Identification1 == runwayIdent1)
                            selectedRunway = r;
                    txtRealBearing.Text = $"{selectedRunway.Bearing1}°/{selectedRunway.Bearing2}°";
                    //Showing runway length
                    if (MainWindow.APP_SETTINGS.UNIT_RWY == "M")
                        txtLength.Text = $"{selectedRunway.RwyLength} m";
                    else if (MainWindow.APP_SETTINGS.UNIT_RWY == "FT")
                        txtLength.Text = $"{MetersToFeet(selectedRunway.RwyLength)} ft";
                    else if (MainWindow.APP_SETTINGS.UNIT_RWY == "YD")
                        txtLength.Text = $"{MetersToYards(FeetToMeters(selectedRunway.RwyLength))} yd";
                    else
                        txtLength.Text = selectedRunway.RwyLength.ToString();
                    //Showing runway width
                    if (selectedRunway.RwyWidth != -1)
                    {
                        if (MainWindow.APP_SETTINGS.UNIT_RWY == "M")
                            txtWidth.Text = $"{selectedRunway.RwyWidth} m";
                        else if (MainWindow.APP_SETTINGS.UNIT_RWY == "FT")
                            txtWidth.Text = $"{MetersToFeet(selectedRunway.RwyWidth)} ft";
                        else if (MainWindow.APP_SETTINGS.UNIT_RWY == "YD")
                            txtWidth.Text = $"{MetersToYards(selectedRunway.RwyWidth)} yd";
                    }
                    else
                        txtWidth.Text = "Not available";
                    txtSurface.Text = char.ToUpper(selectedRunway.Surface[0]) + selectedRunway.Surface.Substring(1);
                    //Drawing runway
                    double heading = double.Parse(selectedRunway.Bearing1.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    double bearing2 = double.Parse(selectedRunway.Bearing2.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    DrawRunway(heading, selectedRunway.Identification1, selectedRunway.Identification2);
                }
            }
        }
        private void DrawRunway(double heading, string R1, string R2)
        {
            runwayRectangle.Visibility = Visibility.Visible;
            txtRwy1.Visibility = Visibility.Visible;
            txtRwy2.Visibility = Visibility.Visible;
            txtRwy1.Content = R1;
            txtRwy2.Content = R2;
            runwayGrid.RenderTransform = new RotateTransform(heading+180);
            //Wind arrow on runway drawing
            if (MainWindow.APP_SETTINGS.ShowWindOnRunway && metarInfo != null)
            {
                //Showing arrow
                if (metarInfo.SurfaceWind.VariableDirection != true)
                {
                    imgWind.Source = new BitmapImage(new Uri("Icons/arrow3.png", UriKind.Relative));
                    imgWind.RenderTransform = new RotateTransform(Convert.ToInt16(metarInfo.SurfaceWind.MeanDirection.ActualValue) - heading);
                }
            }
        }
        #endregion

        #region Getting information
        private async void AIRACdetails()
        {
            try { txtCurrentAirac.Text = await AVWXLib.GetCurrentAiracCycle(); }
            catch { txtCurrentAirac.Text = "Not avail."; }
            try { string AIRACexpiry = await AVWXLib.GetCurrentAiracExpiryDate(); txtCurrentAiracExpDate.Text = AIRACexpiry.Split('T')[0]; }
            catch { txtCurrentAiracExpDate.Text = "Not avail."; }
        }
        private async void GetStationInfo(string icao)
        {
            try
            {
                if (APP_SETTINGS.IsInformationAVWX)
                    currentAirport = new Airport(await AVWXLib.GetStationInfo(icao, MainWindow.APP_SETTINGS.AVWX_TOKEN));
                else if (APP_SETTINGS.IsInformationOpenAip)
                    currentAirport = new Airport(await OpenAipLib.GetAirportInfo(icao, MainWindow.APP_SETTINGS.OPENAIP_TOKEN));
                //Showing station information
                if (currentAirport != null)
                {
                    txtAirportTitle.Text = currentAirport.Name;
                    txtCity.Text = currentAirport.City;
                    txtCountry.Text = currentAirport.Country;
                    if (MainWindow.APP_SETTINGS.UNIT_ELEV == "M")
                        txtElevation.Text = currentAirport.Elevation + " m";
                    else if (MainWindow.APP_SETTINGS.UNIT_ELEV == "FT")
                        txtElevation.Text = MetersToFeet(currentAirport.Elevation) + " ft";
                    else if (MainWindow.APP_SETTINGS.UNIT_ELEV == "YD")
                        txtElevation.Text = MetersToYards(Convert.ToInt16(currentAirport.Elevation)) + " yd";
                    txtIATA.Text = currentAirport.IATACode;
                    txtLatitude.Text = $"{currentAirport.Latitude} °N";
                    txtLongitude.Text = $"{currentAirport.Longitude} °N";
                    txtNotes.Text = currentAirport.Notes;
                    txtWebsite.Text = currentAirport.Website;
                    //Showing Runway info
                    foreach (Runway r in currentAirport.Runways)
                        cbxRunways.Items.Add(r.Identification1 + "/" + r.Identification2);
                    cbxRunways.SelectedIndex = 0;
                }
                //Getting and showing ATIS
                if (APP_SETTINGS.IsAtisIVAO)
                {
                    try
                    {
                        IVAOAtisLib.Atis airportAtis = await IVAOAtisLib.GetAtis(icao);
                        currentAirport.ATIS = airportAtis.Value;
                        txtATIS.Text = airportAtis.Value;
                        txtAtisCallsign.Text = airportAtis.Callsign;
                        txtAtisRevision.Text = airportAtis.Revision;
                        txtAtisTimestamp.Text = $"{airportAtis.Timestamp.Hour}:{airportAtis.Timestamp.Minute}";
                        txtAtisTimestampUTC.Text = $"{airportAtis.TimestampZulu.Hour}:{airportAtis.TimestampZulu.Minute}";
                    }
                    catch (Exception ex) { HandleWarning("Unable to get ATIS from IVAO\n\nError:\n" + ex.Message); }
                }
                else
                    txtATIS.Text = "ATIS is not enabled. Check your settings.";
                //Getting and showing frequencies
                if (APP_SETTINGS.IsFrequenciesOpenAip)
                {
                    try
                    {
                        Airport tempAirport = new Airport(await OpenAipLib.GetAirportInfo(icao, MainWindow.APP_SETTINGS.OPENAIP_TOKEN));
                        if (tempAirport.Frequencies != null)
                            grdFrequencies.ItemsSource = tempAirport.Frequencies;
                    }
                    catch (Exception) { MessageBox.Show("An error occurred while searching for frequencies in use. Your request has been closed by OpenAIP server.", "Errore", MessageBoxButton.OK, MessageBoxImage.Asterisk); }
                }
            }
            catch (System.Net.WebException) { HandleWarning("Please insert a valid ICAO code."); }
            catch (Exception ex) { HandleException(ex); }
            //Handle favourite (bookmark)
            if (APP_SETTINGS.Bookmarks != null)
                foreach (string s in APP_SETTINGS.Bookmarks)
                    if (s == icao.ToUpper())
                    {
                        currentAirport.IsFavourite = true;
                        imgToolbarBtnFavourite.Source = new BitmapImage(new Uri(APP_SETTINGS.GetApplicationFolderPath() + "\\Icons\\48x48_bookmark_full.png", UriKind.Absolute));
                    }
                    else
                        imgToolbarBtnFavourite.Source = new BitmapImage(new Uri(APP_SETTINGS.GetApplicationFolderPath() + "\\Icons\\48x48_bookmark_empty.png", UriKind.Absolute));
        }
        private async void GetStationMetarTaf(string icao)
        {
            //Getting station METAR
            try 
            {
                if (APP_SETTINGS.IsWeatherAVWX)
                    { txbMetar.Text = await AVWXLib.GetMetar(icao, MainWindow.APP_SETTINGS.AVWX_TOKEN); ParseMetar(txbMetar.Text); }
                else if (APP_SETTINGS.IsWeatherNOAA)
                    { txbMetar.Text = await NOAALib.GetStationMetar(icao); ParseMetar(txbMetar.Text); }
                else
                    txbMetar.Text = "Weather info have been disabled in the settings.";
            }
            catch { txbMetar.Text = "METAR Information not available. Station might not provide weather information."; }
            //Getting station TAF
            try 
            { 
                if(APP_SETTINGS.IsWeatherAVWX)
                    txbTaf.Text = await AVWXLib.GetTaf(icao, MainWindow.APP_SETTINGS.AVWX_TOKEN); 
                else if (APP_SETTINGS.IsWeatherNOAA)
                    txbTaf.Text = await NOAALib.GetStationTaf(icao);
                else
                    txbTaf.Text = "Weather info have been disabled in the settings.";
            }
            catch { txbTaf.Text = "TAF Information not available. Station might not provide weather information"; }
        }
        private void GetStationCharts(string icao)
        {
            if (MainWindow.APP_SETTINGS.IsChartServiceJeppesen) JeppChart = new ChartJeppesen(MainWindow.APP_SETTINGS.JP_USER, MainWindow.APP_SETTINGS.JP_PASSWORD, MainWindow.APP_SETTINGS.DATA_PATH_CHARTS);
            else if (MainWindow.APP_SETTINGS.IsChartServiceLido)
            {
                lidoChart = new ChartLufthansa(MainWindow.APP_SETTINGS.LD_USER, MainWindow.APP_SETTINGS.LD_PASSWORD);
                while (true) { try { lidoChart.SetConfig(lidoChart.SystemConfig); break; } catch (ArgumentException) { } }
            }

            if (JeppChart != null && MainWindow.APP_SETTINGS.IsChartServiceJeppesen)
            {
                var task = Task.Run(() =>
                {
                    lastDownloadedJeppCharts = JeppChart.GetAirportCharts(icao, false);
                });
                task.ContinueWith((t) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ShowSelectedCharts();
                    });
                });
            }
            else if (lidoChart != null && MainWindow.APP_SETTINGS.IsChartServiceLido)
            {
                var task = Task.Run(() =>
                {
                    try { lastDownloadedLidoCharts = lidoChart.GetAirportCharts(icao, false); }
                    catch (Exception ex) { HandleException(ex); }
                });
                task.ContinueWith((t) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ShowSelectedCharts();
                    });
                });
            }
        }
        private void ParseMetar(string metar)
        {
            csharp_metar_decoder.MetarDecoder metarObj = new csharp_metar_decoder.MetarDecoder();
            metarInfo = metarObj.Parse(txbMetar.Text);

            //Parsing metar data
            if (metarInfo != null)
            {
                
                //Metar Date/Time
                txtWeatherTime.Text = (metarInfo.Day < 10) ? $"{metarInfo.Day}/{DateTime.Now.Month} at {metarInfo.Time}" : $"{metarInfo.Day}/{DateTime.Now.Month} at {metarInfo.Time}";
                //Metar pressure
                if (metarInfo.Pressure != null)
                {
                    if (MainWindow.APP_SETTINGS.UNIT_PRES == "HPA" && metarInfo.Pressure.ActualUnit.ToString().ToLower() == "hectopascal")
                        txtWeatherPressure.Text = metarInfo.Pressure.ActualValue.ToString() + " hpa";
                    else if (MainWindow.APP_SETTINGS.UNIT_PRES == "HPA" && metarInfo.Pressure.ActualUnit.ToString().ToLower() == "mercuryinch")
                        txtWeatherPressure.Text = INHGtoHPA(Convert.ToDouble(metarInfo.Pressure.ActualValue)) + " hpa";
                    else if (MainWindow.APP_SETTINGS.UNIT_PRES == "INHG" && metarInfo.Pressure.ActualUnit.ToString().ToLower() == "mercuryinch")
                        txtWeatherPressure.Text = metarInfo.Pressure.ActualValue.ToString() + " inhg";
                    else if (MainWindow.APP_SETTINGS.UNIT_PRES == "INHG" && metarInfo.Pressure.ActualUnit.ToString().ToLower() == "hectopascal")
                        txtWeatherPressure.Text = HPAtoINHG(int.Parse(metarInfo.Pressure.ActualValue.ToString())) + " inhg";
                }
                //Metar temperature
                if (metarInfo.AirTemperature != null)
                {
                    if (MainWindow.APP_SETTINGS.UNIT_TEMP == "C" && metarInfo.AirTemperature.ActualUnit.ToString().ToLower() == "degreecelsius")
                        txtWeatherTemperature.Text = metarInfo.AirTemperature.ActualValue.ToString() + " °C";
                    else if (MainWindow.APP_SETTINGS.UNIT_TEMP == "C" && metarInfo.AirTemperature.ActualUnit.ToString().ToLower() == "degreefahrenheit")
                        txtWeatherTemperature.Text = FahrenheitToCelsius(Convert.ToInt16(metarInfo.AirTemperature.ActualValue)) + " °C";
                    else if (MainWindow.APP_SETTINGS.UNIT_TEMP == "F" && metarInfo.AirTemperature.ActualUnit.ToString().ToLower() == "degreefahrenheit")
                        txtWeatherTemperature.Text = metarInfo.AirTemperature.ActualValue.ToString() + " °F";
                    else if (MainWindow.APP_SETTINGS.UNIT_TEMP == "F" && metarInfo.AirTemperature.ActualUnit.ToString().ToLower() == "degreecelsius")
                        txtWeatherTemperature.Text = CelsiusToFahrenheit(Convert.ToInt16(metarInfo.AirTemperature.ActualValue)) + " °F";
                }
                //Metar dew point
                if (metarInfo.DewPointTemperature != null)
                {
                    if (MainWindow.APP_SETTINGS.UNIT_TEMP == "C" && metarInfo.DewPointTemperature.ActualUnit.ToString().ToLower() == "degreecelsius")
                        txtWeatherDewPoint.Text = metarInfo.DewPointTemperature.ActualValue.ToString() + " °C";
                    else if (MainWindow.APP_SETTINGS.UNIT_TEMP == "C" && metarInfo.DewPointTemperature.ActualUnit.ToString().ToLower() == "degreefahrenheit")
                        txtWeatherDewPoint.Text = FahrenheitToCelsius(Convert.ToInt16(metarInfo.DewPointTemperature.ActualValue)) + " °C";
                    else if (MainWindow.APP_SETTINGS.UNIT_TEMP == "F" && metarInfo.DewPointTemperature.ActualUnit.ToString().ToLower() == "degreefahrenheit")
                        txtWeatherDewPoint.Text = metarInfo.DewPointTemperature.ActualValue.ToString() + " °F";
                    else if (MainWindow.APP_SETTINGS.UNIT_TEMP == "F" && metarInfo.DewPointTemperature.ActualUnit.ToString().ToLower() == "degreecelsius")
                        txtWeatherDewPoint.Text = CelsiusToFahrenheit(Convert.ToInt16(metarInfo.DewPointTemperature.ActualValue)) + " °F";
                }
                //Metar surface wind
                string wind = null;
                if (metarInfo.SurfaceWind.MeanSpeed.ActualValue != 0)
                {
                    //Direction
                    if (metarInfo.SurfaceWind.VariableDirection && metarInfo.SurfaceWind.DirectionVariations != null)
                        wind += $"From {metarInfo.SurfaceWind.DirectionVariations.ToString().Split('V')[0]} to {metarInfo.SurfaceWind.DirectionVariations.ToString().Split('V')[1]} ";
                    else if (metarInfo.SurfaceWind.VariableDirection && metarInfo.SurfaceWind.DirectionVariations == null)
                        wind += "Variable ";
                    else
                        wind += $"From {metarInfo.SurfaceWind.MeanDirection.ActualValue}° ";
                    //Speed
                    if (MainWindow.APP_SETTINGS.UNIT_WIND == "KTS" && metarInfo.SurfaceWind.MeanSpeed.ActualUnit.ToString().ToLower() == "knot")
                        wind += "at " + metarInfo.SurfaceWind.MeanSpeed.ActualValue + " kts";
                    else if (MainWindow.APP_SETTINGS.UNIT_WIND == "MS" && metarInfo.SurfaceWind.MeanSpeed.ActualUnit.ToString().ToLower() == "knot")
                        wind += "at " + KTStoMS(Convert.ToInt16(metarInfo.SurfaceWind.MeanSpeed.ActualValue)) + " m/s";
                    else if (MainWindow.APP_SETTINGS.UNIT_WIND == "KPH" && metarInfo.SurfaceWind.MeanSpeed.ActualUnit.ToString().ToLower() == "knot")
                        wind += "at " + MStoKMH(KTStoMS(Convert.ToInt16(metarInfo.SurfaceWind.MeanSpeed.ActualValue))) + " kph";
                    else if (MainWindow.APP_SETTINGS.UNIT_WIND == "MPH" && metarInfo.SurfaceWind.MeanSpeed.ActualUnit.ToString().ToLower() == "knot")
                        wind += "at " + MStoMPH(KTStoMS(Convert.ToInt16(metarInfo.SurfaceWind.MeanSpeed.ActualValue))) + " mph";
                    /*if (metarInfo.SurfaceWind.SpeedVariations == null) wind += $"at {metarInfo.SurfaceWind.MeanSpeed}";
                    else 
                        wind += $"at {metarInfo.SurfaceWind.SpeedVariations}";*/
                    txtWeatherWind.Text = wind;
                }
                else 
                    txtWeatherWind.Text = "No wind";
                //Metar Visibility
                if (metarInfo.Cavok)
                    txtWeatherVisibility.Text = "Ceiling and Visibility OK";
                else
                {
                    if (MainWindow.APP_SETTINGS.UNIT_VISIB == "M" && metarInfo.Visibility.PrevailingVisibility.ActualUnit.ToString().ToLower() == "meter")
                        txtWeatherVisibility.Text = metarInfo.Visibility.PrevailingVisibility.ActualValue + " m";
                    else if (MainWindow.APP_SETTINGS.UNIT_VISIB == "M" && metarInfo.Visibility.PrevailingVisibility.ActualUnit.ToString().ToLower() == "statutemile")
                        txtWeatherVisibility.Text = MilesToMeters(metarInfo.Visibility.PrevailingVisibility.ActualValue) + " m";
                    else if (MainWindow.APP_SETTINGS.UNIT_VISIB == "MI" && metarInfo.Visibility.PrevailingVisibility.ActualUnit.ToString().ToLower() == "statutemile")
                        txtWeatherVisibility.Text = metarInfo.Visibility.PrevailingVisibility.ActualValue + " mi";
                    else if (MainWindow.APP_SETTINGS.UNIT_VISIB == "M" && metarInfo.Visibility.PrevailingVisibility.ActualUnit.ToString().ToLower() == "kilometers")
                        txtWeatherVisibility.Text = Convert.ToInt16(metarInfo.Visibility.PrevailingVisibility.ActualValue*1000) + " m";
                }
                //Metar clouds layer
                string clouds = null;
                if (metarInfo.Clouds.Count > 0)
                    for (int i = 0; i < metarInfo.Clouds.Count; i++)
                        clouds += $"{metarInfo.Clouds[i].Amount} at {metarInfo.Clouds[i].BaseHeight}\n";
                txtWeatherClouds.Text = (metarInfo.Clouds.Count > 0) ? clouds : "No clouds";
            }
        }
        #endregion

        #region Units conversion
        private string MetersToFeet(int value)
        {
            double inft, convert, CONSTANT = 0.3048;
            int ft, inchesLeft;
            convert = double.Parse(value.ToString());
            inft = convert / CONSTANT;
            ft = (int)inft;
            double temp = (inft - Math.Truncate(inft)) / 0.8333;
            inchesLeft = (int)temp;
            if(inchesLeft == 0)
                return $"{ft}";
            else
                return $"{ft}/{inchesLeft}";
        }
        private int FeetToMeters(int value) { return Convert.ToInt16(value / 3.2808399); }
        private int MetersToYards(int value) { return Convert.ToInt16(value * 1.09361); }
        private int YardsToMeters(int value) { return Convert.ToInt16(value / 1.09361); }
        private double MetersToNauticalMiles(int value) { return Math.Round(value * 0.0005399568035, 1); }
        private int NauticalMilesToMeters(int value) { return Convert.ToInt16(Math.Round(value / 0.0005399568035, 1)); }
        private double NauticalMilesToMiles(int value) { return Math.Round(value * 1.15078, 1); }
        private double MilesToNauticalMiles(int value) { return Math.Round(value * 0.86898, 1); }
        private int MilesToMeters(double value) { return Convert.ToInt16(Math.Round(value / 0.0000621,2)); }
        private double MetersToMiles(int value) { return Math.Round(value * 0.0000621, 2); }
        private int MStoKMH(int value) { return Convert.ToInt16(3.6 * value); }
        private int KMHtoMS(int value) { return Convert.ToInt16(3.6 / value); }
        private int MStoMPH(int value) { return Convert.ToInt16(0.277778 * value); }
        private int MPHtoMS(int value) { return Convert.ToInt16(0.277778 / value); }
        private int MStoKTS(int value) { return Convert.ToInt16(value * 1.9438444924574); }
        private int KTStoMS(int value) { return Convert.ToInt16(value * 0.5144444444310); }
        private int CelsiusToFahrenheit(int value) { return Convert.ToInt16(((value * 18) / 10) + 32); }
        private int FahrenheitToCelsius(int value) { return Convert.ToInt16(((value - 32) * 5) / 9); }
        private double HPAtoINHG(int value) { return Convert.ToDouble((Math.Round(value * 0.02953, 2))); }
        private int INHGtoHPA(double value) { return Convert.ToInt16((Math.Round(value / 0.02953, 2))); }
        #endregion
    }
}
