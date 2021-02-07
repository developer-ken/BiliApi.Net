using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BiliApi.Exceptions
{
    public class ExceptionCollection : Exception, IEnumerable
    {
        private Queue<Exception> exceptions;
        public ExceptionCollection() : base("A series of exception was throwed")
        {
            exceptions = new Queue<Exception>();
        }

        public void Add(Exception e)
        {
            exceptions.Enqueue(e);
        }

        public Exception[] GetAll()
        {
            return exceptions.ToArray();
        }

        public IEnumerator GetEnumerator()
        {
            return exceptions.GetEnumerator();
        }

        public int Count
        {
            get { return exceptions.Count; }
        }
    }
}
