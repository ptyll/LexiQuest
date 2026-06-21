namespace LexiQuest.E2E.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class E2ECollection : ICollectionFixture<E2EEnvironmentFixture>
{
    public const string Name = "LexiQuest E2E";
}
