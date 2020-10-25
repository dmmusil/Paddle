using System.Threading.Tasks;
using DomainTactics.Messaging;

namespace DomainTactics.Persistence
{
    public interface IDocumentStorage
    {
        Task<T> Load<T>(string identifier);
        Task Save(IHaveIdentifier document);
    }
}