// File da salvare come: C:\Users\fabio\OneDrive\Desktop\WanderGlobe\wwwroot\js\globe-hover.js

document.addEventListener('DOMContentLoaded', function () {
    console.log("Inizializzazione effetto hover sui marker...");

    // 1. Crea il div del popup
    const popup = document.createElement('div');
    popup.className = 'marker-popup';
    popup.style.display = 'none';
    document.body.appendChild(popup);

    // 2. Inizializza con dati di esempio se non esistono
    if (!window.visitedPins || window.visitedPins.length === 0) {
        console.log("Nessun dato per i pin trovato, uso dati di esempio");
        window.visitedPins = [
            {
                id: 1, name: "Italia", countryName: "Italia",
                countryCode: "it", lat: 41.9, lng: 12.5,
                visitDate: "2023-06-15", description: "Bellissimo viaggio in Italia"
            },
            {
                id: 2, name: "Francia", countryName: "Francia",
                countryCode: "fr", lat: 48.8, lng: 2.3,
                visitDate: "2022-08-10", description: "Viaggio romantico a Parigi"
            }
        ];
    }

    // 3. Metodo per l'aggiornamento del contenuto del popup
    function updatePopupContent(pinData) {
        const visitDate = new Date(pinData.visitDate);
        const formattedDate = visitDate.toLocaleDateString('it-IT', {
            day: 'numeric', month: 'long', year: 'numeric'
        });

        popup.innerHTML = `
            <div class="popup-header">
                <img src="/images/destinations/${pinData.countryCode}.jpg" 
                     alt="${pinData.name}" 
                     class="popup-main-image"
                     onerror="this.src='/images/destinations/unknown.jpg'">
                <div class="popup-country-flag">
                    <img src="/images/flags/${pinData.countryCode}.png" 
                         alt="${pinData.countryName}"
                         onerror="this.src='/images/flags/unknown.png'">
                </div>
                <div class="popup-city-info">
                    <h3>${pinData.name}</h3>
                    <p>${pinData.countryName}</p>
                </div>
            </div>
            
            <div class="popup-body">
                <div class="popup-visit-info">
                    <div class="popup-date">
                        <i class="far fa-calendar"></i> ${formattedDate}
                    </div>
                </div>
                
                <div class="popup-description">
                    "${pinData.description}"
                </div>
                
                <div class="popup-actions">
                    <button class="popup-action-btn view-details" onclick="window.location.href='/Timeline?highlight=${pinData.id}'">
                        <i class="fas fa-info-circle"></i> Dettagli
                    </button>
                    <button class="popup-action-btn add-favorite">
                        <i class="far fa-heart"></i> Preferiti
                    </button>
                </div>
            </div>
        `;
    }

    // 4. Funzioni per mostrare e nascondere il popup
    function showPopup(x, y) {
        popup.style.left = `${x + 20}px`;
        popup.style.top = `${y}px`;
        popup.style.display = 'block';

        gsap.fromTo(popup,
            { opacity: 0, scale: 0.8, y: y + 20 },
            { opacity: 1, scale: 1, y: y, duration: 0.3, ease: "back.out(1.7)" }
        );
    }

    function hidePopup() {
        gsap.to(popup, {
            opacity: 0,
            scale: 0.8,
            duration: 0.2,
            onComplete: () => {
                popup.style.display = 'none';
            }
        });
    }

    // 5. Trova tutti i punti rossi (marker) sulla canvas
    function setupMarkerHoverEvents() {
        console.log("Configurazione eventi hover sui marker");

        // Ottieni l'elemento canvas del globo
        const globeCanvas = document.querySelector('canvas');
        if (!globeCanvas) {
            console.error("Canvas del globo non trovata");
            return;
        }

        // Intercetta gli eventi mouse sull'intera canvas
        globeCanvas.addEventListener('mousemove', function (event) {
            handleMouseMove(event);
        });

        // Implementa un sistema di hover basato sulla posizione del mouse
        let activePin = null;
        let mouseOverMarker = false;

        function handleMouseMove(event) {
            const mouseX = event.clientX;
            const mouseY = event.clientY;

            // Controlla se siamo sopra un marker
            mouseOverMarker = false;
            let closestPin = null;
            let minDistance = Infinity;

            // Converti i punti 3D in 2D basandoci sulle coordinate dello schermo
            window.visitedPins.forEach(pin => {
                // Usa la proiezione semplificata per trovare la posizione 2D del marker
                const position2D = projectPointToScreen(pin.lat, pin.lng);
                if (!position2D) return;

                const distance = Math.sqrt(
                    Math.pow(mouseX - position2D.x, 2) +
                    Math.pow(mouseY - position2D.y, 2)
                );

                // Se il mouse è abbastanza vicino (entro 20 pixel dal centro del marker)
                if (distance < 20 && distance < minDistance) {
                    mouseOverMarker = true;
                    minDistance = distance;
                    closestPin = pin;
                }
            });

            // Gestisci l'hover
            if (mouseOverMarker && closestPin) {
                document.body.style.cursor = 'pointer';

                // Se abbiamo un nuovo marker attivo
                if (activePin !== closestPin) {
                    activePin = closestPin;
                    updatePopupContent(closestPin);
                    showPopup(mouseX, mouseY);
                }
            } else {
                document.body.style.cursor = 'default';

                // Se il mouse non è più su un marker e non è sul popup
                if (activePin && !isMouseOverPopup(event)) {
                    activePin = null;
                    hidePopup();
                }
            }
        }

        function isMouseOverPopup(event) {
            if (popup.style.display === 'none') return false;

            const rect = popup.getBoundingClientRect();
            return (
                event.clientX >= rect.left &&
                event.clientX <= rect.right &&
                event.clientY >= rect.top &&
                event.clientY <= rect.bottom
            );
        }
    }

    // Funzione per proiettare coordinate lat/lng sulla schermo 2D
    function projectPointToScreen(lat, lng) {
        // Ottieni le dimensioni della canvas
        const globeCanvas = document.querySelector('canvas');
        if (!globeCanvas) return null;

        const width = globeCanvas.width;
        const height = globeCanvas.height;
        const centerX = width / 2;
        const centerY = height / 2;

        // Converti lat/lng in coordinate 3D sferiche
        const phi = (90 - lat) * Math.PI / 180;
        const theta = (lng + 180) * Math.PI / 180;

        // Calcola la posizione sulla sfera
        // Nota: Questa è una proiezione semplificata che funziona bene solo per il lato visibile del globo
        // Per una proiezione più accurata, avremmo bisogno della matrice di proiezione di Three.js
        const radius = Math.min(width, height) * 0.4; // Raggio approssimativo del globo su schermo

        let x = centerX - radius * Math.sin(phi) * Math.cos(theta);
        let y = centerY - radius * Math.cos(phi);

        // Se il punto è sul lato nascosto del globo, non mostrarlo
        const visibleThreshold = -0.2; // Consenti una piccola parte del lato nascosto
        const visibility = Math.sin(phi) * Math.sin(theta);
        if (visibility < visibleThreshold) {
            return null;
        }

        return { x, y };
    }

    // 6. Avvia il rilevamento dopo un breve ritardo (per essere sicuri che il globo sia caricato)
    setTimeout(setupMarkerHoverEvents, 1000);

    // Debug: mostra che lo script è stato caricato
    console.log("Script di hover sui marker caricato con successo");
});