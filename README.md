Studentnaam: Daniël van de Ruit

Studentnummer: 593977

---

# Algemene beschrijving applicatie

MultiChat is een applicatie die het mogelijk maakt om tekstberichten te versturen naar andere gebruikers die verbonden zijn met dezelfde server. In de applicatie is bewust geen gebruik gemaakt van een nieuwe techniek om te leren hoe een student om moet gaan met een verouderde techniek zoals TcpClient en TcpListener.

De applicatie is verdeeld in 3 delen. Allereerst de server. De server is een Xamarin Mac applicatie waarin het mogelijk is om de server te starten/stoppen, de clients te bekijken, de berichten te zien en om zelf berichten te sturen. De server maakt zoals gezegd onder de motorkap gebruik van een TcpListener. Daarnaast zijn de meest bijzondere technieken die gebruik zijn een variabele buffer size (aan te passen wanneer de server gestart is) en het gebruik van de NSTableView van Apple. Dit laatste is bijzonder omdat er een bepaalde structuur aangehouden moet worden welke Apple voorschrijft, wil dit werken.

Het tweede deel van de server is de client. De client is ook een Xamarin Mac applicatie die min of meer dezelfde mogelijkheden heeft als de server; berichten ontvangen/sturen. Uiteraard start de client geen server, maar verbindt de client met de server. Zowel de variabele buffer size als de NSTableView zijn ook in de client geïmplementeerd.

Tot slot de Core van de applicatie. Deze Xamarin Mac Class Library is een gedeelde library voor de client en de server. Sommige functionaliteiten zijn identiek voor de client en de server. Daarom zijn deze geabstraheerd naar de Core. Bij deze functionaliteiten kunt u denken aan het tonen van foutmeldingen, het valideren van diverse inputvelden en het versturen van berichten over de network stream.

## Generics

### Beschrijving van concept in eigen woorden

Generics zijn een goed voorbeeld van het Type-safe maken van een bepaald stuk code of functionaliteit. Het meest klassieke voorbeeld is een list. Voor een list geïnitialiseerd wordt, moet aangegeven worden welk datatype er in de list bewaard moet worden.
Dit zorgt voor een stukje type-safety, omdat de gebruiker altijd weet welk type er in bijvoorbeeld de list aanwezig is. Zie het volgende voorbeeld.

```cs
public class MyClass {
    private readonly string _myValue;

    public void AddValueRecord(string newValue) {
        // Do some other things that make this function not a simple setter
        _myValue = newValue;
    }

    public string GetValueRecord() {
        // Do some other things that make this function not a simple getter
        return _myValue;
    }
}

var class = new MyClass();
```

Zoals in het bovenstaande fragment te zien, is er een eenvoudige C# klasse aangemaakt. Het probleem van deze klasse is echter, dat we niet anders kunnen dan `string`s te gebruiken. Willen we deze functionaliteit voor bijvoorbeeld een `int`, dan zal er een hele nieuwe klasse moeten worden gemaakt. Hierbij zal deze klasse worden gekopieerd en dat is iets wat we willen voorkomen.

Een eigen klasse Generic maken is eenvoudig. We kunnen een bepaald type meegeven op de volgende manier:

```cs
public class MyClass<T> {
    private readonly T _myValue;

    public void AddValueRecord(T newValue) {
        // Do some other things that make this function not a simple setter
        _myValue = newValue;
    }

    public T GetValueRecord() {
        // Do some other things that make this function not a simple getter
        return _myValue;
    }
}
var class = new MyClass<int>();
```

Op deze manier is onze eigen klasse generic gemaakt en kunnen we er alle soorten types in stoppen die we willen, zonder alles te hoeven kopieren en plakken.

### Code voorbeeld van je eigen code

`private readonly List<NetworkStream> _connectedStreams = new List<NetworkStream>();`

Dit stuk code geeft precies datgene aan. Wanneer de server laadt, wordt een nieuwe lijst met NetworkStreams aangemaakt. Wanneer later in de code over deze functionaliteit heen wordt gelopen, weet de applicatie dat het altijd een NetworkStream betreft. Met deze kennis kunnen bepaalde specifieke methoden van NetworkStream worden aangeroepen.

### Alternatieven & adviezen

Een alternatief aanbevelen voor Generics is lastig. Generics maakt het ontwikkelen van applicaties vele male makkelijker. Daarnaast zorgt het voor beter herbruikbare code.

### Authentieke en gezaghebbende bronnen

- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/
- http://www.jot.fm/issues/issue_2013_11/article1

## Boxing & Unboxing

### Beschrijving van concept in eigen woorden

Boxing en Unboxing is een techniek in C# die tegenwoordig veel onder water gebeurt. In de basis komt het op het volgende neer:

C# kent in principe 2 types, de reference types (voornamelijk class, interface, delegate, object, string) en value types (alle andere types zoals int, double, etc.). Het grootste verschil tussen deze twee types is dat reference types een verwijzing zijn naar hun data op de heap. Value types daarentegen bevatten de instantie van een type.

