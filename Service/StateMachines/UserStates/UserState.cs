using Telegram.Altayskaya97.Core.Enum;

namespace Telegram.Altayskaya97.Service.StateMachines.UserStates
{
    public class UserState<T> where T : struct
    {
        public T CurrentState { get; set; }
        public virtual void ExecuteNextStage()
        {
            int stateVal = EnumConvertor.EnumToInt<T>(CurrentState);
            stateVal++;
            CurrentState = EnumConvertor.IntToEnum<T>(stateVal);
        }
        public virtual void End()
        {
            CurrentState = System.Enum.Parse<T>("Stop");
        }
        public virtual bool IsFinished => System.Enum.GetName(typeof(T), CurrentState) == "Stop";
    }
}
