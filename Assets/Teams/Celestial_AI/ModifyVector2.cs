using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace IIM
{
	[TaskCategory("IIM")]
	public class ModifyVector2 : Action
	{
		public enum OPERATOR
		{
			SET = 0,
			ADD = 1,
			SUBSTRAT = 2
		}

		[Tooltip("Variable to modify")]
		public SharedVector2 variable;
		[Tooltip("Modification operator")]
		public OPERATOR op;
		[Tooltip("Value used with operator")]
		public SharedVector2 value;

		public override TaskStatus OnUpdate()
		{
			switch (op)
			{
				case OPERATOR.SET: variable.Value = value.Value; break;
				case OPERATOR.ADD: variable.Value = variable.Value + value.Value; break;
				case OPERATOR.SUBSTRAT: variable.Value = variable.Value - value.Value; break;
			}
			return TaskStatus.Success;
		}
	}
}