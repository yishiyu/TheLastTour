using TheLastTour.Manager;

namespace TheLastTour.Event
{
    public static class GameEvents
    {
        public static GameStateChangedEvent GameStateChangedEvent = new GameStateChangedEvent();
    }


    public class GameStateChangedEvent : GameEvent
    {
        public EGameState PreviousState;
        public EGameState CurrentState;
    }
}