using UnityEngine;

public class Vector3Pid {
    // Proportional gain. The main driving force towards the target.
    private float Kp { get; }
    // Integral gain. Corrects for steady-state error, helping to reach the target exactly.
    private float Ki { get; }
    // Derivative gain. Damps oscillations and reduces overshooting.
    private float Kd { get; }
    private IClampStrategy ClampStrategy { get; }

    public Vector3 Value { get; private set; }

    private Vector3? _integral;
    private Vector3? _previousError;

    public Vector3Pid(float kp, float ki, float kd, IClampStrategy clampStrategy) {
        Kp = kp;
        Ki = ki;
        Kd = kd;
        ClampStrategy = clampStrategy;
        Reset();
    }

    public void Calculate(Vector3 currentPosition, Vector3 targetPosition, float deltaTime) {
        // 1. Calculate the error vector (the direction and distance to the target)
        var error = targetPosition - currentPosition;
        _previousError ??= error;
        
        // 2. Proportional Term: Directly proportional to the error.
        // This is the main force that pushes the object towards the target.
        var p_term = Kp * error;
        
        // 3. Integral Term: Accumulates the error over time.
        // This helps to eliminate any small, steady-state errors (e.g., if friction stops it just short of the target).
        _integral ??= Vector3.zero;
        _integral += error * Time.fixedDeltaTime;
        var i_term = Ki * _integral.Value;
        
        // 4. Derivative Term: Responds to the rate of change of the error.
        // This acts as a damper, slowing the object as it approaches the target to prevent overshooting.
        var derivative = (error - _previousError.Value) / deltaTime;
        _previousError = error; // Update the previous error for the next frame
        var d_term = Kd * derivative;
        
        // Calculate the total output force by summing the P, I, and D terms.
        var value = p_term + i_term + d_term;
        
        // Clamp the force to the maximum value to prevent wild, unstable behavior.
        Value = ClampStrategy switch {
            Clamped clamped => Vector3.ClampMagnitude(value, clamped.MaxMagnitude),
            _ => value
        };
    }

    public void Reset() {
        _integral = null;
        _previousError = null;
    }

    public class Unclamped : IClampStrategy { }
    public class Clamped : IClampStrategy {
        public float MaxMagnitude { get; }

        public Clamped(float maxMagnitude) {
            MaxMagnitude = maxMagnitude;
        }
    }
    public interface IClampStrategy { }
}