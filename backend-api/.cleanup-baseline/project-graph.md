# Project graph

## Solution

`Ecom.sln` currently contains four .NET 10 projects:

```text
Ecom.API -> Ecom.Infrastructure -> Ecom.Application -> Ecom.Domain
```

Direct project references:

- `Ecom.Application` references `Ecom.Domain`.
- `Ecom.Infrastructure` references `Ecom.Application`.
- `Ecom.API` references `Ecom.Infrastructure`.

## Baseline size

- C# source files, excluding `bin` and `obj`: 1,673
- Application feature folders: 61
- Controller classes: 46
- SignalR hub classes: 1

No dedicated test project was found in the solution.

