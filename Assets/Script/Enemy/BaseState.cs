//基本抽象类
public abstract class BaseState 
{
    protected Enemy currentEnemy;
    public abstract void OnEnter(Enemy enemy);

    public abstract void LogicUpdate();

    public abstract void PhysicsUpdate();

    public abstract void OnExit();
}