Boxing is het converteren van een value type naar een reference type. Dit wordt gedaan door bijvoorbeeld:

```cs
double dbl = 10.2;
object o = dbl;
```

`o` is nu een reference type, die refereert naar de value van dbl op de heap. Wanneer we dit terug willen converteren naar een value type, moeten we dit proces weer omkeren. Dit doen we door te casten. Dit proces heet unboxing:

```cs
double unboxedDbl = (double)o;
```

### Code voorbeeld van je eigen code

Omdat er geen gebruik gemaakt is van expliciete casts in de code, is er geen voorbeeld uit mijn eigen code te geven op de manier waarop hierboven boxing/unboxing is uitgelegd. Wel is er op een andere manier een value unboxed.

Inputvelden in Xamarin.Mac zijn uit te lezen door het `StringValue` veld te lezen. De naam zegt het al, hieruit komt een `string`. Dit stuk code komt uit de Validation.cs klasse in de MultiChat Core.

```cs
public static int ValidatePort(string enteredPort)
{
    int validatedPort;

    try
    {
        validatedPort = Int32.Parse(enteredPort);
    }
    catch (FormatException)
    {
        throw new InvalidInputException("Invalid server port",
            "The port you entered was invalid. Please make sure the port is a numeric value.");
    }

    return validatedPort;
}
```

In dit voorbeeld wordt van het reference type `enteredPort` (reference type omdat het een `string` betreft) een `int` gemaakt. Omdat `int` een value type is, wordt de `string` in `enteredPort` dus ge-unboxt naar een `int`.

### Alternatieven & adviezen

Op sommige plekken is het boxen en unboxen onvermijdelijk, zoals in het bovenstaande codefragment. Toch zijn er plekken waar vanuit de taal zelf al verbeteringen zijn gemaakt om boxing/unboxing te vermijden. Neem bijvoorbeeld de C# `ArrayList`. Dit is een list van objecten. In het onderstaande fragment staan twee voorbeelden waarin een lijst van `int` bijgehouden wordt:

```cs
ArrayList arrayList = new ArrayList();
arrayList.Add(10);

int arrayListValue = (int)arraylist[0];

Console.WriteLine(arrayListValue);

###

List<int> list = new List<int>();
list.Add(10);

int listValue = list[0];

Console.WriteLine(listValue);
```

In het bovenste deel van het fragment wordt een `ArrayList` aangemaakt. Hierin kunnen `object`s worden gestopt. Wanneer je hier een value uit wil halen, moet je die echter wel eerst casten naar het type waar je mee wil werken, in dit geval een `int`. Dit voegt een extra abstractielaag toe waardoor de code minder makkelijk te lezen en te begrijpen is.

In het onderste deel van de code wordt er gebruik gemaakt van de nieuwere `List<T>` (zie ook het hoofdstuk Generics). Door generics te gebruiken hoeven we niet meer te casten en is de code eenvoudiger. Het advies is hier dan ook: probeer expliciet casten te vermijden en zorg dat je op de hoogte bent van nieuwe implementaties die beter werken dan de oude.

### Authentieke en gezaghebbende bronnen

- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing
- https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-types
- https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/reference-types
- https://dotnetcoretutorials.com/2020/02/03/arraylist-vs-list/

## Delegates & Invoke

### Beschrijving van concept in eigen woorden

Delegates maken het mogelijk om functies als parameter mee te geven aan de `Invoke`-methode. De Invoke-methode, is een methode van de C# Control klasse in de Windows Forms namespace. Omdat in deze applicatie geen gebruik is gemaakt van Windows Froms (WPF) maar van Xamarin.Mac, is er geen gebruik gemaakt van Invoke.

Ook Delegates zijn niet gebruikt in de code, omdat daar simpelweg geen behoefte aan was. Wel zal in het volgende kopje worden beschreven hoe de implementatie van zo'n Delegate er uit zou kunnen zien.

Het principe van Invoke is in een paar woorden uit te leggen: Invoke voert een meegegeven Delegate uit op de UI-thread van het scherm. Op deze manier kunnen UI manipulaties worden uitgevoerd op de Thread die daar door de applicatie voor is gemaakt.

Een Delegate is een lastiger en interessanter onderwerp. Een Delegate lijkt sterk op een van de tecnhieken de bij het semester OOSE zijn aangeleerd; het Strategy-patern. We maken een lege huls waarin we definieren wat er teruggegeven (return type) en meegegeven (de parameters) gaan worden. Verder laten we de implementatie helemaal links liggen. Deze functie kunnen we meegeven aan een stuk code, die zonder dat hij hoeft te weten wat de implementatie is deze functie kan uitvoeren.

### Code voorbeeld van je eigen code

Zoals gezegd is in de eigen applicatie geen gebruik gemaakt van de technieken. Daarom zal een algemeen voorbeeld worden gegeven.

Allereerst defineren we een delegate "MakeSound". Zoals te zien is, is aan deze delegate geen enkele andere invulling gegeven dan dat het een Delegate is, met als return-type `void`. De delegate accepteert geen parameters.

