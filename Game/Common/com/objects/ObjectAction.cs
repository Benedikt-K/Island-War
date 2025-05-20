using Common.com.path;

namespace Common.com.objects
{
    public sealed class ObjectAction
    {
        public ObjectAction()
        {
            CurrentPath = null;
            FightingWithId = -1;
            GoingToFightWithId = -1;
            TransportingToId = -1;
            TransportingFromId = -1;
            IsAggressive = false;
            GoingIntoId = -1;
            InfiltrateId = -1;
            GoingToMurderId = -1;
        }

        public ObjectAction Clone()
        {
            var path = CurrentPath?.Clone();
            return new ObjectAction
            {
                CurrentPath = path,
                FightingWithId = FightingWithId,
                GoingToFightWithId = GoingToFightWithId,
                TransportingToId = TransportingToId,
                TransportingFromId = TransportingFromId,
                ItemTransportIntent = ItemTransportIntent,
                IsOccupied = IsOccupied,
                IsAggressive = IsAggressive,
                UserMade = UserMade,
                GoingIntoId = GoingIntoId,
                InfiltrateId = InfiltrateId,
                GoingToMurderId = GoingToMurderId
            };
        }
        public Path CurrentPath { get; set; }
        public int FightingWithId { get; set; }
        public int GoingToFightWithId { get; set; }

        public int TransportingToId { get; set; }
        public int TransportingFromId { get; set; }
        public Item ItemTransportIntent { get; set; }
        public bool IsOccupied { get; set; }
        
        public bool IsAggressive { get; set; }
        public bool UserMade { get; set; }
        public int GoingIntoId { get; set; }
        public int InfiltrateId { get; set; }
        public int GoingToMurderId { get; set; }
        
    }
}