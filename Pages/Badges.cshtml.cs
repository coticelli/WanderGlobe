using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using WanderGlobe.Data;
using WanderGlobe.Models; // Contiene ApplicationUser, Country, VisitedCountry, Photo, TravelJournal
using WanderGlobe.Models.Custom; // Contiene DreamDestination
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;
// Rimuovi using WanderGlobe.Models.Custom; se DreamDestination è ora solo in Models.ApplicationUser -> No, DreamDestination è in Models.Custom come definito

namespace WanderGlobe.Pages
{
    public class BadgesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BadgesModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<BadgeViewModel> UserBadges { get; set; } = new List<BadgeViewModel>();
        public int TotalBadgesAvailable { get; set; }
        public int BadgesEarnedCount { get; set; }
        public double OverallProgressPercentage { get; set; }

        // Dati utente precaricati
        private ApplicationUser _currentUser;
        private List<VisitedCountry> _userVisitedCountries;
        private List<DreamDestination> _userDreamDestinationsList; // Useremo la lista da ApplicationUser
        private List<Photo> _userPhotos;
        private List<TravelJournal> _userTravelJournals;

        private int _visitedCountriesCountDistinct;
        private List<string> _distinctVisitedContinents;
        private DateTime _userJoinDate;

        private async Task LoadUserDataAsync()
        {
            _currentUser = await _userManager.Users
                                    .Include(u => u.VisitedCountries)
                                        .ThenInclude(vc => vc.Country)
                                    .Include(u => u.DreamDestinations) // DreamDestinations è una List<DreamDestination> definita in ApplicationUser
                                                                       // Non c'è .ThenInclude(dd => dd.Country) perché DreamDestination (Custom) non ha una navigazione Country diretta
                                    .Include(u => u.TravelJournals)
                                        .ThenInclude(tj => tj.Country)
                                    .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (_currentUser == null) return;

            _userJoinDate = _currentUser.JoinDate;
            _userVisitedCountries = _currentUser.VisitedCountries ?? new List<VisitedCountry>();
            _userDreamDestinationsList = _currentUser.DreamDestinations?.ToList() ?? new List<DreamDestination>(); // Viene da ApplicationUser
            _userTravelJournals = _currentUser.TravelJournals?.ToList() ?? new List<TravelJournal>();

            _userPhotos = await _context.Photos
                                .Where(p => p.UserId == _currentUser.Id)
                                .ToListAsync();

            if (_userVisitedCountries.Any())
            {
                _visitedCountriesCountDistinct = _userVisitedCountries.Select(v => v.CountryId).Distinct().Count();
                _distinctVisitedContinents = _userVisitedCountries
                                            .Where(v => v.Country != null && !string.IsNullOrEmpty(v.Country.Continent))
                                            .Select(v => v.Country.Continent.Trim())
                                            .Distinct(StringComparer.OrdinalIgnoreCase)
                                            .ToList();
            }
            else
            {
                _visitedCountriesCountDistinct = 0;
                _distinctVisitedContinents = new List<string>();
            }
        }

        public async Task OnGetAsync()
        {
            await LoadUserDataAsync();
            if (_currentUser == null)
            {
                UserBadges = new List<BadgeViewModel>();
                return;
            }

            var allPossibleBadges = DefineAllBadges();
            TotalBadgesAvailable = allPossibleBadges.Count;

            foreach (var badgeDef in allPossibleBadges)
            {
                bool isEarned = CheckIfBadgeEarned(badgeDef);
                int currentProgress = GetBadgeProgress(badgeDef);

                UserBadges.Add(new BadgeViewModel
                {
                    Id = badgeDef.Id, Name = badgeDef.Name, Description = badgeDef.Description,
                    IconClass = badgeDef.IconClass, IsEarned = isEarned, CriteriaDescription = badgeDef.CriteriaDescription,
                    ProgressCurrent = currentProgress, ProgressTarget = badgeDef.ProgressTarget, Category = badgeDef.Category,
                    Rarity = badgeDef.Rarity, IsSecret = badgeDef.IsSecret
                });

                if (isEarned) BadgesEarnedCount++;
            }

            OverallProgressPercentage = TotalBadgesAvailable > 0 ? Math.Round(((double)BadgesEarnedCount / TotalBadgesAvailable) * 100, 0) : 0;

            UserBadges = UserBadges.OrderByDescending(b => b.IsEarned)
                                   .ThenBy(b => GetCategoryOrder(b.Category))
                                   .ThenBy(b => b.Rarity)
                                   .ThenBy(b => b.Name)
                                   .ToList();
        }

