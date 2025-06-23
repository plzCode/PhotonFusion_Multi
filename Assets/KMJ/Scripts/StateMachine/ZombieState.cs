
namespace Zombie.States
{
    public abstract class ZombieState
    {
        protected ZombieAIController ctrl;

        protected ZombieState(ZombieAIController c) { this.ctrl = c; }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }
}