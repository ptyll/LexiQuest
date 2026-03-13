using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tempo.Blazor.Localization;

namespace LexiQuest.Blazor.Tests.Helpers;

public static class TempoTestHelper
{
    public static void RegisterTempoServices(IServiceCollection services)
    {
        var tmLocalizer = Substitute.For<ITmLocalizer>();
        tmLocalizer[Arg.Any<string>()].Returns(ci => ci.Arg<string>());
        services.AddSingleton(tmLocalizer);
    }
}
