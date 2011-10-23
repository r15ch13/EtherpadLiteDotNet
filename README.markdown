.Net Library For Etherpad Lite API
=========================
The library is written in C# and uses .Net Framework 4.
It implements the Etherpad Lite API, [more information on this can be found at the Etherpad Lite wiki](https://github.com/Pita/etherpad-lite/wiki/HTTP-API)
It tries to conform as closely to the API as it can.
It parses the returned JSON into strongly typed objects to make accessing it easier in .Net and reduce the use of magic strings.

Setup
---------

Download the project and either:
*Include the project in your solution and reference the project 
*Place the dll found in bin/release in your bin folder (or wherever you store external libraries) and reference it in your project

Usage
----------
Add a using for the Etherpad namespace 
	using Etherpad;

Create an instance of the EtherpadLiteDotNet class
	var ether = new EtherpadLiteDotNet("YourApiKeyHere", "localhost", 9001);
The class has three parameters
1. The API key found in the root of your Etherpad Lite folder
2. The host of your etherpad deployment
3. The port to connect on (this parameter is optional)

After instantiating the class you simply call the methods you want
	var getTextReturn = ether.GetText("11");

The object returned by each call is strongly typed and has 3 base properties:
1. ReturnCode: The code returned by the API (for more info on these see the API wiki). The codes are mapped to an enumeration called EtherpadReturnCodeEnum.
2. Message: The message returned by the API, this will either be a success message or a reason the request failed.
3 JSON: This is the raw JSON response in case you want to pass the response on.

The return object also has a forth optional property called data. This is only returned if there is data to display.
Data maps directly to the JSON structure so please see the API wiki for more information on that.
All collections in data use System.Collections.Generic.IEnumerable.