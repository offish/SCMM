﻿namespace SCMM.Shared.Abstractions.WebProxies;

public interface IWebProxyDetails
{
    string Id { get; }

    string Address { get; }

    int Port { get; }

    string Username { get; }

    string Password { get; }

    string CountryCode { get; }

    string CityName { get; }

    bool IsAvailable { get; }

    DateTimeOffset LastCheckedOn { get; }

    IDictionary<string, DateTimeOffset> DomainRateLimits { get; }
}
