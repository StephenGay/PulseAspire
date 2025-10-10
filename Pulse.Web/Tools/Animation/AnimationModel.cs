//using BlazorAnimation;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;
////using Pulse.Web.Annotations;
//using System.Reflection;
//using Pulse.Web.Tools.Animation.Annotations;
////implements IDisposable;

//namespace Pulse.Web.Tools.Animation
//{
//    public class AnimationModel : INotifyPropertyChanged
//    {
//        private string _effect;
//        private string _delay;
//        private string _speed;

//        public AnimationModel(string effect, string delay, string speed, int iterationCount)
//        {
//            Effect = effect;
//            Delay = delay;
//            Speed = speed;
//            IterationCount = iterationCount;
//        }

//        public int IterationCount { get; set; }

//        public string Effect
//        {
//            get => _effect;
//            set
//            {
//                _effect = value;
//                OnPropertyChanged();
//            }
//        }
//        public string Delay
//        {
//            get => _delay;
//            set
//            {
//                _delay = value;
//                OnPropertyChanged();
//            }
//        }
//        public string Speed
//        {
//            get => _speed;
//            set
//            {
//                _speed = value;
//                OnPropertyChanged();
//            }
//        }

//        public event PropertyChangedEventHandler? PropertyChanged;

//        [NotifyPropertyChangedInvocator]
//        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }

        
//    }
//}
