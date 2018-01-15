using Discord;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using FloraCSharp.Extensions;
using System.Threading.Tasks;

namespace FloraCSharp.Services
{
    class EliteColours
    {
        List<string> EliteColourRoleNames = new List<string>()
        {
            "pink-life", "red-life", "blue-life", "orange-life", "purple-life", "yellow-life", "teal-life"
        };

        Dictionary<int, ulong> EliteRoleIds = new Dictionary<int, ulong>()
        {
            { 1, 330094882670772234 }, //Pink
            { 2, 330093327347220490 }, //Red
            { 3, 330092923171635230 }, //Blue
            { 4, 330095123264438272 }, //Orange
            { 5, 330096661940666368 }, //Purple
            { 6, 354727337230729216 }, //Yellow
            { 7, 364191035250704394 } //Teal
        };

        public async Task GiveEliteColour(IGuildUser Sender, IMessageChannel Channel, int Colour)
        {
            if (Channel.Id != 285218502212583424) return;

            foreach (ulong RoleID in Sender.RoleIds)
            {
                IRole role = Sender.Guild.GetRole(RoleID);

                if (EliteColourRoleNames.Contains(role.Name.ToLower()))
                {
                    await Sender.RemoveRoleAsync(role);
                }
            }

            if (Colour != 0)
            {
                //Get role from ID which we grab from the Int to ID Dictionary
                IRole role = Sender.Guild.GetRole(EliteRoleIds[Colour]);

                await Sender.AddRoleAsync(role);
            }

            await Channel.SendSuccessAsync("Success!");
        }
    }
}
