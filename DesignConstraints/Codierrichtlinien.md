## Codierrichtlinien
### C# Code
* Es soll wenn dies m�glich ist niemals var verwendet werden, sondern immer der explizite Typ
* Die Einr�ckung betr�gt 4 Leerzeichen und es sollen keine Tabulatoren verwendet werden
* Es sind immer geschweifte Klammern zu verwenden, auch wenn nur eine einzelne Codezeile folgt
* Bei case-Anweisungen sind keine geschweiften Klammern zu verwenden. Statt dessen wird mit case bzw. default und break; abgegrenzt.
* Methoden die etwas zur�ck geben d�rfen nur eine einzige return Anweisung ganz am Ende enthalten. Die erste Zeile besteht immer aus 
der Definition und ggfs. Initialisierung der R�ckgabewariable. Diese muss grunds�tzlich result heissen. Damit lautet die letzte Zeile einer 
Methode mit R�ckgabewert immer: return result;
* Private Feldvariablen, die durch ein Property gesetzt oder gelsen werden sollen nicht oben in der Klasse, sondern �ber dem Property definiert werden.
* Namespaces sind durch einen Block zu definieren und nicht als einzelne Zeile
* Kommentare sind grunds�tzlich in Englisch zu verfassen
* LINQ-Anweisungen bzw. Lambda sollen nur f�r Such- und Auswahloperationen in Collections verwendet werden, sonst nicht 
* Unn�tige using Anweisungen sind zu entfernen
* In Schleifen als Z�hlervariablen sprechende Namen wie "counter" oder "index" verwenden und nicht z.B. i
* F�r Variablen stets sprechende Namen verwenden z.B. in einer Iterationsschleife "currentElement" wenn die Klasse "Element" heisst 
* Zahlen in Variablenamen wenn m�glich vermeiden
* Wenn dies vermeidbar ist, kein this. verwenden beim Zugriff auf Elemente der Klasse
* Private Feldvariablen beginnen immer mit einem Unterstrich im Namen, z.B. _title
* Blazor-Komponenten enthalten keinen @code-Block. Der C#-Code steht immer in einer Code-behind-Datei (<Komponente>.razor.cs) als partial class im passenden Namespace; in der .razor-Datei bleiben nur das Markup und die Direktiven @inherits und @typeparam (Dependency Injection per [Inject]-Property in der Code-behind-Datei).
