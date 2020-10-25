namespace DomainTactics.Persistence
{
    public interface IHaveIdentifier
    {
        string Identifier { get; set; }
        long Position { get; set; }
    }
}