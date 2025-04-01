namespace MoeBeam.Game.Scripts.Data
{
    public class EnemyKilledData
    {
        public int Xp { get; private set; }
        public long InstanceId { get; private set; }
        
        public EnemyKilledData(int xp, long instanceId)
        {
            Xp = xp;
            InstanceId = instanceId;
        }
    }
}