        private List<BadgeDefinition> DefineAllBadges()
        {
            return new List<BadgeDefinition>
            {
                // Categoria: Primi Passi
                new BadgeDefinition { Id = "first_voyage", Name = "Primo Volo", Description = "Hai registrato la tua prima destinazione!", IconClass = "fas fa-plane-departure", CriteriaDescription = "Registra 1 viaggio", ProgressTarget = 1, Category = "Primi Passi", Rarity = BadgeRarity.Comune },
                new BadgeDefinition { Id = "first_photo_upload", Name = "Click!", Description = "La tua prima foto di viaggio è online.", IconClass = "fas fa-camera", CriteriaDescription = "Carica 1 foto", ProgressTarget = 1, Category = "Primi Passi", Rarity = BadgeRarity.Comune },
                new BadgeDefinition { Id = "first_journal_entry", Name = "Pagine Bianche", Description = "Hai iniziato il tuo diario di viaggio.", IconClass = "fas fa-book-medical", CriteriaDescription = "Scrivi 1 racconto", ProgressTarget = 1, Category = "Primi Passi", Rarity = BadgeRarity.Comune },

                // Categoria: Esploratore Globale
                new BadgeDefinition { Id = "explorer_5", Name = "Esploratore Curioso", Description = "5 paesi visitati. Il mondo inizia ad aprirsi!", IconClass = "fas fa-map-signs", CriteriaDescription = "Visita 5 paesi", ProgressTarget = 5, Category = "Esploratore Globale", Rarity = BadgeRarity.Comune },
                new BadgeDefinition { Id = "explorer_10", Name = "Viaggiatore Navigato", Description = "10 paesi nel tuo passaporto. Impressionante!", IconClass = "fas fa-passport", CriteriaDescription = "Visita 10 paesi", ProgressTarget = 10, Category = "Esploratore Globale", Rarity = BadgeRarity.NonComune },
                new BadgeDefinition { Id = "explorer_25", Name = "Giramondo Esperto", Description = "25 paesi! Conosci bene il pianeta.", IconClass = "fas fa-earth-americas", CriteriaDescription = "Visita 25 paesi", ProgressTarget = 25, Category = "Esploratore Globale", Rarity = BadgeRarity.Raro },
                new BadgeDefinition { Id = "explorer_50", Name = "Leggenda dei Viaggi", Description = "50 paesi! Sei una vera ispirazione.", IconClass = "fas fa-crown", CriteriaDescription = "Visita 50 paesi", ProgressTarget = 50, Category = "Esploratore Globale", Rarity = BadgeRarity.Epico },

                // Categoria: Conquistatore di Continenti
                new BadgeDefinition { Id = "continent_europe", Name = "Europaeus", Description = "Hai calcato il suolo del Vecchio Continente.", IconClass = "fab fa-fort-awesome-alt", CriteriaDescription = "Visita un paese in Europa", Category = "Conquistatore di Continenti", Rarity = BadgeRarity.Comune },
                new BadgeDefinition { Id = "continent_asia", Name = "Asiaticus", Description = "Misteri e meraviglie dell'Asia ti hanno accolto.", IconClass = "fas fa-torii-gate", CriteriaDescription = "Visita un paese in Asia", Category = "Conquistatore di Continenti", Rarity = BadgeRarity.Comune },
                new BadgeDefinition { Id = "continent_africa", Name = "Africanus", Description = "Il cuore pulsante dell'Africa ti ha stregato.", IconClass = "fas fa-drum", CriteriaDescription = "Visita un paese in Africa", Category = "Conquistatore di Continenti", Rarity = BadgeRarity.Comune },
                new BadgeDefinition { Id = "continent_north_america", Name = "Americanus Septentrionalis", Description = "Dalle metropoli ai parchi, il Nord America ti ha visto.", IconClass = "fas fa-city", CriteriaDescription = "Visita un paese in Nord America", Category = "Conquistatore di Continenti", Rarity = BadgeRarity.Comune },
                new BadgeDefinition { Id = "continent_south_america", Name = "Americanus Meridionalis", Description = "Ritmi e paesaggi del Sud America nel tuo cuore.", IconClass = "fas fa-feather-alt", CriteriaDescription = "Visita un paese in Sud America", Category = "Conquistatore di Continenti", Rarity = BadgeRarity.Comune },
                new BadgeDefinition { Id = "continent_oceania", Name = "Oceanicus", Description = "Le isole e le terre di Oceania ti hanno meravigliato.", IconClass = "fas fa-anchor", CriteriaDescription = "Visita un paese in Oceania", Category = "Conquistatore di Continenti", Rarity = BadgeRarity.Comune },
                new BadgeDefinition { Id = "continent_antarctica", Name = "Antarcticus Explorer", Description = "Hai osato sfidare i ghiacci dell'Antartide!", IconClass = "fas fa-icicles", CriteriaDescription = "Visita l'Antartide", Category = "Conquistatore di Continenti", Rarity = BadgeRarity.Epico },
                new BadgeDefinition { Id = "globetrotter_pro", Name = "Maestro Globetrotter", Description = "Tutti i 7 continenti visitati! Pochi possono vantarlo.", IconClass = "fas fa-globe", CriteriaDescription = "Visita tutti i 7 continenti", ProgressTarget = 7, Category = "Conquistatore di Continenti", Rarity = BadgeRarity.Leggendario },

                // Categoria: Pianificazione e Sogni
                new BadgeDefinition { Id = "dream_starter", Name = "Sognatore ad Occhi Aperti", Description = "Il tuo primo desiderio di viaggio è stato annotato.", IconClass = "fas fa-cloud-sun", CriteriaDescription = "Aggiungi 1 destinazione alla Dream Map", ProgressTarget = 1, Category = "Pianificazione e Sogni", Rarity = BadgeRarity.Comune },
                new BadgeDefinition { Id = "dream_weaver_5", Name = "Architetto di Sogni", Description = "La tua mappa dei desideri si sta popolando: 5 sogni!", IconClass = "fas fa-drafting-compass", CriteriaDescription = "Aggiungi 5 sogni", ProgressTarget = 5, Category = "Pianificazione e Sogni", Rarity = BadgeRarity.NonComune },
                new BadgeDefinition { Id = "dream_planner_10", Name = "Cartografo dei Desideri", Description = "10 sogni tracciati, pronti per diventare realtà.", IconClass = "fas fa-map-marked-alt", CriteriaDescription = "Aggiungi 10 sogni", ProgressTarget = 10, Category = "Pianificazione e Sogni", Rarity = BadgeRarity.Raro },
                new BadgeDefinition { Id = "dream_achiever", Name = "Sogno Realizzato!", Description = "Hai trasformato un sogno della Dream Map in un viaggio compiuto!", IconClass = "fas fa-check-double", CriteriaDescription = "Visita un paese che era nella Dream Map", Category = "Pianificazione e Sogni", Rarity = BadgeRarity.Raro },

                // Categoria: Maestria Fotografica
                new BadgeDefinition { Id = "photographer_10", Name = "Cacciatore di Istantanee", Description = "10 foto caricate, la tua galleria prende vita.", IconClass = "fas fa-images", CriteriaDescription = "Carica 10 foto", ProgressTarget = 10, Category = "Maestria Fotografica", Rarity = BadgeRarity.NonComune },
                new BadgeDefinition { Id = "photographer_50", Name = "Artista dell'Obiettivo", Description = "50 scatti memorabili. Un vero portfolio!", IconClass = "fas fa-camera-retro", CriteriaDescription = "Carica 50 foto", ProgressTarget = 50, Category = "Maestria Fotografica", Rarity = BadgeRarity.Raro },
                new BadgeDefinition { Id = "photo_per_continent", Name = "Reporter Continentale", Description = "Hai caricato almeno una foto per ogni continente visitato.", IconClass = "fas fa-globe-europe", CriteriaDescription = "1+ foto per ogni continente visitato", Category = "Maestria Fotografica", Rarity = BadgeRarity.Epico },

                // Categoria: Scrittore di Viaggi
                new BadgeDefinition { Id = "journalist_5", Name = "Narratore Errante", Description = "5 racconti di viaggio. Le tue avventure ispirano!", IconClass = "fas fa-feather", CriteriaDescription = "Scrivi 5 racconti", ProgressTarget = 5, Category = "Scrittore di Viaggi", Rarity = BadgeRarity.NonComune },
                new BadgeDefinition { Id = "journalist_long_story", Name = "Romanziere di Rotte", Description = "Hai scritto un racconto di viaggio particolarmente lungo (es. >1000 caratteri).", IconClass = "fas fa-scroll", CriteriaDescription = "Scrivi un racconto lungo", Category = "Scrittore di Viaggi", Rarity = BadgeRarity.Raro },

                // Categoria: Sfide Speciali
                new BadgeDefinition { Id = "time_traveler_years", Name = "Viaggiatore Temporale", Description = "Viaggi documentati in 5 anni diversi.", IconClass = "fas fa-calendar-alt", CriteriaDescription = "Viaggi in 5 anni diversi", ProgressTarget = 5, Category = "Sfide Speciali", Rarity = BadgeRarity.Raro },
                new BadgeDefinition { Id = "loyalty_wanderglobe", Name = "Fedeltà a WanderGlobe", Description = "Sei con noi da più di un anno!", IconClass = "fas fa-heart", CriteriaDescription = "Utente da 1+ anno", Category = "Sfide Speciali", Rarity = BadgeRarity.NonComune },
                new BadgeDefinition { Id = "leap_year_traveler", Name = "Viaggiatore Bisestile", Description = "Hai registrato un viaggio durante un 29 Febbraio.", IconClass = "fas fa-calendar-day", CriteriaDescription = "Viaggio il 29 Febbraio", Category = "Sfide Speciali", Rarity = BadgeRarity.Raro, IsSecret = true},
                new BadgeDefinition { Id = "hidden_gem_finder", Name = "Scopritore di Gemme Nascoste", Description = "Hai visitato un paese molto poco comune o aggiunto una nota particolarmente dettagliata su un luogo remoto.", IconClass = "fas fa-search-location", CriteriaDescription = "Trova una gemma nascosta", Category = "Sfide Speciali", Rarity = BadgeRarity.Leggendario, IsSecret = true }
            };
        }

