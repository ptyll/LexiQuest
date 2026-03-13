using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components;
using LexiQuest.Blazor.Services;
using LexiQuest.Blazor.Tests.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Components;

public class ErrorBoundaryTests : BunitContext
{
    public ErrorBoundaryTests()
    {
        var localizer = Substitute.For<IStringLocalizer<ErrorBoundary>>();
        localizer["Error.Title"].Returns(new LocalizedString("Error.Title", "Nastala chyba"));
        localizer["Error.Message"].Returns(new LocalizedString("Error.Message", "Něco se pokazilo. Zkuste to znovu."));
        localizer["Button.Retry"].Returns(new LocalizedString("Button.Retry", "Zkusit znovu"));
        localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        Services.AddSingleton(localizer);
        Services.AddSingleton(Substitute.For<IErrorLoggingService>());
        TempoTestHelper.RegisterTempoServices(Services);
    }

    [Fact(DisplayName = "Renders child content when no error")]
    public void ErrorBoundary_WhenNoError_RendersChildContent()
    {
        // Arrange & Act
        var cut = Render<ErrorBoundary>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, "Bezpečný obsah");
                builder.CloseElement();
            }));

        // Assert
        cut.Find("div").TextContent.Should().Be("Bezpečný obsah");
    }

    [Fact(DisplayName = "Inherits from ErrorBoundaryBase")]
    public void ErrorBoundary_InheritsFromErrorBoundaryBase()
    {
        // Assert - ErrorBoundary inherits from ErrorBoundaryBase
        typeof(ErrorBoundary).BaseType.Should().Be(typeof(Microsoft.AspNetCore.Components.ErrorBoundaryBase));
    }

    [Fact(DisplayName = "Recover method is callable")]
    public void ErrorBoundary_RecoverMethod_IsCallable()
    {
        // Arrange
        var cut = Render<ErrorBoundary>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, "Content");
                builder.CloseElement();
            }));

        // Act & Assert - just verifying the component renders and has recover method
        var instance = cut.Instance;
        instance.Should().NotBeNull();
        instance.GetType().GetMethod("Recover").Should().NotBeNull();
    }
}
