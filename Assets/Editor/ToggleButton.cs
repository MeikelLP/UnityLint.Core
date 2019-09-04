using UnityEngine.UIElements;

namespace Editor
{
    public class ToggleButton : Button
    {
        private bool _active;

        public bool Active
        {
            get => _active;
            set
            {
                if (value)
                {
                    var list = parent.Query<ToggleButton>().ToList();
                    foreach (var toggle in list)
                    {
                        toggle.Active = false;
                    }
                }

                _active = value;
            }
        }

        public override void HandleEvent(EventBase evt)
        {
            if (evt is MouseUpEvent)
            {
                Active = true;
            }
            else
            {
                base.HandleEvent(evt);
            }
        }
    }
}
