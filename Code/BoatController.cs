using System;
using Sandbox;

namespace BoatGame;

public partial class BoatController : Component
{
	[Property, Feature( "General" )] List<GameObject> Floaters { get; set; }
	[Property, Feature( "General" )] float SpringStrength { get; set; } = 50;
	[Property, Feature( "General" )] float SpringDamping { get; set; } = 5;
	[Property, Feature( "General" )] public float MaxSpeedMPH { get; set; } = 90;
	public float MaxSpeed { get { return MaxSpeedMPH * 17.6f; } }
	[Property, Feature( "General" )] public float TurnSpeed { get; set; } = 2;
	[Property, Feature( "General" )] Curve AccelerationCurve { get; set; } = Curve.EaseOut;
	[Property, Feature( "Camera" )] Vector3 CameraOffset { get; set; }
	[RequireComponent] Rigidbody Rb { get; set; }
	public Angles EyeAngles { get; set; }
	protected override void OnUpdate()
	{
		UpdateLook();
	}

	protected override void OnFixedUpdate()
	{
		UpdateFloaters();
		
		var angles = WorldRotation.Angles();
		Rb.SmoothRotate( angles.WithYaw( angles.yaw + Input.AnalogMove.y * TurnSpeed ), .1f, Time.Delta );
	}

	public void UpdateFloaters()
	{
		foreach ( var floater in Floaters )
		{
			if ( floater.WorldPosition.z < 0 )
			{
				var floaterVelocity = Rb.GetVelocityAtPoint( floater.WorldPosition );
				var offset = ( floater.WorldPosition.WithZ(0) - floater.WorldPosition ).Length;
				if ( offset < 0 ) return;

				var springDir = Vector3.Up;
				var springVel = Vector3.Dot( springDir, floaterVelocity );
				var springForce = (offset * (SpringStrength * Rb.Mass)) - (springVel * SpringDamping * Rb.Mass);

				Rb.ApplyForceAt( floater.WorldPosition, springDir * springForce );
				UpdateMove( floater );
			}
		}
	}

	public void UpdateMove( GameObject floater )
	{
		var driveDir = WorldRotation.Forward;
		var driveVel = Vector3.Dot( driveDir, Rb.GetVelocityAtPoint( floater.WorldPosition ) );
		
		var driveVelNormal = float.Clamp(MathF.Abs( driveVel ) / MaxSpeed, 0, 1);
		var driveForce = AccelerationCurve.Evaluate( driveVelNormal ) * Input.AnalogMove.x * (float)Math.Pow(Rb.Mass, 2) / Floaters.Count;
		
		if ( Input.AnalogMove.x.AlmostEqual( 0 ) )
		{
			driveForce = -driveVel * Rb.Mass * 0.01f;
		}
		if ( driveVel >= MaxSpeed ) driveForce = 0;
	
		Rb.ApplyForce( driveDir * driveForce );
	}
}
