using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    public class ListExtra<T> : List<T>
    {
        public int currentIndex=0;
        public ListExtra(int capacity)
        {
            // TODO: Complete member initialization
            this.Capacity = capacity;
        }
        public void AddBaseCapacity(T item)
        {
            //cho vào cuối
            if (this.currentIndex == this.Capacity-1)
            { this.RemoveAt(0);
            goto jump;
            }
            currentIndex++;
            //Xoá từ index chèn -> sau
            while(currentIndex<Count)
                this.RemoveAt(currentIndex);
            //chèn vào currentIndex
            jump:
            this.Add(item);
            
            

         
            
        }
    }
}
