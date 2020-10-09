namespace InfiniSwiss.CdpSharp.Commands
{
    public class CdpCommandIdProvider
    {
        public int GetNextId()
        {
            return this.id++;
        }

        private int id = 1;
    }
}
