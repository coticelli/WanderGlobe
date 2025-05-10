using Microsoft.EntityFrameworkCore;
using WanderGlobe.Data;
using WanderGlobe.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WanderGlobe.Services
{
    public class CityService : ICityService
    {
        private readonly ApplicationDbContext _context;

        public CityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<City>> GetAllCitiesAsync()
        {
            return await _context.Cities
                .Include(c => c.Country)
                .ToListAsync();
        }

        public async Task<List<City>> GetCitiesByCountryIdAsync(int countryId)
        {
            return await _context.Cities
                .Where(c => c.CountryId == countryId)
                .ToListAsync();
        }

        public async Task<List<City>> GetCapitalCitiesAsync()
        {
            return await _context.Cities
                .Where(c => c.IsCapital)
                .Include(c => c.Country)
                .ToListAsync();
        }

        public async Task<City> GetCityByIdAsync(int cityId)
        {
            return await _context.Cities
                .Include(c => c.Country)
                .FirstOrDefaultAsync(c => c.Id == cityId);
        }

        public async Task<City?> GetCapitalCityByCountryIdAsync(int countryId)
        {
            return await _context.Cities
                .FirstOrDefaultAsync(c => c.CountryId == countryId && c.IsCapital);
        }
    }
}