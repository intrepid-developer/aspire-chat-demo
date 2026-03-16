namespace AspireChat.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DistributedApplicationCollection : ICollectionFixture<DistributedApplicationFixture>
{
    public const string Name = "distributed-application";
}
