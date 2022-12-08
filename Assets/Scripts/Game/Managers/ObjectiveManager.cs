using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class ObjectiveManager : MonoBehaviour
    {
        List<Objective> objectives = new List<Objective>();
        bool objectivesCompleted = false;

        void Awake()
        {
            Objective.OnObjectiveCreated += RegisterObjective;
        }

        void RegisterObjective(Objective objective) => objectives.Add(objective);

        void Update()
        {
            //If there are no objectives set or if the objectives have already been completed
            if (objectives.Count == 0 || objectivesCompleted)
                return;

            Debug.Log(objectives.Count);
            for (int i = 0; i < objectives.Count; i++)
            {
                // pass every objectives to check if they have been completed
                if (objectives[i].IsBlocking())
                    // break the loop as soon as we find one uncompleted objective
                    return;
            }

            objectivesCompleted = true;
            EventManager.Broadcast(Events.AllObjectivesCompletedEvent);
        }

        void OnDestroy()
        {
            Objective.OnObjectiveCreated -= RegisterObjective;
        }
    }
}