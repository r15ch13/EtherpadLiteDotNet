.Net Library For Etherpad Lite API
=========================
* It implements the Etherpad Lite API, [more infomation on the API can be found on the Etherpad Lite wiki][1].
* It matches the API in structure as closely as it can.
* It parses the returned JSON into strongly typed objects to allow the use of intellisense and reduce the use of magic strings.
* The library is written in C# and uses .Net Framework 4.

Setup
---------

Download the project and either:

* Include the project in your solution and reference the project 
* Place the dll found in bin/release in your bin folder (or wherever you store external libraries) and reference it in your project

Usage
----------
Add a using for the Etherpad namespace

```C#
using Etherpad;
```
Create an instance of the EtherpadLiteDotNet class

```C#
var ether = new EtherpadLiteDotNet("YourApiKeyHere", "localhost", 9001);
```
	
The class constructor has three parameters:

1. The API key found in the root of your Etherpad Lite folder.
2. The host of your etherpad deployment.
3. The port to connect on (this parameter is optional).

After instantiating the class you simply call the methods you want

```C#
var getTextReturn = ether.GetText("11");
```

Return Types
----------

All objects returned by an API call inherit from EtherpadResponse which has 3 base properties:

1. ReturnCode: The code returned by the API (for more info on these see the [API wiki][1]). The codes are mapped to an enumeration called EtherpadReturnCodeEnum.
2. Message: The message returned by the API, this will either be a success message or a reason the request failed.
3. JSON: This is the raw JSON response in case you want to pass the response on.

The API can also return data, this is returned in objects that inherit from the base EtherpadResponse and maps the the JSON structure onto .Net objects, using Generic.IEnumerable collections where needed.
Please see the [API wiki][1] for more information on the JSON structure returned for each method.

So for example to access the data returned by the GetText() above you would:

```C#
string padText = test.Data.Text;
```

Thanks goes to the PHP API for inspiration: <https://github.com/TomNomNom/etherpad-lite-client> 

[1]: https://github.com/Pita/etherpad-lite/wiki/HTTP-API