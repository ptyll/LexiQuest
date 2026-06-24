using Bunit;
using FluentAssertions;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;

namespace LexiQuest.Blazor.Tests.Helpers;

public static class RequiredLabelTestAssertions
{
    public static void AssertNoDuplicateRequiredAsterisks<TComponent>(IRenderedComponent<TComponent> cut)
        where TComponent : IComponent
    {
        var requiredLabels = cut
            .FindAll(".tm-input-label-required, .tm-form-label-required, .tm-form-field-label")
            .Where(IsRequiredLabel)
            .ToList();
        requiredLabels.Should().NotBeEmpty();

        foreach (var label in requiredLabels)
        {
            var asteriskCount = label.TextContent.Count(character => character == '*');
            asteriskCount.Should().BeLessThanOrEqualTo(1, $"required label '{label.TextContent.Trim()}' must not render duplicate asterisks in markup");
        }
    }

    private static bool IsRequiredLabel(IElement element) =>
        element.ClassList.Contains("tm-input-label-required")
        || element.ClassList.Contains("tm-form-label-required")
        || element.QuerySelector(".tm-form-field-required") != null;
}
