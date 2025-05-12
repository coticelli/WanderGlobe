using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WanderGlobe.Data;
using WanderGlobe.Models;
using WanderGlobe.Services;

namespace WanderGlobe.Pages
{
    public class TimelineModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICityService _cityService;

        public TimelineModel(ApplicationDbContext context, ICityService cityService)
        {
            _context = context;
            _cityService = cityService;
        }

        public List<VisitedCountry> Visits { get; set; } = new List<VisitedCountry>();
        public Dictionary<int, List<VisitedCountry>> GroupedVisits { get; set; } = new Dictionary<int, List<VisitedCountry>>();
        public List<int> VisitYears { get; set; } = new List<int>();
        public List<string> Continents { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            Visits = await _context.VisitedCountries
                .Include(v => v.Country) 
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();

            GroupedVisits = Visits
                .Where(v => v.Country != null) 
                .GroupBy(v => v.VisitDate.Year)
                .ToDictionary(g => g.Key, g => g.ToList());

            VisitYears = Visits.Select(v => v.VisitDate.Year).Distinct().OrderByDescending(y => y).ToList();

            Continents = await _context.Countries
                .Select(country => country.Continent) 
                .Where(continentName => !string.IsNullOrEmpty(continentName)) 
                .Distinct()
                .OrderBy(continentName => continentName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostEditVisitAsync(int visitId, DateTime visitDate, string visitNotes)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var visit = await _context.VisitedCountries
                .FirstOrDefaultAsync(v => v.CountryId == visitId && v.UserId == userId);

            if (visit == null)
            {
                return NotFound();
            }

            visit.VisitDate = visitDate;
            visit.Notes = visitNotes;
            visit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        private string GetCurrentUserId()
        {
            return User.Identity?.Name ?? "UnknownUser";
        }
      
        public async Task<string> GetCapitalAsync(string countryCode)
        {
            string originalCountryCodeForDebug = countryCode ?? "NULL_INPUT";
            try
            {
                if (string.IsNullOrEmpty(countryCode))
                {
                    return "DBG:CodeNullOrEmpty";
                }

                string searchCode = countryCode.Trim(); // Manteniamo il Trim qui sull'input
                if (string.IsNullOrEmpty(searchCode))
                {
                    return $"DBG:CodeTrimmedEmpty(Orig:'{originalCountryCodeForDebug}')";
                }

                var italyTestCountry = await _context.Countries.FirstOrDefaultAsync(c => c.Code == "IT");
                string italyTestResult = italyTestCountry == null ? "IT_NotFoundInDB" : $"IT_Found(ID:{italyTestCountry.Id}, Name:{italyTestCountry.Name})";
                
                // Modifica per la query LINQ
                string searchCodeUpper = searchCode.ToUpper(); 

                var country = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Code != null && c.Code.ToUpper() == searchCodeUpper);

                if (country == null)
                {
                    // Il debug per country null rimane, ma potremmo non aver bisogno di tutti i conteggi
                    // se questo risolve l'eccezione.
                    var firstFiveCodes = await _context.Countries.Take(5).Select(c => c.Code ?? "NULL_CODE").ToListAsync();
                    string sampleCodes = string.Join(",", firstFiveCodes);
                    return $"DBG:CountryNull(S_UPPER:'{searchCodeUpper}',O:'{originalCountryCodeForDebug}',ITTest:'{italyTestResult}',Samples:'{sampleCodes}')";
                }
                
                if (_cityService == null)
                {
                    return $"DBG:CitySvcNull(Country:{country.Name},ITTest:'{italyTestResult}')";
                }

                var capital = await _cityService.GetCapitalCityByCountryIdAsync(country.Id);
                
                if (capital == null)
                {
                    bool capitalExistsInDbForCountryId = await _context.Cities.AnyAsync(c => c.CountryId == country.Id && c.IsCapital);
                    return $"DBG:CapitalNull(Country:{country.Name},CountryID:{country.Id},CapitalExistsInDB:{capitalExistsInDbForCountryId},ITTest:'{italyTestResult}')";
                }

                return capital.Name; 
            }
            catch (Exception ex)
            {
                string excType = ex.GetType().Name;
                // Ottieni più dettagli dall'eccezione LINQ
                string excMsg = ex.ToString(); // Ottenere il messaggio completo e lo stack trace può essere utile
                excMsg = excMsg.Length > 200 ? excMsg.Substring(0, 200) : excMsg; // Limita la lunghezza per UI

                return $"DBG:EXC({excType}:'{excMsg}',Orig:'{originalCountryCodeForDebug}')";
            }
        }
    }
}