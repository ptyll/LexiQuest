using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Landing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class TestimonialsSectionTests : TestContext
{
    private readonly IStringLocalizer<TestimonialsSection> _localizer;

    public TestimonialsSectionTests()
    {
        _localizer = Substitute.For<IStringLocalizer<TestimonialsSection>>();
        _localizer["Testimonials.Title"].Returns(new LocalizedString("Testimonials.Title", "Co říkají hráči?"));
        _localizer["Testimonial1.Quote"].Returns(new LocalizedString("Testimonial1.Quote", "LexiQuest mě naučil spoustu nových českých slov."));
        _localizer["Testimonial1.Author"].Returns(new LocalizedString("Testimonial1.Author", "Petra Nováková"));
        _localizer["Testimonial1.Role"].Returns(new LocalizedString("Testimonial1.Role", "Studentka, 23 let"));
        _localizer["Testimonial2.Quote"].Returns(new LocalizedString("Testimonial2.Quote", "Soutěžení v ligách mě strašně chytlo."));
        _localizer["Testimonial2.Author"].Returns(new LocalizedString("Testimonial2.Author", "Jan Svoboda"));
        _localizer["Testimonial2.Role"].Returns(new LocalizedString("Testimonial2.Role", "Programátor, 31 let"));
        _localizer["Testimonial3.Quote"].Returns(new LocalizedString("Testimonial3.Quote", "Moje děti hrají LexiQuest místo toho, aby koukaly do mobilu."));
        _localizer["Testimonial3.Author"].Returns(new LocalizedString("Testimonial3.Author", "Marie Dvořáková"));
        _localizer["Testimonial3.Role"].Returns(new LocalizedString("Testimonial3.Role", "Maminka na rodičovské"));
        
        Services.AddSingleton(_localizer);
        Services.AddSingleton(Substitute.For<ITmLocalizer>());
    }

    [Fact]
    public void TestimonialsSection_Renders_Title()
    {
        // Act
        var cut = Render<TestimonialsSection>();

        // Assert
        cut.Find("[data-testid='testimonials-title']").TextContent.Should().Be("Co říkají hráči?");
    }

    [Fact]
    public void TestimonialsSection_Renders_3Reviews()
    {
        // Act
        var cut = Render<TestimonialsSection>();

        // Assert
        cut.Find("[data-testid='testimonial-1']").Should().NotBeNull();
        cut.Find("[data-testid='testimonial-2']").Should().NotBeNull();
        cut.Find("[data-testid='testimonial-3']").Should().NotBeNull();
    }

    [Fact]
    public void TestimonialsSection_Review1_Renders_QuoteAuthorRole()
    {
        // Act
        var cut = Render<TestimonialsSection>();

        // Assert
        var review = cut.Find("[data-testid='testimonial-1']");
        review.TextContent.Should().Contain("LexiQuest mě naučil spoustu nových českých slov.");
        review.TextContent.Should().Contain("Petra Nováková");
        review.TextContent.Should().Contain("Studentka, 23 let");
    }

    [Fact]
    public void TestimonialsSection_Review2_Renders_QuoteAuthorRole()
    {
        // Act
        var cut = Render<TestimonialsSection>();

        // Assert
        var review = cut.Find("[data-testid='testimonial-2']");
        review.TextContent.Should().Contain("Soutěžení v ligách mě strašně chytlo.");
        review.TextContent.Should().Contain("Jan Svoboda");
        review.TextContent.Should().Contain("Programátor, 31 let");
    }

    [Fact]
    public void TestimonialsSection_Review3_Renders_QuoteAuthorRole()
    {
        // Act
        var cut = Render<TestimonialsSection>();

        // Assert
        var review = cut.Find("[data-testid='testimonial-3']");
        review.TextContent.Should().Contain("Moje děti hrají LexiQuest místo toho, aby koukaly do mobilu.");
        review.TextContent.Should().Contain("Marie Dvořáková");
        review.TextContent.Should().Contain("Maminka na rodičovské");
    }

    [Fact]
    public void TestimonialsSection_AllReviews_Have5Stars()
    {
        // Act
        var cut = Render<TestimonialsSection>();

        // Assert
        var stars1 = cut.FindAll("[data-testid='testimonial-1'] .star-icon");
        var stars2 = cut.FindAll("[data-testid='testimonial-2'] .star-icon");
        var stars3 = cut.FindAll("[data-testid='testimonial-3'] .star-icon");
        
        stars1.Count.Should().Be(5);
        stars2.Count.Should().Be(5);
        stars3.Count.Should().Be(5);
    }

    [Fact]
    public void TestimonialsSection_AllReviews_HaveAvatar()
    {
        // Act
        var cut = Render<TestimonialsSection>();

        // Assert
        cut.Find("[data-testid='testimonial-1'] .testimonial-avatar").Should().NotBeNull();
        cut.Find("[data-testid='testimonial-2'] .testimonial-avatar").Should().NotBeNull();
        cut.Find("[data-testid='testimonial-3'] .testimonial-avatar").Should().NotBeNull();
    }
}
