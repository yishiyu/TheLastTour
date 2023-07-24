using TheLastTour.Controller.Machine;
using TheLastTour.Manager;
using UnityEngine;

namespace TheLastTour.Event
{
    public static class GameEvents
    {
        public static GameStateChangedEvent GameStateChangedEvent = new GameStateChangedEvent();
        public static EditStateChangedEvent EditStateChangedEvent = new EditStateChangedEvent();

        public static SelectedPartPrefabChangedEvent SelectedPartPrefabChangedEvent =
            new SelectedPartPrefabChangedEvent();
        
        public static FocusOnTargetEvent FocusOnTargetEvent = new FocusOnTargetEvent();
    }


    public class GameStateChangedEvent : GameEvent
    {
        public EGameState PreviousState;
        public EGameState CurrentState;
    }

    public class EditStateChangedEvent : GameEvent
    {
        public EEditState PreviousState;
        public EEditState CurrentState;
    }

    public class SelectedPartPrefabChangedEvent : GameEvent
    {
        public int CurrentSelectedPartIndex;
        public PartController CurrentSelectedPart;
    }

    public class FocusOnTargetEvent : GameEvent
    {
        public Transform Target;
    }
}