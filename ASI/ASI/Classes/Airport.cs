using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI
{
    public class Airport
    {
        public string Name;
        public string City;
        public string Country;
        public string IATACode;
        public string ICAOCode;
        public string Latitude;
        public string Longitude;
        public string Website;
        public string Notes;
        public int Elevation;
        public List<Runway> Runways;
        public List<Frequency> Frequencies;
        public string ATIS;
        public bool IsFavourite = false;

        public Airport(OpenAipLib.Airport a)
        {
            Name = a.Name;
            City = "Not available";
            Country = a.Country;
            IATACode = "Not available";
            ICAOCode = a.ICAOCode;
            Latitude = a.Latitude;
            Longitude = a.Longitude;
            Notes = "No notes provided";
            Website = "Not available";
            Elevation = Convert.ToInt16(a.Elevation);
            //Converting Runways
            List<Runway> rwys = new List<Runway>();
            foreach(OpenAipLib.Runway r in a.Runways)
                rwys.Add(new Runway(r.Identification1, r.Identification2, int.Parse(r.Bearing1), int.Parse(r.Bearing2), r.Surface, int.Parse(r.Length), int.Parse(r.Width)));
            Runways = rwys;
            //Converting Frequencies
            List<Frequency> freqs = new List<Frequency>();
            foreach (OpenAipLib.Frequency f in a.Frequencies)
                freqs.Add(new Frequency(f.Value, f.Name, f.Type));
            Frequencies = freqs;
            ATIS = "No ATIS available.";
        }
        public Airport(AVWXLib.Airport a)
        {
            Name = a.Name;
            City = a.City;
            Country = a.Country;
            IATACode = a.IATACode;
            ICAOCode = a.ICAOCode;
            Latitude = a.Latitude;
            Longitude = a.Longitude;
            Notes = a.Note;
            Website = a.Website;
            Elevation = Convert.ToInt16(a.Elevation);
            //Converting Elevation in meters (as standard)
            Elevation = FeetToMeters(Elevation);
            //Converting Runways
            List<Runway> rwys = new List<Runway>();
            foreach (AVWXLib.Runway r in a.Runways)
            {
                string b1 = r.Bearing1.Substring(0, r.Bearing1.Length - 2), b2 = r.Bearing2.Substring(0, r.Bearing2.Length - 2);
                rwys.Add(new Runway(r.Identification1, r.Identification2, int.Parse(b1), int.Parse(b2), r.Surface, int.Parse(r.Length), -1));
            }
                
            Runways = rwys;
            Frequencies = null;
            ATIS = "No ATIS available.";
        }
        private int FeetToMeters(int value) { return Convert.ToInt16(value / 3.2808399); }
    }
    public class Runway
    {
        public string Identification1;
        public string Identification2;
        public int Bearing1;
        public int Bearing2;
        public string Surface;
        public int RwyLength;
        public int RwyWidth;
        public Runway(string id1, string id2, int b1, int b2, string s, int l, int w)
        {
            Identification1 = id1; Identification2 = id2; Bearing1 = b1; Bearing2 = b2; Surface = s; RwyLength = l; RwyWidth = w; 
        }
    }
    public class Frequency
    {
        public string Value;
        public string Name;
        public string Type;
        public Frequency(string v, string n, string t)
        {
            Value = v; Name = n; Type = t;
        }
    }

}
