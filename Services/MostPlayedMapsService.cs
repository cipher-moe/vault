using System;
using System.Collections.Generic;

namespace vault.Services
{
    public class MostPlayedMapsService
    {
        public List<(string, int)> MostPlayedMaps = new ();
        public DateTime LastUpdated = DateTime.Now;
    }
}