// Globe Markers Hover Effects Module
(function() {
    // Variabili globali per il gestore dei marker
    let activeMarker = null;
    let markerPopup = null;

    // Esponi le funzioni necessarie all'oggetto window
    window.initGlobeMarkers = initGlobeMarkers;
    window.openDetailView = openDetailView;
    window.openGallery = openGallery;

    function initGlobeMarkers() {
        // Crea l'elemento popup che apparirà al passaggio del mouse
        markerPopup = document.createElement('div');
        markerPopup.className = 'marker-popup';
        markerPopup.style.display = 'none';
        document.body.appendChild(markerPopup);

        // Crea i marker sul globo con eventi
        setupMarkers();

        // Aggiungi listener globale per il movimento del mouse
        window.addEventListener('mousemove', updatePopupPosition);

        // Listener per chiudere il popup quando si clicca fuori
        document.addEventListener('click', function (e) {
            if (!markerPopup.contains(e.target) && !e.target.classList.contains('globe-marker')) {
                hidePopup();
            }
        });
    }

    function setupMarkers() {
        // Per ogni marker sulla mappa...
        pins.forEach(pin => {
            // Crea il marker sulla superficie del globo
            const marker = addGlobePin(globe, pin.lat, pin.lng, 0xff4757);

            // Collega i dati della visita al marker
            marker.userData = {
                id: pin.id,
                name: pin.name,
                cityName: pin.cityName || pin.name,
                countryName: pin.countryName,
                countryCode: pin.countryCode,
                visitDate: pin.visitDate,
                description: pin.description,
                mainPhoto: pin.mainPhoto || `/images/destinations/${pin.countryCode.toLowerCase()}.jpg`,
                photos: pin.photos || [],
                weather: pin.weather || { temp: "22°C", condition: "Soleggiato" },
                tags: pin.tags || ["Viaggio", "Esplorazione"]
            };

            // Aggiungi interattività al marker
            marker.cursor = 'pointer';
            marker.interactive = true;

            // Eventi del mouse sul marker
            marker.addEventListener('mouseover', handleMarkerHover);
            marker.addEventListener('mouseout', handleMarkerOut);
            marker.addEventListener('click', handleMarkerClick);
        });
    }

    function addGlobePin(globe, lat, lng, color) {
        const phi = (90 - lat) * Math.PI / 180;
        const theta = (lng + 180) * Math.PI / 180;

        const radius = 51;
        const x = -radius * Math.sin(phi) * Math.cos(theta);
        const y = radius * Math.cos(phi);
        const z = radius * Math.sin(phi) * Math.sin(theta);

        const pinGeometry = new THREE.SphereGeometry(0.8, 16, 16);
        const pinMaterial = new THREE.MeshBasicMaterial({ color: color });
        const pin = new THREE.Mesh(pinGeometry, pinMaterial);

        pin.position.set(x, y, z);
        scene.add(pin);

        // Effetto glow
        const glowGeometry = new THREE.SphereGeometry(1.2, 16, 16);
        const glowMaterial = new THREE.MeshBasicMaterial({
            color: color,
            transparent: true,
            opacity: 0.3
        });
        const glow = new THREE.Mesh(glowGeometry, glowMaterial);
        glow.position.set(x, y, z);
        scene.add(glow);

        // Aggiungi animazione pulsante
        animateGlowEffect(glow);

        return pin;
    }

    function animateGlowEffect(glow) {
        let scale = 1.0;
        let growing = true;

        function animate() {
            if (growing) {
                scale += 0.01;
                if (scale >= 1.3) growing = false;
            } else {
                scale -= 0.01;
                if (scale <= 1.0) growing = true;
            }

            glow.scale.set(scale, scale, scale);
            requestAnimationFrame(animate);
        }

        animate();
    }

    function handleMarkerHover(event) {
        // Salva il marker attivo
        activeMarker = event.target;

        // Aumenta leggermente la dimensione del marker
        gsap.to(activeMarker.scale, { x: 1.5, y: 1.5, z: 1.5, duration: 0.3, ease: "power2.out" });

        // Aggiorna il contenuto del popup
        updatePopupContent(activeMarker.userData);

        // Mostra il popup con animazione
        showPopup();
    }

    function handleMarkerOut(event) {
        // Non nascondere subito il popup quando il mouse esce dal marker
        // Lo gestiremo con un po' di ritardo per facilitare l'interazione con il popup

        // Ripristina la dimensione originale del marker
        gsap.to(event.target.scale, { x: 1, y: 1, z: 1, duration: 0.3, ease: "power2.out" });

        // Controlliamo se il mouse è entrato nel popup
        markerPopup.addEventListener('mouseenter', function () {
            // Il mouse è entrato nel popup, non lo nascondiamo
            clearTimeout(markerPopup.hideTimeout);
        });

        markerPopup.addEventListener('mouseleave', function () {
            // Il mouse è uscito dal popup, lo nascondiamo
            hidePopup();
        });

        // Imposta un timeout per nascondere il popup se il mouse non ci entra
        markerPopup.hideTimeout = setTimeout(hidePopup, 300);
    }

    function handleMarkerClick(event) {
        // Apri la vista dettagliata del luogo
        const markerData = event.target.userData;
        openDetailView(markerData.id);
    }

    function updatePopupContent(markerData) {
        // Formatta la data in modo leggibile
        const formattedDate = markerData.visitDate ? new Date(markerData.visitDate).toLocaleDateString('it-IT', {
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        }) : 'Data sconosciuta';

        // Prepara il markup per il popup
        markerPopup.innerHTML = `
        <div class="popup-header">
            <img src="${markerData.mainPhoto}" alt="${markerData.cityName}" class="popup-main-image">
            <div class="popup-country-flag">
                <img src="/images/flags/${markerData.countryCode.toLowerCase()}.png" alt="${markerData.countryName}">
            </div>
            <div class="popup-city-info">
                <h3>${markerData.cityName}</h3>
                <p>${markerData.countryName}</p>
            </div>
        </div>
        
        <div class="popup-body">
            <div class="popup-visit-info">
                <div class="popup-date">
                    <i class="far fa-calendar"></i> ${formattedDate}
                </div>
                <div class="popup-weather">
                    <i class="fas fa-temperature-high"></i> ${markerData.weather.temp} · ${markerData.weather.condition}
                </div>
            </div>
            
            <div class="popup-description">
                "${markerData.description || 'Nessuna descrizione disponibile per questa destinazione.'}"
            </div>
            
            <div class="popup-tags">
                ${markerData.tags.map(tag => `<span class="popup-tag">${tag}</span>`).join('')}
            </div>
            
            ${generatePhotoGallery(markerData.photos, markerData.id)}
            
            <div class="popup-actions">
                <button class="popup-action-btn view-details" onclick="openDetailView(${markerData.id})">
                    <i class="fas fa-info-circle"></i> Dettagli
                </button>
                <button class="popup-action-btn view-gallery" onclick="openGallery(${markerData.id})">
                    <i class="fas fa-images"></i> Galleria
                </button>
                <button class="popup-action-btn add-favorite">
                    <i class="far fa-heart"></i> Preferiti
                </button>
            </div>
        </div>
    `;

        // Anima i tag
        setTimeout(() => {
            const tags = markerPopup.querySelectorAll('.popup-tag');
            tags.forEach((tag, i) => {
                gsap.from(tag, {
                    opacity: 0,
                    y: 10,
                    delay: 0.1 + (i * 0.1),
                    duration: 0.4
                });
            });
        }, 300);
    }

    function generatePhotoGallery(photos, markerId) {
        if (!photos || photos.length === 0) {
            return '';
        }

        // Crea un layout di tipo collage per le foto
        return `
        <div class="popup-photo-gallery">
            ${photos.slice(0, 4).map((photo, index) => {
            const classes = photos.length <= 2 ? 'photo-item large' : 'photo-item';
            return `
                    <div class="${classes}" data-index="${index}">
                        <img src="${photo.url}" alt="${photo.caption || 'Foto della destinazione'}">
                    </div>
                `;
        }).join('')}
            ${photos.length > 4 ? `
                <div class="photo-item more-photos" onclick="openGallery(${markerId})">
                    <span>+${photos.length - 4}</span>
                </div>
            ` : ''}
        </div>
    `;
    }

    function showPopup() {
        // Mostra il popup
        markerPopup.style.display = 'block';

        // Posiziona inizialmente il popup
        updatePopupPosition();

        // Anima l'entrata del popup
        gsap.fromTo(markerPopup,
            { opacity: 0, scale: 0.8, y: 20 },
            { opacity: 1, scale: 1, y: 0, duration: 0.3, ease: "back.out(1.7)" }
        );

        // Anima l'immagine principale
        const mainImage = markerPopup.querySelector('.popup-main-image');
        gsap.fromTo(mainImage,
            { scale: 1.2, opacity: 0.5 },
            { scale: 1, opacity: 1, duration: 0.6, ease: "power2.out" }
        );

        // Anima l'entrata delle foto della galleria
        const galleryPhotos = markerPopup.querySelectorAll('.photo-item');
        galleryPhotos.forEach((photo, i) => {
            gsap.fromTo(photo,
                { opacity: 0, y: 15 },
                { opacity: 1, y: 0, duration: 0.4, delay: 0.1 + (i * 0.1), ease: "power2.out" }
            );
        });
    }

    function hidePopup() {
        // Nascondi il popup con animazione
        gsap.to(markerPopup, {
            opacity: 0,
            scale: 0.8,
            y: -20,
            duration: 0.2,
            onComplete: () => {
                markerPopup.style.display = 'none';
            }
        });

        // Reset del marker attivo
        activeMarker = null;
    }

    function updatePopupPosition(e) {
        if (!activeMarker || markerPopup.style.display === 'none') return;
        
        // Se abbiamo un evento mouse, posiziona vicino al puntatore
        if (e) {
            // Calcola la posizione ottimale per il popup
            const x = e.clientX + 15;
            const y = e.clientY - 15;
            
            // Assicurati che il popup non esca dallo schermo
            const popupWidth = markerPopup.offsetWidth || 300;
            const popupHeight = markerPopup.offsetHeight || 200;
            
            // Aggiusta posizione se necessario
            let finalX = x;
            let finalY = y;
            
            if (finalX + popupWidth > window.innerWidth) {
                finalX = e.clientX - popupWidth - 15;
            }
            
            if (finalY + popupHeight > window.innerHeight) {
                finalY = window.innerHeight - popupHeight - 15;
            }
            
            if (finalY < 10) finalY = 10;
            
            // Applica la posizione
            markerPopup.style.left = `${finalX}px`;
            markerPopup.style.top = `${finalY}px`;
        }
        // Altrimenti, posiziona vicino al marker (utile per chiamate manuali)
        else if (activeMarker) {
            // Converti posizione 3D in coordinate schermo
            const vector = new THREE.Vector3();
            vector.copy(activeMarker.position);
            
            // Proietta sul canvas
            vector.project(camera);
            
            // Converti in coordinate 2D
            const canvas = renderer.domElement;
            const x = (vector.x * 0.5 + 0.5) * canvas.clientWidth + canvas.offsetLeft;
            const y = (1 - (vector.y * 0.5 + 0.5)) * canvas.clientHeight + canvas.offsetTop;
            
            // Posiziona il popup
            markerPopup.style.left = `${x + 15}px`;
            markerPopup.style.top = `${y - 15}px`;
        }
    }

    function get2DPositionFromMarker(marker) {
        // Crea un vettore temporaneo per la posizione del marker
        const vector = new THREE.Vector3();

        // Ottieni la posizione mondiale del marker
        marker.getWorldPosition(vector);

        // Proietta dal 3D al 2D
        vector.project(camera);

        // Converti le coordinate normalizzate in coordinate pixel
        const x = (vector.x * 0.5 + 0.5) * window.innerWidth;
        const y = (vector.y * -0.5 + 0.5) * window.innerHeight;

        // Controlla se il marker è davanti alla telecamera
        if (vector.z > 1) {
            return null; // Il marker è dietro la telecamera
        }

        return { x, y };
    }

    function openDetailView(visitId) {
        // Reindirizza alla pagina dettaglio
        window.location.href = `/Timeline?highlight=${visitId}`;
    }

    function openGallery(visitId) {
        // Apri la galleria a schermo intero
        const photoModal = new bootstrap.Modal(document.getElementById('galleryModal'));

        // Recupera le foto dell'utente per questo visitId
        fetchUserPhotos(visitId).then(photos => {
            // Imposta le foto nel carousel
            const carousel = document.querySelector('#galleryCarousel .carousel-inner');
            carousel.innerHTML = '';

            photos.forEach((photo, index) => {
                const slideClass = index === 0 ? 'carousel-item active' : 'carousel-item';
                const slideHtml = `
                <div class="${slideClass}">
                    <img src="${photo.url}" class="d-block" alt="${photo.caption || ''}">
                    <div class="carousel-caption d-none d-md-block">
                        <p>${photo.caption || ''}</p>
                    </div>
                </div>
            `;
                carousel.innerHTML += slideHtml;
            });

            photoModal.show();
        });
    }

    // Funzione mock per recuperare le foto dell'utente
    function fetchUserPhotos(visitId) {
        // Qui in una implementazione reale faresti una chiamata AJAX
        // Per ora restituiamo alcuni dati di esempio
        return Promise.resolve([
            { id: 1, url: "/images/sample-photos/photo1.jpg", caption: "Vista panoramica" },
            { id: 2, url: "/images/sample-photos/photo2.jpg", caption: "Piazza principale" },
            { id: 3, url: "/images/sample-photos/photo3.jpg", caption: "Monumento storico" },
            { id: 4, url: "/images/sample-photos/photo4.jpg", caption: "Tramonto sulla spiaggia" }
        ]);
    }
})();
