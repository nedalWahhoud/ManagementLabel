// clear search box 
window.clearSearchBox = (id) => {
        const el = document.getElementById(id);
        if (el) {
            el.value = '';
            el.dispatchEvent(new Event('input', { bubbles: true }));

        }
};
// select text in input
window.selectTextById = (id) => {
    const el = document.getElementById(id);
    if (el && el.select) {
        el.select();
    }
};
// script wenn Handy dann wird Whatsapp geöffent sonst wird webseite von Whatsapp geöffenet
window.whatsappRedirect = {
    openWhatsAppWithoutNumber: function(message) {
        const text = encodeURIComponent(message);
        const url = `https://api.whatsapp.com/send?text=${text}`;

        // Gerättype prüfen
        const isMobile = /Android|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop/i.test(navigator.userAgent);

        if (isMobile) {
            // auf Handy versuche erst app zu öffnen
            window.location = url;
        } else {
            // auf Computer app nicht möglich, öffne Web
            window.open(url, '_blank');
        }
    },
openWhatsApp: function(phone, message) {
        const number = String(phone).replace(/\D/g, '');
        const text = message ? encodeURIComponent(message) : '';

        // linke
        const appUrl = text
            ? `whatsapp://send?phone=${number}&text=${text}`
            : `whatsapp://send?phone=${number}`;
        const webUrl = text
            ? `https://api.whatsapp.com/send?phone=${number}&text=${text}`
            : `https://wa.me/${number}`;

        // Gerättype prüfen
        const isMobile = /Android|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop/i.test(navigator.userAgent);

        if (isMobile) {
            // auf Handy versuche erst app zu öffnen
            window.location = appUrl;
        } else {
            // auf Computer app nicht möglich, öffne Web
            window.open(webUrl, '_blank');
        }
    }
};
// Funktion zur Statusprüfung bei der Rückkehr zum Browser
function checkAndReloadIfDead() {
    // Wenn der Benutzer zum Browser zurückkehrt und die Meldung „Verbindung wiederherstellen“ sieht, bedeutet dies, dass die Sitzung unterbrochen ist.

    // Wir laden die Seite sofort neu, um die korrekte Funktion der Schaltflächen zu gewährleisten.
    const reconnectModal = document.querySelector('.components-reconnect-show');
    if (reconnectModal) {
        console.log("Mobile browser resumed with dead session. Reloading...");
        location.reload();
    }
}

// Überwachung der Rückkehr des Nutzers zum Browser (nur mobil)
document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible') {
        checkAndReloadIfDead();
    }
});
//  Verbindung wiederherstellen
Blazor.start({
    circuit: {
        reconnectionHandler: {
            onConnectionDown: (options, error) => {
                return new Promise((resolve, reject) => {
                    const maxRetries = 8; // Anzahl der Versuche
                    let count = 0;

                    const attempt = () => {
                        count++;
                        if (navigator.onLine) {
                            resolve(); // Versuch, die Verbindung wiederherzustellen
                        } else {
                            if (count > maxRetries) {
                                location.reload(); // Wenn es 10 Mal fehlschlägt, führe eine vollständige Aktualisierung durch
                            } else {
                                const delay = Math.min(1000 * count, 5000); // Die einfache Strategie des „exponentiellen Rückgangs“: Die Wartezeit verlängert sich mit jedem Fehlschlag
                                setTimeout(attempt, delay); // Warten Sie und versuchen Sie es erneut.
                            }
                        }
                    };
                    attempt();
                });
            },
            // Wenn der Server die alte Sitzung nicht erkennt 
            onConnectionUp: () => {
                const checkCircuit = setTimeout(() => {
                    console.warn("Schaltkreis wiederhergestellt, reagiert aber nicht. Wird neu geladen...");
                    location.reload();
                }, 3000);
            }
        }
    }
});
// Code zum Wiederverbinden nach der Rückkehr von WhatsApp oder aus dem Hintergrund
window.addEventListener('focus', async () => {
    try {
        // Versuch der manuellen Wiederverbindung
        await Blazor.reconnect();
    } catch (e) {
        console.log("Reconnection attempt failed, but Blazor will keep trying...");
    }
});
// OnMap
window.mapRedirect = {
    openMap: function(latitude, longitude, address = '') {
        latitude = String(latitude).trim();
        longitude = String(longitude).trim();
        address = String(address || '').trim();

        // Wenn ein Name vorhanden ist, verwenden wir diesen nur als Suchanfrage; andernfalls verwenden wir die Koordinaten.
        const query = address ? encodeURIComponent(address) : `${latitude},${longitude}`;

        // Links basierend auf Namen oder Koordinaten
        const appleUrl = `maps://?ll=${latitude},${longitude}`;
        const googleAppUrl = `comgooglemaps://?q=${query}`;
        const webUrl = `https://www.google.com/maps/search/?api=1&query=${query}`;
        const androidUrl = `geo:${latitude},${longitude}?q=${query}`;

        const ua = navigator.userAgent || window.opera;
        const isIOS = /iPad|iPhone|iPod/.test(ua) && !window.MSStream;
        const isAndroid = /Android/.test(ua);

        const openWebFallback = () => window.open(webUrl, '_blank');

        if (isIOS) {
            // iOS: Versuchen Sie es zuerst mit Apple Maps.
            window.location = appleUrl;

            // Gleich: Probieren Sie die Google Maps App aus.
            setTimeout(() => {
                window.location = googleAppUrl;

                // Nach einer Sekunde: Öffnen Sie das Web als letzte Option.
                setTimeout(() => openWebFallback(), 1000);
            }, 1000);

        } else if (isAndroid) {
            // Android: nutzen geo URI
            window.location = androidUrl;

            // Web-Fallback nach einer Sekunde
            setTimeout(() => openWebFallback(), 1000);

        } else {
            //Jedes andere Gerät → Web direkt öffnen
            openWebFallback();
        }
    }
};
// OnPhone
window.phoneRedirect = {
    openPhoneDialer: function(phone) {
        const number = String(phone).replace(/[^\d+]/g, '');

        if (number) {
            window.location.href = `tel:${number}`;
        }
    }
};

// barcode scannen
var html5QrCode;
window.startLiveScanner = (dotNetHelper) => {
    // Erstelle das Objekt und verknüpfe es mit dem Element mit der ID "reader"
    html5QrCode = new Html5Qrcode("reader");

    const config = {
        fps: 10,
        qrbox: { width: 250, height: 150 } //Definition des Untersuchungsgebiets
    };

    html5QrCode.start(
        { facingMode: "environment" }, // Rückfahrkamera
        config,
        (decodedText) => {
            // Wenn die Messung erfolgreich ist, senden wir das Ergebnis an Bledzor.
            dotNetHelper.invokeMethodAsync('OnBarcodeScanned', decodedText);
            window.stopLiveScanner(); // Schalten Sie die Kamera nach dem Lesen aus.
        },
        (errorMessage) => { /* Die Suche wird fortgesetzt... */ }
    ).catch(err => {
        console.error("Unable to start scanning.", err);
    });
};

window.stopLiveScanner = () => {
    if (html5QrCode && html5QrCode.isScanning) {
        html5QrCode.stop().then(() => {
            html5QrCode.clear();
        });
    }
};
