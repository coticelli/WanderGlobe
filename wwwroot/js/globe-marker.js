// File: wwwroot/js/globe-markers.js
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        console.log("Script globe-markers.js caricato");

        // Attendi che il globo sia renderizzato completamente
        setTimeout(setupMarkers, 1500);
    });

    function setupMarkers() {
        console.log("Configurazione dei marker avviata");

        // 1. Trova il container del globo
        const globeContainer = document.getElementById('globe-container');
        if (!globeContainer) {
            console.error("Container del globo non trovato");
            return;
        }

        // 2. Trova la canvas principale
        const canvas = globeContainer.querySelector('canvas');
        if (!canvas) {
            console.error("Canvas del globo non trovata");
            return;
        }

        console.log("Canvas trovata:", canvas.width, "x", canvas.height);

        // 3. Crea l'overlay per i marker
        const overlay = document.createElement('div');
        overlay.className = 'globe-markers-overlay';
        overlay.style.cssText = `
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            z-index: 1000;
        `;
        globeContainer.appendChild(overlay);

        // 4. Crea il popup
        const popup = document.createElement('div');
        popup.className = 'country-popup';
        popup.style.display = 'none';
        document.body.appendChild(popup);

        // 5. Verifica i dati dei pin
        const pins = window.visitedPins || [];
        console.log(`Trovati ${pins.length} pin da visualizzare`);

        // 6. Inizializza un oggetto per memorizzare i marker creati
        const markers = [];

        // 7. Aggiungi una funzione per aggiornare i marker
        function updateMarkerPositions() {
            const markerElements = document.querySelectorAll('.custom-marker');
            markerElements.forEach(marker => {
                const pinId = parseInt(marker.dataset.id);
                const pin = pins.find(p => p.id === pinId);
                if (!pin) return;

                const position = getScreenPosition(pin.lat, pin.lng);
                if (position) {
                    marker.style.left = position.x + 'px';
                    marker.style.top = position.y + 'px';
                    marker.style.display = 'block';
                } else {
                    // Nascondi marker sul retro del globo
                    marker.style.display = 'none';
                }
            });
        }

        // 8. Crea i marker per ogni pin
        pins.forEach(pin => {
            const marker = document.createElement('div');
            marker.className = 'custom-marker';
            marker.dataset.id = pin.id;
            marker.style.cssText = `
                position: absolute;
                width: 24px;
                height: 24px;
                background-color: rgba(255, 71, 87, 0.8);
                border: 3px solid white;
                border-radius: 50%;
                transform: translate(-50%, -50%);
                cursor: pointer;
                pointer-events: auto;
                box-shadow: 0 0 10px rgba(0,0,0,0.3);
                transition: transform 0.2s ease, box-shadow 0.2s ease;
                z-index: 1001;
            `;

            // Eventi hover
            marker.addEventListener('mouseenter', function () {
                console.log("Hover su marker:", pin.name);
                this.style.transform = 'translate(-50%, -50%) scale(1.3)';
                this.style.boxShadow = '0 0 15px rgba(255,71,87,0.7)';

                // Mostra popup al hover
                const rect = this.getBoundingClientRect();
                updatePopup(pin, rect.left, rect.top);
            });

            marker.addEventListener('mouseleave', function () {
                this.style.transform = 'translate(-50%, -50%) scale(1)';
                this.style.boxShadow = '0 0 10px rgba(0,0,0,0.3)';
            });

            // Evento click
            marker.addEventListener('click', function (e) {
                e.stopPropagation();
                console.log("Click su marker:", pin.name);

                // Mostra popup
                const rect = this.getBoundingClientRect();
                updatePopup(pin, rect.left, rect.top);
            });

            overlay.appendChild(marker);
            markers.push(marker);
        });

        // 9. Funzione per aggiornare il popup
        function updatePopup(pin, x, y) {
            // Assicuriamoci che visitDate sia gestito correttamente
            let visitDate;
            try {
                visitDate = new Date(pin.visitDate).toLocaleDateString('it-IT', {
                    day: 'numeric', month: 'long', year: 'numeric'
                });
            } catch (e) {
                visitDate = pin.visitDate || "Data sconosciuta";
            }

            // Crea il contenuto HTML del popup
            popup.innerHTML = `
                <div class="popup-header">
                    <img src="/images/destinations/${pin.countryCode?.toLowerCase()}.jpg" 
                         onerror="this.src='/images/destinations/unknown.jpg'" 
                         alt="${pin.name}" 
                         class="popup-image">
                    <div class="popup-title">
                        <h3>${pin.name}</h3>
                        <p>${pin.countryName}</p>
                        <span class="popup-date">${visitDate}</span>
                    </div>
                    <button class="popup-close">&times;</button>
                </div>
                <div class="popup-content">
                    <p>${pin.description || "Hai visitato questo paese"}</p>
                    <div class="popup-actions">
                        <button class="popup-button" onclick="window.location.href='/Timeline?highlight=${pin.id}'">
                            Vedi dettagli
                        </button>
                    </div>
                </div>
            `;

            // Posizione il popup vicino al marker ma non fuori dallo schermo
            const popupWidth = 300;
            const popupHeight = 200;

            let popupX = x + 20;
            let popupY = y - 20;

            if (popupX + popupWidth > window.innerWidth) {
                popupX = x - popupWidth - 20;
            }

            if (popupY + popupHeight > window.innerHeight) {
                popupY = window.innerHeight - popupHeight - 20;
            }

            popup.style.left = popupX + 'px';
            popup.style.top = popupY + 'px';
            popup.style.display = 'block';

            // Evento per chiudere il popup
            popup.querySelector('.popup-close').addEventListener('click', function () {
                popup.style.display = 'none';
            });

            // Chiudi il popup quando si clicca altrove
            document.addEventListener('click', closePopupOnOutsideClick);
        }

        function closePopupOnOutsideClick(e) {
            if (!popup.contains(e.target) && !e.target.classList.contains('custom-marker')) {
                popup.style.display = 'none';
                document.removeEventListener('click', closePopupOnOutsideClick);
            }
        }

        // 10. Funzione per convertire lat/lng in posizione sullo schermo
        function getScreenPosition(lat, lng) {
            // Estrai le dimensioni dell'elemento canvas
            const rect = canvas.getBoundingClientRect();
            const width = rect.width;
            const height = rect.height;
            const centerX = width / 2;
            const centerY = height / 2;

            // Controlla se window.earth esiste (aggiunto dalla funzione initGlobe)
            if (window.earth && window.earthControls) {
                // Usa la rotazione attuale del globo per calcolare la posizione
                const rotation = window.earth.rotation;

                // Converti lat/lng in coordinate 3D (considerando la rotazione)
                const phi = (90 - lat) * Math.PI / 180;
                const theta = (lng + 180 - (rotation.y * 180 / Math.PI)) * Math.PI / 180;

                // Raggio del globo sullo schermo (approssimato)
                const radius = Math.min(width, height) * 0.4;

                // Calcola posizione 3D
                const x = centerX + radius * Math.sin(phi) * Math.cos(theta);
                const y = centerY - radius * Math.cos(phi);
                const z = radius * Math.sin(phi) * Math.sin(theta);

                // Se il punto è sul lato nascosto, non mostrarlo
                if (z < -0.5 * radius) {
                    return null;
                }

                return { x, y };
            } else {
                // Fallback: calcolo semplificato se window.earth non è disponibile
                const phi = (90 - lat) * Math.PI / 180;
                const theta = (lng + 180) * Math.PI / 180;

                const radius = Math.min(width, height) * 0.4;
                const x = centerX + radius * Math.sin(phi) * Math.cos(theta);
                const y = centerY - radius * Math.cos(phi);

                return { x, y };
            }
        }

        // 11. Aggiungi riferimento alla rotazione del globo
        if (!window.earth) {
            console.log("Creazione riferimento globo...");
            // Trova l'oggetto globo tramite Three.js
            const scenes = THREE.Object3D.getScene();
            if (scenes && scenes.length > 0) {
                const earthObjects = scenes[0].children.filter(obj =>
                    obj.type === "Mesh" && obj.geometry.type.includes("Sphere"));
                if (earthObjects.length > 0) {
                    window.earth = earthObjects[0];
                    console.log("Riferimento al globo creato");
                }
            }
        }

        // 12. Aggiorna posizione dei marker periodicamente
        updateMarkerPositions();
        setInterval(updateMarkerPositions, 100);

        // Gestisci clic globale per chiudere il popup
        document.addEventListener('click', function (e) {
            // Se il click non è su un marker e non è sul popup, chiudi il popup
            if (!e.target.classList.contains('custom-marker') && !popup.contains(e.target)) {
                popup.style.display = 'none';
            }
        });

        console.log("Setup dei marker completato con successo!");
    }
})();