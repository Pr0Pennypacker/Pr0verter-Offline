# Installation
Um pr0verter offline nutzen zu können, **müsst ihr ffmpeg installieren**!
1. Downloadet euch ein [Windows Build von ffmpeg](https://ffmpeg.zeranoe.com/builds/)
2. Entpackt die ZIP und zieht den darin befindlichen Ordner irgendwo hin
3. Nun in der Windows suche (oder speziell in der Systemsteuerung) nach "Systemumgebungsvariablen bearbeiten" suchen. Klickt auf "Umgebungsvariablen..." und wählt im zweiten Abschnitt Path aus. Klickt nun auf bearbeiten. Anschließend erstellt einen neuen Eintrag mit dem Pfad zu dem bin ordner.
4. Öffnet die Eingabeaufforderung und gebt ffmpeg und ffprobe ein. Beides sollte euch Informationen über die Programme liefern, dann habt ihr alles richtig gemacht.

Jetzt einfach auf die aktuelle .exe in GitHub klicken, downloaden und loslegen!

PS: Die Windows Defender Meldung kommt, weil die .exe von einem unbekannten Herausgeber ist. @5yn74x hat sich den [Code angeschaut](https://pr0gramm.com/new/3322321:comment31584466)
# Features
- Konvertierung in das MP4 oder WebM format, MP4 wird auf dem Server nicht neu kodiert
- Konvertiere alle Frames in einem Zeitbereich in PNG's und erstelle deine eigenen Memes
- Wähle aus 6 Presets und schlag dich nicht mit Einstellungen herum
- Die Typischen pr0verter Optionen (Dateigröße, Start-/Endzeit, Audioqualität, Auflösung anpassen)
- 2-Pass Kodierung
- Frameratekappung bei 30 Rahmen/Sekunde
- Audiokanäle zu einem Monokanal mixen
# FAQ
**Warum geht der Scheiß nicht?**<br>
Bist du der Installationsanleitung gefolgt? Sonst kannst du [mir](https://pr0gramm.com/user/Pennypacker) gerne eine Nachricht senden, oder ein Issue auf Github eröffnen.

**Warum ist die Datei bei der MP4 Konvertierung kleiner als bei der WebM Konvertierung?**<br>
Wäre die Datei größer, würde Pr0gramm neu kodieren und sie kleiner machen. Euer WebM wird im Endeffekt auch so klein sein und auch noch doppelt kodiert.<br>

**Kannst du XY hinzufügen?**<br>
Schreib [mir](https://pr0gramm.com/user/Pennypacker) gerne eine Nachricht mit Verbesserungsvorschlägen!
# Changelog
**Version 1.2 Hotfix**
- Es wird nun auf Updates geprüft

**Version 1.2**
- Unterstützung für mp4 hinzugefügt
- Frames können nun in einem Zeitbereich als PNG's exportiert werden
- Der Dateiname, unter dem gespeichert wird, kann nun gewählt werden (bei PNG der Ordner)
- FPS werden (wenn >30) auf 30 reduziert
- Presets (HQ, Normal, Schnell, Pr0mium HQ, Pr0mium Normal, Pr0mium schnell) hinzugefügt
- Es muss nun keine Start-/Endzeit mehr angegeben werden, wenn das komplette Video konvertiert werden soll
- Es wurden einige Exceptions hinzugefügt
- Komplette Überarbeitung des Codes
# Credits
Erstellt von [Pennypacker](https://pr0gramm.com/user/Pennypacker)
