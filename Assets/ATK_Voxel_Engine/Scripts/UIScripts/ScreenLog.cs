using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    public class ScreenLog : ITickable
    {
        public TickType TickType => TickType.FIXED_UPDATE;

        public Label Label { get; protected set; }
        float _dur;
        float _timer;

        public ScreenLog(Label label)
        {
            Label = label;
            label.style.display = DisplayStyle.None;
        }

        public void Log(string msg, float dur)
        {
            System.DateTime time = System.DateTime.Now;
            string timeString = string.Format("{0:HH:mm:ss}", time);
            Label.text = "[" + timeString + "]" + " " + msg;
            Label.style.display = DisplayStyle.Flex;
            Label.style.opacity = 1;

            _timer = 0;
            _dur = dur;
            Register();
        }

        public void Register()
        {
            TickRateManager.OnFixedUpdate += Tick;
        }

        public void UnRegister()
        {
            TickRateManager.OnFixedUpdate -= Tick;
        }

        public void Tick(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer > _dur)
            {
                Label.style.display = DisplayStyle.None;
                Label.style.opacity = 0;
                ScreenLogger.AddToStack(this);
                UnRegister();
            }
        }
    }
}