        private bool CheckIfBadgeEarned(BadgeDefinition badgeDef)
        {
            if (_currentUser == null) return false;
            _userVisitedCountries ??= new List<VisitedCountry>();
            _userDreamDestinationsList ??= new List<DreamDestination>();
            _userPhotos ??= new List<Photo>();
            _userTravelJournals ??= new List<TravelJournal>();
            _distinctVisitedContinents ??= new List<string>();

            switch (badgeDef.Id)
            {
                case "first_voyage": return _visitedCountriesCountDistinct > 0;
                case "first_photo_upload": return _userPhotos.Any();
                case "first_journal_entry": return _userTravelJournals.Any();

                case "explorer_5": return _visitedCountriesCountDistinct >= 5;
                case "explorer_10": return _visitedCountriesCountDistinct >= 10;
                case "explorer_25": return _visitedCountriesCountDistinct >= 25;
                case "explorer_50": return _visitedCountriesCountDistinct >= 50;

                // CORREZIONE: Usare .Any() con .Equals() per confronto case-insensitive
                case "continent_europe": return _distinctVisitedContinents.Any(c => c.Equals("Europa", StringComparison.OrdinalIgnoreCase));
                case "continent_asia": return _distinctVisitedContinents.Any(c => c.Equals("Asia", StringComparison.OrdinalIgnoreCase));
                case "continent_africa": return _distinctVisitedContinents.Any(c => c.Equals("Africa", StringComparison.OrdinalIgnoreCase));
                case "continent_north_america": return _distinctVisitedContinents.Any(c => c.Equals("Nord America", StringComparison.OrdinalIgnoreCase) || c.Equals("North America", StringComparison.OrdinalIgnoreCase));
                case "continent_south_america": return _distinctVisitedContinents.Any(c => c.Equals("Sud America", StringComparison.OrdinalIgnoreCase) || c.Equals("South America", StringComparison.OrdinalIgnoreCase));
                case "continent_oceania": return _distinctVisitedContinents.Any(c => c.Equals("Oceania", StringComparison.OrdinalIgnoreCase));
                case "continent_antarctica": return _distinctVisitedContinents.Any(c => c.Equals("Antartide", StringComparison.OrdinalIgnoreCase) || c.Equals("Antarctica", StringComparison.OrdinalIgnoreCase));
                case "globetrotter_pro":
                    var requiredContinents7 = new List<string> { "Europa", "Asia", "Africa", "Nord America", "Sud America", "Oceania", "Antartide" };
                    return requiredContinents7.All(rc =>
                        _distinctVisitedContinents.Any(uvc => uvc.Equals(rc, StringComparison.OrdinalIgnoreCase) ||
                                                              (rc == "Nord America" && uvc.Equals("North America", StringComparison.OrdinalIgnoreCase)) ||
                                                              (rc == "Sud America" && uvc.Equals("South America", StringComparison.OrdinalIgnoreCase)) ||
                                                              (rc == "Antartide" && uvc.Equals("Antarctica", StringComparison.OrdinalIgnoreCase))
                    ));

                case "dream_starter": return _userDreamDestinationsList.Any();
                case "dream_weaver_5": return _userDreamDestinationsList.Count >= 5;
                case "dream_planner_10": return _userDreamDestinationsList.Count >= 10;
                case "dream_achiever":
                    // CORREZIONE: Confrontare CountryCode e usare dream.CreatedAt
                    return _userDreamDestinationsList.Any(dream =>
                        _userVisitedCountries.Any(visited =>
                            visited.Country != null && // Assicurati che Country sia caricato
                            !string.IsNullOrEmpty(visited.Country.Code) && // Assicurati che il codice esista nel paese visitato
                            !string.IsNullOrEmpty(dream.CountryCode) &&    // Assicurati che il codice esista nel sogno
                            visited.Country.Code.Equals(dream.CountryCode, StringComparison.OrdinalIgnoreCase) &&
                            visited.VisitDate >= dream.CreatedAt // Usa CreatedAt da DreamDestination
                        )
                    );

                case "photographer_10": return _userPhotos.Count >= 10;
                case "photographer_50": return _userPhotos.Count >= 50;
                case "photo_per_continent":
                    if (!_distinctVisitedContinents.Any()) return false;
                    return _distinctVisitedContinents.All(cont =>
                        _userVisitedCountries
                            .Where(vc => vc.Country != null && vc.Country.Continent.Equals(cont, StringComparison.OrdinalIgnoreCase))
                            .Any(vc_in_continent =>
                                _userPhotos.Any(p => 
                                    // Update these property checks
                                    (p.TravelJournalUserId == vc_in_continent.UserId ||
                                     p.UserId == vc_in_continent.UserId) &&
                                    (p.TravelJournalCountryId == vc_in_continent.CountryId ||
                                     (p.VisitedCity != null && p.VisitedCity.City.CountryId == vc_in_continent.CountryId))
                                )
                            )
                    );


                case "journalist_5": return _userTravelJournals.Count >= 5;
                case "journalist_long_story": return _userTravelJournals.Any(tj => tj.Notes?.Length > 1000);

                case "time_traveler_years": return _userVisitedCountries.Select(v => v.VisitDate.Year).Distinct().Count() >= 5;
                case "loyalty_wanderglobe": return (DateTime.UtcNow - _userJoinDate).TotalDays > 365;
                case "leap_year_traveler": return _userVisitedCountries.Any(v => v.VisitDate.Month == 2 && v.VisitDate.Day == 29);

                // Nota: "hidden_gem_finder" non ha una logica implementata qui, restituirà sempre false.
                // Dovrai aggiungere la logica per questo badge se vuoi che sia ottenibile.
                // Esempio:
                // case "hidden_gem_finder":
                //    return _userVisitedCountries.Any(vc => IsConsideredHiddenGem(vc.CountryId)) ||
                //           _userTravelJournals.Any(tj => IsDetailedNoteForRemotePlace(tj));

                default: return false;
            }
        }

