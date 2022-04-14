using UnityEngine;
namespace StateStuff
{
	public class StateMachine<T> : MonoBehaviour
    {
        public State<T> currentState { get; private set; }
        public T Owner;
		public bool stopBehavior;

        public StateMachine(T _o)
        {
            Owner = _o;
            currentState = null;
        }

        public void ChangeState(State<T> _newstate)
        {
            if(currentState != null)
                currentState.ExitState(Owner);
            currentState = _newstate;
            currentState.EnterState(Owner);
        }

        public void Update()
        {
			if (currentState != null && !stopBehavior)
                currentState.UpdateState(Owner);
        }
    }

	public abstract class State<T> : MonoBehaviour
    {
		public abstract bool EnterState(T _owner);
        public abstract void ExitState(T _owner);
        public abstract void UpdateState(T _owner);

		public Coroutine currentCoroutine;

		public virtual void ForceInterruption (T _owner){
		}

		public virtual void OnWeaponTriggerEnter (T _owner, Collider slashed){
		}
    }
}