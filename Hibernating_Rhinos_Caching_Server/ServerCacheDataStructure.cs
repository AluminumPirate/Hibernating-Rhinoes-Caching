using System;
using System.Collections.Generic;
using System.Text;

namespace Hibernating_Rhinos_Caching_Server
{
  public class Entry
  {
    public Entry(string name, string value, int size)
    {
      this.Name = name;
      this.Value = value;
      this.Size = size;
    }
    public string Name
    {
      set;
      get;
    }
    public string Value
    {
      set;
      get;
    }
    public int Size
    {
      set;
      get;
    }
  }

  public class DataHolder
  {
    public DataHolder()
    {
      Enrties = new List<Entry>();
    }
    public List<Entry> Enrties
    {
      set;
      get;
    }

  }
}
