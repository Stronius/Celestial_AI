using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DoNotModify;
using BehaviorDesigner.Runtime;

namespace CelestialTeam {

	public class CelestialAIController : BaseSpaceShipController
	{
		BehaviorTree celestialBehavior;
		Animator animator;

		public override void Initialize(SpaceShipView spaceship, GameData data)
		{
			animator = GetComponent<Animator>();
			celestialBehavior = GetComponent<BehaviorTree>();
		}

		public override InputData UpdateInput(SpaceShipView spaceship, GameData data)
		{
			SpaceShipView otherSpaceship = data.GetSpaceShipForOwner(1 - spaceship.Owner);

			UpdateBlackboard(spaceship, otherSpaceship, data);

			float thrust = 1.0f;
			float targetOrient = spaceship.Orientation + 90.0f;
			bool needShoot = AimingHelpers.CanHit(spaceship, otherSpaceship.Position, otherSpaceship.Velocity, 0.15f);

			InputData input = new InputData(thrust, 
											targetOrient, 
											(bool)celestialBehavior.GetVariable("OutShoot").GetValue(), 
											(bool)celestialBehavior.GetVariable("OutMine").GetValue(), 
											(bool)celestialBehavior.GetVariable("OutShockwave").GetValue());

			ResetBlackboard();

			return input;
		}

		void UpdateBlackboard(SpaceShipView spaceship, SpaceShipView otherSpaceship, GameData data)
        {
			celestialBehavior.SetVariableValue("TimeLeft", data.timeLeft);
			celestialBehavior.SetVariableValue("Energy", spaceship.Energy);
			celestialBehavior.SetVariableValue("EnemyEnergy", otherSpaceship.Energy);
			animator.SetFloat("Time", data.timeLeft);
		}

		void ResetBlackboard()
        {
			celestialBehavior.SetVariableValue("OutShoot", false);
			celestialBehavior.SetVariableValue("OutMine", false);
			celestialBehavior.SetVariableValue("OutShockwave", false);
		}
	}
}
