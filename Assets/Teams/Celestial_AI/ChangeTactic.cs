using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace CelestialTeam
{
    [TaskCategory("Celestial")]
    public class ChangeBehavior : Action
    {
        public enum BEHAVIORS
        {
            STANDARD_ATTACK = 0,
            BURST_ATTACK = 1,
            CAPTURE_POINTS = 2,
        }


        [Tooltip("Behavior variable")]
        public SharedInt variable;

        [Tooltip("New Behavior")]
        public BEHAVIORS behavior;


        public override TaskStatus OnUpdate()
        {
            switch (behavior)
            {
                case BEHAVIORS.STANDARD_ATTACK: variable = 0; break;
                case BEHAVIORS.BURST_ATTACK: variable = 1; break;
                case BEHAVIORS.CAPTURE_POINTS: variable = 2; break;
            }
            return TaskStatus.Success;
        }
    }
}
