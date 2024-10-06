# Demolite.Http

Demolite.Http is a simple-ish wrapper around Flurl and Serilog which is meant to reduce the amount of boilerplate code 
needed for creating new projects. 

It is used actively in [Conservices POS](https://github.com/dreieichcon/conservices_pos) for all requests .

Using this package works as follows: 

It is best to create an abstract repository for every API you want to contact which inherits from the 
AbstractHttpRepository and implements the methods necessary to override.

See for example the [AbstractPretixRepository](https://github.com/dreieichcon/conservices_pos/blob/main/code/Innkeep/Innkeep.Api.Pretix/Repositories/Core/AbstractPretixRepository.cs) in the Conservices POS Repository.

From that repository, create inherited repositories which then implement the methods needed. 

The AbstractUrlBuilder and the IParameterBuilder can be used in tandem to create classes which allow building Urls in a fluent manner. 

Examples can be found in the [PretixUrlBuilder](https://github.com/dreieichcon/conservices_pos/blob/main/code/Innkeep/Innkeep.Api/Endpoints/Pretix/PretixUrlBuilder.cs) and the [PretixParameterBuilder](https://github.com/dreieichcon/conservices_pos/blob/main/code/Innkeep/Innkeep.Api/Endpoints/Pretix/PretixParameterBuilder.cs).

The PrepareRequest method can then automatically use the builder to build request Uris without much effort. 

Of course, all methods are overrideable, so if you wish to implement your own URL building that is always possible.