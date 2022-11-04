﻿using System.Collections;
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
		Vector2 shootTarget = Vector2.zero;
		List<BulletView> enemyBullets;
        List<BulletView> myBullets;
		int bulletCount;


		GameObject enemy;

        public override void Initialize(SpaceShipView spaceship, GameData data)
		{
			animator = GetComponent<Animator>();
			celestialBehavior = GetComponent<BehaviorTree>();

			/*BaseSpaceShipController[] ships = FindObjectsOfType<BaseSpaceShipController>();

            for (int i = 0; i < ships.Length; i++)
            {
				if (ships[i].gameObject != gameObject)
                {
					enemy = ships[i].gameObject;
				}
            }

			celestialBehavior.SetVariableValue("Enemy", enemy);
			celestialBehavior.SetVariableValue("TargetToReach", enemy);*/
		}

		public override InputData UpdateInput(SpaceShipView spaceship, GameData data)
		{
			SpaceShipView otherSpaceship = data.GetSpaceShipForOwner(1 - spaceship.Owner);

			UpdateBlackboard(spaceship, otherSpaceship, data);

			float thrust = (float)celestialBehavior.GetVariable("Thrust").GetValue();
			LookAt(spaceship, (Vector2)celestialBehavior.GetVariable("Target").GetValue(), thrust, out float orientation, out float thrustSpeed);

			bool shoot = (bool)celestialBehavior.GetVariable("OutShootTarget").GetValue() || (bool)celestialBehavior.GetVariable("OutShootEnemy").GetValue();
			bool dropMine = (bool)celestialBehavior.GetVariable("OutMine").GetValue();
			bool useShockwave = (bool)celestialBehavior.GetVariable("OutShockwave").GetValue();


			InputData input = new InputData(thrustSpeed, orientation, shoot, dropMine, useShockwave);

			ResetBlackboard();

			return input;
		}

		void UpdateBlackboard(SpaceShipView spaceship, SpaceShipView otherSpaceship, GameData data)
        {
			bool capturedClosestPoint = CheckForCapture(spaceship);

			GetClosestMine(data.Mines);
			GetClosestWaypoint(data.WayPoints, spaceship, data);

			distanceToOtherSpaceship = Vector2.Distance(transform.position, otherSpaceship.Position);

			bool hasEnemyShot = /*EnemyShootsAtUs(data.Bullets, otherSpaceship, spaceship);*/HasEnemyShot(data.Bullets, spaceship);
			bool enemyIsShootingAtUs = AimingHelpers.CanHit(otherSpaceship, spaceship.Position, spaceship.Velocity, 0.15f) && hasEnemyShot;
			bool canHitEnemy = false;
			bool canHitTarget = false;

			if ((bool)celestialBehavior.GetVariable("OutShootEnemy").GetValue())
			{
				canHitEnemy = AimingHelpers.CanHit(spaceship, otherSpaceship.Position, otherSpaceship.Velocity, 0.15f);
			}
			else if ((bool)celestialBehavior.GetVariable("OutShootTarget").GetValue())
			{
				canHitTarget = AimingHelpers.CanHit(spaceship, shootTarget, 0.15f);
			}

			celestialBehavior.SetVariableValue("TimeLeft", data.timeLeft);
			celestialBehavior.SetVariableValue("Energy", spaceship.Energy);
			celestialBehavior.SetVariableValue("EnemyEnergy", otherSpaceship.Energy);
			celestialBehavior.SetVariableValue("ClosestWaypoint", closestWaypoint.Position);
			celestialBehavior.SetVariableValue("DistanceToClosestWaypoint", distanceToClosestWaypoint);
			celestialBehavior.SetVariableValue("JustCapturedAPoint", capturedClosestPoint);
            celestialBehavior.SetVariableValue("EnemyPosition", otherSpaceship.Position);
			celestialBehavior.SetVariableValue("DistanceToEnemy", distanceToOtherSpaceship);
            celestialBehavior.SetVariableValue("CanHitEnemy", canHitEnemy);
            celestialBehavior.SetVariableValue("CanHitTarget", canHitTarget);
            celestialBehavior.SetVariableValue("IsEnemyShootingAtUs", enemyIsShootingAtUs);

			if (closestMine != null)
            {
				celestialBehavior.SetVariableValue("ClosestMine", closestMine.Position);
				celestialBehavior.SetVariableValue("DistanceToClosestMine", distanceToClosestMine);
			}
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
			closestMine = null;

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
			closestWaypoint = null;

			for (int i = 0; i < waypointList.Count; i++)
			{
				if (waypointList[i].Owner != spaceship.Owner)
                {
					if (closestWaypoint == null)
					{
						closestWaypoint = waypointList[i];
						distanceToClosestWaypoint = Vector2.Distance(spaceship.Position, closestWaypoint.Position);
					}
					else if (distanceToClosestWaypoint > Vector2.Distance(spaceship.Position, waypointList[i].Position))
					{
						closestWaypoint = waypointList[i];
						distanceToClosestWaypoint = Vector2.Distance(spaceship.Position, closestWaypoint.Position);
					}
				}
			}
		}

		bool CheckForCapture(SpaceShipView spaceship)
        {
			if (closestWaypoint != null)
				if (closestWaypoint.Owner == spaceship.Owner)
					return true;

			return false;
		}

		bool EnemyShootsAtUs(List<BulletView> bulletList, SpaceShipView spaceship, SpaceShipView otherSpaceship)
        {
			bool enemyShootsAtUs = false;

			if (bulletCount < bulletList.Count)
            {
				//faire un check si la balle est la notre (avec un overlap sphere si je detect une balle c'est la notre)
				enemyShootsAtUs = AimingHelpers.CanHit(otherSpaceship, spaceship.Position, spaceship.Velocity, 0.15f);
			}

			bulletCount = bulletList.Count;

			return enemyShootsAtUs;
		}

		bool HasEnemyShot(List<BulletView> bulletList, SpaceShipView spaceship)
		{
			List<BulletView> curEnemyBullets = new();
			List<BulletView> curAllyBullets = new();

			bool newEnemyShots = false;

			for(int bi = 0; bi < bulletList.Count; bi++)
			{
				BulletView bullet = bulletList[bi];
				bool isNotNew = false;
				for(int ebi = 0; ebi < enemyBullets.Count; ebi++)
				{
					if(bullet == enemyBullets[ebi])
					{
						curEnemyBullets.Add(bullet);
						isNotNew = true;
						break;
					}
				}

                if (isNotNew)
                {
                    break;
                }

                for (int mbi = 0; mbi < myBullets.Count; mbi++)
                {
					if (bullet == myBullets[mbi])
                    {
						curAllyBullets.Add(bullet);
						isNotNew = true;
                        break;
                    }
                }

				if (isNotNew)
				{
					break;
				}

		        if(bullet.Velocity.normalized != spaceship.LookAt.normalized)
				{
                    curEnemyBullets.Add(bullet);

                    newEnemyShots = true;
                }
				else
				{
					curAllyBullets.Add(bullet);
				}
			}

			myBullets = curAllyBullets;
			enemyBullets = curEnemyBullets;

			return newEnemyShots;
		}
	}
}
