using System;


namespace FirstGearGames.FPSLand.Characters.Motors
{
    /// <summary>
    /// Additional forces to apply when using Move in motors.
    /// </summary>
    [Flags]
    public enum AdditionalMoveForces
    {
        None = 0,
        VerticalVelocity = 1,
        ExternalForces = 2
    }


}