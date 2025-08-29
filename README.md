# Demolite.Http

Demolite.Http is a simple wrapper around the HttpClient which is meant to reduce the amount of boilerplate code needed
when creating new projects that make calls to web apis.

# UPGRADING TO 0.1.0

I have rewritten this library to completely remove the dependencies on flurl and serilog and instead decided to go with
base .NET functionality only.
This will make this library more resilient to breaking dependency changes or license changes in dependent libraries.
If you decide to upgrade from 0.0.X to a version greater than 0.1.0, _**your existing code will break!**_

I have completely rewritten the methods to make the implementation and usage a lot easier.

Changes:

- There is no longer a UrlBuilder and ParameterBuilder, instead you will need to pass an instance of IRequestUri into
  the
  request methods.
  By inheriting from RequestUri, you can implement custom methods for parameters or segments which are needed often.
- The repository is no longer type-dependent.
  It will return a raw IHttpResponse object, and you will need to handle deserialization yourself.
  This makes the class a lot smaller and less complex.
- For deserialization, an abstract deserialization helper has been introduced, which takes the IHttpResponse and
  deserializes it.
  To use it, you need to inherit from that class and implement custom deserialization logic if required.
  You can use it in your implemented repositories to convert from IHttpResponse into an object of your choice.
- You no longer need to override all header and setup methods.
  This makes using the repository a lot easier and removes unnecessary overrides for functionality you do not need.
- Logging is no longer dependant on Serilog, but uses a Log event on the repository itself, which you can subscribe to
  and then use your favorite logger to write the messages.

# Functionality

Currently, the Library supports the following methods:

- GET (With and without UrlEncoded Form Content)
- POST (With JSON string content)
- PUT (With JSON string content)
- PATCH (With JSON string content)
- DELETE

This should be enough to handle about 95% of all common api requests.
If you need more methods or a different implementations, feel free to open an Issue
on [GitHub](https://github.com/demolite-nuget/DemoliteHttp)!

# Usage

## Inheriting:

I find it easiest to create an abstract repository for every API you want to access.
Inherit from AbstractHttpRepository and override the methods you need which are shared across all calls in this api.

You can then inherit from your repository and implement methods for the various endpoints of your API.

Methods you can override include:

```cs 
virtual async Task PrepareRequest();
```

A method which is executed before _every_ request and allows you to do things such as initializing or reloading
authentication providers, etc.

```cs 
virtual void SetGetHeaders();
virtual void SetPostHeaders();
virtual void SetPutHeaders();
virtual void SetPatchHeaders();
virtual void SetDeleteHeaders();
```

Methods which are executed before the requests but after the PrepareRequest call.
This allows for setting authentication headers and other things for specific requests.

In the legacy version, these used to be abstract, therefore requiring overrides for every inherited class.

If you override them, make sure to call

```cs 
base.SetXYZHeaders();
```

in the first line of your implementation, or call

```cs 
Client.ClearDefaultRequestHeaders();
``` 

manually because that clears the default headers of the HttpClient and then allows you to set new ones for your specific
call.

## Deserializing

The AbstractDeserializer allows for easy deserialization of JSON responses to objects of your choice. 

You can override 
```csharp
GetSerializerOptions()
```
to implement custom serialization rules.

If you directly pass the IHttpResponse, you will get a default() of your specified type if the request failed or the response body was null.