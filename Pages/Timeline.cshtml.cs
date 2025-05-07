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
using System.Security.Claims;

namespace WanderGlobe.Pages
{
    public class TimelineModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TimelineModel(ApplicationDbContext context)
        {
            _context = context;
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
        }


        public string GetCapital(string countryCode)
        {
            var capitals = new Dictionary<string, string>
            {
                // Europa
                {"IT", "Roma"}, {"GB", "Londra"}, {"FR", "Parigi"}, {"DE", "Berlino"},
                {"ES", "Madrid"}, {"PT", "Lisbona"}, {"NL", "Amsterdam"}, {"BE", "Bruxelles"},
                {"LU", "Lussemburgo"}, {"CH", "Berna"}, {"AT", "Vienna"}, {"GR", "Atene"},
                {"SE", "Stoccolma"}, {"NO", "Oslo"}, {"DK", "Copenaghen"}, {"FI", "Helsinki"},
                {"IE", "Dublino"}, {"IS", "Reykjavik"}, {"MT", "La Valletta"}, {"CY", "Nicosia"},
                {"PL", "Varsavia"}, {"CZ", "Praga"}, {"SK", "Bratislava"}, {"HU", "Budapest"},
                {"RO", "Bucarest"}, {"BG", "Sofia"}, {"HR", "Zagabria"}, {"SI", "Lubiana"},
                {"RS", "Belgrado"}, {"ME", "Podgorica"}, {"AL", "Tirana"}, {"MK", "Skopje"},
                {"BA", "Sarajevo"}, {"MD", "Chisinau"}, {"BY", "Minsk"}, {"LT", "Vilnius"},
                {"LV", "Riga"}, {"EE", "Tallinn"}, {"UA", "Kiev"}, {"VA", "Città del Vaticano"},
                {"SM", "San Marino"}, {"MC", "Monaco"}, {"AD", "Andorra la Vella"}, {"LI", "Vaduz"},
                
                // Resto del mondo...
                {"RU", "Mosca"}, {"JP", "Tokyo"}, {"CN", "Pechino"}, {"IN", "Nuova Delhi"},
                {"KR", "Seoul"}, {"KP", "Pyongyang"}, {"TH", "Bangkok"}, {"VN", "Hanoi"},
                {"LA", "Vientiane"}, {"KH", "Phnom Penh"}, {"MY", "Kuala Lumpur"}, {"SG", "Singapore"},
                {"ID", "Giacarta"}, {"PH", "Manila"}, {"TR", "Ankara"}, {"SA", "Riyadh"},
                {"AE", "Abu Dhabi"}, {"IL", "Gerusalemme"}, {"LB", "Beirut"}, {"JO", "Amman"},
                {"QA", "Doha"}, {"KW", "Kuwait City"}, {"IQ", "Baghdad"}, {"IR", "Teheran"},
                {"AF", "Kabul"}, {"PK", "Islamabad"}, {"BD", "Dhaka"}, {"LK", "Colombo"},
                {"MM", "Naypyidaw"}, {"KZ", "Astana"}, {"UZ", "Tashkent"}, {"TM", "Ashgabat"},
                {"KG", "Bishkek"}, {"TJ", "Dushanbe"}, {"MN", "Ulaanbaatar"}, {"BT", "Thimphu"},
                {"NP", "Kathmandu"}, {"MV", "Male"}, {"BN", "Bandar Seri Begawan"}, {"TL", "Dili"},
                {"AM", "Yerevan"}, {"GE", "Tbilisi"}, {"AZ", "Baku"},
                {"ZA", "Pretoria"}, {"EG", "Cairo"}, {"MA", "Rabat"}, {"NG", "Abuja"},
                {"KE", "Nairobi"}, {"ET", "Addis Abeba"}, {"TZ", "Dodoma"}, {"MZ", "Maputo"},
                {"ZW", "Harare"}, {"AO", "Luanda"}, {"NA", "Windhoek"}, {"BW", "Gaborone"},
                {"ZM", "Lusaka"}, {"CD", "Kinshasa"}, {"CG", "Brazzaville"}, {"GA", "Libreville"},
                {"CM", "Yaoundé"}, {"TD", "N'Djamena"}, {"NE", "Niamey"}, {"ML", "Bamako"},
                {"SN", "Dakar"}, {"CI", "Yamoussoukro"}, {"GH", "Accra"}, {"TG", "Lomé"},
                {"BJ", "Porto-Novo"}, {"CV", "Praia"},
                {"US", "Washington D.C."}, {"CA", "Ottawa"}, {"MX", "Città del Messico"},
                {"BR", "Brasilia"}, {"AR", "Buenos Aires"}, {"CO", "Bogotá"}, {"CL", "Santiago"},
                {"PE", "Lima"}, {"VE", "Caracas"}, {"EC", "Quito"}, {"BO", "La Paz"},
                {"PY", "Asunción"}, {"UY", "Montevideo"}, {"GY", "Georgetown"}, {"SR", "Paramaribo"},
                {"PA", "Panama"}, {"CR", "San José"}, {"NI", "Managua"}, {"HN", "Tegucigalpa"},
                {"SV", "San Salvador"}, {"GT", "Città del Guatemala"}, {"BZ", "Belmopan"},
                {"CU", "L'Avana"}, {"BS", "Nassau"}, {"JM", "Kingston"}, {"HT", "Port-au-Prince"},
                {"DO", "Santo Domingo"}, {"KN", "Basseterre"}, {"AG", "Saint John's"},
                {"DM", "Roseau"}, {"LC", "Castries"}, {"VC", "Kingstown"}, {"BB", "Bridgetown"},
                {"GD", "St. George's"}, {"TT", "Port of Spain"},
                {"AU", "Canberra"}, {"NZ", "Wellington"}, {"PG", "Port Moresby"}, {"FJ", "Suva"},
                {"SB", "Honiara"}, {"VU", "Port Vila"}, {"TO", "Nukuʻalofa"}, {"WS", "Apia"},
                {"PW", "Ngerulmud"}, {"FM", "Palikir"}, {"MH", "Majuro"}, {"KI", "Tarawa"},
                {"NR", "Yaren"}, {"TV", "Funafuti"}
            };

            if (string.IsNullOrEmpty(countryCode))
                return "Capitale sconosciuta";

            return capitals.ContainsKey(countryCode) ? capitals[countryCode] : $"Capitale di {countryCode}";
        }
    }
}