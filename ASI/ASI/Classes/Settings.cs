using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI
{
    public class Settings
    {
        // DATA SAVING PATH
        public readonly string DATA_PATH_ROOT     = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ASI\\",
                               DATA_PATH_SETTINGS = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ASI\\Settings.txt",
                               DATA_PATH_CHARTS   = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ASI\\Charts\\";
        // SERVICES
        public bool IsChartServiceJeppesen { get; set; }
        public bool IsChartServiceLido { get; set; }
        public bool IsFrequenciesOpenAip { get; set; }
        public bool IsInformationAVWX { get; set; }
        public bool IsInformationOpenAip { get; set; }
        public bool IsWeatherAVWX { get; set; }
        public bool IsWeatherIVAO { get; set; }
        public bool IsWeatherNOAA { get; set; }
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

        public Settings() 
        {
            //Default settings
            IsChartServiceJeppesen = IsChartServiceLido = IsWeatherIVAO = IsWeatherAVWX = false;
            IsWeatherNOAA = IsInformationAVWX = true;
            IsFrequenciesOpenAip = true;
            //Default credentials
            JP_USER = "evapilot2";
            JP_PASSWORD = "pilot002";
            LD_USER = "Lufthansa";
            LD_PASSWORD = "kbP6Mwt";
            AVWX_TOKEN = "MWlQgmuag-2MaxWtKGxCmdFnjoNBvmaadnYUOl86nq4";
            OPENAIP_TOKEN = "1eba052dd328ac3b9894d0c3d62678a6";
            //Default UoM
            UNIT_VISIB = "M";
            UNIT_RWY = "M";
            UNIT_WIND = "KTS";
            UNIT_ELEV = "FT";
            UNIT_TEMP = "C";
            UNIT_PRES = "HPA";
            //Default graphics
            ShowWindOnRunway = true;
        }
    }
}