Onder deze code wordt een nieuw private veld gemaakt met als type onze nieuwe Delegate. Delegates zijn te gebruiken als type en maken de code dus mooi Type-safe.

Als de Main even overgeslagen wordt, zien we drie public functies. De functies zijn allemaal van het type void zonder parameters. Iedere functie implementeert een ander dierengeluid. Wat opvalt, is dat al deze functies precies voldoen aan onze Delegate.

In de Main van de applicatie wordt vervolgens een invulling gegeven aan ons lege _makeAnimalSound veld. We voegen hier eerst "MakeDuckSound" aan toe. We voeren de Delegate uit en doen ditzelfde vervolgens met alle andere methoden.

```cs
public delegate void MakeSound();
private MakeSound _makeAnimalSound;

public static void Main(string[] args) {
    _makeAnimalSound = MakeDuckSound;
    _makeAnimalSound(); // 1

    _makeAnimalSound = MakeCatSound;
    _makeAnimalSound(); // 2

    _makeAnimalSound = MakeCowSound;
    _makeAnimalSound(); // 3
}

public void MakeDuckSound() {
    Console.Write("Quack");
}

public void MakeCatSound() {
    Console.Write("Meow");
}

public void MakeCowSound() {
    Console.Write("Moo");
}
```

Wanneer we de applicatie nu draaien, zullen we zien dat de console bij het eerste comment "Quack" zal tonen. Het tweede comment toont "Meow" en het derde comment toont "Moo". Op deze manier zien we dat de delegate vervangen kan worden door een andere methode die aan de voorwaarden van de delegate voldoet. 

### Alternatieven & adviezen

Een delegate heeft veel weg  van een interface. Daarom is een van de alternatieven voor een delegate het maken van een klasse die overerft van een interface met een bepaalde methode waaraan een eigen invulling gegeven moet worden. 

Dit is echter niet een hele goede vervanging voor een delegate. Delegates maken het mogelijk om een eigen implementatie te bieden op methode-niveau, waar een interface een hele klasse vereist. Dit maakt de code onnodig groter en complexer.

### Authentieke en gezaghebbende bronnen

- https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.control.invoke?view=net-5.0
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/

## Threading & Async

### Beschrijving van concept in eigen woorden

Threading en Async maakt het mogelijk om het programma dat geschreven wordt te versnellen.
Door deze technieken te gebruiken wordt de PC waar de code op draait beter benut en wordt
de user experience beter. Het is namelijk zo dat MultiThreaded code de UI niet blokkeert. Hierdoor
heeft de gebruiker nooit het gevoel dat het programma vastloopt.

### Code voorbeeld van je eigen code

In de code van de applicatie is veel gebruik gemaakt van Async. Bij deze implementatie is zo veel mogelijk gebruik gemaakt van de eigen Async implementatie van de code die gebruikt wordt. Van heel veel code van C# is namelijk ook een Async-variant beschikbaar.

In het onderstaande voorbeeld uit de Core van de applicatie wordt gebruik gemaakt van de Async-variant van `ns.Write()`, het schrijven naar een NetworkStream. Wanneer we deze functionaliteit willen gebruiken, is het zaak dat we het woordje `await` voor de aanroep van de methode zetten.

Zonder dat woordje zijn we er echter nog niet. Omdat we nu een Async aanroep in de code hebben staan, wordt de methode zelf ook Async. Dit is wellicht een nadeel van Async/Await: Voor je het weet zijn al je methoden Async.

De methode is dus ook Async gemaakt. Dat deze methode "Async" is, is te zien aan het `async`-keyword. Deze methode retourneert een ietwat vreemd type: `Task`. `Task` is vergelijkbaar met een `void`, alleen heeft dit wat voordelen. Het grootste probleem van `async void` methoden, is dat eventuele excepties die hierin optreden niet goed doorgegeven worden. Het is dan heel makkelijk om de applicatie kapot te krijgen zonder dat duidelijk wordt wat er aan de hand is.

```cs
public static async Task SendMessage(Message message, NetworkStream ns)
{
    var buffer = Encoding.ASCII.GetBytes(message.ToJson() + "@");
    await ns.WriteAsync(buffer, 0, buffer.Length);
}
```

### Alternatieven & adviezen

Het is aangeraden om zo veel mogelijk van het handmatig aanmaken van Threads af te stappen. Gebruik Async waar dat kan, om de applicatie responsive en snel te houden. Daarnaast is het zo, dat goed opgelet moet worden dat een `void`-methode die Async wordt, vertaald wordt naar een `Task` om vervelende bugs te voorkomen.

Een alternatief voor Async/Await is de Task Parallel Library van C#. In deze library creeer je tasks, welke op een later moment kunnen worden uitgevoerd. Dit gedrag is vergelijkbaar met Async/Await.

### Authentieke en gezaghebbende bronnen

- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
- https://docs.microsoft.com/en-us/dotnet/standard/threading/using-threads-and-threading
- https://www.baeldung.com/cs/async-vs-multi-threading
- https://www.c-sharpcorner.com/article/parallel-programming-using-tpl-in-net/
