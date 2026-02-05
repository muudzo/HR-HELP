namespace HrDesk.PeopleHum.Extensions;

using Microsoft.Extensions.DependencyInjection;
using HrDesk.Core.Interfaces;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPeopleHumConnector(this IServiceCollection services)
    {
        services.AddScoped<IPeopleHumClient, FakePeopleHumClient>();
        return services;
    }
}
