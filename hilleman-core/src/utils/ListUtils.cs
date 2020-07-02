using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.utils
{
    public static class ListUtils
    {
        public static List<List<T>> splitInChunks<T>(List<T> list, Int32 chunkSize)
        {
            List<List<T>> chunks = new List<List<T>>();
            chunks.Add(new List<T>());
            int currentChunkIdx = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (i >= chunkSize && i % chunkSize == 0)
                {
                    currentChunkIdx++;
                    chunks.Add(new List<T>());
                }
                chunks[currentChunkIdx].Add(list[i]);
            }
            return chunks;
        }

        public static IList<T> join<T>(IList<T> listOne, IList<T> listTwo)
        {
            if (listOne == null && listTwo == null)
            {
                return new List<T>();;
            }
            if (listOne != null && listTwo == null)
            {
                return listOne;
            }
            if (listOne == null && listTwo != null)
            {
                return listTwo;
            }

            T[] tempArray = new T[listOne.Count + listTwo.Count];
            if (listOne.Count > 0)
            {
                listOne.CopyTo(tempArray, 0);
            }
            if (listTwo.Count > 0)
            {
                listTwo.CopyTo(tempArray, listOne.Count);
            }

            return new List<T>(tempArray);
        }
    }
}