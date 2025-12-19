namespace BoatGame;

public partial class BoatController : Component
{
    public void UpdateLook()
	{
		Scene.Camera.FieldOfView = Scene.Camera.FieldOfView.LerpTo( Preferences.FieldOfView * Rb.Velocity.Length
			.Remap(0, 1000, 1, 1.2f), Time.Delta * 5);

		EyeAngles += Input.AnalogLook;
		EyeAngles = EyeAngles.WithPitch( EyeAngles.pitch.Clamp( -90, 90 ) );
		var offset = EyeAngles.ToRotation().Right * CameraOffset.x + EyeAngles.ToRotation().Up * CameraOffset.z + EyeAngles.ToRotation().Backward * CameraOffset.y;
		var tr = Scene.Trace.Ray( WorldPosition + WorldRotation.Up * 20, WorldPosition + offset ).IgnoreGameObjectHierarchy(GameObject).Run();
		Scene.Camera.WorldPosition = tr.Hit ? tr.HitPosition + -tr.Direction * 5 : tr.EndPosition;
		Scene.Camera.WorldRotation = EyeAngles;
	}
}