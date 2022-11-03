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
		MineView closestMine;
		float distanceToClosestMine;
		WayPointView closestWaypoint;
		float distanceToClosestWaypoint;
		float distanceToOtherSpaceship;

		Vector2 goToTarget;


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

			bool shoot = (bool)celestialBehavior.GetVariable("OutShoot").GetValue();
			bool dropMine = (bool)celestialBehavior.GetVariable("OutMine").GetValue();
			bool useShockwave = (bool)celestialBehavior.GetVariable("OutShockwave").GetValue();



			InputData input = new InputData(thrust, targetOrient, shoot, dropMine, useShockwave);

			ResetBlackboard();

			return input;
		}

		void UpdateBlackboard(SpaceShipView spaceship, SpaceShipView otherSpaceship, GameData data)
        {
			GetClosestMine(data.Mines);
			GetClosestWaypoint(data.WayPoints, spaceship);
			distanceToOtherSpaceship = Vector2.Distance(transform.position, otherSpaceship.Position);

			if ((bool)celestialBehavior.GetVariable("OutShootEnemy").GetValue())
			{
				AimingHelpers.CanHit(spaceship, otherSpaceship.Position, otherSpaceship.Velocity, 0.15f);
			}
			else if ((bool)celestialBehavior.GetVariable("OutShootMine").GetValue())
            {
				AimingHelpers.CanHit(spaceship, closestMine.Position, 0.15f);
			}

			celestialBehavior.SetVariableValue("TimeLeft", data.timeLeft);
			celestialBehavior.SetVariableValue("Energy", spaceship.Energy);
			celestialBehavior.SetVariableValue("EnemyEnergy", otherSpaceship.Energy);
			animator.SetFloat("Time", data.timeLeft);
		}

		void ResetBlackboard()
        {
			celestialBehavior.SetVariableValue("OutShootTarget", false);
			celestialBehavior.SetVariableValue("OutShootEnemy", false);
			celestialBehavior.SetVariableValue("OutMine", false);
			celestialBehavior.SetVariableValue("OutShockwave", false);
		}

		void GetClosestMine(List<MineView> mineList)
        {
            for (int i = 0; i < mineList.Count; i++)
            {
				if (closestMine == null)
                {
					closestMine = mineList[i];
					distanceToClosestMine = Vector2.Distance(transform.position, closestMine.Position);
				}
				else if (distanceToClosestMine > Vector2.Distance(transform.position, mineList[i].Position))
				{
					closestMine = mineList[i];
					distanceToClosestMine = Vector2.Distance(transform.position, closestMine.Position);
				}
			}
		}

		void GetClosestWaypoint(List<WayPointView> waypointList, SpaceShipView spaceship)
		{
			for (int i = 0; i < waypointList.Count; i++)
			{
				if (waypointList[i].Owner != spaceship.Owner)
                {
					if (closestWaypoint == null)
					{
						closestWaypoint = waypointList[i];
						distanceToClosestWaypoint = Vector2.Distance(transform.position, closestWaypoint.Position);
					}
					else if (distanceToClosestWaypoint > Vector2.Distance(transform.position, waypointList[i].Position))
					{
						closestWaypoint = waypointList[i];
						distanceToClosestWaypoint = Vector2.Distance(transform.position, closestWaypoint.Position);
					}
				}
			}
		}
	}
}
