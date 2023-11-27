// this class is useful for limiting updates to a specified interval.
//
// Example (update will run every 1.1 seconds):
//
//   public partial class MySystem : SystemBase
//   {
//      UpdateTimer m_timer = new UpdateTimer(1.1f);
//      protected override void OnUpdate()
//      {
//          if (m_timer.IsNotReady(Time.DeltaTime)) { return; }
//          .... do stuff here....
//      }
//   } 
//
//   public partial struct MySystem : ISystem
//   {
//      UpdateTimer m_timer;
//      void OnCreate(ref SystemState state)
//      {
//         m_timer = new UpdateTimer(1.1f);
//      }
//      void OnUpdate(ref SystemState state)
//      {
//         if (m_timer.IsNotReady(Time.DeltaTime)) { return; }
//      }
//   }

namespace AnimCooker
    {
    public struct UpdateTimer
    {
        float m_cooldownInterval;
        float m_cooldown;

        // constructor
        public UpdateTimer(float cooldownInterval)
        {
            m_cooldownInterval = cooldownInterval;
            m_cooldown = m_cooldownInterval;
        }

        // returns the current interval set by the user
        public float GetInterval() { return m_cooldownInterval; }

        // returns the current frequency set by the user
        public float GetFrequency() { return 1.0f / m_cooldownInterval; }

        // set the interval (resets the timer)
        public void SetInterval(float cooldownInterval)
        {
            m_cooldownInterval = cooldownInterval;
            m_cooldown = m_cooldownInterval;
        }

        // returns true if the timer hasn't cooled down yet.
        // if the cooldown is reached, the timer will be reset and false will be returned.
        public bool IsNotReady(float deltaTime)
        {
            // only let this function get called periodically
            m_cooldown -= deltaTime;
            if (m_cooldown > 0f) { return true; }
            m_cooldown = m_cooldownInterval; // reset the timer
            return false;
        }

        // returns the amount of time that has ellapsed so far (since the last reset)
        public float GetEllapsedTime()
        {
            return m_cooldownInterval - m_cooldown;
        }

        // returns the number of seconds remaining before cooldown.
        public float GetTimeRemaining()
        {
            return m_cooldown;
        }

        public bool IsReady(float deltaTime)
        {
            return !IsNotReady(deltaTime);
        }
    }
} // namespace