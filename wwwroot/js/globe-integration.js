namespace WanderGlobe.wwwroot.js
{
    public class globe_integration
    {
        // Modifica la funzione initHeroGlobe nel tuo file JavaScript principale

function initHeroGlobe() {
        const container = document.getElementById('heroGlobe');

        // Inizializza scena, camera, renderer come prima
        const scene = new THREE.Scene();
        const camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 0.1, 1000);
        camera.position.z = 200;

        const renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });
        renderer.setSize(window.innerWidth, window.innerHeight);
        renderer.setPixelRatio(window.devicePixelRatio);
        container.appendChild(renderer.domElement);

        // Crea il globo e le nuvole come prima
        // ...

        // Dopo aver creato il globo e impostato la scena

        // Aggiungi il Raycaster per interazione con gli oggetti 3D
        const raycaster = new THREE.Raycaster();
        const mouse = new THREE.Vector2();

        // Event listener per il mouse
        container.addEventListener('mousemove', onMouseMove, false);
        container.addEventListener('click', onMouseClick, false);

        function onMouseMove(event) {
            // Calcola posizione del mouse normalizzata
            mouse.x = (event.clientX / window.innerWidth) * 2 - 1;
            mouse.y = -(event.clientY / window.innerHeight) * 2 + 1;

            // Aggiorna il raycaster
            raycaster.setFromCamera(mouse, camera);

            // Interseca gli oggetti
            const intersects = raycaster.intersectObjects(scene.children, true);

            // Verifica se stiamo passando sopra un marker
            let hoveredMarker = false;

            for (let i = 0; i < intersects.length; i++) {
                const object = intersects[i].object;
                if (object.type === 'Mesh' && object !== globe && object !== clouds) {
                    // Abbiamo trovato un marker
                    hoveredMarker = true;

                    // Cambia il cursore
                    container.style.cursor = 'pointer';

                    // Esegui l'evento di hover se non è già attivo
                    if (activeMarker !== object) {
                        if (activeMarker) {
                            // Simula uscita dal marker precedente
                            handleMarkerOut({ target: activeMarker });
                        }

                        // Simula entrata nel nuovo marker
                        handleMarkerHover({ target: object });
                    }

                    break;
                }
            }

            // Se non ci sono marker sotto il mouse
            if (!hoveredMarker && activeMarker) {
                container.style.cursor = 'default';
                // Gestisci l'uscita dal marker solo se non siamo sul popup
                if (!isMouseOverPopup(event)) {
                    handleMarkerOut({ target: activeMarker });
                }
            }
        }

        function onMouseClick(event) {
            // Aggiorna il raycaster
            raycaster.setFromCamera(mouse, camera);

            // Interseca gli oggetti
            const intersects = raycaster.intersectObjects(scene.children, true);

            for (let i = 0; i < intersects.length; i++) {
                const object = intersects[i].object;
                if (object.type === 'Mesh' && object !== globe && object !== clouds) {
                    // Abbiamo cliccato su un marker
                    handleMarkerClick({ target: object });
                    break;
                }
            }
        }

        // Verifica se il mouse è sopra il popup
        function isMouseOverPopup(event) {
            if (!markerPopup || markerPopup.style.display === 'none') return false;

            const rect = markerPopup.getBoundingClientRect();
            return (
                event.clientX >= rect.left &&
                event.clientX <= rect.right &&
                event.clientY >= rect.top &&
                event.clientY <= rect.bottom
            );
        }

        // Carica i dati dei pin
        const pins = @Html.Raw(Json.Serialize(Model.VisitedPins));

        // Inizializza i marker del globo
        initGlobeMarkers();

        // Animazione del globo come prima
        function animate() {
            requestAnimationFrame(animate);

            globe.rotation.y += 0.0005;
            clouds.rotation.y += 0.0007;

            renderer.render(scene, camera);
        }

        animate();

        // Responsive
        window.addEventListener('resize', function () {
            camera.aspect = window.innerWidth / window.innerHeight;
            camera.updateProjectionMatrix();
            renderer.setSize(window.innerWidth, window.innerHeight);
        });
    }
    }
}