        // Metodo helper di esempio per "hidden_gem_finder" (da implementare)
        // private bool IsConsideredHiddenGem(int countryId) { /* ... logica ... */ return false; }
        // private bool IsDetailedNoteForRemotePlace(TravelJournal journal) { /* ... logica ... */ return false; }


        private int GetBadgeProgress(BadgeDefinition badgeDef)
        {
            if (_currentUser == null) return 0;
            _userVisitedCountries ??= new List<VisitedCountry>();
            _userDreamDestinationsList ??= new List<DreamDestination>();
            _userPhotos ??= new List<Photo>();
            _userTravelJournals ??= new List<TravelJournal>();
            _distinctVisitedContinents ??= new List<string>();

            switch (badgeDef.Id)
            {
                case "first_voyage": return _visitedCountriesCountDistinct > 0 ? 1 : 0;
                case "first_photo_upload": return _userPhotos.Any() ? 1 : 0;
                case "first_journal_entry": return _userTravelJournals.Any() ? 1 : 0;

                case "explorer_5": case "explorer_10": case "explorer_25": case "explorer_50":
                    return _visitedCountriesCountDistinct;

                case "globetrotter_pro":
                    var requiredContinents7 = new List<string> { "Europa", "Asia", "Africa", "Nord America", "Sud America", "Oceania", "Antartide" };
                    return requiredContinents7.Count(rc =>
                        _distinctVisitedContinents.Any(uvc => uvc.Equals(rc, StringComparison.OrdinalIgnoreCase) ||
                                                              (rc == "Nord America" && uvc.Equals("North America", StringComparison.OrdinalIgnoreCase)) ||
                                                              (rc == "Sud America" && uvc.Equals("South America", StringComparison.OrdinalIgnoreCase)) ||
                                                              (rc == "Antartide" && uvc.Equals("Antarctica", StringComparison.OrdinalIgnoreCase))
                    ));

                case "dream_starter": return _userDreamDestinationsList.Any() ? 1 : 0;
                case "dream_weaver_5": case "dream_planner_10":
                    return _userDreamDestinationsList.Count;

                case "photographer_10": case "photographer_50":
                    return _userPhotos.Count;

                case "journalist_5":
                    return _userTravelJournals.Count;
                case "journalist_long_story":
                    return _userTravelJournals.Any(tj => tj.Notes?.Length > 1000) ? 1 : 0;

                case "time_traveler_years":
                    return _userVisitedCountries.Select(v => v.VisitDate.Year).Distinct().Count();

                // Per i badge senza target > 1 specifico, o quelli che sono solo "ottenuto/non ottenuto"
                // la progressione può essere 0 o 1 (basata su IsEarned)
                // I badge per continente singolo rientrano qui, dato che ProgressTarget è 1.
                case "continent_europe":
                case "continent_asia":
                case "continent_africa":
                case "continent_north_america":
                case "continent_south_america":
                case "continent_oceania":
                case "continent_antarctica":
                case "dream_achiever":
                case "photo_per_continent":
                case "loyalty_wanderglobe":
                case "leap_year_traveler":
                case "hidden_gem_finder": // Anche se la logica di ottenimento non è implementata
                    return CheckIfBadgeEarned(badgeDef) ? 1 : 0;

                default:
                    // Se un badge avesse un ProgressTarget > 1 non gestito sopra,
                    // questa logica di fallback si applicherebbe.
                    // Per la maggior parte dei tuoi badge con ProgressTarget = 1,
                    // la logica sopra è più esplicita.
                    if (badgeDef.ProgressTarget == 1)
                        return CheckIfBadgeEarned(badgeDef) ? 1 : 0;
                    return 0; // Nessuna progressione specifica definita
            }
        }

        public int GetCategoryOrder(string categoryName)
        {
            return categoryName switch
            {
                "Primi Passi" => 1, "Esploratore Globale" => 2, "Conquistatore di Continenti" => 3,
                "Pianificazione e Sogni" => 4, "Maestria Fotografica" => 5, "Scrittore di Viaggi" => 6,
                "Sfide Speciali" => 8,
                _ => 99,
            };
        }
    }

    public enum BadgeRarity { Comune, NonComune, Raro, Epico, Leggendario }

    public class BadgeDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }
        public string CriteriaDescription { get; set; }
        public int ProgressTarget { get; set; } = 1;
        public string Category { get; set; }
        public BadgeRarity Rarity { get; set; } = BadgeRarity.Comune;
        public bool IsSecret { get; set; } = false;
    }

    public class BadgeViewModel : BadgeDefinition
    {
        public bool IsEarned { get; set; }
        public int ProgressCurrent { get; set; }
        public double ProgressPercentage => ProgressTarget > 0 ? Math.Min((double)ProgressCurrent / ProgressTarget * 100, 100) : (IsEarned ? 100 : 0);
    }
}