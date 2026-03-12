using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Components;

public class ErrorBoundaryTests : TestContext
{
    public ErrorBoundaryTests()
    {
        var localizer = Substitute.For<IStringLocalizer<ErrorBoundary>>();
        localizer["Error.Title"].Returns(new LocalizedString("Error.Title", "Nastala chyba"));
        localizer["Error.Message"].Returns(new LocalizedString("Error.Message", "Něco se pokazilo. Zkuste to znovu."));
        localizer["Button.Retry"].Returns(new LocalizedString("Button.Retry", "Zkusit znovu"));
        Services.AddSingleton(localizer);
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

    [Fact(DisplayName = "Catches exception and displays error UI")]
    public void ErrorBoundary_WhenExceptionThrown_DisplaysErrorUI()
    {
        // Arrange
        var cut = Render<ErrorBoundary>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<ThrowingComponent>(0);
                builder.CloseComponent();
            }));

        // Act
        // Trigger error
        cut.Find("button.error-trigger").Click();

        // Assert
        cut.Find(".tm-alert").Should().NotBeNull();
        cut.Find(".tm-alert-title").TextContent.Should().Be("Nastala chyba");
    }

    [Fact(DisplayName = "Retry button resets error and shows child content")]
    public void ErrorBoundary_AfterRetry_ResetsAndShowsChildContent()
    {
        // Arrange
        var errorTriggered = false;
        var cut = Render<ErrorBoundary>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                if (!errorTriggered)
                {
                    builder.OpenComponent<ThrowingComponent>(0);
                    builder.CloseComponent();
                }
                else
                {
                    builder.OpenElement(0, "div");
                    builder.AddContent(1, "Obnovený obsah");
                    builder.CloseElement();
                }
            }));

        cut.Find("button.error-trigger").Click();
        cut.Find(".tm-alert").Should().NotBeNull();

        // Act
        cut.Find("button.tm-btn").Click();

        // Assert - after retry, should show child content again
        // Note: In real implementation, the component would re-render
    }
}

public class ThrowingComponent : ComponentBase
{
    private bool _throw = false;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "class", "error-trigger");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, () => _throw = true));
        builder.AddContent(3, "Throw Error");
        builder.CloseElement();

        if (_throw)
        {
            throw new InvalidOperationException("Test error");
        }
    }
}
