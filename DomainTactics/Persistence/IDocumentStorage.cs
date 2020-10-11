using System.Threading.Tasks;

namespace DomainTactics.Persistence
{
    public interface IDocumentStorage
    {
        Task<T> Load<T>(string identifier);
        Task Save(IHaveIdentifier document);
    }
}