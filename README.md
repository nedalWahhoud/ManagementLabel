# 💡 Über das Projekt
Ich habe diese einfache Webanwendung entwickelt, um meine Fähigkeiten in Softwareentwicklung zu zeigen.
![Screenshot 2025-05-17 213139](https://github.com/user-attachments/assets/73f141e3-eb69-470e-9d47-1a2736abde25)
Die App ermöglicht die Verwaltung von Produkten und Erstellung von Barcode/Etikett mit unterschiedlichen Benutzerrechten:
## 👤 Normale Benutzer können:
- Barcode für Product erstellen enthält: Id,Name,Preis
- Etikett für Produkt erstellen
## 🔐 Admins haben zusätzlich die Möglichkeit:
- Neue Produkte hinzuzufügen
- Produkte zu bearbeiten
- Produkte zu löschen </br>
Beide Benutzerrollen können also Barcode/Etikett erstellen, speichern oder ausdrucken
![Screenshot 2025-05-17 213206](https://github.com/user-attachments/assets/897fec4a-5d7a-41e4-a165-bfde8ecbf7a0)
## 🛠️ Verwendete Technologien
- Frontend:   
  - Blazor WebAssembly-App (C#)
  - HTML
  - CSS
  - Javascript
- Backend:
  - PHP API zur Login-Authentifizierung und Token-Erzeugung
- Databank:
  - MySQL
# 👥 Benutzerverwaltung
Neue Benutzerkonten werden ausschließlich vom Datenbankadministrator in der Datenbank angelegt.</br>
Eine Registrierung über die Web-App ist nicht vorgesehen.
# Testzugang
- Admin-Konto </br>
  Benutzername: admin </br>
  Passwort: !123456
- User-Konto </br>
  Benutzername: user </br>
  Passwort: user123456 
