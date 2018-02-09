using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBSync
{
    public static class Extensions
    {
        public static void Sort<T, K>(this ObservableCollection<T> observable, Func<T, K> keySelector, IComparer<K> comp)
        {
            List<T> sorted = observable.OrderBy(keySelector, comp).ToList();
            observable.Clear();
            foreach (T s in sorted)
            {
                observable.Add(s);
            }
        }
    }
}
