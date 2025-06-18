public abstract class ZombieState
{
    protected ZombieAIController controller;

    public ZombieState(ZombieAIController controller)
    {
        this.controller = controller;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}