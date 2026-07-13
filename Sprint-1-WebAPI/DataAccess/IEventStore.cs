using Sprint_1_WebAPI.Models;

namespace Sprint_1_WebAPI.DataAccess;

public interface IEventStore
{
    Event? GetById(Guid id);
    void Update(Event eventItem);
}
