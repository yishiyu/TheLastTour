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

        public static SelectedPartChangedEvent SelectedPartChangedEvent = new SelectedPartChangedEvent();
        public static AllObjectivesCompletedEvent AllObjectivesCompletedEvent = new AllObjectivesCompletedEvent();
        public static ObjectiveUpdateEvent ObjectiveUpdateEvent = new ObjectiveUpdateEvent();

        public static NewSceneLoadedEvent NewSceneLoadedEvent = new NewSceneLoadedEvent();
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

    public class SelectedPartChangedEvent : GameEvent
    {
        public PartController PreviousSelectedPart;
        public PartController CurrentSelectedPart;
    }

    public class AllObjectivesCompletedEvent : GameEvent
    {
    }

    public class ObjectiveUpdateEvent : GameEvent
    {
        public Objective ObjectiveObj;
        public bool IsComplete;
    }

    public class NewSceneLoadedEvent : GameEvent
    {
    }
}