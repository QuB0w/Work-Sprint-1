using Event;

namespace Interfaces;

public interface IEventService
{
    List<Events> GetAllEvent();
    Events GetEventById(int id);
    void CreateEvent(Events newEvent);
    void UpdateEvent(int id, Events updatedEvent);
    void DeleteEvent(int id);
}