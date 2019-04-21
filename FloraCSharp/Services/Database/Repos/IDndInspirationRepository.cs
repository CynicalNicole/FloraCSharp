using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Repos
{
    public interface IDndInspirationRepository : IRepository<DndInspiration>
    {
        DndInspiration GetOrCreateInspiration(string Name, string Desc, int TNum, int CNum);
        DndInspiration GetInspirationTableCard(int Tnum, int CNum);
        void RemoveInspiration(int Tnum, int CNum);
        int CountInTable(int TNum);
    }
}
