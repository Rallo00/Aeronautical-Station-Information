using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace ASI
{
    public class Settings
    {
        // DATA SAVING PATH
        public readonly string DATA_PATH_ROOT = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ASI\\",
                               DATA_PATH_SETTINGS = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ASI\\Settings.txt",
                               DATA_PATH_CHARTS = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ASI\\Charts\\",
                               DATA_PATH_DATABASE = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ASI\\db.sqlite";
        // SERVICES
        public bool IsInformationAVWX { get; set; }
        public bool IsInformationOpenAip { get; set; }
        public bool IsChartServiceJeppesen { get; set; }
        public bool IsChartServiceLido { get; set; }
        public bool IsWeatherAVWX { get; set; }
        public bool IsWeatherIVAO { get; set; }
        public bool IsWeatherNOAA { get; set; }
        public bool IsFrequenciesOpenAip { get; set; }
        public bool IsAtisIVAO { get; set; }
        // CREDENTIALS
        public string JP_USER { get; set; }
        public string JP_PASSWORD { get; set; }
        public string LD_USER { get; set; }
        public string LD_PASSWORD { get; set; }
        public string AVWX_TOKEN { get; set; }
        public string OPENAIP_TOKEN { get; set; }
        // UNITS OF MEASUREMENT
        public string UNIT_DIST { get; set; }
        public string UNIT_RWY { get; set; }
        public string UNIT_WIND { get; set; }
        public string UNIT_ELEV { get; set; }
        public string UNIT_TEMP { get; set; }
        public string UNIT_PRES { get; set; }
        public string UNIT_VISIB { get; set; }
        // GRAPHIC SETTINGS
        public bool ShowWindOnRunway { get; set; }
        //FAVOURITES LIST
        public List<string> Favourites { get; set; }

        public Settings()
        {
            CheckDirectoryStructure();
            LoadSettings();
            LoadFavourites();
            //Default graphics ---- add it to database
            ShowWindOnRunway = true;
        }

        private void CheckDirectoryStructure()
        {
            if (!System.IO.Directory.Exists(DATA_PATH_ROOT))
                System.IO.Directory.CreateDirectory(DATA_PATH_ROOT);
            if (!System.IO.Directory.Exists(DATA_PATH_CHARTS))
                System.IO.Directory.CreateDirectory(DATA_PATH_CHARTS);
            if (!System.IO.File.Exists(DATA_PATH_DATABASE))
            {
                SQLiteConnection dbCon = null;
                try { dbCon = new SQLiteConnection("URI=file:" + DATA_PATH_DATABASE); dbCon.Open(); CreateDefaultDatabase(); }
                catch (Exception ex) { MainWindow.HandleException(ex); }
                finally { if (dbCon != null) dbCon.Close(); }
            }
        }
        private void CreateDefaultDatabase()
        {
            string scriptPath = System.IO.Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Backup\\db_create.sql";
            RunDatabaseScriptFromFile(scriptPath);
        }
        private void RunDatabaseScriptFromFile(string filePath)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(filePath);
            string scriptContent = reader.ReadToEnd();
            string[] instructions = System.Text.RegularExpressions.Regex.Split(scriptContent, "GO");

            SQLiteConnection dbCon = null;
            try
            {
                dbCon = new SQLiteConnection("URI=file:" + DATA_PATH_DATABASE);
                dbCon.Open();
                foreach (string s in instructions)
                {
                    string cleanedUpInstruction = s.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                    SQLiteCommand cmd = new SQLiteCommand(cleanedUpInstruction, dbCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            catch (Exception ex) { MainWindow.HandleException(ex); }
            finally { if (dbCon != null) dbCon.Close(); }
        }
        public void SaveSettings()
        {
            SQLiteConnection dbCon = null;
            dbCon = new SQLiteConnection("URI=file:" + DATA_PATH_DATABASE);
            SQLiteCommand cmd = null;
            try
            {
                dbCon.Open();
                //--- SAVING CREDENTIALS ---
                cmd = new SQLiteCommand($"UPDATE Credentials SET username = NULL, password = '{AVWX_TOKEN}' WHERE service_id = 1", dbCon);
                cmd.ExecuteNonQuery();
                cmd = new SQLiteCommand($"UPDATE Credentials SET username = NULL, password = '{OPENAIP_TOKEN}' WHERE service_id = 2", dbCon);
                cmd.ExecuteNonQuery();
                cmd = new SQLiteCommand($"UPDATE Credentials SET username = '{JP_USER}', password = '{JP_PASSWORD}' WHERE service_id = 3", dbCon);
                cmd.ExecuteNonQuery();
                cmd = new SQLiteCommand($"UPDATE Credentials SET username = '{LD_USER}', password = '{LD_PASSWORD}' WHERE service_id = 4", dbCon);
                cmd.ExecuteNonQuery();
                //--- SAVING UNITS ---
                //Saving unit distance
                if (UNIT_DIST == "M")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 1 WHERE description = 'DISTANCE'", dbCon);
                else if (UNIT_DIST == "FT")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 2 WHERE description = 'DISTANCE'", dbCon);
                else if (UNIT_DIST == "YD")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 5 WHERE description = 'DISTANCE'", dbCon);
                else if (UNIT_DIST == "NM")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 4 WHERE description = 'DISTANCE'", dbCon);
                cmd.ExecuteNonQuery();
                //Saving wind speed
                if (UNIT_WIND == "MPS")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 10 WHERE description = 'WINDSPEED'", dbCon);
                else if (UNIT_WIND == "KPH")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 11 WHERE description = 'WINDSPEED'", dbCon);
                else if (UNIT_WIND == "MPH")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 12 WHERE description = 'WINDSPEED'", dbCon);
                else if (UNIT_WIND == "KTS")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 13 WHERE description = 'WINDSPEED'", dbCon);
                cmd.ExecuteNonQuery();
                //Saving temperature
                if (UNIT_TEMP == "C")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 6 WHERE description = 'TEMPERATURE'", dbCon);
                else if (UNIT_TEMP == "F")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 7 WHERE description = 'TEMPERATURE'", dbCon);
                cmd.ExecuteNonQuery();
                //Saving pressure
                if (UNIT_PRES == "HPA")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 8 WHERE description = 'PRESSURE'", dbCon);
                else if (UNIT_PRES == "INHG")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 9 WHERE description = 'PRESSURE'", dbCon);
                cmd.ExecuteNonQuery();
                //Saving runway distance
                if (UNIT_RWY == "M")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 1 WHERE description = 'RWYDISTANCE'", dbCon);
                else if (UNIT_RWY == "FT")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 2 WHERE description = 'RWYDISTANCE'", dbCon);
                else if (UNIT_RWY == "YD")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 5 WHERE description = 'RWYDISTANCE'", dbCon);
                cmd.ExecuteNonQuery();
                //Saving runway elevation
                if (UNIT_ELEV == "M")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 1 WHERE description = 'RWYELEVATION'", dbCon);
                else if (UNIT_ELEV == "FT")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 2 WHERE description = 'RWYELEVATION'", dbCon);
                else if (UNIT_ELEV == "YD")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 5 WHERE description = 'RWYELEVATION'", dbCon);
                cmd.ExecuteNonQuery();
                //Saving visibility
                if (UNIT_VISIB == "M")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 1 WHERE description = 'VISIBILITY'", dbCon);
                else if (UNIT_VISIB == "MI")
                    cmd = new SQLiteCommand($"UPDATE Units SET uom_id = 3 WHERE description = 'VISIBILITY'", dbCon);
                cmd.ExecuteNonQuery();
                //--- SAVING SERVICES ---
                if (!IsInformationAVWX && !IsInformationOpenAip) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 0 WHERE description = 'INFO'", dbCon);
                else if (IsInformationAVWX) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 1 WHERE description = 'INFO'", dbCon);
                else if (IsInformationOpenAip) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 2 WHERE description = 'INFO'", dbCon);
                cmd.ExecuteNonQuery();
                if (!IsChartServiceJeppesen && !IsChartServiceLido) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 0 WHERE description = 'CHARTS'", dbCon);
                else if (IsChartServiceJeppesen) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 3 WHERE description = 'CHARTS'", dbCon);
                else if (IsChartServiceLido) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 4 WHERE description = 'CHARTS'", dbCon);
                cmd.ExecuteNonQuery();
                if (!IsWeatherNOAA && !IsWeatherAVWX) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 0 WHERE description = 'WEATHER'", dbCon);
                else if (IsWeatherAVWX) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 1 WHERE description = 'WEATHER'", dbCon);
                else if (IsWeatherNOAA) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 5 WHERE description = 'WEATHER'", dbCon);
                cmd.ExecuteNonQuery();
                if (!IsFrequenciesOpenAip) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 0 WHERE description = 'FREQUENCIES'", dbCon);
                else if (IsFrequenciesOpenAip) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 2 WHERE description = 'FREQUENCIES'", dbCon);
                cmd.ExecuteNonQuery();
                if (!IsAtisIVAO) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 0 WHERE description = 'ATIS'", dbCon);
                else if (IsAtisIVAO) cmd = new SQLiteCommand($"UPDATE Integrations SET service_id = 6 WHERE description = 'ATIS'", dbCon);
                cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
            catch (Exception ex) { MainWindow.HandleException(ex); }
            finally { if (dbCon != null) dbCon.Close(); }
        }
        private void LoadSettings()
        {
            SQLiteConnection dbCon = null;
            dbCon = new SQLiteConnection("URI=file:" + DATA_PATH_DATABASE);
            SQLiteDataReader sdr = null;
            try
            {
                dbCon.Open();
                //--- LOADING CREDENTIALS ---
                JP_USER = new SQLiteCommand("SELECT username FROM Credentials c INNER JOIN Services s ON s.id = c.service_id WHERE s.description = 'JEPPESEN'", dbCon).ExecuteScalar().ToString();
                JP_PASSWORD = new SQLiteCommand("SELECT password FROM Credentials c INNER JOIN Services s ON s.id = c.service_id WHERE s.description = 'JEPPESEN'", dbCon).ExecuteScalar().ToString();
                LD_USER = new SQLiteCommand("SELECT username FROM Credentials c INNER JOIN Services s ON s.id = c.service_id WHERE s.description = 'LIDO'", dbCon).ExecuteScalar().ToString();
                LD_PASSWORD = new SQLiteCommand("SELECT password FROM Credentials c INNER JOIN Services s ON s.id = c.service_id WHERE s.description = 'LIDO'", dbCon).ExecuteScalar().ToString();
                AVWX_TOKEN = new SQLiteCommand("SELECT password FROM Credentials c INNER JOIN Services s ON s.id = c.service_id WHERE s.description = 'AVWX'", dbCon).ExecuteScalar().ToString();
                OPENAIP_TOKEN = new SQLiteCommand("SELECT password FROM Credentials c INNER JOIN Services s ON s.id = c.service_id WHERE s.description = 'OPENAIP'", dbCon).ExecuteScalar().ToString();

                //--- LOADING INTEGRATIONS ---
                string integrationInfo, integrationChart, integrationWeather, integrationFrequencies, integrationAtis;
                try { integrationInfo = new SQLiteCommand("SELECT s.description FROM Services s INNER JOIN Integrations i ON s.id = i.service_id WHERE i.description = 'INFO'", dbCon).ExecuteScalar().ToString(); }
                catch { integrationInfo = "0"; }
                try { integrationChart = new SQLiteCommand("SELECT s.description FROM Services s INNER JOIN Integrations i ON s.id = i.service_id WHERE i.description = 'CHARTS'", dbCon).ExecuteScalar().ToString(); }
                catch { integrationChart = "0"; }
                try { integrationWeather = new SQLiteCommand("SELECT s.description FROM Services s INNER JOIN Integrations i ON s.id = i.service_id WHERE i.description = 'WEATHER'", dbCon).ExecuteScalar().ToString(); }
                catch { integrationWeather = "0"; }
                try { integrationFrequencies = new SQLiteCommand("SELECT s.description FROM Services s INNER JOIN Integrations i ON s.id = i.service_id WHERE i.description = 'FREQUENCIES'", dbCon).ExecuteScalar().ToString(); }
                catch { integrationFrequencies = "0"; }
                try { integrationAtis = new SQLiteCommand("SELECT s.description FROM Services s INNER JOIN Integrations i ON s.id = i.service_id WHERE i.description = 'ATIS'", dbCon).ExecuteScalar().ToString(); }
                catch { integrationAtis = "0"; }
                //Information
                if (integrationInfo == "0") IsInformationAVWX = IsInformationOpenAip = false;
                else if (integrationInfo == "AVWX") { IsInformationAVWX = true; IsInformationOpenAip = false; }
                else if (integrationInfo == "OPENAIP") { IsInformationAVWX = false; IsInformationOpenAip = true; }
                //Charts
                if (integrationChart == "0") IsChartServiceJeppesen = IsChartServiceLido = false;
                else if (integrationChart == "JEPPESEN") { IsChartServiceJeppesen = true; IsChartServiceLido = false; }
                else if (integrationChart == "LIDO") { IsChartServiceJeppesen = false; IsChartServiceLido = true; }
                //Weather
                if (integrationWeather == "0") IsWeatherAVWX = IsWeatherNOAA = IsWeatherIVAO = false;
                else if (integrationWeather == "AVWX") { IsWeatherAVWX = true; IsWeatherNOAA = false; IsWeatherIVAO = false; }
                else if (integrationWeather == "NOAA") { IsWeatherAVWX = false; IsWeatherNOAA = true; IsWeatherIVAO = false; }
                //Frequencies
                if (integrationFrequencies == "0") IsFrequenciesOpenAip = false;
                else if (integrationFrequencies == "OPENAIP") IsFrequenciesOpenAip = true;
                //Atis
                if (integrationAtis == "0") IsAtisIVAO = false;
                else if (integrationAtis == "IVAO") IsAtisIVAO = true;
                //--- LOADING UNITS ---
                UNIT_DIST = new SQLiteCommand("SELECT UoM.symbol FROM Units u INNER JOIN UoM ON u.uom_id = UoM.id WHERE u.description = 'DISTANCE'", dbCon).ExecuteScalar().ToString();
                UNIT_TEMP = new SQLiteCommand("SELECT UoM.symbol FROM Units u INNER JOIN UoM ON u.uom_id = UoM.id WHERE u.description = 'TEMPERATURE'", dbCon).ExecuteScalar().ToString();
                UNIT_PRES = new SQLiteCommand("SELECT UoM.symbol FROM Units u INNER JOIN UoM ON u.uom_id = UoM.id WHERE u.description = 'PRESSURE'", dbCon).ExecuteScalar().ToString();
                UNIT_RWY = new SQLiteCommand("SELECT UoM.symbol FROM Units u INNER JOIN UoM ON u.uom_id = UoM.id WHERE u.description = 'RWYDISTANCE'", dbCon).ExecuteScalar().ToString();
                UNIT_ELEV = new SQLiteCommand("SELECT UoM.symbol FROM Units u INNER JOIN UoM ON u.uom_id = UoM.id WHERE u.description = 'RWYELEVATION'", dbCon).ExecuteScalar().ToString();
                UNIT_WIND = new SQLiteCommand("SELECT UoM.symbol FROM Units u INNER JOIN UoM ON u.uom_id = UoM.id WHERE u.description = 'WINDSPEED'", dbCon).ExecuteScalar().ToString();
                UNIT_VISIB = new SQLiteCommand("SELECT UoM.symbol FROM Units u INNER JOIN UoM ON u.uom_id = UoM.id WHERE u.description = 'VISIBILITY'", dbCon).ExecuteScalar().ToString();
            }
            catch (Exception ex) { MainWindow.HandleException(ex); }
            finally { if (dbCon != null) dbCon.Close(); }
        }
        private void LoadFavourites()
        {
            Favourites = new List<string>();
            SQLiteConnection dbCon = null;
            dbCon = new SQLiteConnection("URI=file:" + DATA_PATH_DATABASE);
            SQLiteDataReader sdr = null;
            try
            {
                dbCon.Open();
                SQLiteCommand cmd = new SQLiteCommand("SELECT icao FROM Favourites", dbCon);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    Favourites.Add(reader.GetString(0));
            }
            catch (Exception ex) { MainWindow.HandleException(ex); }
            finally { if (dbCon != null) dbCon.Close(); }
        }
        public void AddFavourite(string icao)
        {
            //Adding locally
            Favourites.Add(icao);
            //Adding to DB
            SQLiteConnection dbCon = null;
            dbCon = new SQLiteConnection("URI=file:" + DATA_PATH_DATABASE);
            try
            {
                dbCon.Open();
                SQLiteCommand cmd = new SQLiteCommand($"INSERT INTO Favourites(icao) VALUES ('{icao.ToUpper()}')", dbCon);                
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex) { MainWindow.HandleException(ex); }
            finally { if (dbCon != null) dbCon.Close(); }
        }
        public void RemoveFavourite(string icao)
        {
            //Removing locally
            for (int i = 0; i < Favourites.Count; i++)
                if (icao == Favourites[i])
                    Favourites.RemoveAt(i);
            //Removing from DB
            SQLiteConnection dbCon = null;
            dbCon = new SQLiteConnection("URI=file:" + DATA_PATH_DATABASE);
            try
            {
                dbCon.Open();
                SQLiteCommand cmd = new SQLiteCommand($"DELETE FROM Favourites WHERE icao = '{icao}'", dbCon);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex) { MainWindow.HandleException(ex); }
            finally { if (dbCon != null) dbCon.Close(); }
        }
        public string GetApplicationFolderPath()
        {
            return System.IO.Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString();
        }
    }
}