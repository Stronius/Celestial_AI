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

		public Vector2 goToTarget = Vector2.zero;


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
			LookAt(spaceship, goToTarget, thrust, out float orientation, out float thrustSpeed);

			bool needShoot = AimingHelpers.CanHit(spaceship, otherSpaceship.Position, otherSpaceship.Velocity, 0.15f);

			bool shoot = (bool)celestialBehavior.GetVariable("OutShoot").GetValue();
			bool dropMine = (bool)celestialBehavior.GetVariable("OutMine").GetValue();
			bool useShockwave = (bool)celestialBehavior.GetVariable("OutShockwave").GetValue();



			InputData input = new InputData(thrustSpeed, orientation, shoot, dropMine, useShockwave);

			ResetBlackboard();

			return input;
		}

		void UpdateBlackboard(SpaceShipView spaceship, SpaceShipView otherSpaceship, GameData data)
        {
			GetClosestMine(data.Mines);
			GetClosestWaypoint(data.WayPoints, spaceship, data);
			goToTarget = closestWaypoint.Position;

			distanceToOtherSpaceship = Vector2.Distance(transform.position, otherSpaceship.Position);

				/*if ((bool)celestialBehavior.GetVariable("OutShootEnemy").GetValue())
				{
					AimingHelpers.CanHit(spaceship, otherSpaceship.Position, otherSpaceship.Velocity, 0.15f);
				}
				else if ((bool)celestialBehavior.GetVariable("OutShootTarget").GetValue())
				{
					AimingHelpers.CanHit(spaceship, closestMine.Position, 0.15f);
				}*/

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


		public void LookAt(SpaceShipView spaceship, Vector2 targetPosition, float thrustSpeed, out float return1, out float return2)
		{
			return1 = AimingHelpers.ComputeSteeringOrient(spaceship, targetPosition, 1.2f);

			return2 = thrustSpeed;
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

		void GetClosestWaypoint(List<WayPointView> waypointList, SpaceShipView spaceship, GameData data)
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
