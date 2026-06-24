using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Services;

namespace LexiQuest.Blazor.Tests.Pages;

public class ContactPageTests : BunitContext
{
    private readonly IStringLocalizer<Contact> _localizer;

    public ContactPageTests()
    {
        _localizer = Substitute.For<IStringLocalizer<Contact>>();

        SetupLocalizer();

        Services.AddSingleton(_localizer);
        Services.AddSingleton(new ToastService());
        TempoTestHelper.RegisterTempoServices(Services);
    }

    [Fact]
    public void ContactPage_Renders_RequiredFieldsWithSingleRequiredMarkerInMarkup()
    {
        // Act
        var cut = Render<Contact>();

        // Assert
        RequiredLabelTestAssertions.AssertNoDuplicateRequiredAsterisks(cut);
        cut.FindAll(".tm-form-field-required").Count.Should().Be(3);
    }

    private void SetupLocalizer()
    {
        _localizer["Contact_PageTitle"].Returns(new LocalizedString("Contact_PageTitle", "Kontakt"));
        _localizer["Contact_Title"].Returns(new LocalizedString("Contact_Title", "Kontakt"));
        _localizer["Contact_Description"].Returns(new LocalizedString("Contact_Description", "Napište nám."));
        _localizer["Contact_NameLabel"].Returns(new LocalizedString("Contact_NameLabel", "Jméno"));
        _localizer["Contact_NamePlaceholder"].Returns(new LocalizedString("Contact_NamePlaceholder", "Vaše jméno"));
        _localizer["Contact_EmailLabel"].Returns(new LocalizedString("Contact_EmailLabel", "Email"));
        _localizer["Contact_EmailPlaceholder"].Returns(new LocalizedString("Contact_EmailPlaceholder", "vas@email.cz"));
        _localizer["Contact_SubjectLabel"].Returns(new LocalizedString("Contact_SubjectLabel", "Předmět"));
        _localizer["Contact_SubjectPlaceholder"].Returns(new LocalizedString("Contact_SubjectPlaceholder", "Předmět zprávy"));
        _localizer["Contact_MessageLabel"].Returns(new LocalizedString("Contact_MessageLabel", "Zpráva"));
        _localizer["Contact_MessagePlaceholder"].Returns(new LocalizedString("Contact_MessagePlaceholder", "Vaše zpráva"));
        _localizer["Contact_Submitting"].Returns(new LocalizedString("Contact_Submitting", "Odesílám..."));
        _localizer["Contact_SubmitButton"].Returns(new LocalizedString("Contact_SubmitButton", "Odeslat"));
        _localizer["Contact_SuccessMessage"].Returns(new LocalizedString("Contact_SuccessMessage", "Zpráva byla odeslána"));
        _localizer["Contact_ErrorMessage"].Returns(new LocalizedString("Contact_ErrorMessage", "Zprávu se nepodařilo odeslat"));
    }
}
