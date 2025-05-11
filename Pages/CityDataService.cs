using Microsoft.EntityFrameworkCore;
using WanderGlobe.Data;
using WanderGlobe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WanderGlobe.Pages
{
    public class CityDataService
    {
        private readonly ApplicationDbContext _dbContext;

        public CityDataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CityInfo>> GetAllCitiesWithCountryAsync()
        {
            try
            {
                // Ottieni tutte le città con i dati dei paesi
                var cities = await _dbContext.Cities
                    .Include(c => c.Country)
                    .ToListAsync();

                // Filtriamo e mappiamo i risultati
                var results = cities
                    .Where(c => c.Country != null)
                    .Select(c => new CityInfo
                    {
                        Name = c.Name,
                        Country = c.Country.Name,
                        CountryCode = c.Country.Code
                    })
                    .OrderBy(c => c.Country)
                    .ThenBy(c => c.Name)
                    .ToList();

                return results;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel recupero delle città: {ex.Message}");
                return new List<CityInfo>();
            }
        }
    }

    public class CityInfo
    {
        public string? Name { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
    }
}
