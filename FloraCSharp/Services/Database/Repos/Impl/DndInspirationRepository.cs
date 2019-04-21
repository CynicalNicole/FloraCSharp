using FloraCSharp.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloraCSharp.Services.Database.Repos.Impl
{
    public class DndInspirationRepository : Repository<DndInspiration>, IDndInspirationRepository
    {
        public DndInspirationRepository(DbContext context) : base(context)
        {
        }

        public DndInspiration GetInspirationTableCard(int TNum, int CNum)
        {
            //Get uniqueID
            int uniqueID = Convert.ToInt32(string.Format("{0}{1}", TNum, CNum));

            //Get it
            return _set.FirstOrDefault(x => x.CombinedNumber == uniqueID);
        }

        public DndInspiration GetOrCreateInspiration(string Name, string Desc, int TNum, int CNum)
        {
            DndInspiration toReturn;

            int uniqueID = Convert.ToInt32(string.Format("{0}{1}", TNum, CNum));

            toReturn = _set.FirstOrDefault(x => x.CombinedNumber == uniqueID);

            if (toReturn == null)
            {
                _set.Add(toReturn = new DndInspiration()
                {
                    CombinedNumber = uniqueID,
                    Name = Name,
                    Description = Desc,
                    TableNumber = TNum,
                    CardNumber = CNum
                });
                _context.SaveChanges();
            }

            return toReturn;
        }

        public DndInspiration RemoveInspiration(int TNum, int CNum)
        {
            DndInspiration toRemove = GetInspirationTableCard(TNum, CNum);
            _set.Remove(toRemove);
            _context.SaveChanges();
            return toRemove;
        }

        public int CountInTable(int TNum)
        {
            return _set.Where(x => x.TableNumber == TNum).ToArray().Count();
        }
    }
}
