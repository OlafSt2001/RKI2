Ein Versuch, mein altes WPF-Projekt zur Anzeige von Corona-Inzidenzen auf MVVM umzubauen.

Wobei Umbau sehr nett formuliert ist, tatsächlich ist es eine völlige Neuentwicklung basierend auf den Erfahrungen aus dem alten Projekt und ein paar Dingen, die ich
on the Job aufgeschnappt habe.

Anstelle nun jedesmal aufwändig die Bundesländer, Kreise und deren Geokoordinaten aus dem riesigen und wirklich dämlich konstruierten RKI-JSON zu extrahieren, habe ich einen
kleinen Extraktor gebastelt, der diese Daten aus dem JSON herauslöst und in einem Satz CSV-Files bzw. einer Access-DB hinterlegt.
Anschließend entwarf ich eine DLL, die diese Daten fein aufbereitet und mit einem einfachen Interface für interessierte Applikationen wie eben dieser MVVM-App zur Verfügung stellt.
Wie immer geht es in der DLL heftig mit LINQ, PLINQ und Multithreaded zur Sache, denn die Geokoordinaten zum Zeichnen der Landkreise haben z.T. enormen Umfang, da sollte man alle Kerne
nutzen, die man kriegen kann.

Die App selbst soll idealerweise so aussehen wie die alte Anwendung, allerdings vollständig mit dem MVVM-Pattern gebaut.

Geplant ist, das ganze auch noch mit Blazor zu machen, das sollte dann ja nur mehr ein kleines Problem sein, die Views sollen sich ja im Easy-Going-Mode austauschen lassen
und das MVVM-Pattern muss hier zeigen, was geht - sonst ist das Mist.
