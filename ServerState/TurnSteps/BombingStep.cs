using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.Server.TurnSteps
{
    using Nova.Common;
    using Nova.Common.Waypoints;

    class BombingStep : ITurnStep
    {
        public List<Message> Process(ServerData serverState)
        {
            List<Message> messages = new List<Message>();

            foreach (Fleet fleet in serverState.IterateAllFleets())
            {
                if (fleet.InOrbit != null && fleet.HasBombers)
                    if (fleet.InOrbit != null)
                    {
                        Star star = serverState.AllStars[fleet.InOrbit.Name];
                        if (serverState.AllEmpires[fleet.Owner].IsEnemy(star.Owner))
                        {
                            Bombing bombing = new Bombing();
                            messages.AddRange(bombing.Bomb(fleet, star));
                        }
                    }
            }
         return messages;
       }
    }
}
