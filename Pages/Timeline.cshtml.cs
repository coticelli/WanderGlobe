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
            // Carica le visite ordinate per data
            Visits = await _context.VisitedCountries
                .Include(v => v.Country)
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();

            // Raggruppa per anno
            GroupedVisits = Visits
                .GroupBy(v => v.VisitDate.Year)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Estrai gli anni di visita
            VisitYears = Visits.Select(v => v.VisitDate.Year).Distinct().OrderByDescending(y => y).ToList();

            // Carica i continenti unici
            Continents = _context.Countries
                .Select(c => c.Continent)
                .Where(c => c != null)
                .Distinct()
                .ToList();
        }

        public async Task<IActionResult> OnPostEditVisitAsync(int visitId, DateTime visitDate, string visitNotes)
        {
            // Ottieni l'ID dell'utente come stringa
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Usa LINQ con l'ID utente corretto
            var visit = await _context.VisitedCountries
                .FirstOrDefaultAsync(v => v.CountryId == visitId && v.UserId == userId);

            if (visit == null)
            {
                return NotFound();
            }

            // Aggiorna i valori
            visit.VisitDate = visitDate;

            // Salva le note se la proprietà esiste nel modello
            // Se hai aggiunto la proprietà Notes al modello, scommentare queste righe
            visit.Notes = visitNotes;

            visit.UpdatedAt = DateTime.UtcNow; // Aggiorna il timestamp di modifica

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        private string GetCurrentUserId()
        {
            // Ottieni il nome utente invece dell'ID numerico
            return User.Identity?.Name;
        }        public async Task<string> GetCapitalAsync(string countryCode)
{
    try
    {
        if (string.IsNullOrEmpty(countryCode))
            return "Capitale sconosciuta";

        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Code.Equals(countryCode, StringComparison.OrdinalIgnoreCase));

        if (country == null)
            return $"Capitale di {countryCode}";

        var capital = await _cityService.GetCapitalCityByCountryIdAsync(country.Id);
          return capital?.Name ?? $"Capitale di {country.Name}";
    }
    catch (Exception)
    {
        // Si è verificato un errore durante il recupero della capitale
        return $"Capitale di {countryCode}";
    }
}
    }
}